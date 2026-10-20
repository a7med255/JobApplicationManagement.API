using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Domain.Entities;

public class ApplicationStatusHistory
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public JobApplication Application { get; set; } = null!;
    public JobApplicationStatus? OldStatus { get; set; }
    public JobApplicationStatus NewStatus { get; set; }
    public string ChangedByUserId { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Note { get; set; }
}
