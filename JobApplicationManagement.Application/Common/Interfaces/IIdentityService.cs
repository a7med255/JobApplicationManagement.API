namespace JobApplicationManagement.Application.Common.Interfaces;

public enum CredentialsValidationStatus
{
    Success,
    InvalidCredentials,
    EmailNotConfirmed,
    LockedOut
}

public record CredentialsValidationResult(
    CredentialsValidationStatus Status,
    string UserId,
    string Email,
    IEnumerable<string> Roles);

public record IdentityUserInfo(
    string UserId,
    string Email,
    string FullName,
    bool EmailConfirmed,
    bool IsLocked,
    IEnumerable<string> Roles);

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

    Task DeleteUserAsync(string userId);
    Task<IReadOnlyCollection<IdentityUserInfo>> GetUsersAsync(int skip, int take);
    Task SetUserLockoutAsync(string userId, bool locked);
    Task UpdateUserRolesAsync(string userId, IReadOnlyCollection<string> roles);

    Task<CredentialsValidationResult> ValidateCredentialsAsync(
        string email, string password);

    Task<(string UserId, string Email, IEnumerable<string> Roles)?> GetUserByIdAsync(string userId);

    Task<(string UserId, string Email, IEnumerable<string> Roles)?> GetUserByEmailAsync(string email);

    Task<string?> GenerateEmailVerificationOtpAsync(string email);
    
    Task<(bool Succeeded, string[] Errors)> VerifyEmailOtpAsync(string email, string otp);

    Task<string?> GeneratePasswordResetOtpAsync(string email);

    Task<(bool Succeeded, string[] Errors)> ResetPasswordWithOtpAsync(string email, string otp, string newPassword);

    Task<(bool Succeeded, string UserId, string[] Errors)> CreateExternalUserAsync(string email, string fullName, string role);
}
