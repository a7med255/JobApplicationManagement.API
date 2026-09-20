using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Applications.Queries;

public record GetJobApplicationsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<JobApplicationResponseDto>>;

public class GetJobApplicationsQueryHandler : IRequestHandler<GetJobApplicationsQuery, PaginatedResult<JobApplicationResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetJobApplicationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<JobApplicationResponseDto>> Handle(GetJobApplicationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var recruiter = await _unitOfWork.Recruiters.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);

        if (recruiter is null)
        {
            return new PaginatedResult<JobApplicationResponseDto> { Items = new List<JobApplicationResponseDto>(), PageNumber = request.PageNumber, PageSize = request.PageSize, TotalCount = 0 };
        }

        var query = _unitOfWork.JobApplications.Query()
            .AsNoTracking()
            .Where(a => a.Job.RecruiterId == recruiter.Id);

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
