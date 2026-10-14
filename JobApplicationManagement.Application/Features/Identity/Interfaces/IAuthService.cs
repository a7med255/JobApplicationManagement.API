using JobApplicationManagement.Application.Features.Identity.DTOs;

namespace JobApplicationManagement.Application.Features.Identity.Interfaces;

/// <summary>
/// Defines authentication operations.
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    
    Task<string> RequestEmailVerificationOtpAsync(string email);
    Task<bool> VerifyEmailAsync(string email, string otp);
    
    Task<string> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);

    Task<AuthResponseDto> ExternalLoginAsync(string provider, string idToken);
}
