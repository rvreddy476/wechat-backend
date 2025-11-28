using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        _logger.LogInformation(
            "Incoming request: {Method} {Path}",
            requestMethod,
            requestPath);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
                requestMethod,
                requestPath,
                statusCode,
                elapsed);
        }
    }
}
