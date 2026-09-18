using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Application.Features.Jobs.Interfaces;
using JobApplicationManagement.Domain.Entities;
using AppValidationException = JobApplicationManagement.Application.Common.Exceptions.ValidationException;

namespace JobApplicationManagement.Application.Features.Jobs.Services;

/// <summary>
/// Application service for Job operations.
/// Orchestrates validation, mapping, business defaults, persistence, and response projection.
/// </summary>
public class JobService : IJobService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateJobDto> _createValidator;
    private readonly IValidator<UpdateJobDto> _updateValidator;
    private readonly ILogger<JobService> _logger;

    private const string AdminRole = "Admin";

    public JobService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateJobDto> createValidator,
        IValidator<UpdateJobDto> updateValidator,
        ILogger<JobService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<JobResponseDto> CreateJobAsync(CreateJobDto createJobDto, string createdByUserId)
    {
        _logger.LogInformation("Creating job with title {JobTitle}", createJobDto.Title);

        await ValidateAsync(_createValidator, createJobDto);

        var recruiter = await _unitOfWork.Recruiters.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == createdByUserId);

        if (recruiter is null)
        {
            _logger.LogWarning("Recruiter profile not found for user {UserId}", createdByUserId);
            throw new ForbiddenException("Only registered recruiters can create jobs.");
        }

        var job = _mapper.Map<Job>(createJobDto);
        job.IsActive = true;
        job.RecruiterId = recruiter.Id;

        await _unitOfWork.Jobs.AddAsync(job);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Job created successfully with Id {JobId} and title {JobTitle}", job.Id, job.Title);

        return _mapper.Map<JobResponseDto>(job);
    }

    public async Task<PaginatedResult<JobResponseDto>> GetAllJobsAsync(int pageNumber, int pageSize)
    {
        _logger.LogInformation("Getting jobs page {PageNumber} with size {PageSize}", pageNumber, pageSize);

        // IQueryable pipeline: AsNoTracking → Count → Skip → Take → Projection → Materialize
        var query = _unitOfWork.Jobs.Query().AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(j => j.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobResponseDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                IsActive = j.IsActive
            })
            .ToListAsync();

        return new PaginatedResult<JobResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<JobResponseDto> GetJobByIdAsync(int id)
    {
        _logger.LogInformation("Getting job with Id {JobId}", id);

        var job = await _unitOfWork.Jobs.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job is null)
            throw new NotFoundException(nameof(Job), id);

        return _mapper.Map<JobResponseDto>(job);
    }

    public async Task<JobResponseDto> UpdateJobAsync(int id, UpdateJobDto updateJobDto, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Updating job with Id {JobId}", id);

        await ValidateAsync(_updateValidator, updateJobDto);

        var job = await _unitOfWork.Jobs.GetByIdAsync(id);
        if (job is null)
            throw new NotFoundException(nameof(Job), id);

        // Ownership check — Admin can update any job, Recruiter must own it
        await VerifyOwnershipOrAdminAsync(job, userId, roles, "update");

        _mapper.Map(updateJobDto, job);
        _unitOfWork.Jobs.Update(job);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Job {JobId} updated successfully by user {UserId}", id, userId);

        return _mapper.Map<JobResponseDto>(job);
    }

    public async Task DeleteJobAsync(int id, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Deleting job with Id {JobId}", id);

        var job = await _unitOfWork.Jobs.GetByIdAsync(id);
        if (job is null)
            throw new NotFoundException(nameof(Job), id);

        // Ownership check — Admin can delete any job, Recruiter must own it
        await VerifyOwnershipOrAdminAsync(job, userId, roles, "delete");

        _unitOfWork.Jobs.Delete(job);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Job {JobId} deleted successfully by user {UserId}", id, userId);
    }

    public async Task<CloseJobResponseDto> CloseJobAsync(int id, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Closing job {JobId} by user {UserId}", id, userId);

        var job = await _unitOfWork.Jobs.GetByIdAsync(id);
        if (job is null)
            throw new NotFoundException(nameof(Job), id);

        // Ownership check — Admin can close any job, Recruiter must own it
        await VerifyOwnershipOrAdminAsync(job, userId, roles, "close");

        var recruiter = await _unitOfWork.Recruiters.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId);

        job.IsActive = false;
        job.ClosedAt = DateTime.UtcNow;
        job.ClosedById = recruiter?.Id;

        _unitOfWork.Jobs.Update(job);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Job {JobId} closed by user {UserId}", id, userId);

        return _mapper.Map<CloseJobResponseDto>(job);
    }

    /// <summary>
    /// Verifies that the user owns the job or has the Admin role.
    /// Throws ForbiddenException if neither condition is met.
    /// </summary>
    private async Task VerifyOwnershipOrAdminAsync(Job job, string userId, IEnumerable<string> roles, string operation)
    {
        var rolesList = roles.ToList();
        if (rolesList.Contains(AdminRole))
            return;

        var recruiter = await _unitOfWork.Recruiters.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId);

        if (recruiter is null || job.RecruiterId != recruiter.Id)
        {
            _logger.LogWarning(
                "User {UserId} attempted to {Operation} job {JobId} but does not own it",
                userId, operation, job.Id);
            throw new ForbiddenException($"You do not have permission to {operation} this job.");
        }
    }

    /// <summary>
    /// Shared validation helper — validates a DTO and throws AppValidationException on failure.
    /// </summary>
    private async Task ValidateAsync<T>(IValidator<T> validator, T dto)
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            _logger.LogWarning("Validation failed. Errors: {Errors}", errors);
            throw new AppValidationException(errors);
        }
    }
}
