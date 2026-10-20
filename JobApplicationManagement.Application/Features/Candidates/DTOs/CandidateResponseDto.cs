namespace JobApplicationManagement.Application.Features.Candidates.DTOs;

public class CandidateResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CvUrl { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? ProfessionalTitle { get; set; }
    public string? Summary { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public int? YearsOfExperience { get; set; }
    public int ProfileCompleteness { get; set; }
}
