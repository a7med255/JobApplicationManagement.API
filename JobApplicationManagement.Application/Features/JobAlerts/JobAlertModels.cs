using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.JobAlerts;

public sealed class JobAlertDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Keyword { get; set; }
    public string? Location { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class SaveJobAlertDto
{
    public string Name { get; set; } = string.Empty;
    public string? Keyword { get; set; }
    public string? Location { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public bool IsActive { get; set; } = true;
}
