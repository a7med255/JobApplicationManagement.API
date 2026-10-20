namespace JobApplicationManagement.Domain.Entities;

public class AccountOneTimeCode
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public int FailedAttempts { get; set; }
}
