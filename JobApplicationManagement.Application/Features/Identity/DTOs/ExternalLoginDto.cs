namespace JobApplicationManagement.Application.Features.Identity.DTOs;

public class ExternalLoginDto
{
    public string Provider { get; set; } = string.Empty;
    public string IdToken { get; set; } = string.Empty;
}
