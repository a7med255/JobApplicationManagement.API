using System.Text.Json;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Infrastructure.Email;

public sealed class ProtectedEmailQueue : IEmailQueue
{
    internal const string MessageType = "email.v1";
    private readonly ApplicationDbContext _dbContext;
    private readonly IDataProtector _protector;

    public ProtectedEmailQueue(ApplicationDbContext dbContext, IDataProtectionProvider protectionProvider)
    {
        _dbContext = dbContext;
        _protector = protectionProvider.CreateProtector("Careerly.EmailOutbox.v1");
    }

    public async Task QueueAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(message.RecipientUserId))
        {
            var preferences = await _dbContext.NotificationPreferences.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == message.RecipientUserId, cancellationToken);
            if (preferences is { EmailEnabled: false }) return;
        }
        var payload = JsonSerializer.Serialize(message);
        await _dbContext.OutboxMessages.AddAsync(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = MessageType,
            ProtectedPayload = _protector.Protect(payload),
            OccurredAt = DateTime.UtcNow
        }, cancellationToken);
    }
}
