namespace JobApplicationManagement.Application.Common.Interfaces;

/// <summary>
/// Abstraction for JWT token generation.
/// Lives in Application so the Application layer does not depend on
/// System.IdentityModel.Tokens.Jwt or any Infrastructure concern.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT access token for the given user.
    /// </summary>
    /// <param name="userId">Identity UserId.</param>
    /// <param name="email">User's email.</param>
    /// <param name="roles">User's assigned roles.</param>
    /// <returns>JWT access token string.</returns>
    string GenerateAccessToken(string userId, string email, IEnumerable<string> roles);

    /// <summary>
    /// Generates a cryptographically secure refresh token string.
    /// </summary>
    string GenerateRefreshToken();
}
