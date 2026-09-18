using Microsoft.AspNetCore.Mvc;
using JobApplicationManagement.Application.Features.Identity.DTOs;
using JobApplicationManagement.Application.Features.Identity.Interfaces;

namespace JobApplicationManagement.API.Controllers;

/// <summary>
/// API controller for authentication operations.
/// </summary>
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
    /// Registers a new user account with the specified role.
    /// </summary>
    /// <response code="200">Registration successful — returns access and refresh tokens.</response>
    /// <response code="400">Validation failed or email already in use.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        _logger.LogInformation("POST /api/auth/register called for email {Email}", registerDto.Email);

        var result = await _authService.RegisterAsync(registerDto);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        _logger.LogInformation("POST /api/auth/login called for email {Email}", loginDto.Email);

        var result = await _authService.LoginAsync(loginDto);
        return Ok(result);
    }

    /// <summary>
    /// Issues a new access token and refresh token using a valid refresh token.
    /// </summary>
    /// <response code="200">New tokens issued.</response>
    /// <response code="400">Refresh token is invalid or expired.</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        _logger.LogInformation("POST /api/auth/refresh-token called");

        var result = await _authService.RefreshTokenAsync(refreshTokenDto);
        return Ok(result);
    }
}
