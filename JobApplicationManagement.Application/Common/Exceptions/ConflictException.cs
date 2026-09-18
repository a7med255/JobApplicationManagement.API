namespace JobApplicationManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown when an operation cannot proceed due to a conflicting state.
/// Mapped to HTTP 409 Conflict by the global exception handler.
/// Example: Trying to cancel an already-accepted application.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
