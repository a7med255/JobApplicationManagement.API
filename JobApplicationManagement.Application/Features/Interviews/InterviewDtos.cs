using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.Interviews;

public sealed class ScheduleInterviewDto
{
    public int ApplicationId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public InterviewType InterviewType { get; set; }
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public sealed class RescheduleInterviewDto
{
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public InterviewType InterviewType { get; set; }
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public sealed class InterviewDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public InterviewType InterviewType { get; set; }
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public InterviewStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
