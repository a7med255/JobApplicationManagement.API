using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Infrastructure.Persistence;

namespace JobApplicationManagement.Infrastructure.Identity;

/// <summary>
/// Refresh token service implementation.
/// Stores SHA-256 hashed tokens in the database — raw tokens are never persisted.
/// Enforces refresh token rotation with token family reuse detection (RFC 6819) and IP auditing.
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(ApplicationDbContext context, ILogger<RefreshTokenService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt, string? createdByIp = null)
    {
        var tokenHash = HashToken(refreshToken);

        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = createdByIp
        };

        await _context.RefreshTokens.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<string?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash
                                    && rt.RevokedAt == null
                                    && rt.ExpiresAt > DateTime.UtcNow);

        return storedToken?.UserId;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? revokedByIp = null)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (storedToken is not null && storedToken.RevokedAt is null)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = revokedByIp;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string?> RotateRefreshTokenAsync(
        string currentRefreshToken,
        string newRefreshToken,
        DateTime expiresAt,
        string? createdByIp = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var currentHash = HashToken(currentRefreshToken);
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == currentHash);

        if (storedToken is null)
        {
            _logger.LogWarning("Refresh token rotation failed: token not found from IP {IpAddress}", createdByIp ?? "unknown");
            return null;
        }

        // Token reuse detection (RFC 6819 section 5.2.2.3):
        // If an already-revoked refresh token is presented, compromise is suspected.
        // Invalidate all tokens for this user immediately.
        if (storedToken.RevokedAt != null)
        {
            _logger.LogWarning(
                "SECURITY ALERT: Attempted reuse of revoked refresh token for User {UserId} from IP {IpAddress}. Revoking all user tokens.",
                storedToken.UserId, createdByIp ?? "unknown");

            var activeTokens = await _context.RefreshTokens
                .Where(x => x.UserId == storedToken.UserId && x.RevokedAt == null)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var token in activeTokens)
            {
                token.RevokedAt = now;
                token.RevokedByIp = createdByIp;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return null;
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token rotation failed: token expired for User {UserId}", storedToken.UserId);
            return null;
        }

        var newHash = HashToken(newRefreshToken);
        var nowUtc = DateTime.UtcNow;

        storedToken.RevokedAt = nowUtc;
        storedToken.RevokedByIp = createdByIp;
        storedToken.ReplacedByTokenHash = newHash;

        await _context.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = storedToken.UserId,
            TokenHash = newHash,
            CreatedAt = nowUtc,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        _logger.LogInformation(
            "Refresh token rotated successfully for User {UserId} from IP {IpAddress}",
            storedToken.UserId, createdByIp ?? "unknown");

        return storedToken.UserId;
    }

    public async Task RevokeAllForUserAsync(string userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync();
        var now = DateTime.UtcNow;
        foreach (var token in tokens)
            token.RevokedAt = now;
        await _context.SaveChangesAsync();
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
