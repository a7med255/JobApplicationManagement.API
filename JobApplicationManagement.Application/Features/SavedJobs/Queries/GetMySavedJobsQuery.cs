using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.SavedJobs.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.SavedJobs.Queries;

/// <summary>
/// Returns a paginated list of saved jobs for the authenticated candidate.
/// </summary>
public record GetMySavedJobsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<SavedJobResponseDto>>;

public class GetMySavedJobsQueryHandler : IRequestHandler<GetMySavedJobsQuery, PaginatedResult<SavedJobResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMySavedJobsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<SavedJobResponseDto>> Handle(GetMySavedJobsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var candidate = await _unitOfWork.Candidates.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (candidate is null)
        {
            return new PaginatedResult<SavedJobResponseDto>
            {
                Items = new List<SavedJobResponseDto>(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = 0
            };
        }

        var query = _unitOfWork.SavedJobs.Query()
            .AsNoTracking()
            .Where(sj => sj.CandidateId == candidate.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(sj => sj.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sj => new SavedJobResponseDto
            {
                Id = sj.Id,
                JobId = sj.JobId,
                JobTitle = sj.Job.Title,
                JobDescription = sj.Job.Description,
                IsActive = sj.Job.IsActive,
                SavedAt = sj.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<SavedJobResponseDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
