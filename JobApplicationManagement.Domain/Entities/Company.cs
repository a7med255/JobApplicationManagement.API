namespace JobApplicationManagement.Domain.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? LogoPublicId { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? CompanySize { get; set; }
    public string? Headquarters { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Recruiter> Recruiters { get; set; } = new List<Recruiter>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
