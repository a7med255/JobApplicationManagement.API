namespace JobApplicationManagement.Application.Features.CandidateCvs.DTOs;

public class CandidateCvResponseDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsDefault { get; set; }
    public DateTime UploadedAt { get; set; }
}
