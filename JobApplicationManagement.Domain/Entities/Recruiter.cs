namespace JobApplicationManagement.Domain.Entities;

/// <summary>
/// Represents a recruiter who manages jobs in the system.
/// </summary>
public class Recruiter
{
    public int Id { get; set; }

    /// <summary>
    /// Identity UserId linking this recruiter to their authentication account.
    /// Used for ownership verification.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
