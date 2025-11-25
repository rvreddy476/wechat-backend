using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Realtime.Api.Services;
using Realtime.Api.Hubs;
using StackExchange.Redis;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Redis
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Redis connection string not found");
var redis = ConnectionMultiplexer.Connect(redisConnectionString);

builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not found");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        // For SignalR authentication
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// SignalR with Redis backplane
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnectionString, options =>
    {
        options.Configuration.ChannelPrefix = "Realtime";
    });

// Application Services
builder.Services.AddSingleton<IPresenceService, PresenceService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddRedis(
        redisConnectionString,
        name: "redis",
        timeout: TimeSpan.FromSeconds(3));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PresenceHub>("/hubs/presence");

app.MapHealthChecks("/health");

app.MapGet("/", () => new
{
    service = "Realtime.Api",
    version = "1.0.0",
    status = "running",
    timestamp = DateTime.UtcNow
});

// Background service to cleanup stale connections
var cleanupTimer = new System.Threading.Timer(async _ =>
{
    try
    {
        var presenceService = app.Services.GetRequiredService<IPresenceService>();
        await presenceService.CleanupStaleConnectionsAsync();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error cleaning up stale connections");
    }
}, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

try
{
    Log.Information("Starting Realtime.Api");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Realtime.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
