using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Features.Identity.DTOs;
using JobApplicationManagement.Application.Features.Identity.Interfaces;
using Microsoft.AspNetCore.RateLimiting;

namespace JobApplicationManagement.API.Controllers;

/// <summary>
/// Provides endpoints for authentication and user registration.
/// </summary>
/// <remarks>
/// Allows any anonymous user to register for a new account, login to receive JWT tokens,
/// and refresh expired access tokens.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <remarks>
    /// Automatically creates a linked profile (Candidate or Recruiter) based on the specified role.
    /// Authentication is not required.
    /// </remarks>
    /// <param name="registerDto">Contains the user email, password, full name, and role.</param>
    /// <returns>Authentication response containing access and refresh tokens.</returns>
    /// <response code="200">Registration successful.</response>
    /// <response code="400">Validation failed or email already in use.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var ip = GetClientIpAddress();
        _logger.LogInformation("User registration requested for {Email} from IP {IpAddress}", MaskEmail(registerDto.Email), ip);

        var result = await _authService.RegisterAsync(registerDto, ip);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    /// <remarks>
    /// Authentication is not required.
    /// </remarks>
    /// <param name="loginDto">Contains the user email and password.</param>
    /// <returns>Authentication response containing access and refresh tokens.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Invalid credentials or unverified account.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var ip = GetClientIpAddress();
        _logger.LogInformation("User login attempt for {Email} from IP {IpAddress} (UserAgent: {UserAgent})",
            MaskEmail(loginDto.Email), ip, GetUserAgent());

        var result = await _authService.LoginAsync(loginDto, ip);
        return Ok(result);
    }

    /// <summary>
    /// Issues a new access token using a valid refresh token.
    /// </summary>
    /// <remarks>
    /// Authentication is not required.
    /// </remarks>
    /// <param name="refreshTokenDto">Contains the expired access token and the active refresh token.</param>
    /// <returns>Authentication response containing new access and refresh tokens.</returns>
    /// <response code="200">New tokens issued successfully.</response>
    /// <response code="400">Refresh token is invalid or expired.</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var ip = GetClientIpAddress();
        _logger.LogInformation("Refresh token requested from IP {IpAddress}", ip);

        var result = await _authService.RefreshTokenAsync(refreshTokenDto, ip);
        return Ok(result);
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        var ip = GetClientIpAddress();
        _logger.LogInformation("Email verification attempt for {Email} from IP {IpAddress}", MaskEmail(dto.Email), ip);
        await _authService.VerifyEmailAsync(dto.Email, dto.Otp);
        return Ok(new { message = "Email verified successfully." });
    }

    [HttpPost("resend-verification")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerification([FromBody] ForgotPasswordDto dto)
    {
        var ip = GetClientIpAddress();
        _logger.LogInformation("Resend verification requested for {Email} from IP {IpAddress}", MaskEmail(dto.Email), ip);
        await _authService.RequestEmailVerificationOtpAsync(dto.Email);
        return Ok(new { message = "If the account exists, a verification email will be sent." });
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var ip = GetClientIpAddress();
        _logger.LogInformation("Forgot password requested for {Email} from IP {IpAddress}", MaskEmail(dto.Email), ip);
        await _authService.ForgotPasswordAsync(dto.Email);
        return Ok(new { message = "If the account exists, password reset instructions will be sent." });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var ip = GetClientIpAddress();
        _logger.LogInformation("Password reset attempt for {Email} from IP {IpAddress}", MaskEmail(dto.Email), ip);
        await _authService.ResetPasswordAsync(dto.Email, dto.Otp, dto.NewPassword);
        return Ok(new { message = "Password reset successfully." });
    }

    [HttpPost("external-login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto dto)
    {
        var ip = GetClientIpAddress();
        _logger.LogInformation("External login attempt for provider {Provider} from IP {IpAddress}", dto.Provider, ip);
        var result = await _authService.ExternalLoginAsync(dto.Provider, dto.IdToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
    {
        var ip = GetClientIpAddress();
        _logger.LogInformation("Logout request from IP {IpAddress}", ip);
        await _authService.LogoutAsync(dto.RefreshToken, ip);
        return NoContent();
    }

    private string GetClientIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
        {
            var firstIp = forwardedFor.ToString().Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(firstIp)) return firstIp;
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string GetUserAgent()
    {
        return Request.Headers.UserAgent.ToString() ?? "unknown";
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return "***";
        var parts = email.Split('@');
        var name = parts[0];
        var maskedName = name.Length <= 2 ? name[0] + "*" : $"{name[0]}***{name[^1]}";
        return $"{maskedName}@{parts[1]}";
    }
}
