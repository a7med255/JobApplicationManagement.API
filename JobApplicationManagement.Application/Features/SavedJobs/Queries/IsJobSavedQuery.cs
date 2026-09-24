using JobApplicationManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.SavedJobs.Queries;

/// <summary>
/// Checks whether a specific job is saved by the authenticated candidate.
/// </summary>
public record IsJobSavedQuery(int JobId) : IRequest<bool>;

public class IsJobSavedQueryHandler : IRequestHandler<IsJobSavedQuery, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public IsJobSavedQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(IsJobSavedQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var candidate = await _unitOfWork.Candidates.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (candidate is null)
            return false;

        return await _unitOfWork.SavedJobs.Query()
            .AsNoTracking()
            .AnyAsync(sj => sj.CandidateId == candidate.Id && sj.JobId == request.JobId, cancellationToken);
    }
}
