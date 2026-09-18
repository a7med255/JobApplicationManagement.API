namespace JobApplicationManagement.Application.Features.Jobs.DTOs;

/// <summary>
/// Response DTO returned after closing a job.
/// </summary>
public class CloseJobResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosedByUserId { get; set; }
}
