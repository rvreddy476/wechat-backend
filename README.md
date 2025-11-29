# WeChat Backend - Clean Architecture Microservices

A comprehensive social media backend built with .NET 8, Clean Architecture, CQRS, and microservices architecture.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with **CQRS** pattern using **MediatR**.

### Microservices

1. **Auth Service** - User authentication, registration, JWT tokens
2. **Chat Service** - Real-time messaging, conversations
3. **UserProfile Service** - User profiles, friends, followers
4. **PostFeed Service** - Social media posts, likes, comments
5. **Media Service** - Image/file uploads, storage
6. **Notification Service** - Push notifications, email/SMS
7. **Gateway** - API Gateway (Ocelot)

### Clean Architecture Layers

Each microservice follows 4-layer Clean Architecture:

```
ServiceName/
├── ServiceName.Domain/          # Entities, Value Objects, Domain Events
├── ServiceName.Application/     # Use Cases (CQRS Commands/Queries)
├── ServiceName.Infrastructure/  # Data Access, External Services
└── ServiceName.Api/            # REST API Controllers, Middleware
```

### Shared Libraries

```
Shared/
├── Shared.Domain/          # Common domain logic (Result, Entity, ValueObjects)
├── Shared.Contracts/       # DTOs, Interfaces
└── Shared.Infrastructure/  # Common infrastructure (JWT, MongoDB, Redis)
```

## 🛠️ Technology Stack

- **.NET 8.0** - Latest LTS framework
- **PostgreSQL** - Auth service database
- **MongoDB** - All other services
- **Redis** - Caching and SignalR backplane
- **MediatR** - CQRS implementation
- **FluentValidation** - Request validation
- **Serilog** - Structured logging
- **Docker** - Containerization
- **SignalR** - Real-time communication

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- PostgreSQL (or use Docker)
- MongoDB (or use Docker)
- Redis (or use Docker)

### Run with Docker Compose

```bash
docker-compose up -d
```

### Run Locally

```bash
# Restore dependencies
dotnet restore

# Run a specific service
cd src/Auth/Auth.Api
dotnet run

# Run all services (use multiple terminals)
```

## 📊 Database

- **Auth Service**: PostgreSQL (relational)
- **Other Services**: MongoDB (document-based)

## 🔐 Authentication

All services use JWT Bearer authentication with shared secret. Tokens are issued by Auth Service.

## 📡 API Endpoints

### Auth Service (Port 5001)
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh token

### Chat Service (Port 5004)
- `POST /api/messages` - Send message
- `GET /api/messages` - Get conversation messages
- `GET /api/conversations` - Get user conversations

### UserProfile Service (Port 5002)
- `GET /api/profile/{userId}` - Get user profile
- `PUT /api/profile` - Update profile
- `POST /api/profile/avatar` - Upload avatar

### PostFeed Service (Port 5003)
- `POST /api/posts` - Create post
- `GET /api/feed` - Get user feed
- `POST /api/posts/{id}/like` - Like post
- `POST /api/posts/{id}/comment` - Comment on post

## 🐳 Docker Services

| Service | Port | Database |
|---------|------|----------|
| Gateway | 5000 | - |
| Auth | 5001 | PostgreSQL |
| UserProfile | 5002 | MongoDB |
| PostFeed | 5003 | MongoDB |
| Chat | 5004 | MongoDB |
| Media | 5005 | MongoDB |
| Notification | 5007 | MongoDB |
| PostgreSQL | 5432 | - |
| MongoDB | 27017 | - |
| Redis | 6379 | - |

## 📝 Project Structure

```
wechat-backend/
├── src/
│   ├── Shared/                 # Shared libraries
│   │   ├── Shared.Domain/
│   │   ├── Shared.Contracts/
│   │   └── Shared.Infrastructure/
│   ├── Auth/                   # Auth microservice
│   ├── Chat/                   # Chat microservice
│   ├── UserProfile/            # UserProfile microservice
│   ├── PostFeed/               # PostFeed microservice
│   ├── Media/                  # Media microservice
│   ├── Notification/           # Notification microservice
│   └── Gateway/                # API Gateway
├── docker-compose.yml
└── README.md
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific service tests
cd tests/Auth.Tests
dotnet test
```

## 📦 NuGet Packages Used

- MediatR - CQRS pattern
- FluentValidation - Input validation
- MongoDB.Driver - MongoDB client
- Npgsql - PostgreSQL client
- Dapper - Lightweight ORM
- StackExchange.Redis - Redis client
- Serilog - Logging
- Swashbuckle (Swagger) - API documentation
- Microsoft.AspNetCore.Authentication.JwtBearer - JWT auth

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

## 📄 License

This project is licensed under the MIT License.

## 👥 Authors

WeChat Backend Team

## 🔗 Additional Documentation

- [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Microservices Architecture](https://microservices.io/)
