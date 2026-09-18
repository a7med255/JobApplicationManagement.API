namespace JobApplicationManagement.Domain.Entities;

/// <summary>
/// Represents a job candidate.
/// </summary>
public class Candidate
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CvUrl { get; set; } = string.Empty;

    /// <summary>
    /// Identity UserId linking this candidate to their authentication account.
    /// Used for ownership verification when cancelling applications.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
}
