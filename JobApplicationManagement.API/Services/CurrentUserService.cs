using System.Security.Claims;
using JobApplicationManagement.Application.Common.Interfaces;

namespace JobApplicationManagement.API.Services;

/// <summary>
/// Reads the current authenticated user's identity from HttpContext JWT claims.
/// Registered as Scoped so it resolves per-request.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyCollection<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly()
        ?? (IReadOnlyCollection<string>)Array.Empty<string>();
}
