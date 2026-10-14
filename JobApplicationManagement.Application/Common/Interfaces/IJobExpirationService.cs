namespace JobApplicationManagement.Application.Common.Interfaces;

public interface IJobExpirationService
{
    Task ExpireJobsAsync(CancellationToken cancellationToken);
}
