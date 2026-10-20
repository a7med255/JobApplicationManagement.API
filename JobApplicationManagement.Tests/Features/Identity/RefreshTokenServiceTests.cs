using System;
using System.Threading.Tasks;
using JobApplicationManagement.Infrastructure.Identity;
using JobApplicationManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JobApplicationManagement.Tests.Features.Identity;

public class RefreshTokenServiceTests
{
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task StoreRefreshToken_Stores_Hashed_Token_With_Ip()
    {
        using var dbContext = CreateInMemoryDbContext();
        var loggerMock = new Mock<ILogger<RefreshTokenService>>();
        var service = new RefreshTokenService(dbContext, loggerMock.Object);

        var expires = DateTime.UtcNow.AddDays(7);
        await service.StoreRefreshTokenAsync("user-1", "plain-token-123", expires, "192.168.1.100");

        var stored = await dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == "user-1");
        Assert.NotNull(stored);
        Assert.NotEqual("plain-token-123", stored.TokenHash);
        Assert.Equal("192.168.1.100", stored.CreatedByIp);
        Assert.Null(stored.RevokedAt);
    }

    [Fact]
    public async Task RotateRefreshToken_Rotates_And_Sets_ReplacedBy_And_RevokedByIp()
    {
        using var dbContext = CreateInMemoryDbContext();
        var loggerMock = new Mock<ILogger<RefreshTokenService>>();
        var service = new RefreshTokenService(dbContext, loggerMock.Object);

        await service.StoreRefreshTokenAsync("user-2", "token-v1", DateTime.UtcNow.AddDays(7), "10.0.0.1");

        var newExpires = DateTime.UtcNow.AddDays(7);
        var userId = await service.RotateRefreshTokenAsync("token-v1", "token-v2", newExpires, "10.0.0.2");

        Assert.Equal("user-2", userId);

        var allTokens = await dbContext.RefreshTokens.ToListAsync();
        Assert.Equal(2, allTokens.Count);

        var oldToken = allTokens.Find(t => t.CreatedByIp == "10.0.0.1");
        var newToken = allTokens.Find(t => t.CreatedByIp == "10.0.0.2");

        Assert.NotNull(oldToken);
        Assert.NotNull(newToken);
        Assert.NotNull(oldToken.RevokedAt);
        Assert.Equal("10.0.0.2", oldToken.RevokedByIp);
        Assert.Equal(newToken.TokenHash, oldToken.ReplacedByTokenHash);

        Assert.Null(newToken.RevokedAt);
    }

    [Fact]
    public async Task RotateRefreshToken_Detects_Token_Reuse_And_Revokes_All_User_Tokens()
    {
        using var dbContext = CreateInMemoryDbContext();
        var loggerMock = new Mock<ILogger<RefreshTokenService>>();
        var service = new RefreshTokenService(dbContext, loggerMock.Object);

        // Initial token
        await service.StoreRefreshTokenAsync("victim-user", "legit-token-1", DateTime.UtcNow.AddDays(7), "1.1.1.1");

        // Legit rotation: legit-token-1 -> legit-token-2
        var legitRotatedUserId = await service.RotateRefreshTokenAsync(
            "legit-token-1", "legit-token-2", DateTime.UtcNow.AddDays(7), "1.1.1.1");
        Assert.Equal("victim-user", legitRotatedUserId);

        // Attacker attempts to replay already-revoked legit-token-1!
        var attackerResult = await service.RotateRefreshTokenAsync(
            "legit-token-1", "attacker-token-3", DateTime.UtcNow.AddDays(7), "6.6.6.6");

        // Attack must be rejected
        Assert.Null(attackerResult);

        // AND all tokens belonging to the user family must be revoked immediately
        var remainingTokens = await dbContext.RefreshTokens.ToListAsync();
        Assert.All(remainingTokens, t => Assert.NotNull(t.RevokedAt));
    }

    [Fact]
    public async Task RotateRefreshToken_Fails_When_Token_Expired()
    {
        using var dbContext = CreateInMemoryDbContext();
        var loggerMock = new Mock<ILogger<RefreshTokenService>>();
        var service = new RefreshTokenService(dbContext, loggerMock.Object);

        await service.StoreRefreshTokenAsync("user-expired", "expired-token", DateTime.UtcNow.AddDays(-1), "10.0.0.1");

        var result = await service.RotateRefreshTokenAsync("expired-token", "new-token", DateTime.UtcNow.AddDays(7), "10.0.0.1");
        Assert.Null(result);
    }

    [Fact]
    public async Task RevokeRefreshToken_Marks_Revoked_With_Ip()
    {
        using var dbContext = CreateInMemoryDbContext();
        var loggerMock = new Mock<ILogger<RefreshTokenService>>();
        var service = new RefreshTokenService(dbContext, loggerMock.Object);

        await service.StoreRefreshTokenAsync("user-3", "token-to-revoke", DateTime.UtcNow.AddDays(7), "127.0.0.1");
        await service.RevokeRefreshTokenAsync("token-to-revoke", "127.0.0.2");

        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == "user-3");
        Assert.NotNull(token);
        Assert.NotNull(token.RevokedAt);
        Assert.Equal("127.0.0.2", token.RevokedByIp);
    }
}
