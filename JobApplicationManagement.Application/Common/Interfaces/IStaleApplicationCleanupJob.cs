namespace JobApplicationManagement.Application.Common.Interfaces;

/// <summary>
/// Service contract for the recurring background job that cleans up stale job applications.
/// </summary>
public interface IStaleApplicationCleanupJob
{
    /// <summary>
    /// Finds applications that have remained in Applied or UnderReview status 
    /// for longer than the specified threshold (default 30 days) and marks them as Rejected.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CloseStaleApplicationsAsync(CancellationToken cancellationToken = default);
}
