namespace JobApplicationManagement.Application.Features.Jobs.DTOs;

public class JobDetailsDto : JobSummaryDto
{
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string Responsibilities { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string? PreferredQualifications { get; set; }
    public string? EducationRequirements { get; set; }
    public string? Benefits { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public IReadOnlyCollection<string> PreferredSkills { get; set; } = Array.Empty<string>();
    public bool IsSaved { get; set; }
    public bool HasApplied { get; set; }
    public int? ApplicationId { get; set; }
}
