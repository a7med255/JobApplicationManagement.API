using System.Security.Claims;
using System.Text.Json;
using JobApplicationManagement.Application.Common.Exceptions;

namespace JobApplicationManagement.API.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Maps known application exceptions to HTTP status codes and returns
/// a consistent JSON error response.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(
                "Validation exception caught by middleware. Errors: {Errors}",
                ex.Errors);

            await WriteErrorResponseAsync(
                context,
                statusCode: StatusCodes.Status400BadRequest,
                message: ex.Message,
                errors: ex.Errors,
                traceId: context.TraceIdentifier);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);

            await WriteErrorResponseAsync(
                context,
                statusCode: StatusCodes.Status404NotFound,
                message: ex.Message,
                errors: null,
                traceId: context.TraceIdentifier);
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning("Forbidden access: {Message}", ex.Message);

            await WriteErrorResponseAsync(
                context,
                statusCode: StatusCodes.Status403Forbidden,
                message: ex.Message,
                errors: null,
                traceId: context.TraceIdentifier);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning("Conflict: {Message}", ex.Message);

            await WriteErrorResponseAsync(
                context,
                statusCode: StatusCodes.Status409Conflict,
                message: ex.Message,
                errors: null,
                traceId: context.TraceIdentifier);
        }
        catch (Exception ex)
        {
            var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = context.User?.FindFirstValue(ClaimTypes.Email);

            _logger.LogError(
                ex,
                "An unexpected error occurred while processing {Method} {Path} [TraceId: {TraceId}, UserId: {UserId}, UserEmail: {UserEmail}]",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier,
                userId ?? "Anonymous",
                userEmail ?? "N/A");

            await WriteErrorResponseAsync(
                context,
                statusCode: StatusCodes.Status500InternalServerError,
                message: "An unexpected error occurred. Please try again later.",
                errors: null,
                traceId: context.TraceIdentifier);
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string message,
        IDictionary<string, string[]>? errors,
        string traceId)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new { statusCode, message, errors, traceId };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
