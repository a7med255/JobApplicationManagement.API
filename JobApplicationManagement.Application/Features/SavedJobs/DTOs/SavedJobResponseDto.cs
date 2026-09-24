namespace JobApplicationManagement.Application.Features.SavedJobs.DTOs;

/// <summary>
/// Response DTO for a saved job. Includes key job details for the candidate's saved-jobs list.
/// </summary>
public class SavedJobResponseDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime SavedAt { get; set; }
}
