namespace JobApplicationManagement.Domain.Entities;

/// <summary>
/// Represents a job that a candidate has saved/bookmarked for later review.
/// This is independent of a JobApplication — saving a job does not create an application.
/// </summary>
public class SavedJob
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public Candidate Candidate { get; set; } = null!;

    public int JobId { get; set; }

    public Job Job { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
