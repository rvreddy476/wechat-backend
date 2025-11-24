using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Shared.Infrastructure.Redis;

public static class RedisExtensions
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "RedisSettings")
    {
        var settings = configuration.GetSection(sectionName).Get<RedisSettings>()
            ?? throw new InvalidOperationException($"Redis settings not found in configuration section '{sectionName}'");

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configurationOptions = ConfigurationOptions.Parse(settings.ConnectionString);
            configurationOptions.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        services.AddSingleton<IRedisService, RedisService>(sp =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            return new RedisService(multiplexer, settings.DatabaseNumber);
        });

        return services;
    }
}
