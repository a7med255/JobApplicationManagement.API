using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobApplicationManagement.Application.Features.Jobs.Services;

public class JobExpirationService : IJobExpirationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JobExpirationService> _logger;

    public JobExpirationService(IUnitOfWork unitOfWork, ILogger<JobExpirationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExpireJobsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expiredJobs = await _unitOfWork.Jobs.Query()
            .Where(job => job.Status == JobStatus.Published &&
                          job.ApplicationDeadline.HasValue &&
                          job.ApplicationDeadline <= now)
            .ToListAsync(cancellationToken);

        foreach (var job in expiredJobs)
            JobLifecycle.Expire(job, now);

        if (expiredJobs.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Expired {JobCount} job postings", expiredJobs.Count);
        }
    }
}
