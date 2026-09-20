using System.Security.Claims;
using Serilog.Context;

namespace JobApplicationManagement.API.Middleware;

public class RequestContextLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestContextLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Push TraceId to LogContext so it's available for all logs in this request.
        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        {
            var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = context.User?.FindFirstValue(ClaimTypes.Email);

            IDisposable? userIdContext = null;
            IDisposable? userEmailContext = null;

            if (!string.IsNullOrEmpty(userId))
            {
                userIdContext = LogContext.PushProperty("UserId", userId);
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                userEmailContext = LogContext.PushProperty("UserEmail", userEmail);
            }

            try
            {
                await _next(context);
            }
            finally
            {
                userIdContext?.Dispose();
                userEmailContext?.Dispose();
            }
        }
    }
}
