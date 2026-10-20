using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Queries;

public record GetMyJobsQuery(JobSearchRequest Request) : IRequest<PaginatedResult<JobSummaryDto>>;

public class GetMyJobsQueryHandler : IRequestHandler<GetMyJobsQuery, PaginatedResult<JobSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyJobsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<JobSummaryDto>> Handle(
        GetMyJobsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
            throw new UnauthorizedException("Authentication is required to view your jobs.");

        var recruiterId = await _unitOfWork.Recruiters.Query()
            .Where(recruiter => recruiter.UserId == _currentUserService.UserId)
            .Select(recruiter => (int?)recruiter.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (!recruiterId.HasValue)
            throw new ForbiddenException("Only registered recruiters can manage job postings.");

        var filters = request.Request;
        var pageNumber = Math.Max(filters.PageNumber, 1);
        var pageSize = Math.Clamp(filters.PageSize, 1, 50);
        var query = _unitOfWork.Jobs.Query().AsNoTracking()
            .Where(job => job.RecruiterId == recruiterId.Value);
        if (filters.Status.HasValue)
            query = query.Where(job => job.Status == filters.Status.Value);

        query = JobSearchFilters.Apply(query, filters, DateTime.UtcNow);
        var totalCount = await query.CountAsync(cancellationToken);
        var jobs = await JobSearchFilters.ApplySort(query, filters.Sort)
            .Include(job => job.Company)
            .Include(job => job.JobSkills).ThenInclude(jobSkill => jobSkill.Skill)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<JobSummaryDto>
        {
            Items = jobs.Select(JobDtoFactory.ToSummary).ToArray(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
