namespace JobApplicationManagement.Application.Features.Jobs.DTOs;

public class CompanySummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? CompanySize { get; set; }
    public string? Headquarters { get; set; }
}
