namespace JobApplicationManagement.Application.Features.Companies.DTOs;

public class UpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? CompanySize { get; set; }
    public string? Headquarters { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}
