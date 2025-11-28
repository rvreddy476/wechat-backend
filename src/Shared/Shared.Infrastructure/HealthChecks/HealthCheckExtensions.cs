using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Shared.Infrastructure.HealthChecks;

/// <summary>
/// Extension methods for health checks
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds MongoDB health check
    /// </summary>
    public static IHealthChecksBuilder AddMongoDbHealthCheck(
        this IHealthChecksBuilder builder,
        string connectionString,
        string name = "mongodb")
    {
        return builder.AddMongoDb(
            connectionString,
            name: name,
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "database", "mongodb" });
    }

    /// <summary>
    /// Adds PostgreSQL health check
    /// </summary>
    public static IHealthChecksBuilder AddPostgreSqlHealthCheck(
        this IHealthChecksBuilder builder,
        string connectionString,
        string name = "postgresql")
    {
        return builder.AddNpgSql(
            connectionString,
            name: name,
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "database", "postgresql" });
    }

    /// <summary>
    /// Adds Redis health check
    /// </summary>
    public static IHealthChecksBuilder AddRedisHealthCheck(
        this IHealthChecksBuilder builder,
        string connectionString,
        string name = "redis")
    {
        return builder.AddRedis(
            connectionString,
            name: name,
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "cache", "redis" });
    }

    /// <summary>
    /// Maps health check endpoints with JSON response
    /// </summary>
    public static void MapHealthCheckEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthCheckResponse
        });
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                exception = entry.Value.Exception?.Message,
                data = entry.Value.Data
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, options));
    }
}
