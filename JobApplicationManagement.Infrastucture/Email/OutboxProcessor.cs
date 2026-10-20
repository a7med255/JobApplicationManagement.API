using System.Text.Json;
using Hangfire;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobApplicationManagement.Infrastructure.Email;

public sealed class OutboxProcessor : IOutboxProcessor
{
    private const int MaximumAttempts = 5;
    private readonly ApplicationDbContext _dbContext;
    private readonly IDataProtector _protector;
    private readonly IEmailSender _sender;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        ApplicationDbContext dbContext,
        IDataProtectionProvider protectionProvider,
        IEmailSender sender,
        ILogger<OutboxProcessor> logger)
    {
        _dbContext = dbContext;
        _protector = protectionProvider.CreateProtector("Careerly.EmailOutbox.v1");
        _sender = sender;
        _logger = logger;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var messages = await _dbContext.OutboxMessages
            .Where(x => x.Type == ProtectedEmailQueue.MessageType && x.ProcessedAt == null &&
                        x.AttemptCount < MaximumAttempts && (x.NextAttemptAt == null || x.NextAttemptAt <= now))
            .OrderBy(x => x.OccurredAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var item in messages)
        {
            try
            {
                var json = _protector.Unprotect(item.ProtectedPayload);
                var email = JsonSerializer.Deserialize<EmailMessage>(json)
                    ?? throw new InvalidOperationException("The email outbox payload is invalid.");
                await _sender.SendAsync(email, cancellationToken);
                item.ProcessedAt = DateTime.UtcNow;
                item.ProtectedPayload = string.Empty;
                item.LastError = null;
                _logger.LogInformation("Email outbox message {OutboxId} delivered", item.Id);
            }
            catch (Exception ex)
            {
                item.AttemptCount++;
                item.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, item.AttemptCount));
                item.LastError = ex.GetType().Name + ": " + ex.Message[..Math.Min(ex.Message.Length, 1800)];
                _logger.LogError(ex, "Email outbox message {OutboxId} failed on attempt {Attempt}", item.Id, item.AttemptCount);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
