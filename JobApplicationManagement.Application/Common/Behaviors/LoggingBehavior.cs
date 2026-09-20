using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using JobApplicationManagement.Application.Common.Interfaces;

namespace JobApplicationManagement.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId ?? "Anonymous";

        _logger.LogInformation("Handling {RequestName} by User {UserId}", requestName, userId);
        
        var timer = new Stopwatch();
        timer.Start();

        var response = await next();

        timer.Stop();
        
        _logger.LogInformation("Completed {RequestName} in {ElapsedMilliseconds}ms", requestName, timer.ElapsedMilliseconds);

        return response;
    }
}
