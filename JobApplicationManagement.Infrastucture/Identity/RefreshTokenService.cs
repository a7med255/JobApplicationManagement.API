using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Infrastructure.Persistence;

namespace JobApplicationManagement.Infrastructure.Identity;

/// <summary>
/// Refresh token service implementation.
/// Stores SHA-256 hashed tokens in the database — raw tokens are never persisted.
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt)
    {
        var tokenHash = HashToken(refreshToken);

        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
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

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (storedToken is not null)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
