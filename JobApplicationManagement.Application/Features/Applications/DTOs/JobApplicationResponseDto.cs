using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.Applications.DTOs;

public class JobApplicationResponseDto
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public int JobId { get; set; }
    public JobApplicationStatus JobApplicationStatus { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime StatusUpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
