using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.SavedJobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobApplicationManagement.Application.Features.SavedJobs.Commands;

/// <summary>
/// Saves a job for the authenticated candidate.
/// </summary>
public record SaveJobCommand(int JobId) : IRequest<SavedJobResponseDto>;

public class SaveJobCommandHandler : IRequestHandler<SaveJobCommand, SavedJobResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SaveJobCommandHandler> _logger;

    public SaveJobCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<SaveJobCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<SavedJobResponseDto> Handle(SaveJobCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var candidate = await _unitOfWork.Candidates.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (candidate is null)
            throw new ForbiddenException("Only registered candidates can save jobs.");

        var job = await _unitOfWork.Jobs.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);

        if (job is null)
            throw new NotFoundException(nameof(Job), request.JobId);

        var alreadySaved = await _unitOfWork.SavedJobs.Query()
            .AsNoTracking()
            .AnyAsync(sj => sj.CandidateId == candidate.Id && sj.JobId == request.JobId, cancellationToken);

        if (alreadySaved)
            throw new ConflictException("You have already saved this job.");

        var savedJob = new SavedJob
        {
            CandidateId = candidate.Id,
            JobId = request.JobId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SavedJobs.AddAsync(savedJob);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("JobSaved: CandidateId {CandidateId} saved JobId {JobId}", candidate.Id, request.JobId);

        return new SavedJobResponseDto
        {
            Id = savedJob.Id,
            JobId = job.Id,
            JobTitle = job.Title,
            JobDescription = job.Description,
            IsActive = job.IsActive,
            SavedAt = savedJob.CreatedAt
        };
    }
}
