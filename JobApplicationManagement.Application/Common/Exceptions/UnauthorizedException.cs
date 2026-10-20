namespace JobApplicationManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown when authentication fails due to invalid credentials or unverified account.
/// Mapped to HTTP 401 Unauthorized by the global exception handler.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Invalid email or password.")
        : base(message)
    {
    }
}
