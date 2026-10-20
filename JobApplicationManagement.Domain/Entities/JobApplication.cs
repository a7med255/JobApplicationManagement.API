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

    /// <summary>
    /// Exact immutable CV version submitted with this application.
    /// Null is retained only for applications created before CV versioning existed.
    /// </summary>
    public int? SubmittedCvId { get; set; }

    public CandidateCv? SubmittedCv { get; set; }

    public JobApplicationStatus JobApplicationStatus { get; set; }

    public DateTime AppliedAt { get; set; }

    public DateTime StatusUpdatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when this application was cancelled. Null if not cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    public string? CoverLetter { get; set; }

    public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = new List<ApplicationStatusHistory>();

    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
}
