using Microsoft.AspNetCore.Identity;
using JobApplicationManagement.Application.Common.Interfaces;

namespace JobApplicationManagement.Infrastructure.Identity;

/// <summary>
/// Identity service wrapping UserManager and SignInManager operations.
/// Implements the Application layer's IIdentityService interface.
/// </summary>
public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<(bool Succeeded, string UserId, string[] Errors)> CreateUserAsync(
        string email, string password, string fullName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName
        };

        var result = await _userManager.CreateAsync(user, password);

        return (
            result.Succeeded,
            user.Id,
            result.Errors.Select(e => e.Description).ToArray()
        );
    }

    public async Task<bool> AddToRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<(bool Succeeded, string UserId, string Email, IEnumerable<string> Roles)> ValidateCredentialsAsync(
        string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return (false, string.Empty, string.Empty, Enumerable.Empty<string>());

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return (false, string.Empty, string.Empty, Enumerable.Empty<string>());

        var roles = await _userManager.GetRolesAsync(user);

        return (true, user.Id, user.Email!, roles);
    }

    public async Task<(string UserId, string Email, IEnumerable<string> Roles)?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return (user.Id, user.Email!, roles);
    }

    public async Task<(string UserId, string Email, IEnumerable<string> Roles)?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return (user.Id, user.Email!, roles);
    }

    public async Task<string?> GenerateEmailVerificationOtpAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return null;

        return await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
    }

    public async Task<(bool Succeeded, string[] Errors)> VerifyEmailOtpAsync(string email, string otp)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return (false, new[] { "User not found" });

        var valid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", otp);
        if (!valid) return (false, new[] { "Invalid OTP" });

        user.EmailConfirmed = true;
        var result = await _userManager.UpdateAsync(user);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<string?> GeneratePasswordResetOtpAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return null;

        return await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
    }

    public async Task<(bool Succeeded, string[] Errors)> ResetPasswordWithOtpAsync(string email, string otp, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return (false, new[] { "User not found" });

        var valid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", otp);
        if (!valid) return (false, new[] { "Invalid OTP" });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string UserId, string[] Errors)> CreateExternalUserAsync(string email, string fullName, string role)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true // External logins are implicitly verified
        };

        var result = await _userManager.CreateAsync(user);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        return (
            result.Succeeded,
            user.Id,
            result.Errors.Select(e => e.Description).ToArray()
        );
    }
}
