using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;
using MediaService.Api.Repositories;
using MediaService.Api.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// MongoDB
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB")
    ?? throw new InvalidOperationException("MongoDB connection string not found");
var mongoDatabase = builder.Configuration["MongoDB:DatabaseName"] ?? "wechat_media";
var mongoClient = new MongoClient(mongoConnectionString);
var database = mongoClient.GetDatabase(mongoDatabase);

builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton(database);

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
    });

builder.Services.AddAuthorization();

// Application Services
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<IStorageService, LocalStorageService>();
builder.Services.AddScoped<IMediaProcessingService, MediaProcessingService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongoConnectionString,
        name: "mongodb",
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

// Serve static files from uploads directory
var uploadsPath = builder.Configuration["MediaSettings:LocalStoragePath"] ?? "./uploads";
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), uploadsPath)),
    RequestPath = "/media"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapGet("/", () => new
{
    service = "MediaService.Api",
    version = "1.0.0",
    status = "running",
    timestamp = DateTime.UtcNow
});

// Background service to clean up expired uploads
var cleanupTimer = new System.Threading.Timer(async _ =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMediaRepository>();
        await repository.CleanupExpiredUploadsAsync();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error cleaning up expired uploads");
    }
}, null, TimeSpan.FromMinutes(30), TimeSpan.FromHours(6));

try
{
    Log.Information("Starting MediaService.Api");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MediaService.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
