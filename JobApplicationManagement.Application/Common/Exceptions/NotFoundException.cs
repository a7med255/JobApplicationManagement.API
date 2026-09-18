namespace JobApplicationManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found.
/// Mapped to HTTP 404 by the global exception handler.
/// Created here for future use (e.g., GetJobById, GetApplicationById).
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with key '{key}' was not found.")
    {
    }
}
