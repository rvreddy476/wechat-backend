# Solution Folder Structure

## Overview

The WeChat backend solution is organized with each microservice in its own solution folder, following Clean Architecture with CQRS pattern.

## Folder Structure

```
wechat-backend/
├── src/
│   ├── Auth/                          ✅ COMPLETED - Clean Architecture with CQRS
│   │   ├── AuthService.Domain/
│   │   ├── AuthService.Application/
│   │   ├── AuthService.Infrastructure/
│   │   └── AuthService.Api/
│   │
│   ├── Chat/                          🔄 TODO - Convert to Clean Architecture
│   │   ├── ChatService.Domain/
│   │   ├── ChatService.Application/
│   │   ├── ChatService.Infrastructure/
│   │   └── ChatService.Api/
│   │
│   ├── UserProfile/                   🔄 TODO - Convert to Clean Architecture
│   │   ├── UserProfileService.Domain/
│   │   ├── UserProfileService.Application/
│   │   ├── UserProfileService.Infrastructure/
│   │   └── UserProfileService.Api/
│   │
│   ├── PostFeed/                      🔄 TODO - Convert to Clean Architecture
│   │   ├── PostFeedService.Domain/
│   │   ├── PostFeedService.Application/
│   │   ├── PostFeedService.Infrastructure/
│   │   └── PostFeedService.Api/
│   │
│   ├── Media/                         🔄 TODO - Convert to Clean Architecture
│   │   ├── MediaService.Domain/
│   │   ├── MediaService.Application/
│   │   ├── MediaService.Infrastructure/
│   │   └── MediaService.Api/
│   │
│   ├── Video/                         🔄 TODO - Convert to Clean Architecture
│   │   ├── VideoService.Domain/
│   │   ├── VideoService.Application/
│   │   ├── VideoService.Infrastructure/
│   │   └── VideoService.Api/
│   │
│   ├── Notification/                  🔄 TODO - Convert to Clean Architecture
│   │   ├── NotificationService.Domain/
│   │   ├── NotificationService.Application/
│   │   ├── NotificationService.Infrastructure/
│   │   └── NotificationService.Api/
│   │
│   ├── Realtime/                      🔄 TODO - Convert to Clean Architecture
│   │   ├── Realtime.Domain/
│   │   ├── Realtime.Application/
│   │   ├── Realtime.Infrastructure/
│   │   └── Realtime.Api/
│   │
│   ├── Gateway/                       🔄 Existing - API Gateway
│   │   └── Gateway.Api/
│   │
│   └── Shared/                        ✅ Shared libraries (cross-cutting concerns)
│       ├── Shared.Domain/
│       ├── Shared.Contracts/
│       └── Shared.Infrastructure/
│
├── Database/                          Database scripts organized by service
│   ├── AuthService/
│   ├── ChatService/
│   ├── UserProfileService/
│   └── ...
│
└── docs/                              Documentation
    ├── CLEAN_ARCHITECTURE_MIGRATION.md
    ├── AUTH_README.md
    ├── FRIEND_REQUEST_README.md
    └── CHAT_README.md
```

## Clean Architecture Layers (Per Service)

Each microservice follows the same Clean Architecture structure:

### 1. **Domain Layer** (`[Service].Domain/`)
**Purpose**: Core business entities, enums, constants, and interfaces

**Contains**:
- `Entities/` - Domain entities
- `Enums/` - Type-safe enumerations
- `Constants/` - All hardcoded values (messages, limits, etc.)
- `Interfaces/` - Repository and service interfaces

**Dependencies**: None (pure domain logic)

**Example** (Auth):
```
AuthService.Domain/
├── Entities/
│   ├── User.cs
│   ├── VerificationCode.cs
│   ├── RefreshToken.cs
│   └── PasswordResetToken.cs
├── Enums/
│   ├── GenderType.cs
│   └── VerificationType.cs
├── Constants/
│   ├── ValidationMessages.cs
│   ├── ErrorMessages.cs
│   ├── SuccessMessages.cs
│   ├── EmailConstants.cs
│   └── AuthConstants.cs
└── Interfaces/
    ├── IAuthRepository.cs
    ├── IVerificationRepository.cs
    ├── IEmailService.cs
    └── ISmsService.cs
```

### 2. **Application Layer** (`[Service].Application/`)
**Purpose**: Business logic, CQRS commands/queries, validators

**Contains**:
- `Commands/` - Write operations (Create, Update, Delete)
- `Queries/` - Read operations (Get, List, Search)
- `Validators/` - FluentValidation validators
- `Behaviors/` - MediatR pipeline behaviors
- `Common/` - Shared DTOs and utilities

**Dependencies**: Domain layer only

**Example** (Auth):
```
AuthService.Application/
├── Commands/
│   ├── Register/
│   │   ├── RegisterCommand.cs
│   │   ├── RegisterCommandHandler.cs
│   │   └── RegisterCommandValidator.cs
│   ├── Login/
│   │   ├── LoginCommand.cs
│   │   ├── LoginCommandHandler.cs
│   │   └── LoginCommandValidator.cs
│   ├── VerifyCode/
│   └── SendVerificationCode/
├── Queries/
│   └── GetUser/
├── Behaviors/
│   └── ValidationBehavior.cs (auto-validates all requests)
├── Common/
│   ├── Result.cs
│   └── AuthResponse.cs
└── DependencyInjection.cs
```

### 3. **Infrastructure Layer** (`[Service].Infrastructure/`)
**Purpose**: External dependencies (database, email, SMS, etc.)

**Contains**:
- `Persistence/` - Repository implementations
- `Services/` - External service implementations

**Dependencies**: Domain and Application layers

**Example** (Auth):
```
AuthService.Infrastructure/
├── Persistence/
│   ├── AuthRepository.cs
│   └── VerificationRepository.cs
├── Services/
│   ├── EmailService.cs (SendGrid)
│   └── SmsService.cs (Twilio)
└── DependencyInjection.cs
```

### 4. **API Layer** (`[Service].Api/`)
**Purpose**: HTTP endpoints, request/response mapping

**Contains**:
- `Controllers/` - Thin controllers using MediatR
- `Program.cs` - Startup configuration

**Dependencies**: All layers (but controllers only call MediatR)

**Example** (Auth):
```
AuthService.Api/
├── Controllers/
│   └── v2/
│       └── AuthController.cs (MediatR-based)
├── Program.cs (DI configuration)
└── appsettings.json
```

## Benefits of This Structure

### 1. **Clear Separation**
- Each service is self-contained in its own folder
- Easy to navigate and understand service boundaries
- Clear ownership and responsibilities

### 2. **Consistency**
- All services follow the same Clean Architecture pattern
- Same folder structure across all services
- Easy for developers to switch between services

### 3. **Independent Deployment**
- Each service can be deployed independently
- Docker containers per service
- Microservices best practices

### 4. **Scalability**
- Easy to add new services
- Easy to add new features to existing services
- Clear patterns to follow

### 5. **Testability**
- Each layer can be tested independently
- Mock interfaces for unit tests
- Integration tests per service

## Shared Libraries

The `Shared/` folder contains cross-cutting concerns used by all services:

```
Shared/
├── Shared.Domain/           - Common domain logic
│   └── Common/
│       └── Result.cs
├── Shared.Contracts/        - API contracts (DTOs)
│   ├── Auth/
│   ├── Chat/
│   └── Common/
└── Shared.Infrastructure/   - Common infrastructure
    ├── Authentication/      (JWT services)
    └── Redis/              (Caching)
```

## Project References

Each layer references layers below it:

```
API → Infrastructure → Application → Domain
                    ↓
                  Shared
```

**Example**:
- `AuthService.Api` references: Domain, Application, Infrastructure, Shared.*
- `AuthService.Infrastructure` references: Domain, Application
- `AuthService.Application` references: Domain
- `AuthService.Domain` references: None (pure domain)

## Migration Status

### ✅ Completed Services

1. **Auth** - Full Clean Architecture with CQRS
   - 4 projects (Domain, Application, Infrastructure, Api)
   - MediatR for CQRS
   - FluentValidation for all validations
   - All constants extracted (no hardcoded values)

### 🔄 Services to Migrate

The following services need to be converted to Clean Architecture:

1. **Chat** - Conversation and messaging
2. **UserProfile** - User profiles and friend requests
3. **PostFeed** - Posts, likes, comments
4. **Media** - Image/video upload and storage
5. **Video** - Video processing and streaming
6. **Notification** - Push notifications
7. **Realtime** - SignalR real-time communication

## Development Workflow

### Adding a New Feature to Auth Service

1. **Create Command** in `AuthService.Application/Commands/[FeatureName]/`
   ```csharp
   public class MyCommand : IRequest<Result<MyResponse>> { }
   ```

2. **Create Validator** in same folder
   ```csharp
   public class MyCommandValidator : AbstractValidator<MyCommand> { }
   ```

3. **Create Handler** in same folder
   ```csharp
   public class MyCommandHandler : IRequestHandler<MyCommand, Result<MyResponse>> { }
   ```

4. **Add Controller Endpoint** in `AuthService.Api/Controllers/v2/AuthController.cs`
   ```csharp
   [HttpPost("my-endpoint")]
   public async Task<ActionResult> MyEndpoint([FromBody] MyRequest request)
   {
       var command = new MyCommand { ... };
       var result = await _mediator.Send(command);
       return Ok(result);
   }
   ```

That's it! Validation happens automatically via `ValidationBehavior`.

### Adding a New Service

1. Create folder: `src/NewService/`
2. Create 4 projects: Domain, Application, Infrastructure, Api
3. Copy structure from `src/Auth/`
4. Implement domain entities and interfaces
5. Implement commands/queries with handlers
6. Implement repositories and services
7. Add controllers

## Naming Conventions

- **Folders**: PascalCase (e.g., `Auth`, `Chat`, `UserProfile`)
- **Projects**: `[ServiceName].[LayerName]` (e.g., `AuthService.Domain`)
- **Classes**: PascalCase (e.g., `RegisterCommand`, `UserRepository`)
- **Interfaces**: `I` prefix (e.g., `IAuthRepository`)
- **Constants**: PascalCase in static classes (e.g., `ValidationMessages.EmailRequired`)
- **Enums**: PascalCase (e.g., `GenderType`)

## Docker Deployment

Each service has its own Dockerfile:

```bash
# Build Auth service
docker build -f src/Auth/AuthService.Api/Dockerfile -t wechat-auth .

# Build Chat service
docker build -f src/Chat/ChatService.Api/Dockerfile -t wechat-chat .
```

## Summary

This folder structure provides:
- ✅ Clear service boundaries
- ✅ Consistent architecture across services
- ✅ Easy to navigate and maintain
- ✅ Scalable and testable
- ✅ Following microservices best practices
- ✅ Clean Architecture with CQRS pattern
- ✅ No hardcoded values (all in Constants)
- ✅ Automatic validation with FluentValidation

**Next Steps**: Convert remaining services (Chat, UserProfile, etc.) to this structure.
