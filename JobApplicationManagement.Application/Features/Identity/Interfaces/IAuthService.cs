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
}
