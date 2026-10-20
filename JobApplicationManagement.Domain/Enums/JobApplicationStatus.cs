namespace JobApplicationManagement.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a job application.
/// </summary>
public enum JobApplicationStatus
{
    Applied = 0,
    UnderReview = 1,
    Cancelled = 2,
    Interview = 3,
    [Obsolete("Use Interview.")]
    InterView = Interview,
    Accepted = 4,
    Rejected = 5,
    Shortlisted = 6
}
