using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Domain.Entities;

public class Interview
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public JobApplication Application { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public InterviewType InterviewType { get; set; }
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public InterviewStatus Status { get; set; }
    public string ScheduledByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
