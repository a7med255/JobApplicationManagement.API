using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.Jobs.DTOs;

public class UpdateJobDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Location { get; set; }
    public WorkplaceType WorkplaceType { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public int MinimumYearsExperience { get; set; }
    public int? MaximumYearsExperience { get; set; }
    public string Responsibilities { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string? PreferredQualifications { get; set; }
    public string? EducationRequirements { get; set; }
    public string? Benefits { get; set; }
    public decimal? MinimumSalary { get; set; }
    public decimal? MaximumSalary { get; set; }
    public string? Currency { get; set; }
    public SalaryPeriod? SalaryPeriod { get; set; }
    public bool IsSalaryVisible { get; set; }
    public int NumberOfVacancies { get; set; } = 1;
    public DateTime? ApplicationDeadline { get; set; }
    public IReadOnlyCollection<string> RequiredSkills { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> PreferredSkills { get; set; } = Array.Empty<string>();
}
