using ChatService.Domain.Repositories;
using ChatService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ChatService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MongoDB
        var connectionString = configuration.GetConnectionString("MongoDB")
            ?? throw new InvalidOperationException("MongoDB connection string not found");

        var databaseName = configuration["MongoDbSettings:DatabaseName"]
            ?? "ChatDb";

        services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });

        // Repositories
        services.AddScoped<IConversationRepository, MongoConversationRepository>();
        services.AddScoped<IMessageRepository, MongoMessageRepository>();

        return services;
    }
}
