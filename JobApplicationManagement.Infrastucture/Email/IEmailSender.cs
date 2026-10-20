using JobApplicationManagement.Application.Common.Models;

namespace JobApplicationManagement.Infrastructure.Email;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
