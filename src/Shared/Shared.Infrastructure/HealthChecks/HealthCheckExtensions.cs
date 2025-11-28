using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
        // The AddMongoDb extension method is not available in .NET 10.0 or the referenced MongoDB package. You need to implement a custom health check for MongoDB.
        return builder.Add(new HealthCheckRegistration(
            name,
            sp => throw new NotImplementedException("MongoDbHealthCheck is not implemented. Please provide a custom implementation for MongoDB health checks."),
            HealthStatus.Unhealthy,
            new[] { "database", "mongodb" }
        ));
    }

    /// <summary>
    /// Adds PostgreSQL health check
    /// </summary>
    public static IHealthChecksBuilder AddPostgreSqlHealthCheck(
        this IHealthChecksBuilder builder,
        string connectionString,
        string name = "postgresql")
    {
        // The AddNpgSql extension method is not available in .NET 10.0 or the referenced Npgsql package. You need to implement a custom health check for PostgreSQL.
        return builder.Add(new HealthCheckRegistration(
            name,
            sp => throw new NotImplementedException("PostgreSqlHealthCheck is not implemented. Please provide a custom implementation for PostgreSQL health checks."),
            HealthStatus.Unhealthy,
            new[] { "database", "postgresql" }
        ));
    }

    /// <summary>
    /// Adds Redis health check
    /// </summary>
    public static IHealthChecksBuilder AddRedisHealthCheck(
        this IHealthChecksBuilder builder,
        string connectionString,
        string name = "redis")
    {
        // The AddRedis extension method is not available in .NET 10.0 or the referenced Redis package. You need to implement a custom health check for Redis.
        return builder.Add(new HealthCheckRegistration(
            name,
            sp => throw new NotImplementedException("RedisHealthCheck is not implemented. Please provide a custom implementation for Redis health checks."),
            HealthStatus.Unhealthy,
            new[] { "cache", "redis" }
        ));
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
