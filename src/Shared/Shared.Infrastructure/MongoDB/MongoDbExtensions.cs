using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Shared.Infrastructure.MongoDB;

public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "MongoDbSettings")
    {
        var settings = configuration.GetSection(sectionName).Get<MongoDbSettings>()
            ?? throw new InvalidOperationException($"MongoDB settings not found in configuration section '{sectionName}'");

        services.AddSingleton<IMongoClient>(sp =>
        {
            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(settings.DatabaseName);
        });

        return services;
    }
}
