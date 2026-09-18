using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;
using JobApplicationManagement.Application.Features.Recruiters.Interfaces;
using JobApplicationManagement.Domain.Entities;
using AppValidationException = JobApplicationManagement.Application.Common.Exceptions.ValidationException;

namespace JobApplicationManagement.Application.Features.Recruiters.Services;

public class RecruiterService : IRecruiterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateRecruiterDto> _createValidator;
    private readonly IValidator<UpdateRecruiterDto> _updateValidator;
    private readonly ILogger<RecruiterService> _logger;

    private const string AdminRole = "Admin";

    public RecruiterService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateRecruiterDto> createValidator,
        IValidator<UpdateRecruiterDto> updateValidator,
        ILogger<RecruiterService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<RecruiterResponseDto> CreateRecruiterAsync(CreateRecruiterDto createDto)
    {
        _logger.LogInformation("Creating recruiter with UserId {UserId}", createDto.UserId);

        await ValidateAsync(_createValidator, createDto);

        var recruiter = _mapper.Map<Recruiter>(createDto);

        await _unitOfWork.Recruiters.AddAsync(recruiter);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Recruiter created successfully with Id {RecruiterId}", recruiter.Id);

        return _mapper.Map<RecruiterResponseDto>(recruiter);
    }

    public async Task<PaginatedResult<RecruiterResponseDto>> GetAllRecruitersAsync(int pageNumber, int pageSize)
    {
        _logger.LogInformation("Getting recruiters page {PageNumber} with size {PageSize}", pageNumber, pageSize);

        var query = _unitOfWork.Recruiters.Query().AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(r => r.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RecruiterResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                UserId = r.UserId
            })
            .ToListAsync();

        return new PaginatedResult<RecruiterResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<RecruiterResponseDto> GetRecruiterByIdAsync(int id, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Getting recruiter with Id {RecruiterId}", id);

        var recruiter = await _unitOfWork.Recruiters.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recruiter is null)
            throw new NotFoundException(nameof(Recruiter), id);

        VerifyOwnershipOrAdmin(recruiter, userId, roles, "read");

        return _mapper.Map<RecruiterResponseDto>(recruiter);
    }

    public async Task<RecruiterResponseDto> UpdateRecruiterAsync(int id, UpdateRecruiterDto updateDto, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Updating recruiter with Id {RecruiterId}", id);

        await ValidateAsync(_updateValidator, updateDto);

        var recruiter = await _unitOfWork.Recruiters.GetByIdAsync(id);
        if (recruiter is null)
            throw new NotFoundException(nameof(Recruiter), id);

        VerifyOwnershipOrAdmin(recruiter, userId, roles, "update");

        _mapper.Map(updateDto, recruiter);
        _unitOfWork.Recruiters.Update(recruiter);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Recruiter {RecruiterId} updated successfully by user {UserId}", id, userId);

        return _mapper.Map<RecruiterResponseDto>(recruiter);
    }

    public async Task DeleteRecruiterAsync(int id, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Deleting recruiter with Id {RecruiterId}", id);

        var recruiter = await _unitOfWork.Recruiters.GetByIdAsync(id);
        if (recruiter is null)
            throw new NotFoundException(nameof(Recruiter), id);

        VerifyOwnershipOrAdmin(recruiter, userId, roles, "delete");

        _unitOfWork.Recruiters.Delete(recruiter);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Recruiter {RecruiterId} deleted successfully by user {UserId}", id, userId);
    }

    private void VerifyOwnershipOrAdmin(Recruiter recruiter, string userId, IEnumerable<string> roles, string operation)
    {
        if (roles.Contains(AdminRole))
            return;

        if (recruiter.UserId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to {Operation} recruiter {RecruiterId} but does not own it",
                userId, operation, recruiter.Id);
            throw new ForbiddenException($"You do not have permission to {operation} this recruiter profile.");
        }
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
