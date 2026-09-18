namespace JobApplicationManagement.Infrastructure.Identity;

/// <summary>
/// Represents a refresh token stored in the database.
/// Token is stored as a SHA-256 hash for security — raw tokens are never persisted.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the refresh token. Never store the raw token.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// If set, this token has been revoked and can no longer be used.
    /// </summary>
    public DateTime? RevokedAt { get; set; }
}
