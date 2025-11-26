using Serilog;
using VideoProcessing.Worker;
using VideoProcessing.Worker.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/video-processing-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Video Processing Worker");

    var builder = Host.CreateApplicationBuilder(args);

    // Add Serilog
    builder.Services.AddSerilog();

    // Register services
    builder.Services.AddSingleton<IVideoTranscodingService, VideoTranscodingService>();
    builder.Services.AddSingleton<IStorageService, GcpStorageService>();
    builder.Services.AddSingleton<IVideoRepository, VideoRepository>();
    builder.Services.AddSingleton<IJobQueueService, RedisJobQueueService>();
    builder.Services.AddScoped<VideoProcessingService>();

    // Register background worker
    builder.Services.AddHostedService<Worker>();

    // Health checks
    builder.Services.AddHealthChecks()
        .AddRedis(
            builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379",
            name: "redis",
            tags: new[] { "ready" })
        .AddMongoDb(
            builder.Configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017",
            name: "mongodb",
            tags: new[] { "ready" });

    var host = builder.Build();

    await host.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Video Processing Worker terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
