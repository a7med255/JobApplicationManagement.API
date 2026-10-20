using JobApplicationManagement.Application.Common.Models;

namespace JobApplicationManagement.Application.Common.Interfaces;

public interface IEmailQueue
{
    Task QueueAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public interface IOutboxProcessor
{
    Task ProcessPendingAsync(CancellationToken cancellationToken = default);
}
