using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.Jobs.DTOs;

public class JobSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public CompanySummaryDto Company { get; set; } = new();
    public string Department { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public WorkplaceType WorkplaceType { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public int MinimumYearsExperience { get; set; }
    public int? MaximumYearsExperience { get; set; }
    public decimal? MinimumSalary { get; set; }
    public decimal? MaximumSalary { get; set; }
    public string? Currency { get; set; }
    public SalaryPeriod? SalaryPeriod { get; set; }
    public bool IsSalaryVisible { get; set; }
    public int NumberOfVacancies { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? ApplicationDeadline { get; set; }
    public JobStatus Status { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyCollection<string> RequiredSkills { get; set; } = Array.Empty<string>();
}
