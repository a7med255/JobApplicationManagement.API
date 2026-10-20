using System.Security.Claims;
using System.Text.Json;
using JobApplicationManagement.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

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
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Authentication failure: {Message}", ex.Message);

            await WriteErrorResponseAsync(
                context,
                statusCode: StatusCodes.Status401Unauthorized,
                message: ex.Message,
                errors: null,
                traceId: context.TraceIdentifier);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access: {Message}", ex.Message);

            await WriteErrorResponseAsync(
                context,
                statusCode: StatusCodes.Status401Unauthorized,
                message: ex.Message,
                errors: null,
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
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "A database constraint rejected the requested operation.");
            await WriteErrorResponseAsync(
                context,
                statusCode: StatusCodes.Status409Conflict,
                message: "The requested operation conflicts with existing data.",
                errors: null,
                traceId: context.TraceIdentifier);
        }
        catch (Exception ex)
        {
            var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogError(
                ex,
                "An unexpected error occurred while processing {Method} {Path} [TraceId: {TraceId}, UserId: {UserId}]",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier,
                userId ?? "Anonymous");

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
        if (context.Response.HasStarted)
        {
            return;
        }

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
