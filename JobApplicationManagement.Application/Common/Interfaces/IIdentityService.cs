namespace JobApplicationManagement.Application.Common.Interfaces;

/// <summary>
/// Abstraction for Identity user management operations.
/// Lives in Application so the service layer does not depend on
/// Microsoft.AspNetCore.Identity directly.
/// </summary>
public interface IIdentityService
{
    Task<(bool Succeeded, string UserId, string[] Errors)> CreateUserAsync(
        string email, string password, string fullName);

    Task<bool> AddToRoleAsync(string userId, string role);

    Task<(bool Succeeded, string UserId, string Email, IEnumerable<string> Roles)> ValidateCredentialsAsync(
        string email, string password);

    Task<(string UserId, string Email, IEnumerable<string> Roles)?> GetUserByIdAsync(string userId);
}
