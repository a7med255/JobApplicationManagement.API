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

    public string? ProfileImageUrl { get; set; }
    public string? ProfileImagePublicId { get; set; }
    public string? ProfessionalTitle { get; set; }
    public string? Summary { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public int? YearsOfExperience { get; set; }

    public ICollection<CandidateCv> Cvs { get; set; } = new List<CandidateCv>();
    public ICollection<JobAlert> JobAlerts { get; set; } = new List<JobAlert>();
}
