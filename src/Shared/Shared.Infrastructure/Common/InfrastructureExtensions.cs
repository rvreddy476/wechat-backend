using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure.Common;

/// <summary>
/// Common infrastructure extension methods
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    /// Adds common infrastructure services
    /// </summary>
    public static IServiceCollection AddCommonInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
