using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.Applications.DTOs;

public class JobApplicationResponseDto
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public string? CandidateName { get; set; }
    public int JobId { get; set; }
    public string? JobTitle { get; set; }
    public int? SubmittedCvId { get; set; }
    public string? SubmittedCvDisplayName { get; set; }
    public string? SubmittedCvFileName { get; set; }
    public JobApplicationStatus JobApplicationStatus { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime StatusUpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
