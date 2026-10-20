using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Domain.Entities;

public class JobAlert
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Keyword { get; set; }
    public string? Location { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
