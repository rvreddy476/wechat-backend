using Serilog;
using ChatService.Api.Hubs;
using ChatService.Api.Repositories;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.MongoDB;
using Shared.Infrastructure.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "WeChat Chat API", Version = "v1" });

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

// Add MongoDB
builder.Services.AddMongoDb(builder.Configuration);

// Add Redis
builder.Services.AddRedis(builder.Configuration);

// Add SignalR with Redis backplane
var redisConnection = builder.Configuration.GetSection("RedisSettings:ConnectionString").Value!;
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnection, options =>
    {
        options.Configuration.ChannelPrefix = "WeChat:Chat";
    });

// Add repositories
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddMongoDb(
        builder.Configuration.GetSection("MongoDbSettings:ConnectionString").Value!,
        name: "mongodb",
        tags: new[] { "db", "mongodb" }
    );

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hub
app.MapHub<ChatHub>("/hubs/chat");

app.MapHealthChecks("/health");

app.MapGet("/", () => new
{
    service = "WeChat ChatService",
    version = "1.0.0",
    status = "running",
    signalrHub = "/hubs/chat"
});

try
{
    Log.Information("Starting ChatService API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ChatService API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
