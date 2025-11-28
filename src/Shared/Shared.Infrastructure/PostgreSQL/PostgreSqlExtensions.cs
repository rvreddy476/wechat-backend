using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure.PostgreSQL;

/// <summary>
/// Extension methods for PostgreSQL configuration
/// </summary>
public static class PostgreSqlExtensions
{
    /// <summary>
    /// Adds PostgreSQL connection factory to the DI container
    /// </summary>
    public static IServiceCollection AddPostgreSqlConnectionFactory(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<IPostgreSqlConnectionFactory>(
            _ => new PostgreSqlConnectionFactory(connectionString));

        return services;
    }
}
