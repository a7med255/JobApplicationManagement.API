namespace JobApplicationManagement.Domain.Entities;

public class AdminAuditLog
{
    public long Id { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
