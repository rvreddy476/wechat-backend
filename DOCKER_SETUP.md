# WeChat Social Media Platform - Docker Setup Guide

## Prerequisites

- Docker Engine 20.10+ installed
- Docker Compose 2.0+ installed
- At least 8GB RAM allocated to Docker
- 20GB free disk space

## Quick Start

### 1. Clone and Setup

```bash
git clone <repository-url>
cd wechat-backend
cp .env.example .env
```

### 2. Configure Environment Variables

Edit `.env` file and set:
- `POSTGRES_PASSWORD`: Strong password for PostgreSQL
- `MONGO_PASSWORD`: Strong password for MongoDB
- `REDIS_PASSWORD`: Strong password for Redis
- `JWT_SECRET`: Strong secret key (min 32 characters)
- `GCP_BUCKET_NAME`: Your GCP storage bucket name

### 3. Setup GCP Credentials

Place your GCP service account JSON file:
```bash
mkdir -p config
cp /path/to/your-gcp-credentials.json config/gcp-credentials.json
```

### 4. Setup Firebase Credentials (Optional)

For push notifications:
```bash
cp /path/to/firebase-credentials.json config/firebase-credentials.json
```

### 5. Start All Services

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# View logs for specific service
docker-compose logs -f auth-service
```

## Architecture

### Services

| Service | Port | Description |
|---------|------|-------------|
| Gateway | 5000 | API Gateway (reverse proxy) |
| Auth Service | 5001 | Authentication & authorization |
| UserProfile Service | 5002 | User profiles & follows |
| PostFeed Service | 5003 | Posts, comments, reactions |
| Chat Service | 5004 | Direct messaging |
| Video Service | 5005 | Video uploads & management |
| Media Service | 5006 | Image uploads & management |
| Notification Service | 5007 | Push notifications |
| Realtime Service | 5008 | SignalR real-time hub |
| Video Worker | - | Background video processing |

### Infrastructure

| Service | Port | Description |
|---------|------|-------------|
| PostgreSQL | 5432 | Auth service database |
| MongoDB | 27017 | All other services database |
| Redis | 6379 | Cache & job queue |

## Service URLs

When all services are running:

- **API Gateway**: http://localhost:5000
- **Auth API**: http://localhost:5001
- **Swagger UIs**: Available at `http://localhost:<port>/swagger`

## Common Commands

### Start Services

```bash
# Start all services
docker-compose up -d

# Start specific services
docker-compose up -d auth-service userprofile-service

# Start with rebuild
docker-compose up -d --build
```

### Stop Services

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (WARNING: deletes all data)
docker-compose down -v
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f auth-service

# Last 100 lines
docker-compose logs --tail=100 auth-service
```

### Scale Services

```bash
# Scale video worker to 3 instances
docker-compose up -d --scale video-processing-worker=3
```

### Rebuild Services

```bash
# Rebuild all services
docker-compose build

# Rebuild specific service
docker-compose build auth-service

# Rebuild and restart
docker-compose up -d --build auth-service
```

## Health Checks

All services have health check endpoints:

```bash
# Check service health
curl http://localhost:5001/health

# Check all services
for port in {5000..5008}; do
  echo "Checking port $port..."
  curl -s http://localhost:$port/health | jq
done
```

## Database Access

### PostgreSQL

```bash
# Connect to PostgreSQL
docker exec -it wechat-postgres psql -U wechat_admin -d wechat_auth

# Backup database
docker exec wechat-postgres pg_dump -U wechat_admin wechat_auth > backup.sql

# Restore database
docker exec -i wechat-postgres psql -U wechat_admin wechat_auth < backup.sql
```

### MongoDB

```bash
# Connect to MongoDB
docker exec -it wechat-mongodb mongosh -u wechat_admin -p wechat_secure_password_123

# Backup database
docker exec wechat-mongodb mongodump --username=wechat_admin --password=wechat_secure_password_123 --out=/backup

# Restore database
docker exec wechat-mongodb mongorestore --username=wechat_admin --password=wechat_secure_password_123 /backup
```

### Redis

```bash
# Connect to Redis
docker exec -it wechat-redis redis-cli -a wechat_redis_password

# View all keys
docker exec wechat-redis redis-cli -a wechat_redis_password KEYS '*'

# Monitor commands
docker exec wechat-redis redis-cli -a wechat_redis_password MONITOR
```

## Troubleshooting

### Services Won't Start

1. Check if ports are available:
```bash
netstat -tuln | grep -E '5000|5432|27017|6379'
```

2. Check service logs:
```bash
docker-compose logs <service-name>
```

3. Verify environment variables:
```bash
docker-compose config
```

### Out of Memory

Increase Docker memory allocation:
- Docker Desktop: Settings → Resources → Memory (increase to 8GB+)
- Linux: Configure in `/etc/docker/daemon.json`

### Container Keeps Restarting

1. Check health check status:
```bash
docker ps -a
```

2. View container logs:
```bash
docker logs wechat-<service-name>
```

3. Check dependencies:
```bash
docker-compose ps
```

### Database Connection Issues

1. Verify database is healthy:
```bash
docker-compose ps postgres mongodb redis
```

2. Check connection strings in `.env`

3. Restart dependent services:
```bash
docker-compose restart auth-service
```

## Development Workflow

### 1. Make Code Changes

Edit code in `src/<ServiceName>/`

### 2. Rebuild Service

```bash
docker-compose build <service-name>
docker-compose up -d <service-name>
```

### 3. Test Changes

```bash
curl http://localhost:<port>/health
```

## Production Deployment

### Security Checklist

- [ ] Change all default passwords
- [ ] Use strong JWT secret (64+ characters)
- [ ] Configure HTTPS/TLS
- [ ] Enable firewall rules
- [ ] Restrict database access
- [ ] Use secrets management (AWS Secrets Manager, Azure Key Vault)
- [ ] Enable logging and monitoring
- [ ] Configure backup strategy
- [ ] Set resource limits

### Environment-Specific Configurations

Create separate compose files:
- `docker-compose.yml` - Base configuration
- `docker-compose.dev.yml` - Development overrides
- `docker-compose.prod.yml` - Production overrides

Deploy to production:
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## Monitoring

### Resource Usage

```bash
# View resource usage
docker stats

# View disk usage
docker system df
```

### Container Metrics

```bash
# View container details
docker inspect wechat-<service-name>
```

## Cleanup

### Remove Unused Resources

```bash
# Remove stopped containers
docker container prune

# Remove unused images
docker image prune -a

# Remove unused volumes
docker volume prune

# Remove everything (WARNING: deletes all Docker data)
docker system prune -a --volumes
```

## Support

For issues and questions:
1. Check service logs
2. Review environment variables
3. Verify network connectivity
4. Check GitHub issues
