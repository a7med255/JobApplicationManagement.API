namespace JobApplicationManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown when FluentValidation constraints are violated at the service/application layer.
/// Distinct from ASP.NET model binding errors — this catches business-layer validation failures.
/// The global exception handler maps this to HTTP 400.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Dictionary of field names to their error messages.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures occurred.")
    {
        Errors = errors;
    }
}
