using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.Jobs.DTOs;

public enum JobSortOrder
{
    Newest,
    Oldest,
    SalaryLowToHigh,
    SalaryHighToLow
}

public class JobSearchRequest
{
    public string? Search { get; set; }
    public int? CompanyId { get; set; }
    public string? Department { get; set; }
    public string? Category { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public WorkplaceType? WorkplaceType { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
    public decimal? MinimumSalary { get; set; }
    public decimal? MaximumSalary { get; set; }
    public string? Skill { get; set; }
    public int? PostedWithinDays { get; set; }
    public JobStatus? Status { get; set; }
    public bool IncludeInactive { get; set; }
    public JobSortOrder Sort { get; set; } = JobSortOrder.Newest;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
