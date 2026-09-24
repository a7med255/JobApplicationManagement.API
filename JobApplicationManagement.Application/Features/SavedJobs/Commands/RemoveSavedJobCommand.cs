using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobApplicationManagement.Application.Features.SavedJobs.Commands;

/// <summary>
/// Removes a saved job for the authenticated candidate.
/// </summary>
public record RemoveSavedJobCommand(int JobId) : IRequest<Unit>;

public class RemoveSavedJobCommandHandler : IRequestHandler<RemoveSavedJobCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RemoveSavedJobCommandHandler> _logger;

    public RemoveSavedJobCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<RemoveSavedJobCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Unit> Handle(RemoveSavedJobCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var candidate = await _unitOfWork.Candidates.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (candidate is null)
            throw new ForbiddenException("Only registered candidates can manage saved jobs.");

        var savedJob = await _unitOfWork.SavedJobs.Query()
            .FirstOrDefaultAsync(sj => sj.CandidateId == candidate.Id && sj.JobId == request.JobId, cancellationToken);

        if (savedJob is null)
            throw new NotFoundException(nameof(SavedJob), request.JobId);

        _unitOfWork.SavedJobs.Delete(savedJob);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("SavedJobRemoved: CandidateId {CandidateId} removed saved JobId {JobId}", candidate.Id, request.JobId);

        return Unit.Value;
    }
}
