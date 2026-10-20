using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Identity.DTOs;
using JobApplicationManagement.Application.Features.Identity.Services;
using JobApplicationManagement.Application.Features.Identity.Validators;
using JobApplicationManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JobApplicationManagement.Tests.Features.Identity;

public class AuthServiceTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEmailQueue> _emailQueueMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var configValues = new Dictionary<string, string?>
        {
            ["Jwt:AccessTokenExpirationMinutes"] = "15",
            ["Jwt:RefreshTokenExpirationDays"] = "7"
        };
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        _authService = new AuthService(
            _identityServiceMock.Object,
            _jwtServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _unitOfWorkMock.Object,
            new RegisterValidator(),
            new LoginValidator(),
            _loggerMock.Object,
            _emailQueueMock.Object,
            _configuration);
    }

    [Fact]
    public async Task LoginAsync_Throws_Unauthorized_When_Invalid_Credentials()
    {
        var loginDto = new LoginDto { Email = "user@example.com", Password = "WrongPassword123!" };
        _identityServiceMock.Setup(x => x.ValidateCredentialsAsync(loginDto.Email, loginDto.Password))
            .ReturnsAsync(new CredentialsValidationResult(CredentialsValidationStatus.InvalidCredentials, "", "", Array.Empty<string>()));

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.LoginAsync(loginDto));
        Assert.Equal("Invalid email or password.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_Throws_Unauthorized_When_Email_Not_Confirmed()
    {
        var loginDto = new LoginDto { Email = "unverified@example.com", Password = "ValidPassword123!" };
        _identityServiceMock.Setup(x => x.ValidateCredentialsAsync(loginDto.Email, loginDto.Password))
            .ReturnsAsync(new CredentialsValidationResult(CredentialsValidationStatus.EmailNotConfirmed, "user-1", loginDto.Email, new[] { "Candidate" }));

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.LoginAsync(loginDto));
        Assert.Contains("Email is not confirmed", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_Succeeds_And_Stores_RefreshToken_With_Ip()
    {
        var loginDto = new LoginDto { Email = "candidate@example.com", Password = "ValidPassword123!" };
        var clientIp = "192.168.1.50";

        _identityServiceMock.Setup(x => x.ValidateCredentialsAsync(loginDto.Email, loginDto.Password))
            .ReturnsAsync(new CredentialsValidationResult(CredentialsValidationStatus.Success, "user-1", loginDto.Email, new[] { "Candidate" }));

        _jwtServiceMock.Setup(x => x.GenerateAccessToken("user-1", loginDto.Email, It.IsAny<IEnumerable<string>>()))
            .Returns("access-token-jwt");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token-val");

        _refreshTokenServiceMock.Setup(x => x.StoreRefreshTokenAsync("user-1", "refresh-token-val", It.IsAny<DateTime>(), clientIp))
            .Returns(Task.CompletedTask);

        var response = await _authService.LoginAsync(loginDto, clientIp);

        Assert.Equal("access-token-jwt", response.AccessToken);
        Assert.Equal("refresh-token-val", response.RefreshToken);
        Assert.Equal("candidate@example.com", response.Email);
        _refreshTokenServiceMock.Verify(x => x.StoreRefreshTokenAsync("user-1", "refresh-token-val", It.IsAny<DateTime>(), clientIp), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Throws_Forbidden_When_Registering_Admin()
    {
        var registerDto = new RegisterDto
        {
            FullName = "Admin User",
            Email = "admin@example.com",
            Password = "Password123!",
            Role = "Admin"
        };

        await Assert.ThrowsAsync<ForbiddenException>(() => _authService.RegisterAsync(registerDto));
    }

    [Fact]
    public async Task RegisterAsync_Creates_User_And_Queues_Verification_Email_Without_AccessToken()
    {
        var registerDto = new RegisterDto
        {
            FullName = "New Candidate",
            Email = "new@example.com",
            Password = "SecurePassword123!",
            Role = "Candidate"
        };

        _identityServiceMock.Setup(x => x.CreateUserAsync(registerDto.Email, registerDto.Password, registerDto.FullName))
            .ReturnsAsync((true, "user-new", Array.Empty<string>()));
        _identityServiceMock.Setup(x => x.AddToRoleAsync("user-new", "Candidate"))
            .ReturnsAsync(true);
        _identityServiceMock.Setup(x => x.GenerateEmailVerificationOtpAsync(registerDto.Email))
            .ReturnsAsync("123456");

        _unitOfWorkMock.Setup(x => x.Candidates.AddAsync(It.IsAny<Candidate>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailQueueMock.Setup(x => x.QueueAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _authService.RegisterAsync(registerDto, "10.0.0.1");

        Assert.Equal("new@example.com", result.Email);
        Assert.True(string.IsNullOrEmpty(result.AccessToken));
        _unitOfWorkMock.Verify(x => x.Candidates.AddAsync(It.Is<Candidate>(c => c.UserId == "user-new")), Times.Once);
        _emailQueueMock.Verify(x => x.QueueAsync(It.Is<EmailMessage>(m => m.Template == EmailTemplate.VerifyEmail), It.IsAny<CancellationToken>()), Times.Once);
        _emailQueueMock.Verify(x => x.QueueAsync(It.Is<EmailMessage>(m => m.Template == EmailTemplate.Welcome), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_Calls_IdentityService_And_Confirms()
    {
        _identityServiceMock.Setup(x => x.VerifyEmailOtpAsync("user@example.com", "654321"))
            .ReturnsAsync((true, Array.Empty<string>()));

        var result = await _authService.VerifyEmailAsync("user@example.com", "654321");
        Assert.True(result);
    }

    [Fact]
    public async Task ResetPasswordAsync_Calls_IdentityService_And_Queues_PasswordChanged_Email()
    {
        _identityServiceMock.Setup(x => x.ResetPasswordWithOtpAsync("user@example.com", "112233", "NewPassword123!"))
            .ReturnsAsync((true, Array.Empty<string>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _emailQueueMock.Setup(x => x.QueueAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _authService.ResetPasswordAsync("user@example.com", "112233", "NewPassword123!");
        Assert.True(result);
        _emailQueueMock.Verify(x => x.QueueAsync(It.Is<EmailMessage>(m => m.Template == EmailTemplate.PasswordChanged), It.IsAny<CancellationToken>()), Times.Once);
    }
}
