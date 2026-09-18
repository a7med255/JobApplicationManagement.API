namespace JobApplicationManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown when the authenticated user does not have ownership/permission for the requested operation.
/// Mapped to HTTP 403 Forbidden by the global exception handler.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
