using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using JobApplicationManagement.Application.Features.Candidates.Interfaces;
using JobApplicationManagement.Domain.Entities;
using AppValidationException = JobApplicationManagement.Application.Common.Exceptions.ValidationException;

namespace JobApplicationManagement.Application.Features.Candidates.Services;

public class CandidateService : ICandidateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCandidateDto> _createValidator;
    private readonly IValidator<UpdateCandidateDto> _updateValidator;
    private readonly ILogger<CandidateService> _logger;

    private const string AdminRole = "Admin";

    public CandidateService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateCandidateDto> createValidator,
        IValidator<UpdateCandidateDto> updateValidator,
        ILogger<CandidateService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<CandidateResponseDto> CreateCandidateAsync(CreateCandidateDto createDto)
    {
        _logger.LogInformation("Creating candidate with UserId {UserId}", createDto.UserId);

        await ValidateAsync(_createValidator, createDto);

        var candidate = _mapper.Map<Candidate>(createDto);

        await _unitOfWork.Candidates.AddAsync(candidate);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Candidate created successfully with Id {CandidateId}", candidate.Id);

        return _mapper.Map<CandidateResponseDto>(candidate);
    }

    public async Task<PaginatedResult<CandidateResponseDto>> GetAllCandidatesAsync(int pageNumber, int pageSize)
    {
        _logger.LogInformation("Getting candidates page {PageNumber} with size {PageSize}", pageNumber, pageSize);

        var query = _unitOfWork.Candidates.Query().AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CandidateResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                CvUrl = c.CvUrl,
                UserId = c.UserId
            })
            .ToListAsync();

        return new PaginatedResult<CandidateResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CandidateResponseDto> GetCandidateByIdAsync(int id, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Getting candidate with Id {CandidateId}", id);

        var candidate = await _unitOfWork.Candidates.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (candidate is null)
            throw new NotFoundException(nameof(Candidate), id);

        VerifyOwnershipOrAdmin(candidate, userId, roles, "read");

        return _mapper.Map<CandidateResponseDto>(candidate);
    }

    public async Task<CandidateResponseDto> UpdateCandidateAsync(int id, UpdateCandidateDto updateDto, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Updating candidate with Id {CandidateId}", id);

        await ValidateAsync(_updateValidator, updateDto);

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(id);
        if (candidate is null)
            throw new NotFoundException(nameof(Candidate), id);

        VerifyOwnershipOrAdmin(candidate, userId, roles, "update");

        _mapper.Map(updateDto, candidate);
        _unitOfWork.Candidates.Update(candidate);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Candidate {CandidateId} updated successfully by user {UserId}", id, userId);

        return _mapper.Map<CandidateResponseDto>(candidate);
    }

    public async Task DeleteCandidateAsync(int id, string userId, IEnumerable<string> roles)
    {
        _logger.LogInformation("Deleting candidate with Id {CandidateId}", id);

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(id);
        if (candidate is null)
            throw new NotFoundException(nameof(Candidate), id);

        VerifyOwnershipOrAdmin(candidate, userId, roles, "delete");

        _unitOfWork.Candidates.Delete(candidate);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Candidate {CandidateId} deleted successfully by user {UserId}", id, userId);
    }

    private void VerifyOwnershipOrAdmin(Candidate candidate, string userId, IEnumerable<string> roles, string operation)
    {
        if (roles.Contains(AdminRole))
            return;

        if (candidate.UserId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to {Operation} candidate {CandidateId} but does not own it",
                userId, operation, candidate.Id);
            throw new ForbiddenException($"You do not have permission to {operation} this candidate profile.");
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
