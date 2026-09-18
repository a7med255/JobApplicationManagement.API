namespace JobApplicationManagement.Domain.Entities;

/// <summary>
/// Represents a job posting in the system.
/// </summary>
public class Job
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this job posting is currently active.
    /// Defaults to true — a job is published immediately upon creation.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Foreign key to the Recruiter who created and owns this job.
    /// </summary>
    public int RecruiterId { get; set; }
    
    public Recruiter Recruiter { get; set; } = null!;

    /// <summary>
    /// UTC timestamp when this job was closed. Null if still open.
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Foreign key to the Recruiter who closed this job. Null if still open.
    /// </summary>
    public int? ClosedById { get; set; }

    public Recruiter? ClosedBy { get; set; }
}
