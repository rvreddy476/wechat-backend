using Microsoft.AspNetCore.Builder;

namespace Shared.Infrastructure.Middleware;

/// <summary>
/// Extension methods for middleware registration
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Registers global exception handling middleware
    /// </summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    /// <summary>
    /// Registers request logging middleware
    /// </summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
