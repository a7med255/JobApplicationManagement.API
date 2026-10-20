using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.Applications.DTOs;

public class JobApplicationResponseDto
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public string? CandidateName { get; set; }
    public int JobId { get; set; }
    public string? JobTitle { get; set; }
    public string? CompanyName { get; set; }
    public int? SubmittedCvId { get; set; }
    public string? SubmittedCvDisplayName { get; set; }
    public string? SubmittedCvFileName { get; set; }
    public JobApplicationStatus JobApplicationStatus { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime StatusUpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CoverLetter { get; set; }
    public IReadOnlyCollection<ApplicationStatusHistoryDto> StatusHistory { get; set; } = [];
    public IReadOnlyCollection<InterviewSummaryDto> Interviews { get; set; } = [];
}

public sealed class ApplicationStatusHistoryDto
{
    public int Id { get; set; }
    public JobApplicationStatus? OldStatus { get; set; }
    public JobApplicationStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Note { get; set; }
}

public sealed class InterviewSummaryDto
{
    public int Id { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string InterviewType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}
