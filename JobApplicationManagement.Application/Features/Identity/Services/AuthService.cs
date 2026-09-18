using FluentValidation;
using Microsoft.Extensions.Logging;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Identity.DTOs;
using JobApplicationManagement.Application.Features.Identity.Interfaces;
using JobApplicationManagement.Domain.Entities;
using AppValidationException = JobApplicationManagement.Application.Common.Exceptions.ValidationException;

namespace JobApplicationManagement.Application.Features.Identity.Services;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly ILogger<AuthService> _logger;

    // Configuration values — injected via IConfiguration in the future or hardcoded for now
    private const int AccessTokenExpirationMinutes = 60;
    private const int RefreshTokenExpirationDays = 7;

    public AuthService(
        IIdentityService identityService,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        IUnitOfWork unitOfWork,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        ILogger<AuthService> logger)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        _logger.LogInformation("Registering user with email {Email}", registerDto.Email);

        await ValidateAsync(_registerValidator, registerDto);

        // Block Admin self-registration from the public endpoint
        if (string.Equals(registerDto.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Blocked attempt to self-register as Admin for email {Email}", registerDto.Email);
            throw new ForbiddenException("Admin accounts cannot be created through public registration.");
        }

        var (succeeded, userId, errors) = await _identityService.CreateUserAsync(
            registerDto.Email, registerDto.Password, registerDto.FullName);

        if (!succeeded)
        {
            var errorDict = new Dictionary<string, string[]>
            {
                { "Identity", errors }
            };
            throw new AppValidationException(errorDict);
        }

        await _identityService.AddToRoleAsync(userId, registerDto.Role);

        // Auto-create Candidate or Recruiter profile
        if (string.Equals(registerDto.Role, "Candidate", StringComparison.OrdinalIgnoreCase))
        {
            var candidate = new Candidate
            {
                UserId = userId,
                Name = registerDto.FullName
            };
            await _unitOfWork.Candidates.AddAsync(candidate);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Created Candidate profile for user {UserId}", userId);
        }
        else if (string.Equals(registerDto.Role, "Recruiter", StringComparison.OrdinalIgnoreCase))
        {
            var recruiter = new Recruiter
            {
                UserId = userId,
                Name = registerDto.FullName
            };
            await _unitOfWork.Recruiters.AddAsync(recruiter);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Created Recruiter profile for user {UserId}", userId);
        }

        _logger.LogInformation("User {UserId} registered successfully with role {Role}", userId, registerDto.Role);

        var roles = new[] { registerDto.Role };
        return GenerateAuthResponse(userId, registerDto.Email, roles);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation("Login attempt for email {Email}", loginDto.Email);

        await ValidateAsync(_loginValidator, loginDto);

        var (succeeded, userId, email, roles) = await _identityService.ValidateCredentialsAsync(
            loginDto.Email, loginDto.Password);

        if (!succeeded)
        {
            _logger.LogWarning("Login failed for email {Email}", loginDto.Email);
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                { "Credentials", new[] { "Invalid email or password." } }
            });
        }

        _logger.LogInformation("User {UserId} logged in successfully", userId);

        return GenerateAuthResponse(userId, email, roles);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        _logger.LogInformation("Refresh token request received");

        var userId = await _refreshTokenService.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken);

        if (userId is null)
        {
            _logger.LogWarning("Invalid or expired refresh token");
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                { "RefreshToken", new[] { "Invalid or expired refresh token." } }
            });
        }

        // Revoke the used refresh token (rotation)
        await _refreshTokenService.RevokeRefreshTokenAsync(refreshTokenDto.RefreshToken);

        var userInfo = await _identityService.GetUserByIdAsync(userId);
        if (userInfo is null)
        {
            throw new Common.Exceptions.NotFoundException("User", userId);
        }

        var (_, email, roles) = userInfo.Value;

        _logger.LogInformation("Refresh token used successfully for user {UserId}", userId);

        return GenerateAuthResponse(userId, email, roles);
    }

    private AuthResponseDto GenerateAuthResponse(string userId, string email, IEnumerable<string> roles)
    {
        var rolesList = roles.ToList();
        var accessToken = _jwtService.GenerateAccessToken(userId, email, rolesList);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiration = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);

        // Store refresh token (fire and forget within the same scope)
        _refreshTokenService.StoreRefreshTokenAsync(
            userId, refreshToken, DateTime.UtcNow.AddDays(RefreshTokenExpirationDays)).GetAwaiter().GetResult();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = expiration,
            Email = email,
            Roles = rolesList
        };
    }

    private async Task ValidateAsync<T>(IValidator<T> validator, T dto)
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            _logger.LogWarning("Validation failed. Errors: {Errors}", errors);
            throw new AppValidationException(errors);
        }
    }
}
