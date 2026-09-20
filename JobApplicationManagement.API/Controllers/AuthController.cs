using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Features.Identity.DTOs;
using JobApplicationManagement.Application.Features.Identity.Interfaces;

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
        _logger.LogInformation("User registration requested for {Email}", registerDto.Email);

        var result = await _authService.RegisterAsync(registerDto);
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
    /// <response code="400">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        _logger.LogInformation("User login attempt for {Email}", loginDto.Email);

        var result = await _authService.LoginAsync(loginDto);
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
        _logger.LogInformation("Refresh token requested");

        var result = await _authService.RefreshTokenAsync(refreshTokenDto);
        return Ok(result);
    }
}
