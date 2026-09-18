using Microsoft.AspNetCore.Identity;

namespace JobApplicationManagement.Infrastructure.Identity;

/// <summary>
/// Application user extending IdentityUser.
/// Stores the user's full name in addition to default Identity properties.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
