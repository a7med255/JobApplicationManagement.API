namespace JobApplicationManagement.Application.Features.Candidates.DTOs;

public class CandidateResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CvUrl { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
