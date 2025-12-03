# PostBook Backend Architecture - Complete Implementation Guide

> **Purpose**: Comprehensive .NET microservices implementation using Clean Architecture  
> **Framework**: .NET 8.0  
> **Architecture**: Clean Architecture + CQRS + MediatR + Repository Pattern  
> **API Gateway**: Ocelot  
> **Version**: 1.0  
> **Last Updated**: 2025-12-02

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Clean Architecture Principles](#clean-architecture-principles)
3. [Solution Structure](#solution-structure)
4. [Technology Stack](#technology-stack)
5. [Service Implementations](#service-implementations)
6. [API Gateway Setup](#api-gateway-setup)
7. [Authentication & Authorization](#authentication--authorization)
8. [CQRS Pattern with MediatR](#cqrs-pattern-with-mediatr)
9. [Repository Pattern](#repository-pattern)
10. [Cross-Cutting Concerns](#cross-cutting-concerns)
11. [Service Communication](#service-communication)
12. [Error Handling](#error-handling)
13. [Logging & Monitoring](#logging--monitoring)
14. [Testing Strategy](#testing-strategy)
15. [Deployment Guide](#deployment-guide)

---

## Architecture Overview

### Microservices Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     API Gateway (Ocelot)                 │
│              Port 5000 - Entry Point for All             │
└────────┬────────────────────────────────────────────────┘
         │
         ├──────────► Auth Service (Port 5001) - PostgreSQL
         │
         ├──────────► UserProfile Service (Port 5002) - MongoDB
         │
         ├──────────► Chat Service (Port 5003) - MongoDB + SignalR
         │
         ├──────────► PostFeed Service (Port 5004) - MongoDB
         │
         ├──────────► Media Service (Port 5005) - MongoDB + Blob Storage
         │
         └──────────► Notification Service (Port 5006) - MongoDB + SignalR
```

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│                   API/Presentation Layer                 │
│        Controllers, SignalR Hubs, Middleware            │
├─────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                    │
│      DbContext, Repositories, External Services         │
├─────────────────────────────────────────────────────────┤
│                   Application Layer                      │
│    Commands, Queries, Handlers, Validators, DTOs       │
├─────────────────────────────────────────────────────────┤
│                     Domain Layer                         │
│      Entities, Value Objects, Interfaces, Events        │
└─────────────────────────────────────────────────────────┘

Dependency Rule: Dependencies point INWARD only
```

---

## Clean Architecture Principles

### Key Principles

1. **Dependency Inversion**: Inner layers don't depend on outer layers
2. **Separation of Concerns**: Each layer has specific responsibilities
3. **Testability**: Business logic can be tested without infrastructure
4. **Independence**: Can change UI, database, or frameworks without affecting business logic

### Layer Responsibilities

#### Domain Layer (Core - No Dependencies)
- **Entities**: Core business objects with identity
- **Value Objects**: Immutable objects without identity
- **Domain Events**: Events that occur in the domain
- **Enums**: Domain-specific enumerations  
- **Interfaces**: Repository contracts (defined here, implemented in Infrastructure)
- **Exceptions**: Domain-specific exceptions

#### Application Layer (Depends on: Domain)
- **Commands**: Write operations (Create, Update, Delete)
- **Queries**: Read operations (Get, List, Search)
- **Command/Query Handlers**: Process commands and queries via MediatR
- **Validators**: FluentValidation for input validation
- **DTOs**: Data Transfer Objects for API responses
- **Mapping Profiles**: AutoMapper configurations
- **Pipeline Behaviors**: Cross-cutting concerns (Logging, Validation, Caching)
- **Application Interfaces**: Service contracts

#### Infrastructure Layer (Depends on: Application, Domain)
- **DbContext**: EF Core or MongoDB configurations
- **Repositories**: Implementation of repository interfaces
- **External Services**: Email, SMS, Storage, Payment gateways
- **Caching**: Redis implementation
- **Message Bus**: RabbitMQ/Service Bus implementation
- **Identity**: JWT generation, password hashing

#### Presentation/API Layer (Depends on: Application, Infrastructure)
- **Controllers**: API endpoints
- **SignalR Hubs**: Real-time communication
- **Middleware**: Request/response pipeline
- **Filters**: Exception filters, action filters
- **Program.cs**: Application startup and DI configuration

---

## Solution Structure

### Complete Solution Organization

```
PostBook.sln
│
├── src/
│   │
│   ├── ApiGateway/
│   │   └── PostBook.ApiGateway/
│   │       ├── Program.cs
│   │       ├── ocelot.json
│   │       └── appsettings.json
│   │
│   ├── Services/
│   │   │
│   │   ├── Auth/                                    [PostgreSQL]
│   │   │   ├── PostBook.Auth.Domain/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── User.cs
│   │   │   │   │   ├── UserSession.cs
│   │   │   │   │   └── VerificationCode.cs
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── Email.cs
│   │   │   │   │   └── PhoneNumber.cs
│   │   │   │   ├── Enums/
│   │   │   │   │   ├── UserRole.cs
│   │   │   │   │   └── AccountStatus.cs
│   │   │   │   ├── Events/
│   │   │   │   │   └── UserCreatedEvent.cs
│   │   │   │   ├── Exceptions/
│   │   │   │   │   └── AuthException.cs
│   │   │   │   └── Interfaces/
│   │   │   │       └── IUserRepository.cs
│   │   │   │
│   │   │   ├── PostBook.Auth.Application/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateUser/
│   │   │   │   │   │   ├── CreateUserCommand.cs
│   │   │   │   │   │   ├── CreateUserCommandHandler.cs
│   │   │   │   │   │   └── CreateUserCommandValidator.cs
│   │   │   │   │   ├── Login/
│   │   │   │   │   ├── VerifyEmail/
│   │   │   │   │   └── ChangePassword/
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetUser/
│   │   │   │   │   │   ├── GetUserQuery.cs
│   │   │   │   │   │   └── GetUserQueryHandler.cs
│   │   │   │   │   └── GetLoginHistory/
│   │   │   │   ├── DTOs/
│   │   │   │   │   ├── UserDto.cs
│   │   │   │   │   └── LoginResponseDto.cs
│   │   │   │   ├── Mappings/
│   │   │   │   │   └── MappingProfile.cs
│   │   │   │   ├── Behaviors/
│   │   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   │   └── CachingBehavior.cs
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── IJwtTokenGenerator.cs
│   │   │   │   │   └── IPasswordHasher.cs
│   │   │   │   └── DependencyInjection.cs
│   │   │   │
│   │   │   ├── PostBook.Auth.Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   │   ├── ApplicationDbContext.cs
│   │   │   │   │   ├── Configurations/
│   │   │   │   │   │   ├── UserConfiguration.cs
│   │   │   │   │   │   └── UserSessionConfiguration.cs
│   │   │   │   │   ├── Repositories/
│   │   │   │   │   │   └── UserRepository.cs
│   │   │   │   │   └── Migrations/
│   │   │   │   ├── Services/
│   │   │   │   │   ├── JwtTokenGenerator.cs
│   │   │   │   │   ├── PasswordHasher.cs
│   │   │   │   │   └── EmailService.cs
│   │   │   │   ├── Caching/
│   │   │   │   │   └── RedisCacheService.cs
│   │   │   │   ├── Messaging/
│   │   │   │   │   └── EventBusService.cs
│   │   │   │   └── DependencyInjection.cs
│   │   │   │
│   │   │   └── PostBook.Auth.API/
│   │   │       ├── Controllers/
│   │   │       │   └── AuthController.cs
│   │   │       ├── Filters/
│   │   │       │   └── ExceptionFilter.cs
│   │   │       ├── Middleware/
│   │   │       │   ├── ErrorHandlingMiddleware.cs
│   │   │       │   └── RequestLoggingMiddleware.cs
│   │   │       ├── Program.cs
│   │   │       └── appsettings.json
│   │   │
│   │   ├── UserProfile/                            [MongoDB]
│   │   │   ├── PostBook.UserProfile.Domain/
│   │   │   ├── PostBook.UserProfile.Application/
│   │   │   ├── PostBook.UserProfile.Infrastructure/
│   │   │   └── PostBook.UserProfile.API/
│   │   │
│   │   ├── Chat/                                   [MongoDB + SignalR]
│   │   │   ├── PostBook.Chat.Domain/
│   │   │   ├── PostBook.Chat.Application/
│   │   │   ├── PostBook.Chat.Infrastructure/
│   │   │   └── PostBook.Chat.API/
│   │   │       └── Hubs/
│   │   │           └── ChatHub.cs
│   │   │
│   │   ├── PostFeed/                               [MongoDB]
│   │   │   ├── PostBook.PostFeed.Domain/
│   │   │   ├── PostBook.PostFeed.Application/
│   │   │   ├── PostBook.PostFeed.Infrastructure/
│   │   │   └── PostBook.PostFeed.API/
│   │   │
│   │   ├── Media/                                  [MongoDB + Blob Storage]
│   │   │   ├── PostBook.Media.Domain/
│   │   │   ├── PostBook.Media.Application/
│   │   │   ├── PostBook.Media.Infrastructure/
│   │   │   └── PostBook.Media.API/
│   │   │
│   │   └── Notification/                           [MongoDB + SignalR]
│   │       ├── PostBook.Notification.Domain/
│   │       ├── PostBook.Notification.Application/
│   │       ├── PostBook.Notification.Infrastructure/
│   │       └── PostBook.Notification.API/
│   │           └── Hubs/
│   │               └── NotificationHub.cs
│   │
│   └── BuildingBlocks/
│       ├── PostBook.Common/
│       │   ├── Exceptions/
│       │   │   ├── DomainException.cs
│       │   │   ├── NotFoundException.cs
│       │   │   └── ValidationException.cs
│       │   ├── Models/
│       │   │   ├── Result.cs
│       │   │   ├── PagedResult.cs
│       │   │   └── ApiResponse.cs
│       │   ├── Extensions/
│       │   │   └── StringExtensions.cs
│       │   └── Interfaces/
│       │       └── IUnitOfWork.cs
│       │
│       ├── PostBook.EventBus/
│       │   ├── IEventBus.cs
│       │   ├── IntegrationEvent.cs
│       │   └── Implementations/
│       │       ├── RabbitMqEventBus.cs
│       │       └── AzureServiceBusEventBus.cs
│       │
│       └── PostBook.Contracts/
│           ├── Events/
│           │   ├── UserCreatedEvent.cs
│           │   ├── UserProfileUpdatedEvent.cs
│           │   └── PostCreatedEvent.cs
│           └── DTOs/
│               └── UserInfoDto.cs
│
└── tests/
    ├── Unit/
    │   ├── PostBook.Auth.UnitTests/
    │   ├── PostBook.UserProfile.UnitTests/
    │   └── ...
    │
    └── Integration/
        ├── PostBook.Auth.IntegrationTests/
        ├── PostBook.UserProfile.IntegrationTests/
        └── ...
```

---

## Technology Stack

### Core Technologies

**Backend Framework**:
- .NET 8.0 SDK
- ASP.NET Core Web API 8.0
- C# 12

**Databases**:
- PostgreSQL 14+ (Auth Service) with EF Core 8.0
- MongoDB 6+ (All other services) with MongoDB.Driver 2.23+

**CQRS & Patterns**:
- MediatR 12.0+ (Command/Query pattern)
- FluentValidation 11.0+ (Input validation)
- AutoMapper 12.0+ (Object-to-object mapping)

**API Gateway**:
- Ocelot 20.0+ (API Gateway and routing)

**Authentication**:
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0
- BCrypt.Net for password hashing

**Real-Time Communication**:
- SignalR (for Chat and Notifications)

**Message Bus** (choose one):
- RabbitMQ with RabbitMQ.Client 6.5+
- Azure Service Bus with Azure.Messaging.ServiceBus 7.17+

**Caching**:
- Redis with StackExchange.Redis 2.7+
- Microsoft.Extensions.Caching.StackExchangeRedis

**Storage**:
- Azure Blob Storage or AWS S3
- MinIO for local development

**Resilience**:
- Polly 8.0+ (Circuit breaker, retry policies)

**Logging**:
- Serilog 3.1+ (Structured logging)
- Seq or Elasticsearch for log aggregation

**Monitoring**:
- Application Insights or Elastic APM
- Prometheus + Grafana

**Testing**:
- xUnit 2.6+
- Moq 4.20+ (Mocking)
- FluentAssertions 6.12+ (Test assertions)
- Testcontainers (Integration testing with Docker)

---

## Service Implementations

### Service Matrix

| Service | Database | Port | Key Features | Special Components |
|---------|----------|------|--------------|-------------------|
| **Auth** | PostgreSQL | 5001 | JWT tokens, Sessions, Password reset | BCrypt, JWT |
| **UserProfile** | MongoDB | 5002 | Profiles, Education, Skills, Friends | MongoDB, Geo queries |
| **Chat** | MongoDB | 5003 | Real-time messaging, Conversations | SignalR Hub |
| **PostFeed** | MongoDB | 5004 | Posts, Comments, Likes, Hashtags | Text search, Trending |
| **Media** | MongoDB | 5005 | File uploads, CDN integration | Blob Storage |
| **Notification** | MongoDB | 5006 | Notifications, Push | SignalR Hub, TTL |

---

### 1. Auth Service (PostgreSQL)

#### Domain Entities

**User.cs** - Core user entity with business logic:
```csharp
public class User
{
    public Guid UserId { get; private set; }
    public string Username { get; private set; }
    public Email Email { get; private set; } // Value Object
    public string PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public UserRole Role { get; private set; }
    public AccountStatus Status { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? AccountLockedUntil { get; private set; }
    
    // Factory method
    public static User Create(string username, Email email, string passwordHash, ...);
    
    // Business logic
    public void VerifyEmail();
    public void RecordFailedLogin();
    public bool IsAccountLocked();
    public void SoftDelete();
}
```

#### Application Commands

**CreateUserCommand** - CQRS command for user registration:
```csharp
public record CreateUserCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<Result<CreateUserResponse>>;
```

**CreateUserCommandHandler**:
```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    
    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // 1. Validate email and username don't exist
        // 2. Hash password
        // 3. Create User entity
        // 4. Save to repository
        // 5. Publish UserCreatedEvent
        // 6. Return response
    }
}
```

**CreateUserCommandValidator** - FluentValidation:
```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_]+$");
            
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
            
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Must contain uppercase")
            .Matches("[a-z]").WithMessage("Must contain lowercase")
            .Matches("[0-9]").WithMessage("Must contain number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Must contain special character");
    }
}
```

#### Infrastructure - EF Core Configuration

**ApplicationDbContext.cs**:
```csharp
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

**UserConfiguration.cs** - Entity configuration:
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.UserId);
        
        // Configure Email value object
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();
        });
        
        // Indexes
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => new { u.IsDeleted, u.Status });
    }
}
```

**UserRepository.cs**:
```csharp
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<User?> GetByEmailAsync(Email email, CancellationToken ct)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);
    }
    
    public async Task<User> AddAsync(User user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user, ct);
        return user;
    }
}
```

#### API Controller

**AuthController.cs**:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Register([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetUser), 
                new { userId = result.Value.UserId }, 
                ApiResponse<CreateUserResponse>.Success(result.Value));
                
        return BadRequest(ApiResponse.Failure(result.Error));
    }
    
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess 
            ? Ok(ApiResponse<LoginResponse>.Success(result.Value))
            : Unauthorized(ApiResponse.Failure(result.Error));
    }
    
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.GetUserId(); // Extension method
        var query = new GetUserByIdQuery(userId);
        var result = await _mediator.Send(query);
        
        return result.IsSuccess 
            ? Ok(ApiResponse<UserDto>.Success(result.Value))
            : NotFound(ApiResponse.Failure(result.Error));
    }
}
```

---

### 2. UserProfile Service (MongoDB)

#### Domain Entities

**UserProfile.cs** - MongoDB document:
```csharp
public class UserProfile
{
    public ObjectId Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Username { get; private set; }
    public string DisplayName { get; private set; }
    public string Bio { get; private set; }
    public string AvatarUrl { get; private set; }
    public Location Location { get; private set; }
    public List<Education> Education { get; private set; }
    public List<Experience> Experience { get; private set; }
    public List<Skill> Skills { get; private set; }
    public List<Guid> Friends { get; private set; }
    public PrivacySettings PrivacySettings { get; private set; }
    public ProfileStatistics Statistics { get; private set; }
    
    public void AddEducation(Education education);
    public void AddFriend(Guid friendId);
    public void UpdatePrivacySettings(PrivacySettings settings);
}
```

**Education.cs** - Embedded document:
```csharp
public class Education
{
    public string Id { get; private set; }
    public string School { get; private set; }
    public string Degree { get; private set; }
    public string FieldOfStudy { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool Current { get; private set; }
}
```

#### Infrastructure - MongoDB Configuration

**MongoDbContext.cs**:
```csharp
public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    
    public MongoDbContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
        _database = client.GetDatabase("UserProfileServiceDB");
    }
    
    public IMongoCollection<UserProfile> UserProfiles => 
        _database.GetCollection<UserProfile>("user_profiles");
        
    public IMongoCollection<FriendRequest> FriendRequests => 
        _database.GetCollection<FriendRequest>("friend_requests");
}
```

**UserProfileRepository.cs**:
```csharp
public class UserProfileRepository : IUserProfileRepository
{
    private readonly IMongoCollection<UserProfile> _collection;
    
    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _collection
            .Find(p => p.UserId == userId && !p.IsDeleted)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<List<UserProfile>> SearchAsync(string query, int page, int limit, CancellationToken ct)
    {
        var filter = Builders<UserProfile>.Filter.Text(query);
        
        return await _collection
            .Find(filter)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync(ct);
    }
}
```

#### Application Commands

**AddEducationCommand**:
```csharp
public record AddEducationCommand(
    Guid UserId,
    string School,
    string Degree,
    string FieldOfStudy,
    DateTime StartDate,
    DateTime? EndDate,
    bool Current
) : IRequest<Result<EducationDto>>;
```

**AddEducationCommandHandler**:
```csharp
public class AddEducationCommandHandler : IRequestHandler<AddEducationCommand, Result<EducationDto>>
{
    private readonly IUserProfileRepository _repository;
    
    public async Task<Result<EducationDto>> Handle(AddEducationCommand request, CancellationToken ct)
    {
        var profile = await _repository.GetByUserIdAsync(request.UserId, ct);
        if (profile == null) return Result<EducationDto>.Failure("Profile not found");
        
        var education = Education.Create(request.School, request.Degree, ...);
        profile.AddEducation(education);
        
        await _repository.UpdateAsync(profile, ct);
        
        return Result<EducationDto>.Success(_mapper.Map<EducationDto>(education));
    }
}
```

#### API Controller

**ProfilesController.cs**:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProfilesController : ControllerBase
{
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        var query = new GetUserProfileQuery(userId);
        var result = await _mediator.Send(query);
        
        return result.IsSuccess 
            ? Ok(ApiResponse<UserProfileDto>.Success(result.Value))
            : NotFound(ApiResponse.Failure(result.Error));
    }
    
    [HttpPost("me/education")]
    public async Task<IActionResult> AddEducation([FromBody] AddEducationRequest request)
    {
        var userId = User.GetUserId();
        var command = new AddEducationCommand(userId, request.School, ...);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(ApiResponse<EducationDto>.Success(result.Value))
            : BadRequest(ApiResponse.Failure(result.Error));
    }
    
    [HttpPost("{userId:guid}/follow")]
    public async Task<IActionResult> FollowUser(Guid userId)
    {
        var currentUserId = User.GetUserId();
        var command = new FollowUserCommand(currentUserId, userId);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(ApiResponse.Success("User followed successfully"))
            : BadRequest(ApiResponse.Failure(result.Error));
    }
}
```

---

### 3. Chat Service (MongoDB + SignalR)

#### Domain Entities

**Conversation.cs**:
```csharp
public class Conversation
{
    public ObjectId Id { get; private set; }
    public ConversationType Type { get; private set; } // OneOnOne, Group
    public string? Name { get; private set; }
    public List<Participant> Participants { get; private set; }
    public LastMessage? LastMessage { get; private set; }
    public int MessagesCount { get; private set; }
    
    public void AddParticipant(Participant participant);
    public void RemoveParticipant(Guid userId);
    public void UpdateLastMessage(Message message);
}
```

**Message.cs**:
```csharp
public class Message
{
    public ObjectId Id { get; private set; }
    public ObjectId ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; }
    public MessageType Type { get; private set; } // Text, Media, File
    public List<MediaFile> MediaFiles { get; private set; }
    public List<ReadReceipt> ReadBy { get; private set; }
    public List<Reaction> Reactions { get; private set; }
    public ReplyInfo? ReplyTo { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public void MarkAsRead(Guid userId);
    public void AddReaction(Guid userId, string emoji);
    public void SoftDelete();
}
```

#### SignalR Hub

**ChatHub.cs**:
```csharp
[Authorize]
public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatHub> _logger;
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.GetUserId();
        
        // Get user's conversations and join their groups
        var conversations = await _mediator.Send(new GetUserConversationsQuery(userId));
        foreach (var conv in conversations)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conv.Id.ToString());
        }
        
        // Update online status
        await _mediator.Send(new UpdateOnlineStatusCommand(userId, true));
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User.GetUserId();
        await _mediator.Send(new UpdateOnlineStatusCommand(userId, false));
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendMessage(SendMessageRequest request)
    {
        var userId = Context.User.GetUserId();
        var command = new SendMessageCommand(
            userId,
            request.ConversationId,
            request.Content,
            request.Type
        );
        
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            // Broadcast to conversation group
            await Clients.Group(request.ConversationId)
                .SendAsync("ReceiveMessage", result.Value);
        }
    }
    
    public async Task StartTyping(string conversationId)
    {
        var userId = Context.User.GetUserId();
        var username = Context.User.GetUsername();
        
        await Clients.OthersInGroup(conversationId)
            .SendAsync("UserTyping", new { userId, username, conversationId });
    }
    
    public async Task StopTyping(string conversationId)
    {
        var userId = Context.User.GetUserId();
        
        await Clients.OthersInGroup(conversationId)
            .SendAsync("UserStoppedTyping", new { userId, conversationId });
    }
    
    public async Task MarkAsRead(string conversationId, string messageId)
    {
        var userId = Context.User.GetUserId();
        var command = new MarkMessageAsReadCommand(userId, conversationId, messageId);
        
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            await Clients.Group(conversationId)
                .SendAsync("MessageRead", new { userId, messageId });
        }
    }
}
```

#### API Controller

**ConversationsController.cs**:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ConversationsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var userId = User.GetUserId();
        var query = new GetUserConversationsQuery(userId, page, limit);
        var result = await _mediator.Send(query);
        
        return Ok(ApiResponse<PagedResult<ConversationDto>>.Success(result.Value));
    }
    
    [HttpPost("direct")]
    public async Task<IActionResult> CreateDirectConversation([FromBody] CreateDirectConversationRequest request)
    {
        var userId = User.GetUserId();
        var command = new CreateDirectConversationCommand(userId, request.ParticipantId);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(ApiResponse<ConversationDto>.Success(result.Value))
            : BadRequest(ApiResponse.Failure(result.Error));
    }
    
    [HttpPost("group")]
    public async Task<IActionResult> CreateGroupConversation([FromBody] CreateGroupConversationRequest request)
    {
        var userId = User.GetUserId();
        var command = new CreateGroupConversationCommand(userId, request.Name, request.ParticipantIds);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(ApiResponse<ConversationDto>.Success(result.Value))
            : BadRequest(ApiResponse.Failure(result.Error));
    }
    
    [HttpGet("{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(string conversationId, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        var query = new GetMessagesQuery(conversationId, page, limit);
        var result = await _mediator.Send(query);
        
        return Ok(ApiResponse<PagedResult<MessageDto>>.Success(result.Value));
    }
}
```

---

### 4. PostFeed Service (MongoDB)

#### Domain Entities

**Post.cs**:
```csharp
public class Post
{
    public ObjectId Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Username { get; private set; }
    public string Content { get; private set; }
    public List<string> MediaUrls { get; private set; }
    public List<string> Hashtags { get; private set; }
    public List<Mention> Mentions { get; private set; }
    public PostVisibility Visibility { get; private set; }
    public int LikesCount { get; private set; }
    public int CommentsCount { get; private set; }
    public int SharesCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public void IncrementLikes();
    public void IncrementComments();
    public void UpdateContent(string newContent);
}
```

**Comment.cs**:
```csharp
public class Comment
{
    public ObjectId Id { get; private set; }
    public ObjectId PostId { get; private set; }
    public Guid UserId { get; private set; }
    public string Content { get; private set; }
    public ObjectId? ParentCommentId { get; private set; }
    public int LikesCount { get; private set; }
    public int RepliesCount { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
```

#### Application Commands

**CreatePostCommand**:
```csharp
public record CreatePostCommand(
    Guid UserId,
    string Content,
    List<string>? MediaUrls,
    PostVisibility Visibility
) : IRequest<Result<PostDto>>;
```

**CreatePostCommandHandler**:
```csharp
public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Result<PostDto>>
{
    private readonly IPostRepository _repository;
    private readonly IUserProfileService _userProfileService;
    private readonly IEventBus _eventBus;
    
    public async Task<Result<PostDto>> Handle(CreatePostCommand request, CancellationToken ct)
    {
        // Get user info
        var userInfo = await _userProfileService.GetUserInfoAsync(request.UserId, ct);
        
        // Extract hashtags from content
        var hashtags = ExtractHashtags(request.Content);
        
        // Create post
        var post = Post.Create(
            request.UserId,
            userInfo.Username,
            request.Content,
            request.MediaUrls ?? new List<string>(),
            hashtags,
            request.Visibility
        );
        
        await _repository.AddAsync(post, ct);
        
        // Publish event
        await _eventBus.PublishAsync(new PostCreatedEvent(post.Id, post.UserId), ct);
        
        // Update hashtag counts
        await UpdateHashtagCounts(hashtags, ct);
        
        return Result<PostDto>.Success(_mapper.Map<PostDto>(post));
    }
}
```

#### API Controller

**PostsController.cs**:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        var userId = User.GetUserId();
        var command = new CreatePostCommand(userId, request.Content, request.MediaUrls, request.Visibility);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(ApiResponse<PostDto>.Success(result.Value))
            : BadRequest(ApiResponse.Failure(result.Error));
    }
    
    [HttpGet("{postId}")]
    public async Task<IActionResult> GetPost(string postId)
    {
        var query = new GetPostByIdQuery(postId);
        var result = await _mediator.Send(query);
        
        return result.IsSuccess 
            ? Ok(ApiResponse<PostDto>.Success(result.Value))
            : NotFound(ApiResponse.Failure(result.Error));
    }
    
    [HttpPost("{postId}/like")]
    public async Task<IActionResult> LikePost(string postId)
    {
        var userId = User.GetUserId();
        var command = new LikePostCommand(userId, postId);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(ApiResponse.Success("Post liked successfully"))
            : BadRequest(ApiResponse.Failure(result.Error));
    }
    
    [HttpPost("{postId}/comments")]
    public async Task<IActionResult> AddComment(string postId, [FromBody] AddCommentRequest request)
    {
        var userId = User.GetUserId();
        var command = new AddCommentCommand(userId, postId, request.Content);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(ApiResponse<CommentDto>.Success(result.Value))
            : BadRequest(ApiResponse.Failure(result.Error));
    }
    
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var userId = User.GetUserId();
        var query = new GetPersonalizedFeedQuery(userId, page, limit);
        var result = await _mediator.Send(query);
        
        return Ok(ApiResponse<PagedResult<PostDto>>.Success(result.Value));
    }
    
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingPosts([FromQuery] string timeframe = "24h")
    {
        var query = new GetTrendingPostsQuery(timeframe);
        var result = await _mediator.Send(query);
        
        return Ok(ApiResponse<List<PostDto>>.Success(result.Value));
    }
}
```

---

### 5. Media Service (MongoDB + Blob Storage)

#### Domain Entities

**MediaFile.cs**:
```csharp
public class MediaFile
{
    public ObjectId Id { get; private set; }
    public Guid UserId { get; private set; }
    public string FileName { get; private set; }
    public string OriginalFileName { get; private set; }
    public FileType Type { get; private set; } // Image, Video, Document
    public string MimeType { get; private set; }
    public long FileSize { get; private set; }
    public string Url { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public ProcessingStatus Status { get; private set; }
    public DateTime UploadedAt { get; private set; }
    
    public void MarkAsProcessed(string url, string? thumbnailUrl);
    public void MarkAsFailed(string error);
}
```

#### Infrastructure - Storage Service

**BlobStorageService.cs**:
```csharp
public class BlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _configuration;
    
    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient("media");
        await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);
        
        var blobClient = containerClient.GetBlobClient(fileName);
        
        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);
        
        return blobClient.Uri.ToString();
    }
    
    public async Task<Stream> DownloadAsync(string fileName, CancellationToken ct)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient("media");
        var blobClient = containerClient.GetBlobClient(fileName);
        
        var response = await blobClient.DownloadAsync(ct);
        return response.Value.Content;
    }
    
    public async Task DeleteAsync(string fileName, CancellationToken ct)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient("media");
        var blobClient = containerClient.GetBlobClient(fileName);
        
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
    }
}
```

#### Application Commands

**UploadFileCommand**:
```csharp
public record UploadFileCommand(
    Guid UserId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize
) : IRequest<Result<MediaFileDto>>;
```

**UploadFileCommandHandler**:
```csharp
public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<MediaFileDto>>
{
    private readonly IStorageService _storageService;
    private readonly IMediaFileRepository _repository;
    
    public async Task<Result<MediaFileDto>> Handle(UploadFileCommand request, CancellationToken ct)
    {
        // Validate file size and type
        if (request.FileSize > 10 * 1024 * 1024) // 10 MB
            return Result<MediaFileDto>.Failure("File size exceeds limit");
        
        // Generate unique filename
        var uniqueFileName = $"{request.UserId}/{Guid.NewGuid()}{Path.GetExtension(request.FileName)}";
        
        // Upload to blob storage
        var url = await _storageService.UploadAsync(
            request.FileStream,
            uniqueFileName,
            request.ContentType,
            ct
        );
        
        // Create media file entity
        var mediaFile = MediaFile.Create(
            request.UserId,
            uniqueFileName,
            request.FileName,
            DetermineFileType(request.ContentType),
            request.ContentType,
            request.FileSize,
            url
        );
        
        await _repository.AddAsync(mediaFile, ct);
        
        // Queue for thumbnail generation (if image/video)
        if (mediaFile.Type == FileType.Image || mediaFile.Type == FileType.Video)
        {
            await _eventBus.PublishAsync(new MediaUploadedEvent(mediaFile.Id), ct);
        }
        
        return Result<MediaFileDto>.Success(_mapper.Map<MediaFileDto>(mediaFile));
    }
}
```

#### API Controller

**MediaController.cs**:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class MediaController : ControllerBase
{
    [HttpPost("upload")]
    [RequestSizeLimit(10_485_760)] // 10 MB
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Failure("No file provided"));
        
        var userId = User.GetUserId();
        
        using var stream = file.OpenReadStream();
        var command = new UploadFileCommand(
            userId,
            stream,
            file.FileName,
            file.ContentType,
            file.Length
        );
        
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(ApiResponse<MediaFileDto>.Success(result.Value))
            : BadRequest(ApiResponse.Failure(result.Error));
    }
    
    [HttpGet("{mediaId}")]
    public async Task<IActionResult> GetMedia(string mediaId)
    {
        var query = new GetMediaFileQuery(mediaId);
        var result = await _mediator.Send(query);
        
        return result.IsSuccess 
            ? Ok(ApiResponse<MediaFileDto>.Success(result.Value))
            : NotFound(ApiResponse.Failure(result.Error));
    }
    
    [HttpDelete("{mediaId}")]
    public async Task<IActionResult> DeleteMedia(string mediaId)
    {
        var userId = User.GetUserId();
        var command = new DeleteMediaFileCommand(userId, mediaId);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(ApiResponse.Success("Media deleted successfully"))
            : BadRequest(ApiResponse.Failure(result.Error));
    }
}
```

---

### 6. Notification Service (MongoDB + SignalR)

#### Domain Entities

**Notification.cs**:
```csharp
public class Notification
{
    public ObjectId Id { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public Guid? ActorId { get; private set; }
    public string? ActorUsername { get; private set; }
    public string? RelatedEntityId { get; private set; }
    public string? ActionUrl { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    
    public void MarkAsRead();
}
```

#### SignalR Hub

**NotificationHub.cs**:
```csharp
[Authorize]
public class NotificationHub : Hub
{
    private readonly IMediator _mediator;
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        
        await base.OnConnectedAsync();
    }
    
    public async Task MarkAsRead(string notificationId)
    {
        var userId = Context.User.GetUserId();
        var command = new MarkNotificationAsReadCommand(userId, notificationId);
        
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            await Clients.Caller.SendAsync("NotificationRead", notificationId);
        }
    }
}
```

#### Application - Notification Service

**NotificationService.cs**:
```csharp
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IHubContext<NotificationHub> _hubContext;
    
    public async Task SendNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid? actorId = null,
        string? relatedEntityId = null,
        CancellationToken ct = default)
    {
        // Create notification
        var notification = Notification.Create(userId, type, title, message, actorId, relatedEntityId);
        
        await _repository.AddAsync(notification, ct);
        
        // Send real-time notification via SignalR
        await _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("ReceiveNotification", new
            {
                id = notification.Id.ToString(),
                type = notification.Type.ToString(),
                title = notification.Title,
                message = notification.Message,
                actorId = notification.ActorId,
                actionUrl = notification.ActionUrl,
                createdAt = notification.CreatedAt
            }, ct);
    }
}
```

#### API Controller

**NotificationsController.cs**:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool? isRead = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var userId = User.GetUserId();
        var query = new GetNotificationsQuery(userId, isRead, page, limit);
        var result = await _mediator.Send(query);
        
        return Ok(ApiResponse<PagedResult<NotificationDto>>.Success(result.Value));
    }
    
    [HttpGet("unread/count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.GetUserId();
        var query = new GetUnreadCountQuery(userId);
        var result = await _mediator.Send(query);
        
        return Ok(ApiResponse<int>.Success(result.Value));
    }
    
    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        var userId = User.GetUserId();
        var command = new MarkNotificationAsReadCommand(userId, notificationId);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(ApiResponse.Success("Notification marked as read"))
            : BadRequest(ApiResponse.Failure(result.Error));
    }
    
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.GetUserId();
        var command = new MarkAllNotificationsAsReadCommand(userId);
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(ApiResponse.Success($"{result.Value} notifications marked as read"))
            : BadRequest(ApiResponse.Failure(result.Error));
    }
}
```

---

## API Gateway Setup

### Ocelot Configuration

**ocelot.json** - Complete routing configuration:
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/v1/auth/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "localhost", "Port": 5001 }
      ],
      "UpstreamPathTemplate": "/api/v1/auth/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      },
      "RateLimitOptions": {
        "EnableRateLimiting": true,
        "Period": "1m",
        "Limit": 100
      }
    },
    {
      "DownstreamPathTemplate": "/api/v1/profiles/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "localhost", "Port": 5002 }
      ],
      "UpstreamPathTemplate": "/api/v1/profiles/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      },
      "QoSOptions": {
        "ExceptionsAllowedBeforeBreaking": 3,
        "DurationOfBreak": 30000,
        "TimeoutValue": 5000
      }
    },
    {
      "DownstreamPathTemplate": "/api/v1/conversations/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "localhost", "Port": 5003 }
      ],
      "UpstreamPathTemplate": "/api/v1/conversations/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "DownstreamPathTemplate": "/api/v1/posts/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "localhost", "Port": 5004 }
      ],
      "UpstreamPathTemplate": "/api/v1/posts/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      },
      "FileCacheOptions": {
        "TtlSeconds": 30
      }
    },
    {
      "DownstreamPathTemplate": "/api/v1/media/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "localhost", "Port": 5005 }
      ],
      "UpstreamPathTemplate": "/api/v1/media/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "DownstreamPathTemplate": "/api/v1/notifications/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "localhost", "Port": 5006 }
      ],
      "UpstreamPathTemplate": "/api/v1/notifications/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://api.postbook.com",
    "RateLimitOptions": {
      "DisableRateLimitHeaders": false,
      "QuotaExceededMessage": "Rate limit exceeded",
      "HttpStatusCode": 429
    },
    "LoadBalancerOptions": {
      "Type": "RoundRobin"
    }
  }
}
```

**Program.cs** for API Gateway:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
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

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();
```

---

## Authentication & Authorization

### JWT Token Generation

**IJwtTokenGenerator.cs** - Interface:
```csharp
public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}
```

**JwtTokenGenerator.cs** - Implementation:
```csharp
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim("username", user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
```

### JWT Configuration in appsettings.json

```json
{
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-here-minimum-32-characters",
    "Issuer": "PostBookAPI",
    "Audience": "PostBookClient",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Authorization Policies

**Program.cs** - Add authorization policies:
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("Admin"));
        
    options.AddPolicy("RequireVerifiedEmail", policy =>
        policy.RequireClaim("email_verified", "true"));
        
    options.AddPolicy("RequirePremium", policy =>
        policy.RequireClaim("premium", "true"));
});
```

### Custom ClaimsPrincipal Extensions

**ClaimsPrincipalExtensions.cs**:
```csharp
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                          ?? user.FindFirst(JwtRegisteredClaimNames.Sub);
                          
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID claim");
            
        return userId;
    }
    
    public static string GetUsername(this ClaimsPrincipal user)
    {
        return user.FindFirst("username")?.Value 
               ?? throw new UnauthorizedAccessException("Username claim not found");
    }
    
    public static string GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value 
               ?? throw new UnauthorizedAccessException("Email claim not found");
    }
}
```

---

## CQRS Pattern with MediatR

### Command/Query Separation

**Commands** - Modify state:
```csharp
public record CreateUserCommand(...) : IRequest<Result<CreateUserResponse>>;
public record UpdateUserCommand(...) : IRequest<Result<Unit>>;
public record DeleteUserCommand(...) : IRequest<Result<Unit>>;
```

**Queries** - Read state:
```csharp
public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;
public record GetUserListQuery(int Page, int Limit) : IRequest<Result<PagedResult<UserDto>>>;
```

### MediatR Pipeline Behaviors

**ValidationBehavior.cs** - Validate all requests:
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();
            
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
            
        if (failures.Any())
            throw new ValidationException(failures);
            
        return await next();
    }
}
```

**LoggingBehavior.cs** - Log all requests:
```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}: {@Request}", requestName, request);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            stopwatch.Stop();
            
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
                
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

**CachingBehavior.cs** - Cache query results:
```csharp
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheableQuery
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;
        
        // Try to get from cache
        var cachedResponse = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedResponse))
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<TResponse>(cachedResponse);
        }
        
        _logger.LogInformation("Cache miss for {CacheKey}", cacheKey);
        
        // Execute query
        var response = await next();
        
        // Cache the response
        var serializedResponse = JsonSerializer.Serialize(response);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(request.CacheExpirationMinutes)
        };
        
        await _cache.SetStringAsync(cacheKey, serializedResponse, options, cancellationToken);
        
        return response;
    }
}
```

### MediatR Registration

**DependencyInjection.cs**:
```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    // Register MediatR
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    });
    
    // Register pipeline behaviors (order matters!)
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
    
    // Register FluentValidation validators
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    
    // Register AutoMapper
    services.AddAutoMapper(Assembly.GetExecutingAssembly());
    
    return services;
}
```

---

## Repository Pattern

### Generic Repository Interface

**IRepository.cs**:
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
```

### EF Core Repository Implementation

**Repository.cs** (for PostgreSQL with EF Core):
```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync(new[] { id }, ct);
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet.ToListAsync(ct);
    }
    
    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        return entity;
    }
    
    public virtual Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
    
    public virtual Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
```

### MongoDB Repository Implementation

**MongoRepository.cs**:
```csharp
public class MongoRepository<T> : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;
    
    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }
    
    public virtual async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id.ToString()));
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _collection.Find(_ => true).ToListAsync(ct);
    }
    
    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: ct);
        return entity;
    }
    
    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        var idProperty = typeof(T).GetProperty("Id");
        var id = idProperty?.GetValue(entity);
        var filter = Builders<T>.Filter.Eq("_id", id);
        await _collection.ReplaceOneAsync(filter, entity, cancellationToken: ct);
    }
    
    public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        var idProperty = typeof(T).GetProperty("Id");
        var id = idProperty?.GetValue(entity);
        var filter = Builders<T>.Filter.Eq("_id", id);
        await _collection.DeleteOneAsync(filter, ct);
    }
}
```

### Unit of Work Pattern

**IUnitOfWork.cs**:
```csharp
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**UnitOfWork implementation** (EF Core DbContext already implements this):
```csharp
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

---

## Cross-Cutting Concerns

### Common Models (PostBook.Common)

**Result.cs** - Result pattern for error handling:
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    
    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

**PagedResult.cs** - Pagination wrapper:
```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
```

**ApiResponse.cs** - Standardized API response:
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; }
    
    public static ApiResponse<T> Success(T data, string message = null) =>
        new() { Success = true, Data = data, Message = message };
        
    public static ApiResponse<T> Failure(string error) =>
        new() { Success = false, Errors = new List<string> { error } };
}
```

---

## Service Communication

### Event Bus (RabbitMQ)

**IEventBus.cs**:
```csharp
public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IntegrationEvent;
        
    void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;
}
```

**IntegrationEvent.cs** - Base event class:
```csharp
public abstract class IntegrationEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    
    protected IntegrationEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}
```

**UserCreatedEvent.cs** - Example event:
```csharp
public class UserCreatedEvent : IntegrationEvent
{
    public Guid UserId { get; }
    public string Username { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    
    public UserCreatedEvent(Guid userId, string username, string email, string firstName, string lastName)
    {
        UserId = userId;
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
}
```

**RabbitMqEventBus.cs** - Implementation:
```csharp
public class RabbitMqEventBus : IEventBus
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IntegrationEvent
    {
        var eventName = @event.GetType().Name;
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);
        
        _channel.BasicPublish(
            exchange: "postbook_events",
            routingKey: eventName,
            basicProperties: null,
            body: body
        );
    }
    
    public void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name;
        
        _channel.QueueBind(
            queue: "postbook_queue",
            exchange: "postbook_events",
            routingKey: eventName
        );
        
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var @event = JsonSerializer.Deserialize<T>(message);
            
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<TH>();
            await handler.HandleAsync(@event);
        };
        
        _channel.BasicConsume(queue: "postbook_queue", autoAck: true, consumer: consumer);
    }
}
```

**IIntegrationEventHandler.cs**:
```csharp
public interface IIntegrationEventHandler<in T> where T : IntegrationEvent
{
    Task HandleAsync(T @event);
}
```

**UserCreatedEventHandler.cs** - Example handler in UserProfile service:
```csharp
public class UserCreatedEventHandler : IIntegrationEventHandler<UserCreatedEvent>
{
    private readonly IUserProfileRepository _repository;
    private readonly ILogger<UserCreatedEventHandler> _logger;
    
    public async Task HandleAsync(UserCreatedEvent @event)
    {
        _logger.LogInformation("Creating profile for user {UserId}", @event.UserId);
        
        // Create initial user profile
        var profile = UserProfile.Create(
            @event.UserId,
            @event.Username,
            $"{@event.FirstName} {@event.LastName}",
            @event.Email
        );
        
        await _repository.AddAsync(profile);
        
        _logger.LogInformation("Profile created successfully for user {UserId}", @event.UserId);
    }
}
```

---

## Error Handling

### Global Exception Handling Middleware

**ErrorHandlingMiddleware.cs**:
```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            ValidationException validationException => new ApiResponse
            {
                Success = false,
                Errors = validationException.Errors.Select(e => e.ErrorMessage).ToList()
            },
            NotFoundException notFoundException => new ApiResponse
            {
                Success = false,
                Errors = new List<string> { notFoundException.Message }
            },
            UnauthorizedAccessException _ => new ApiResponse
            {
                Success = false,
                Errors = new List<string> { "Unauthorized access" }
            },
            _ => new ApiResponse
            {
                Success = false,
                Errors = new List<string> { "An error occurred processing your request" }
            }
        };
        
        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        
        return context.Response.WriteAsJsonAsync(response);
    }
}
```

### Custom Exceptions

**NotFoundException.cs**:
```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"{name} with key '{key}' was not found")
    {
    }
}
```

**DomainException.cs**:
```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
```

---

## Logging & Monitoring

### Serilog Configuration

**Program.cs** - Configure Serilog:
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Application", "PostBook.Auth")
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File(
        new CompactJsonFormatter(),
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"])
    .CreateLogger();

builder.Host.UseSerilog();
```

**appsettings.json** - Serilog settings:
```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Seq" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  "Seq": {
    "ServerUrl": "http://localhost:5341"
  }
}
```

### Request Logging Middleware

**RequestLoggingMiddleware.cs**:
```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid();
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["Method"] = context.Request.Method,
            ["Path"] = context.Request.Path,
            ["UserId"] = context.User?.GetUserId()
        }))
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
                stopwatch.Stop();
                
                _logger.LogInformation(
                    "Request {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds
                );
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Request {Method} {Path} failed after {ElapsedMilliseconds}ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds
                );
                throw;
            }
        }
    }
}
```

### Application Insights Integration

**Program.cs** - Add Application Insights:
```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
});
```

---

## Testing Strategy

### Unit Testing

**CreateUserCommandHandlerTests.cs**:
```csharp
public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateUserCommandHandler _handler;
    
    public CreateUserCommandHandlerTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventBusMock = new Mock<IEventBus>();
        
        _handler = new CreateUserCommandHandler(
            _repositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object,
            _eventBusMock.Object,
            Mock.Of<ILogger<CreateUserCommandHandler>>()
        );
    }
    
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateUserCommand(
            "testuser",
            "test@example.com",
            "Password123!",
            "Test",
            "User"
        );
        
        _repositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<Email>(), default))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>(), default))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");
        
        // Act
        var result = await _handler.Handle(command, default);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("testuser");
        
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<UserCreatedEvent>(), default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_EmailExists_ReturnsFailure()
    {
        // Arrange
        var command = new CreateUserCommand("testuser", "test@example.com", "Password123!", "Test", "User");
        
        _repositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<Email>(), default))
            .ReturnsAsync(true);
        
        // Act
        var result = await _handler.Handle(command, default);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email already exists");
    }
}
```

### Integration Testing

**AuthApiTests.cs**:
```csharp
public class AuthApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public AuthApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Register_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new
        {
            username = "testuser",
            email = "test@example.com",
            password = "Password123!",
            firstName = "Test",
            lastName = "User"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<CreateUserResponse>>();
        content.Success.Should().BeTrue();
        content.Data.Username.Should().Be("testuser");
    }
}
```

---

## Deployment Guide

### Docker Configuration

**Dockerfile** (per service):
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PostBook.Auth.API/PostBook.Auth.API.csproj", "PostBook.Auth.API/"]
COPY ["PostBook.Auth.Application/PostBook.Auth.Application.csproj", "PostBook.Auth.Application/"]
COPY ["PostBook.Auth.Infrastructure/PostBook.Auth.Infrastructure.csproj", "PostBook.Auth.Infrastructure/"]
COPY ["PostBook.Auth.Domain/PostBook.Auth.Domain.csproj", "PostBook.Auth.Domain/"]

RUN dotnet restore "PostBook.Auth.API/PostBook.Auth.API.csproj"

COPY . .
WORKDIR "/src/PostBook.Auth.API"
RUN dotnet build "PostBook.Auth.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PostBook.Auth.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5001
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PostBook.Auth.API.dll"]
```

**docker-compose.yml**:
```yaml
version: '3.8'

services:
  # API Gateway
  apigateway:
    build:
      context: .
      dockerfile: src/ApiGateway/PostBook.ApiGateway/Dockerfile
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      - auth
      - userprofile
      - chat
      - postfeed
      - media
      - notification

  # Auth Service
  auth:
    build:
      context: .
      dockerfile: src/Services/Auth/PostBook.Auth.API/Dockerfile
    ports:
      - "5001:5001"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__AuthDatabase=Host=postgres;Database=AuthServiceDB;Username=postgres;Password=postgres
      - ConnectionStrings__Redis=redis:6379
    depends_on:
      - postgres
      - redis

  # UserProfile Service
  userprofile:
    build:
      context: .
      dockerfile: src/Services/UserProfile/PostBook.UserProfile.API/Dockerfile
    ports:
      - "5002:5002"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__MongoDB=mongodb://mongo:27017/UserProfileServiceDB
      - ConnectionStrings__Redis=redis:6379
    depends_on:
      - mongo
      - redis

  # Other services...

  # Databases
  postgres:
    image: postgres:14
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_PASSWORD=postgres
    volumes:
      - postgres_data:/var/lib/postgresql/data

  mongo:
    image: mongo:6
    ports:
      - "27017:27017"
    volumes:
      - mongo_data:/data/db

  redis:
    image: redis:7
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest

  seq:
    image: datalust/seq
    ports:
      - "5341:80"
    environment:
      - ACCEPT_EULA=Y

volumes:
  postgres_data:
  mongo_data:
  redis_data:
```

### Environment Configuration

**appsettings.Production.json**:
```json
{
  "ConnectionStrings": {
    "AuthDatabase": "${AUTH_DB_CONNECTION}",
    "MongoDB": "${MONGO_CONNECTION}",
    "Redis": "${REDIS_CONNECTION}"
  },
  "Jwt": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "PostBookAPI",
    "Audience": "PostBookClient"
  },
  "Seq": {
    "ServerUrl": "${SEQ_URL}"
  },
  "ApplicationInsights": {
    "ConnectionString": "${APPINSIGHTS_CONNECTION}"
  }
}
```

---

## Implementation Checklist

### Phase 1: Foundation

- [ ] Create solution structure with all projects
- [ ] Set up Git repository
- [ ] Configure NuGet packages for all projects
- [ ] Implement Common/BuildingBlocks projects
- [ ] Set up CI/CD pipeline

### Phase 2: Auth Service

- [ ] Implement Domain layer (User, Email, PhoneNumber)
- [ ] Implement Application layer (Commands, Queries, Validators)
- [ ] Configure EF Core and PostgreSQL
- [ ] Implement JWT token generation
- [ ] Implement password hashing with BCrypt
- [ ] Create Auth API controllers
- [ ] Write unit tests
- [ ] Write integration tests

### Phase 3: API Gateway

- [ ] Create API Gateway project
- [ ] Configure Ocelot routing
- [ ] Set up JWT authentication
- [ ] Configure rate limiting
- [ ] Configure circuit breaker (QoS)
- [ ] Test all routes

### Phase 4: UserProfile Service

- [ ] Implement Domain layer
- [ ] Implement Application layer
- [ ] Configure MongoDB
- [ ] Implement repositories
- [ ] Create API controllers
- [ ] Implement event handlers (UserCreatedEvent)
- [ ] Write tests

### Phase 5: Chat Service

- [ ] Implement Domain layer
- [ ] Implement Application layer
- [ ] Configure MongoDB
- [ ] Implement SignalR ChatHub
- [ ] Create API controllers
- [ ] Test real-time messaging
- [ ] Write tests

### Phase 6: PostFeed Service

- [ ] Implement Domain layer
- [ ] Implement Application layer
- [ ] Configure MongoDB with text search
- [ ] Implement feed algorithms
- [ ] Create API controllers
- [ ] Write tests

### Phase 7: Media Service

- [ ] Implement Domain layer
- [ ] Implement Application layer
- [ ] Configure MongoDB
- [ ] Integrate Azure Blob Storage or AWS S3
- [ ] Implement file upload/download
- [ ] Implement thumbnail generation
- [ ] Write tests

### Phase 8: Notification Service

- [ ] Implement Domain layer
- [ ] Implement Application layer
- [ ] Configure MongoDB with TTL
- [ ] Implement SignalR NotificationHub
- [ ] Implement push notifications
- [ ] Create API controllers
- [ ] Write tests

### Phase 9: Event Bus

- [ ] Set up RabbitMQ or Azure Service Bus
- [ ] Implement Event Bus abstraction
- [ ] Define integration events
- [ ] Implement event handlers in all services
- [ ] Test event publishing and consumption

### Phase 10: Cross-Cutting Concerns

- [ ] Implement global error handling
- [ ] Configure Serilog in all services
- [ ] Set up Application Insights
- [ ] Implement request logging middleware
- [ ] Configure Redis caching
- [ ] Implement health checks

### Phase 11: Testing

- [ ] Write unit tests for all services (80%+ coverage)
- [ ] Write integration tests for API endpoints
- [ ] Write end-to-end tests for critical flows
- [ ] Performance testing with load testing tools

### Phase 12: Deployment

- [ ] Create Dockerfiles for all services
- [ ] Create docker-compose.yml
- [ ] Set up Kubernetes manifests (if using K8s)
- [ ] Configure environment variables
- [ ] Set up monitoring dashboards
- [ ] Configure alerts
- [ ] Deploy to production

---

**End of Backend Implementation Guide**

This comprehensive guide provides complete instructions for implementing the PostBook backend using Clean Architecture, CQRS, MediatR, and Repository patterns. Any agent following these instructions should be able to create a fully functional, scalable, and maintainable microservices application.
