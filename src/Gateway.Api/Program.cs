using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Shared.Infrastructure.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "WeChat API Gateway", Version = "v1" });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

// Add Rate Limiting
var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var userId = context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: userId,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitConfig.GetValue<int>("PermitLimit", 100),
                Window = TimeSpan.FromSeconds(rateLimitConfig.GetValue<int>("Window", 60)),
                QueueLimit = rateLimitConfig.GetValue<int>("QueueLimit", 10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                ? retryAfter.TotalSeconds
                : 0
        }, cancellationToken);
    };
});

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseCors("AllowAll");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Map health check endpoint
app.MapHealthChecks("/health");

// Gateway info endpoint
app.MapGet("/", () => new
{
    service = "WeChat API Gateway",
    version = "1.0.0",
    status = "running",
    routes = new[]
    {
        new { path = "/api/auth/**", service = "AuthService", port = 5001 },
        new { path = "/api/userprofile/**", service = "UserProfileService", port = 5002 },
        new { path = "/api/posts/**", service = "PostFeedService", port = 5003 },
        new { path = "/api/comments/**", service = "PostFeedService", port = 5003 },
        new { path = "/api/reactions/**", service = "PostFeedService", port = 5003 },
        new { path = "/api/chats/**", service = "ChatService", port = 5004 },
        new { path = "/api/messages/**", service = "ChatService", port = 5004 },
        new { path = "/hubs/chat", service = "ChatService (SignalR)", port = 5004 },
        new { path = "/api/videos/**", service = "VideoService", port = 5005 }
    }
});

// Aggregated health check for all services
app.MapGet("/health/services", async (IHttpClientFactory httpClientFactory) =>
{
    var authHealth = await CheckServiceHealth("http://localhost:5001/health");
    var userProfileHealth = await CheckServiceHealth("http://localhost:5002/health");
    var postFeedHealth = await CheckServiceHealth("http://localhost:5003/health");
    var chatHealth = await CheckServiceHealth("http://localhost:5004/health");
    var videoHealth = await CheckServiceHealth("http://localhost:5005/health");

    var overallStatus = authHealth && userProfileHealth && postFeedHealth && chatHealth && videoHealth ? "healthy" : "unhealthy";

    return Results.Ok(new
    {
        status = overallStatus,
        services = new
        {
            authService = new { status = authHealth ? "healthy" : "unhealthy", url = "http://localhost:5001" },
            userProfileService = new { status = userProfileHealth ? "healthy" : "unhealthy", url = "http://localhost:5002" },
            postFeedService = new { status = postFeedHealth ? "healthy" : "unhealthy", url = "http://localhost:5003" },
            chatService = new { status = chatHealth ? "healthy" : "unhealthy", url = "http://localhost:5004" },
            videoService = new { status = videoHealth ? "healthy" : "unhealthy", url = "http://localhost:5005" }
        },
        timestamp = DateTime.UtcNow
    });

    static async Task<bool> CheckServiceHealth(string url)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
});

// Map YARP routes
app.MapReverseProxy();

try
{
    Log.Information("Starting Gateway API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
