using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Jobs.Queries;

public record GetJobsQuery(JobSearchRequest Request) : IRequest<PaginatedResult<JobSummaryDto>>;

public class GetJobsQueryHandler : IRequestHandler<GetJobsQuery, PaginatedResult<JobSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetJobsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<JobSummaryDto>> Handle(
        GetJobsQuery request,
        CancellationToken cancellationToken)
    {
        var filters = request.Request;
        var pageNumber = Math.Max(filters.PageNumber, 1);
        var pageSize = Math.Clamp(filters.PageSize, 1, 50);
        var now = DateTime.UtcNow;
        var isAdmin = _currentUserService.Roles.Contains("Admin");

        IQueryable<Job> query = _unitOfWork.Jobs.Query().AsNoTracking();

        if (!(isAdmin && filters.IncludeInactive))
        {
            query = query.Where(job =>
                job.Status == JobStatus.Published &&
                (!job.ApplicationDeadline.HasValue || job.ApplicationDeadline > now));
        }
        else if (filters.Status.HasValue)
        {
            query = query.Where(job => job.Status == filters.Status.Value);
        }

        query = JobSearchFilters.Apply(query, filters, now);
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

internal static class JobSearchFilters
{
    public static IQueryable<Job> Apply(IQueryable<Job> query, JobSearchRequest filters, DateTime now)
    {
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.Trim();
            query = query.Where(job =>
                job.Title.Contains(search) ||
                job.Description.Contains(search) ||
                job.Company.Name.Contains(search) ||
                job.JobSkills.Any(jobSkill => jobSkill.Skill.Name.Contains(search)));
        }

        if (filters.CompanyId.HasValue)
            query = query.Where(job => job.CompanyId == filters.CompanyId.Value);
        if (!string.IsNullOrWhiteSpace(filters.Department))
            query = query.Where(job => job.Department == filters.Department.Trim());
        if (!string.IsNullOrWhiteSpace(filters.Category))
            query = query.Where(job => job.Category == filters.Category.Trim());
        if (!string.IsNullOrWhiteSpace(filters.Country))
            query = query.Where(job => job.Country == filters.Country.Trim());
        if (!string.IsNullOrWhiteSpace(filters.City))
            query = query.Where(job => job.City == filters.City.Trim());
        if (filters.WorkplaceType.HasValue)
            query = query.Where(job => job.WorkplaceType == filters.WorkplaceType.Value);
        if (filters.EmploymentType.HasValue)
            query = query.Where(job => job.EmploymentType == filters.EmploymentType.Value);
        if (filters.ExperienceLevel.HasValue)
            query = query.Where(job => job.ExperienceLevel == filters.ExperienceLevel.Value);
        if (filters.MinimumSalary.HasValue)
            query = query.Where(job => job.IsSalaryVisible && job.MaximumSalary >= filters.MinimumSalary.Value);
        if (filters.MaximumSalary.HasValue)
            query = query.Where(job => job.IsSalaryVisible && job.MinimumSalary <= filters.MaximumSalary.Value);
        if (!string.IsNullOrWhiteSpace(filters.Skill))
        {
            var skill = filters.Skill.Trim();
            query = query.Where(job => job.JobSkills.Any(jobSkill => jobSkill.Skill.Name.Contains(skill)));
        }
        if (filters.PostedWithinDays is > 0)
        {
            var postedAfter = now.AddDays(-filters.PostedWithinDays.Value);
            query = query.Where(job => job.PostedAt >= postedAfter);
        }

        return query;
    }

    public static IOrderedQueryable<Job> ApplySort(IQueryable<Job> query, JobSortOrder sort) => sort switch
    {
        JobSortOrder.Oldest => query.OrderBy(job => job.PostedAt).ThenBy(job => job.Id),
        JobSortOrder.SalaryLowToHigh => query.OrderBy(job => job.MinimumSalary == null)
            .ThenBy(job => job.MinimumSalary).ThenByDescending(job => job.PostedAt),
        JobSortOrder.SalaryHighToLow => query.OrderByDescending(job => job.MaximumSalary)
            .ThenByDescending(job => job.PostedAt),
        _ => query.OrderByDescending(job => job.PostedAt).ThenByDescending(job => job.Id)
    };
}
