namespace JobApplicationManagement.Domain.Entities;

public class NotificationPreference
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool InAppEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public bool JobAlertsEnabled { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}
