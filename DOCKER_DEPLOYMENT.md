# Docker Deployment Guide

## Overview

The WeChat backend uses Docker and Docker Compose to orchestrate all microservices, databases, and infrastructure components.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      API Gateway (Port 5000)                 │
│                   Routes to all services                      │
└─────────────────────────────────────────────────────────────┘
                              │
         ┌────────────────────┼────────────────────┐
         │                    │                    │
         ▼                    ▼                    ▼
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│  Auth Service    │  │  Chat Service    │  │ UserProfile      │
│  (Port 5001)     │  │  (Port 5004)     │  │ (Port 5002)      │
│  PostgreSQL      │  │  MongoDB         │  │ MongoDB          │
└──────────────────┘  └──────────────────┘  └──────────────────┘

┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│  PostFeed        │  │  Media Service   │  │ Video Service    │
│  (Port 5003)     │  │  (Port 5005)     │  │ (Port 5006)      │
│  MongoDB         │  │  MongoDB         │  │ MongoDB          │
└──────────────────┘  └──────────────────┘  └──────────────────┘

┌──────────────────┐  ┌──────────────────┐
│  Notification    │  │  Realtime        │
│  (Port 5007)     │  │  (Port 5008)     │
│  MongoDB         │  │  SignalR/Redis   │
└──────────────────┘  └──────────────────┘

┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│  PostgreSQL      │  │  MongoDB         │  │  Redis           │
│  (Port 5432)     │  │  (Port 27017)    │  │  (Port 6379)     │
└──────────────────┘  └──────────────────┘  └──────────────────┘
```

## Services

### Microservices

| Service | Port | Technology | Database | Description |
|---------|------|------------|----------|-------------|
| **Gateway** | 5000 | ASP.NET Core | - | API Gateway (Ocelot) |
| **Auth** | 5001 | ASP.NET Core (Clean Architecture) | PostgreSQL | User authentication, JWT tokens |
| **UserProfile** | 5002 | ASP.NET Core | MongoDB | User profiles, friend requests |
| **PostFeed** | 5003 | ASP.NET Core | MongoDB | Posts, likes, comments |
| **Chat** | 5004 | ASP.NET Core | MongoDB | One-to-one and group chat |
| **Media** | 5005 | ASP.NET Core | MongoDB | Image/video upload |
| **Video** | 5006 | ASP.NET Core | MongoDB | Video processing |
| **Notification** | 5007 | ASP.NET Core | MongoDB | Push notifications |
| **Realtime** | 5008 | ASP.NET Core (SignalR) | Redis | Real-time communication |

### Infrastructure

| Component | Port | Image | Purpose |
|-----------|------|-------|---------|
| **PostgreSQL** | 5432 | postgres:15-alpine | Auth database |
| **MongoDB** | 27017 | mongo:7.0 | Document storage |
| **Redis** | 6379 | redis:7-alpine | Caching & pub/sub |

## Prerequisites

- Docker 20.10+
- Docker Compose 2.0+
- 8GB+ RAM recommended
- 20GB+ free disk space

## Quick Start

### 1. Build and Start All Services

```bash
# From project root
docker-compose up --build
```

### 2. Start Specific Services

```bash
# Start only databases
docker-compose up postgres mongodb redis

# Start Auth service only
docker-compose up auth-service

# Start Auth + dependencies
docker-compose up auth-service postgres redis
```

### 3. Run in Background (Detached Mode)

```bash
docker-compose up -d
```

### 4. View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f auth-service

# Last 100 lines
docker-compose logs --tail=100 auth-service
```

### 5. Stop Services

```bash
# Stop all
docker-compose down

# Stop and remove volumes (WARNING: deletes data)
docker-compose down -v
```

## Service URLs

After starting with `docker-compose up`:

- **API Gateway**: http://localhost:5000
- **Auth Service**: http://localhost:5001
  - Swagger: http://localhost:5001/swagger
  - Health: http://localhost:5001/health
- **Chat Service**: http://localhost:5004
- **UserProfile Service**: http://localhost:5002
- **PostFeed Service**: http://localhost:5003
- **Media Service**: http://localhost:5005
- **Video Service**: http://localhost:5006
- **Notification Service**: http://localhost:5007
- **Realtime Service**: http://localhost:5008

## Database Connections

### PostgreSQL (Auth)
```
Host: localhost
Port: 5432
Database: wechat_auth
Username: postgres
Password: postgres
```

### MongoDB (All other services)
```
Host: localhost
Port: 27017
Username: admin
Password: admin123

Databases:
- wechat_chat
- wechat_userprofile
- wechat_postfeed
- wechat_media
- wechat_video
- wechat_notification
```

### Redis
```
Host: localhost
Port: 6379
No password (development only)
```

## Individual Service Dockerfiles

Each service has its own Dockerfile for independent deployment:

### Auth Service (Clean Architecture)
```dockerfile
# Location: src/Auth/Dockerfile
docker build -f src/Auth/Dockerfile -t wechat-auth ./src
docker run -p 5001:80 wechat-auth
```

### Other Services
```bash
# Chat Service
docker build -f src/ChatService.Api/Dockerfile -t wechat-chat ./src
docker run -p 5004:80 wechat-chat

# UserProfile Service
docker build -f src/UserProfileService.Api/Dockerfile -t wechat-userprofile ./src
docker run -p 5002:80 wechat-userprofile

# And so on for other services...
```

## Environment Variables

All services support environment variable configuration. See `docker-compose.yml` for full list.

### Common Variables

```bash
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Development|Staging|Production
ASPNETCORE_URLS=http://+:80

# JWT Settings (same across all services)
JwtSettings__Secret=your-secret-key-min-32-chars
JwtSettings__Issuer=WeChat.AuthService
JwtSettings__Audience=WeChat.API
JwtSettings__AccessTokenExpirationMinutes=15
JwtSettings__RefreshTokenExpirationDays=7

# Redis
RedisSettings__ConnectionString=redis:6379
RedisSettings__InstanceName=WeChat:ServiceName:

# MongoDB (for services using MongoDB)
MongoDbSettings__ConnectionString=mongodb://admin:admin123@mongodb:27017
MongoDbSettings__DatabaseName=wechat_servicename
```

## Volume Management

### Persistent Volumes

```bash
# List volumes
docker volume ls | grep wechat

# Inspect volume
docker volume inspect wechat-backend_postgres_data

# Backup PostgreSQL data
docker run --rm -v wechat-backend_postgres_data:/data -v $(pwd):/backup \
  alpine tar czf /backup/postgres-backup.tar.gz /data

# Restore PostgreSQL data
docker run --rm -v wechat-backend_postgres_data:/data -v $(pwd):/backup \
  alpine tar xzf /backup/postgres-backup.tar.gz -C /
```

### Clean Up

```bash
# Remove all stopped containers
docker-compose down

# Remove volumes (WARNING: deletes data)
docker-compose down -v

# Remove all unused volumes
docker volume prune

# Full cleanup
docker system prune -a --volumes
```

## Development Workflow

### 1. Start Infrastructure Only

```bash
# Start databases and Redis first
docker-compose up postgres mongodb redis
```

### 2. Run Service Locally

```bash
# Run Auth service locally with hot reload
cd src/Auth/AuthService.Api
dotnet watch run
```

### 3. Use Both (Hybrid)

```bash
# Infrastructure in Docker
docker-compose up postgres mongodb redis

# Services locally for debugging
cd src/Auth/AuthService.Api && dotnet run &
cd src/ChatService.Api && dotnet run &
```

## Health Checks

All services include health check endpoints:

```bash
# Check all services
curl http://localhost:5000/health  # Gateway
curl http://localhost:5001/health  # Auth
curl http://localhost:5002/health  # UserProfile
curl http://localhost:5003/health  # PostFeed
curl http://localhost:5004/health  # Chat
curl http://localhost:5005/health  # Media
curl http://localhost:5006/health  # Video
curl http://localhost:5007/health  # Notification
curl http://localhost:5008/health  # Realtime

# Check database health
docker-compose ps
```

## Troubleshooting

### Service Won't Start

```bash
# Check logs
docker-compose logs service-name

# Check if port is in use
netstat -an | grep 5001

# Rebuild specific service
docker-compose build --no-cache auth-service
docker-compose up auth-service
```

### Database Connection Issues

```bash
# Check if PostgreSQL is ready
docker-compose exec postgres pg_isready -U postgres

# Check MongoDB
docker-compose exec mongodb mongosh --eval "db.runCommand('ping')"

# Check Redis
docker-compose exec redis redis-cli ping
```

### Reset Everything

```bash
# Stop all services
docker-compose down -v

# Remove all images
docker-compose down --rmi all

# Start fresh
docker-compose up --build
```

## Production Deployment

### 1. Environment Variables

Create `.env` file:

```bash
# .env
ASPNETCORE_ENVIRONMENT=Production
POSTGRES_PASSWORD=<strong-password>
MONGO_INITDB_ROOT_PASSWORD=<strong-password>
JWT_SECRET=<strong-secret-key-min-64-chars>
```

### 2. Update docker-compose.prod.yml

```yaml
# docker-compose.prod.yml
version: '3.8'
services:
  auth-service:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__PostgreSQL=${POSTGRES_CONNECTION_STRING}
      - JwtSettings__Secret=${JWT_SECRET}
    restart: always
    # Add resource limits
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
```

### 3. Deploy

```bash
docker-compose -f docker-compose.prod.yml up -d
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build and Deploy

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2

      - name: Build images
        run: docker-compose build

      - name: Push to registry
        run: |
          docker tag wechat-auth ${{ secrets.REGISTRY }}/wechat-auth:${{ github.sha }}
          docker push ${{ secrets.REGISTRY }}/wechat-auth:${{ github.sha }}
```

## Monitoring

### Logging

```bash
# View logs in real-time
docker-compose logs -f

# Export logs to file
docker-compose logs > logs.txt
```

### Resource Usage

```bash
# Check container stats
docker stats

# Check specific service
docker stats wechat-auth-service
```

## Summary

- ✅ All services containerized with Dockerfiles
- ✅ docker-compose.yml orchestrates entire stack
- ✅ Individual service solution files created
- ✅ Development and production configurations
- ✅ Health checks for all services
- ✅ Persistent volumes for data
- ✅ Network isolation with bridge network
- ✅ Easy to scale and deploy

**Commands:**
```bash
# Start everything
docker-compose up --build

# Start specific service
docker-compose up auth-service

# Stop everything
docker-compose down

# View logs
docker-compose logs -f service-name
```
