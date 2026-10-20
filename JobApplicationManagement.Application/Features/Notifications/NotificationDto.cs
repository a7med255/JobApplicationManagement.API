using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.Notifications;

public sealed class NotificationDto
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public sealed record NotificationPreferenceDto(bool InAppEnabled, bool EmailEnabled, bool JobAlertsEnabled);
