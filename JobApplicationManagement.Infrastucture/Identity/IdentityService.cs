using Microsoft.AspNetCore.Identity;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace JobApplicationManagement.Infrastructure.Identity;

/// <summary>
/// Identity service wrapping UserManager and SignInManager operations.
/// Implements the Application layer's IIdentityService interface.
/// </summary>
public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly byte[] _otpPepper;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _otpPepper = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is required."));
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

    public async Task DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
            await _userManager.DeleteAsync(user);
    }

    public async Task<IReadOnlyCollection<IdentityUserInfo>> GetUsersAsync(int skip, int take)
    {
        var users = await _userManager.Users.OrderBy(x => x.Email).Skip(skip).Take(take).ToListAsync();
        var result = new List<IdentityUserInfo>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new IdentityUserInfo(
                user.Id, user.Email ?? string.Empty, user.FullName, user.EmailConfirmed,
                await _userManager.IsLockedOutAsync(user), roles));
        }
        return result;
    }

    public async Task SetUserLockoutAsync(string userId, bool locked)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found.");
        await _userManager.SetLockoutEnabledAsync(user, true);
        var result = await _userManager.SetLockoutEndDateAsync(user, locked ? DateTimeOffset.MaxValue : null);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(x => x.Description)));
    }

    public async Task UpdateUserRolesAsync(string userId, IReadOnlyCollection<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found.");
        var current = await _userManager.GetRolesAsync(user);
        var remove = current.Except(roles, StringComparer.OrdinalIgnoreCase).ToArray();
        var add = roles.Except(current, StringComparer.OrdinalIgnoreCase).ToArray();
        if (remove.Length > 0)
        {
            var removed = await _userManager.RemoveFromRolesAsync(user, remove);
            if (!removed.Succeeded) throw new InvalidOperationException(string.Join(" ", removed.Errors.Select(x => x.Description)));
        }
        if (add.Length > 0)
        {
            var added = await _userManager.AddToRolesAsync(user, add);
            if (!added.Succeeded) throw new InvalidOperationException(string.Join(" ", added.Errors.Select(x => x.Description)));
        }
    }

    public async Task<CredentialsValidationResult> ValidateCredentialsAsync(
        string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new CredentialsValidationResult(
                CredentialsValidationStatus.InvalidCredentials,
                string.Empty,
                string.Empty,
                Enumerable.Empty<string>());
        }

        // Verify password first to prevent account enumeration / state leakage
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            if (_userManager.SupportsUserLockout && await _userManager.GetLockoutEnabledAsync(user))
            {
                await _userManager.AccessFailedAsync(user);
            }

            return new CredentialsValidationResult(
                CredentialsValidationStatus.InvalidCredentials,
                string.Empty,
                string.Empty,
                Enumerable.Empty<string>());
        }

        // Only after password succeeds: check account lockout
        if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
        {
            return new CredentialsValidationResult(
                CredentialsValidationStatus.LockedOut,
                user.Id,
                user.Email!,
                Enumerable.Empty<string>());
        }

        // Check email verification requirement
        if (_userManager.Options.SignIn.RequireConfirmedEmail && !await _userManager.IsEmailConfirmedAsync(user))
        {
            return new CredentialsValidationResult(
                CredentialsValidationStatus.EmailNotConfirmed,
                user.Id,
                user.Email!,
                Enumerable.Empty<string>());
        }

        if (_userManager.SupportsUserLockout)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new CredentialsValidationResult(
            CredentialsValidationStatus.Success,
            user.Id,
            user.Email!,
            roles);
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

        return await GenerateCodeAsync(user.Id, "EmailVerification");
    }

    public async Task<(bool Succeeded, string[] Errors)> VerifyEmailOtpAsync(string email, string otp)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return (false, new[] { "User not found" });

        var code = await ValidateCodeAsync(user.Id, "EmailVerification", otp);
        if (code is null) return (false, new[] { "The verification code is invalid or expired." });

        user.EmailConfirmed = true;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            code.ConsumedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<string?> GeneratePasswordResetOtpAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return null;

        return await GenerateCodeAsync(user.Id, "PasswordReset");
    }

    public async Task<(bool Succeeded, string[] Errors)> ResetPasswordWithOtpAsync(string email, string otp, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return (false, new[] { "User not found" });

        var code = await ValidateCodeAsync(user.Id, "PasswordReset", otp);
        if (code is null) return (false, new[] { "The reset code is invalid or expired." });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (result.Succeeded)
        {
            code.ConsumedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
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

    private async Task<string> GenerateCodeAsync(string userId, string purpose)
    {
        var now = DateTime.UtcNow;
        var activeCodes = await _dbContext.AccountOneTimeCodes
            .Where(x => x.UserId == userId && x.Purpose == purpose && x.ConsumedAt == null && x.ExpiresAt > now)
            .ToListAsync();
        foreach (var activeCode in activeCodes)
            activeCode.ConsumedAt = now;

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        await _dbContext.AccountOneTimeCodes.AddAsync(new AccountOneTimeCode
        {
            UserId = userId,
            Purpose = purpose,
            CodeHash = HashCode(userId, purpose, code),
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(10)
        });
        await _dbContext.SaveChangesAsync();
        return code;
    }

    private async Task<AccountOneTimeCode?> ValidateCodeAsync(string userId, string purpose, string code)
    {
        var now = DateTime.UtcNow;
        var entity = await _dbContext.AccountOneTimeCodes
            .Where(x => x.UserId == userId && x.Purpose == purpose && x.ConsumedAt == null &&
                        x.ExpiresAt > now && x.FailedAttempts < 5)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
        if (entity is null)
            return null;

        var suppliedHash = Convert.FromBase64String(HashCode(userId, purpose, code));
        var expectedHash = Convert.FromBase64String(entity.CodeHash);
        if (CryptographicOperations.FixedTimeEquals(suppliedHash, expectedHash))
            return entity;

        entity.FailedAttempts++;
        await _dbContext.SaveChangesAsync();
        return null;
    }

    private string HashCode(string userId, string purpose, string code)
    {
        using var hmac = new HMACSHA256(_otpPepper);
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{userId}:{purpose}:{code}")));
    }
}
