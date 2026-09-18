namespace JobApplicationManagement.Application.Features.Identity.DTOs;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Must be one of: "Candidate", "Recruiter", "Admin"
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
