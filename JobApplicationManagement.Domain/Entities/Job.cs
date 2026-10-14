using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Domain.Entities;

public class Job
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int RecruiterId { get; set; }
    public Recruiter Recruiter { get; set; } = null!;
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

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

    public int NumberOfVacancies { get; set; }
    public DateTime? ApplicationDeadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PostedAt { get; set; }
    public JobStatus Status { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int? ClosedById { get; set; }
    public Recruiter? ClosedBy { get; set; }

    public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();

    public bool IsActive
    {
        get => Status == JobStatus.Published &&
               (!ApplicationDeadline.HasValue || ApplicationDeadline.Value > DateTime.UtcNow);
        set
        {
            Status = value ? JobStatus.Published : JobStatus.Closed;
            if (value)
            {
                ClosedAt = null;
                ClosedById = null;
            }
        }
    }
}
