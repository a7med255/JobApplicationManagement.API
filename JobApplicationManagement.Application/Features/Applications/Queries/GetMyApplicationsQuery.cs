using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Applications.Queries;

public record GetMyApplicationsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<JobApplicationResponseDto>>;

public class GetMyApplicationsQueryHandler : IRequestHandler<GetMyApplicationsQuery, PaginatedResult<JobApplicationResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyApplicationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<JobApplicationResponseDto>> Handle(GetMyApplicationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var candidate = await _unitOfWork.Candidates.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (candidate is null)
        {
            return new PaginatedResult<JobApplicationResponseDto> { Items = new List<JobApplicationResponseDto>(), PageNumber = request.PageNumber, PageSize = request.PageSize, TotalCount = 0 };
        }

        var query = _unitOfWork.JobApplications.Query()
            .AsNoTracking()
            .Where(a => a.CandidateId == candidate.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
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
            .ToListAsync(cancellationToken);

        return new PaginatedResult<JobApplicationResponseDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
