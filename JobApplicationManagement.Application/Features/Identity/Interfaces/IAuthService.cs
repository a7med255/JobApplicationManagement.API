using JobApplicationManagement.Application.Features.Identity.DTOs;

namespace JobApplicationManagement.Application.Features.Identity.Interfaces;

/// <summary>
/// Defines authentication operations.
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string? ipAddress = null);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string? ipAddress = null);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, string? ipAddress = null);
    
    Task RequestEmailVerificationOtpAsync(string email);
    Task<bool> VerifyEmailAsync(string email, string otp);
    
    Task ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);

    Task<AuthResponseDto> ExternalLoginAsync(string provider, string idToken);
    Task LogoutAsync(string refreshToken, string? ipAddress = null);
}
