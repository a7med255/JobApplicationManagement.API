namespace JobApplicationManagement.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ProtectedPayload { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}
