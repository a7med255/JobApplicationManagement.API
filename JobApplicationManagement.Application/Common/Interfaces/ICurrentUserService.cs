namespace JobApplicationManagement.Application.Common.Interfaces;

/// <summary>
/// Abstraction for accessing the current authenticated user's identity.
/// Implementation lives in the API layer (reads from HttpContext claims).
/// Application/Domain layers depend on this abstraction — never on HttpContext directly.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The Identity UserId of the currently authenticated user, or null if anonymous.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Whether the current request is from an authenticated user.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// The roles assigned to the current user.
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }
}
