using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Domain.Entities;

/// <summary>
/// Represents a candidate's application to a specific job.
/// Navigation properties are defined here; FK configuration is handled via Fluent API in Infrastructure.
/// </summary>
public class JobApplication
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public Candidate Candidate { get; set; } = null!;

    public int JobId { get; set; }

    public Job Job { get; set; } = null!;

    public JobApplicationStatus JobApplicationStatus { get; set; }

    public DateTime AppliedAt { get; set; }

    public DateTime StatusUpdatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when this application was cancelled. Null if not cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }
}
