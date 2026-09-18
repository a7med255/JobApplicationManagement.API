using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Application.Features.Applications.Interfaces;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using AppValidationException = JobApplicationManagement.Application.Common.Exceptions.ValidationException;
namespace JobApplicationManagement.Application.Features.Applications.Services;

/// <summary>
/// Application service for JobApplication operations.
/// </summary>
public class ApplicationService : IApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateJobApplicationDto> _createValidator;
    private readonly IValidator<UpdateJobApplicationDto> _updateValidator;
    private readonly ILogger<ApplicationService> _logger;

    private const string AdminRole = "Admin";
    private const string RecruiterRole = "Recruiter";
    private const string CandidateRole = "Candidate";

    public ApplicationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateJobApplicationDto> createValidator,
        IValidator<UpdateJobApplicationDto> updateValidator,
        ILogger<ApplicationService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<JobApplicationResponseDto> CreateApplicationAsync(CreateJobApplicationDto createDto, string userId)
    {
        _logger.LogInformation("User {UserId} creating application for Job {JobId}", userId, createDto.JobId);

        await ValidateAsync(_createValidator, createDto);

        var candidate = await _unitOfWork.Candidates.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (candidate is null)
        {
            _logger.LogWarning("No candidate profile found for user {UserId}", userId);
            throw new ForbiddenException("Only registered candidates can apply for jobs.");
        }

        var job = await _unitOfWork.Jobs.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == createDto.JobId);

        if (job is null)
            throw new NotFoundException(nameof(Job), createDto.JobId);

        if (!job.IsActive)
            throw new ConflictException("Cannot apply to a closed job.");

        // Check if already applied
        var existingApplication = await _unitOfWork.JobApplications.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.CandidateId == candidate.Id && a.JobId == job.Id);

        if (existingApplication != null)
            throw new ConflictException("You have already applied for this job.");

        var application = _mapper.Map<JobApplication>(createDto);
        application.CandidateId = candidate.Id;
        application.JobApplicationStatus = JobApplicationStatus.Applied;
        application.AppliedAt = DateTime.UtcNow;
        application.StatusUpdatedAt = DateTime.UtcNow;

        await _unitOfWork.JobApplications.AddAsync(application);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("JobApplication created successfully with Id {ApplicationId}", application.Id);

        return _mapper.Map<JobApplicationResponseDto>(application);
    }

    public async Task<PaginatedResult<JobApplicationResponseDto>> GetAllApplicationsAsync(int pageNumber, int pageSize, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Getting job applications page {PageNumber} with size {PageSize} for user {UserId}", pageNumber, pageSize, userId);

        var query = _unitOfWork.JobApplications.Query().AsNoTracking();
        var rolesList = roles.ToList();

        if (!rolesList.Contains(AdminRole))
        {
            if (rolesList.Contains(RecruiterRole))
            {
                var recruiter = await _unitOfWork.Recruiters.Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.UserId == userId);

                if (recruiter != null)
                {
                    query = query.Where(a => a.Job.RecruiterId == recruiter.Id);
                }
                else
                {
                    query = query.Where(a => false); // Return empty if Recruiter profile not found
                }
            }
            else if (rolesList.Contains(CandidateRole))
            {
                var candidate = await _unitOfWork.Candidates.Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (candidate != null)
                {
                    query = query.Where(a => a.CandidateId == candidate.Id);
                }
                else
                {
                    query = query.Where(a => false); // Return empty
                }
            }
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new JobApplicationResponseDto
            {
                Id = a.Id,
                CandidateId = a.CandidateId,
                JobId = a.JobId,
                JobApplicationStatus = a.JobApplicationStatus,
                AppliedAt = a.AppliedAt,
                StatusUpdatedAt = a.StatusUpdatedAt,
                CancelledAt = a.CancelledAt
            })
            .ToListAsync();

        return new PaginatedResult<JobApplicationResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<JobApplicationResponseDto> GetApplicationByIdAsync(int id, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Getting job application with Id {ApplicationId}", id);

        var application = await _unitOfWork.JobApplications.Query()
            .AsNoTracking()
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application is null)
            throw new NotFoundException(nameof(JobApplication), id);

        await VerifyApplicationAccessAsync(application, userId, roles);

        return _mapper.Map<JobApplicationResponseDto>(application);
    }

    public async Task<JobApplicationResponseDto> UpdateApplicationAsync(int id, UpdateJobApplicationDto updateDto, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Updating job application with Id {ApplicationId}", id);

        await ValidateAsync(_updateValidator, updateDto);

        var application = await _unitOfWork.JobApplications.Query()
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application is null)
            throw new NotFoundException(nameof(JobApplication), id);

        // Ownership: Admin or the Recruiter who owns the job. (Candidates cannot update status)
        var rolesList = roles.ToList();
        if (!rolesList.Contains(AdminRole))
        {
            var recruiter = await _unitOfWork.Recruiters.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recruiter is null || application.Job.RecruiterId != recruiter.Id)
            {
                _logger.LogWarning("User {UserId} attempted to update application {ApplicationId} but does not own the related job.", userId, id);
                throw new ForbiddenException("You do not have permission to update this application.");
            }
        }

        application.JobApplicationStatus = updateDto.JobApplicationStatus;
        application.StatusUpdatedAt = DateTime.UtcNow;

        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("JobApplication {ApplicationId} updated successfully by user {UserId}", id, userId);

        return _mapper.Map<JobApplicationResponseDto>(application);
    }

    public async Task DeleteApplicationAsync(int id)
    {
        _logger.LogInformation("Deleting job application with Id {ApplicationId}", id);

        var application = await _unitOfWork.JobApplications.GetByIdAsync(id);
        if (application is null)
            throw new NotFoundException(nameof(JobApplication), id);

        // Only Admin hits this method from Controller.

        _unitOfWork.JobApplications.Delete(application);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("JobApplication {ApplicationId} deleted successfully.", id);
    }

    public async Task CancelApplicationAsync(int applicationId, string userId)
    {
        _logger.LogInformation("Cancelling application {ApplicationId} by user {UserId}", applicationId, userId);

        // 1. Find the application
        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId);
        if (application is null)
            throw new NotFoundException("JobApplication", applicationId);

        // 2. Find the candidate linked to the authenticated user
        var candidate = await _unitOfWork.Candidates.Query()
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (candidate is null)
            throw new ForbiddenException("No candidate profile found for the authenticated user.");

        // 3. Ownership check — candidate must own this application
        if (application.CandidateId != candidate.Id)
            throw new ForbiddenException("You do not have permission to cancel this application.");

        // 4. Status check — only Applied or UnderReview can be cancelled
        if (application.JobApplicationStatus != JobApplicationStatus.Applied &&
            application.JobApplicationStatus != JobApplicationStatus.UnderReview)
        {
            throw new ConflictException(
                $"Cannot cancel application with status '{application.JobApplicationStatus}'. " +
                "Only applications with status 'Applied' or 'UnderReview' can be cancelled.");
        }

        // 5. Apply cancellation
        application.JobApplicationStatus = JobApplicationStatus.Cancelled;
        application.CancelledAt = DateTime.UtcNow;
        application.StatusUpdatedAt = DateTime.UtcNow;

        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Application {ApplicationId} cancelled successfully by user {UserId}", applicationId, userId);
    }

    private async Task VerifyApplicationAccessAsync(JobApplication application, string userId, IEnumerable<string> roles)
    {
        var rolesList = roles.ToList();
        if (rolesList.Contains(AdminRole))
            return;

        if (rolesList.Contains(RecruiterRole))
        {
            var recruiter = await _unitOfWork.Recruiters.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recruiter != null && application.Job.RecruiterId == recruiter.Id)
                return;
        }

        if (rolesList.Contains(CandidateRole))
        {
            var candidate = await _unitOfWork.Candidates.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (candidate != null && application.CandidateId == candidate.Id)
                return;
        }

        _logger.LogWarning("User {UserId} attempted to access application {ApplicationId} without permission", userId, application.Id);
        throw new ForbiddenException("You do not have permission to access this application.");
    }

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
