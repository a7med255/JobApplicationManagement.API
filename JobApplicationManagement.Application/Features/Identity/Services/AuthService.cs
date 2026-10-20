using FluentValidation;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Identity.DTOs;
using JobApplicationManagement.Application.Features.Identity.Interfaces;
using JobApplicationManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AppValidationException = JobApplicationManagement.Application.Common.Exceptions.ValidationException;

namespace JobApplicationManagement.Application.Features.Identity.Services;

public sealed class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly ILogger<AuthService> _logger;
    private readonly IEmailQueue _emailQueue;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public AuthService(
        IIdentityService identityService,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        IUnitOfWork unitOfWork,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        ILogger<AuthService> logger,
        IEmailQueue emailQueue,
        IConfiguration configuration)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _logger = logger;
        _emailQueue = emailQueue;
        _accessTokenExpirationMinutes = int.TryParse(configuration["Jwt:AccessTokenExpirationMinutes"], out var accessMinutes)
            ? accessMinutes : 15;
        _refreshTokenExpirationDays = int.TryParse(configuration["Jwt:RefreshTokenExpirationDays"], out var refreshDays)
            ? refreshDays : 7;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string? ipAddress = null)
    {
        await ValidateAsync(_registerValidator, registerDto);
        if (string.Equals(registerDto.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("Admin accounts cannot be created through public registration.");

        var (succeeded, userId, errors) = await _identityService.CreateUserAsync(
            registerDto.Email, registerDto.Password, registerDto.FullName);
        if (!succeeded)
            throw new AppValidationException(new Dictionary<string, string[]> { ["Identity"] = errors });

        try
        {
            if (!await _identityService.AddToRoleAsync(userId, registerDto.Role))
                throw new InvalidOperationException("The account role could not be assigned.");

            var otp = await _identityService.GenerateEmailVerificationOtpAsync(registerDto.Email)
                ?? throw new InvalidOperationException("The verification code could not be generated.");

            if (string.Equals(registerDto.Role, "Candidate", StringComparison.OrdinalIgnoreCase))
                await _unitOfWork.Candidates.AddAsync(new Candidate { UserId = userId, Name = registerDto.FullName });
            else if (string.Equals(registerDto.Role, "Recruiter", StringComparison.OrdinalIgnoreCase))
                await _unitOfWork.Recruiters.AddAsync(new Recruiter { UserId = userId, Name = registerDto.FullName });

            await _emailQueue.QueueAsync(new EmailMessage(
                registerDto.Email, registerDto.FullName, EmailTemplate.Welcome,
                new Dictionary<string, string>()));
            await _emailQueue.QueueAsync(new EmailMessage(
                registerDto.Email, registerDto.FullName, EmailTemplate.VerifyEmail,
                new Dictionary<string, string> { ["Otp"] = otp }));
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            await _identityService.DeleteUserAsync(userId);
            throw;
        }

        _logger.LogInformation("User {UserId} registered with role {Role} from IP {IpAddress}", userId, registerDto.Role, ipAddress ?? "unknown");
        return new AuthResponseDto { Email = registerDto.Email, Roles = [registerDto.Role] };
    }

    public async Task RequestEmailVerificationOtpAsync(string email)
    {
        var otp = await _identityService.GenerateEmailVerificationOtpAsync(email);
        if (otp is null) return;
        await _emailQueue.QueueAsync(new EmailMessage(
            email, "Careerly member", EmailTemplate.ResendEmailVerification,
            new Dictionary<string, string> { ["Otp"] = otp }));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> VerifyEmailAsync(string email, string otp)
    {
        var (succeeded, errors) = await _identityService.VerifyEmailOtpAsync(email, otp);
        if (!succeeded)
            throw new AppValidationException(new Dictionary<string, string[]> { ["Verification"] = errors });
        return true;
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var otp = await _identityService.GeneratePasswordResetOtpAsync(email);
        if (otp is null) return;
        await _emailQueue.QueueAsync(new EmailMessage(
            email, "Careerly member", EmailTemplate.ForgotPasswordOtp,
            new Dictionary<string, string> { ["Otp"] = otp }));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
    {
        var (succeeded, errors) = await _identityService.ResetPasswordWithOtpAsync(email, otp, newPassword);
        if (!succeeded)
            throw new AppValidationException(new Dictionary<string, string[]> { ["ResetPassword"] = errors });

        await _emailQueue.QueueAsync(new EmailMessage(
            email, "Careerly member", EmailTemplate.PasswordChanged,
            new Dictionary<string, string>()));
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public Task<AuthResponseDto> ExternalLoginAsync(string provider, string idToken) =>
        throw new ForbiddenException(
            $"External login provider '{provider}' is not configured. The unsafe development implementation has been disabled.");

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string? ipAddress = null)
    {
        await ValidateAsync(_loginValidator, loginDto);
        var result = await _identityService.ValidateCredentialsAsync(loginDto.Email, loginDto.Password);
        switch (result.Status)
        {
            case CredentialsValidationStatus.InvalidCredentials:
                throw new UnauthorizedException("Invalid email or password.");
            case CredentialsValidationStatus.EmailNotConfirmed:
                throw new UnauthorizedException("Email is not confirmed. Please verify your email before logging in.");
            case CredentialsValidationStatus.LockedOut:
                throw new UnauthorizedException("Account is temporarily locked out. Please try again later.");
            case CredentialsValidationStatus.Success:
                break;
            default:
                throw new UnauthorizedException("Invalid email or password.");
        }

        _logger.LogInformation("User {UserId} logged in from IP {IpAddress}", result.UserId, ipAddress ?? "unknown");
        return await GenerateAuthResponseAsync(result.UserId, result.Email, result.Roles, ipAddress: ipAddress);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, string? ipAddress = null)
    {
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var userId = await _refreshTokenService.RotateRefreshTokenAsync(
            refreshTokenDto.RefreshToken,
            newRefreshToken,
            DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            ipAddress);
        if (userId is null)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var userInfo = await _identityService.GetUserByIdAsync(userId)
            ?? throw new UnauthorizedException("The refresh token owner no longer exists.");
        var (_, email, roles) = userInfo;
        return await GenerateAuthResponseAsync(userId, email, roles, newRefreshToken, false, ipAddress);
    }

    public Task LogoutAsync(string refreshToken, string? ipAddress = null) =>
        _refreshTokenService.RevokeRefreshTokenAsync(refreshToken, ipAddress);

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(
        string userId,
        string email,
        IEnumerable<string> roles,
        string? refreshToken = null,
        bool storeRefreshToken = true,
        string? ipAddress = null)
    {
        var rolesList = roles.ToList();
        var accessToken = _jwtService.GenerateAccessToken(userId, email, rolesList);
        refreshToken ??= _jwtService.GenerateRefreshToken();
        var expiration = DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes);
        if (storeRefreshToken)
            await _refreshTokenService.StoreRefreshTokenAsync(
                userId, refreshToken, DateTime.UtcNow.AddDays(_refreshTokenExpirationDays), ipAddress);

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
        if (result.IsValid) return;
        var errors = result.Errors.GroupBy(x => x.PropertyName)
            .ToDictionary(x => x.Key, x => x.Select(error => error.ErrorMessage).ToArray());
        throw new AppValidationException(errors);
    }
}
