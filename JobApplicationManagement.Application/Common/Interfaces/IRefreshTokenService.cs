namespace JobApplicationManagement.Application.Common.Interfaces;

/// <summary>
/// Abstraction for refresh token storage and validation.
/// Implementation lives in Infrastructure.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Stores a new refresh token for a user.
    /// </summary>
    Task StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt);

    /// <summary>
    /// Validates a refresh token. Returns the associated userId if valid, null if invalid/expired/revoked.
    /// </summary>
    Task<string?> ValidateRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshToken);
}
