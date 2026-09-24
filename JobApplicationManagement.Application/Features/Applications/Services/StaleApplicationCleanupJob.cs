using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobApplicationManagement.Application.Features.Applications.Services;

/// <summary>
/// Recurring background job implementation for auto-closing / rejecting stale job applications.
/// Applications in 'Applied' or 'UnderReview' status that have not been updated for 30+ days
/// are automatically transitioned to 'Rejected' status with an updated timestamp.
/// </summary>
public class StaleApplicationCleanupJob : IStaleApplicationCleanupJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StaleApplicationCleanupJob> _logger;
    private const int DefaultStaleThresholdDays = 30;

    public StaleApplicationCleanupJob(IUnitOfWork unitOfWork, ILogger<StaleApplicationCleanupJob> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task CloseStaleApplicationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting execution of StaleApplicationCleanupJob.");

        var cutoffDate = DateTime.UtcNow.AddDays(-DefaultStaleThresholdDays);

        var staleApplications = await _unitOfWork.JobApplications.Query()
            .Where(a => (a.JobApplicationStatus == JobApplicationStatus.Applied ||
                         a.JobApplicationStatus == JobApplicationStatus.UnderReview) &&
                        a.StatusUpdatedAt <= cutoffDate)
            .ToListAsync(cancellationToken);

        if (!staleApplications.Any())
        {
            _logger.LogInformation("No stale job applications found matching cutoff date ({CutoffDate}).", cutoffDate);
            return;
        }

        _logger.LogInformation("Found {Count} stale job applications to process.", staleApplications.Count);

        foreach (var application in staleApplications)
        {
            application.JobApplicationStatus = JobApplicationStatus.Rejected;
            application.StatusUpdatedAt = DateTime.UtcNow;
            _unitOfWork.JobApplications.Update(application);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully updated {Count} stale job applications to 'Rejected' status.", staleApplications.Count);
    }
}
