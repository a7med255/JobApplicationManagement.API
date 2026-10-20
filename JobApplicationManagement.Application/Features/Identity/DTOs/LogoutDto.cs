using System.ComponentModel.DataAnnotations;

namespace JobApplicationManagement.Application.Features.Identity.DTOs;

public sealed class LogoutDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
