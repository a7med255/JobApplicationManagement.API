using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Applications.Queries;

public record GetAllApplicationsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<JobApplicationResponseDto>>;

public class GetAllApplicationsQueryHandler : IRequestHandler<GetAllApplicationsQuery, PaginatedResult<JobApplicationResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllApplicationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<JobApplicationResponseDto>> Handle(GetAllApplicationsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.JobApplications.Query().AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new JobApplicationResponseDto
            {
                Id = a.Id,
                CandidateId = a.CandidateId,
                CandidateName = a.Candidate.Name,
                JobId = a.JobId,
                JobTitle = a.Job.Title,
                CompanyName = a.Job.Company.Name,
                SubmittedCvId = a.SubmittedCvId,
                SubmittedCvDisplayName = a.SubmittedCv == null ? null : a.SubmittedCv.DisplayName,
                SubmittedCvFileName = a.SubmittedCv == null ? null : a.SubmittedCv.OriginalFileName,
                JobApplicationStatus = a.JobApplicationStatus,
                AppliedAt = a.AppliedAt,
                StatusUpdatedAt = a.StatusUpdatedAt,
                CancelledAt = a.CancelledAt
                ,CoverLetter = a.CoverLetter
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
