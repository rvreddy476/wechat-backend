# WeChat Backend - Database Documentation

## Overview

This directory contains comprehensive database scripts, schemas, indexes, and documentation for all microservices in the WeChat backend platform.

## Architecture

The platform uses a **polyglot persistence** approach with two database systems:

- **PostgreSQL** - For Auth service (relational data with complex queries and ACID guarantees)
- **MongoDB** - For Chat, UserProfile, PostFeed, Media, and Notification services (flexible documents, scalability)

## Services and Databases

| Service | Database | Purpose |
|---------|----------|---------|
| **Auth** | PostgreSQL | User authentication, sessions, tokens, password management |
| **Chat** | MongoDB | Real-time messaging, conversations, message history |
| **UserProfile** | MongoDB | User profiles, friends, followers, privacy settings |
| **PostFeed** | MongoDB | Social posts, comments, likes, shares, hashtags |
| **Media** | MongoDB | File uploads, media metadata, thumbnails |
| **Notification** | MongoDB | User notifications with auto-expiry |

## Directory Structure

```
Database/
├── README.md                    # This file
│
├── Auth/                        # PostgreSQL - Authentication Service
│   ├── Schema/                  # Table definitions
│   │   ├── 01_CreateTables.sql
│   │   ├── 02_CreateIndexes.sql
│   │   └── 03_CreateConstraints.sql
│   ├── Functions/               # Helper functions
│   │   └── 01_HelperFunctions.sql
│   ├── Procedures/              # Stored procedures
│   │   ├── 01_UserManagement.sql
│   │   └── 02_TokenManagement.sql
│   ├── Migrations/              # Migration scripts
│   │   └── 01_InitialSetup.sql
│   ├── Seeds/                   # Test data
│   │   └── 01_SeedData.sql
│   ├── Backup/                  # Backup documentation
│   │   └── backup_restore.md
│   └── README.md               # Auth database guide
│
├── Chat/                        # MongoDB - Chat Service
│   ├── Collections/             # Collection schemas
│   │   ├── 01_conversations.js
│   │   └── 02_messages.js
│   ├── Indexes/                 # Performance indexes
│   │   ├── 01_conversation_indexes.js
│   │   └── 02_message_indexes.js
│   ├── Queries/                 # Common query patterns
│   │   └── common_queries.js
│   ├── Seeds/                   # Test data
│   │   └── seed_data.js
│   └── README.md               # Chat database guide
│
├── UserProfile/                 # MongoDB - User Profile Service
│   ├── Collections/
│   │   ├── 01_user_profiles.js
│   │   ├── 02_friend_requests.js
│   │   └── 03_user_activities.js
│   ├── Indexes/
│   │   ├── 01_profile_indexes.js
│   │   ├── 02_friend_request_indexes.js
│   │   └── 03_activity_indexes.js
│   ├── Queries/
│   │   └── common_queries.js
│   ├── Seeds/
│   │   └── seed_data.js
│   └── README.md
│
├── PostFeed/                    # MongoDB - Social Feed Service
│   ├── Collections/
│   │   ├── 01_posts.js
│   │   ├── 02_comments.js
│   │   ├── 03_likes.js
│   │   ├── 04_shares.js
│   │   └── 05_hashtags.js
│   ├── Indexes/
│   │   ├── 01_post_indexes.js
│   │   ├── 02_comment_indexes.js
│   │   ├── 03_like_indexes.js
│   │   ├── 04_share_indexes.js
│   │   └── 05_hashtag_indexes.js
│   ├── Queries/
│   │   └── common_queries.js
│   ├── Seeds/
│   │   └── seed_data.js
│   └── README.md
│
├── Media/                       # MongoDB - Media Service
│   ├── Collections/
│   │   └── 01_media_files.js
│   ├── Indexes/
│   │   └── 01_media_indexes.js
│   └── README.md
│
└── Notification/                # MongoDB - Notification Service
    ├── Collections/
    │   └── 01_notifications.js
    ├── Indexes/
    │   └── 01_notification_indexes.js
    └── README.md
```

## Quick Start

### Prerequisites

- **PostgreSQL 15+** for Auth service
- **MongoDB 7+** for other services
- Docker & docker-compose (optional, for containerized setup)

### Setup All Databases

#### Option 1: Using Docker Compose (Recommended)

```bash
# Start all databases
docker-compose up -d postgres mongo

# Wait for databases to be ready
sleep 10

# Run all setup scripts (see sections below)
```

#### Option 2: Local Installation

Install PostgreSQL and MongoDB locally, then run setup scripts.

### Auth Service (PostgreSQL)

```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE wechat_auth;

# Connect to database
\c wechat_auth

# Run migration script (this executes all schema, functions, and procedures)
\i Database/Auth/Migrations/01_InitialSetup.sql

# Optionally load test data (DEVELOPMENT ONLY!)
\i Database/Auth/Seeds/01_SeedData.sql
```

### MongoDB Services

For each MongoDB service (Chat, UserProfile, PostFeed, Media, Notification):

```bash
# Connect to MongoDB
mongosh

# Example for Chat service
use wechat_chat
load('Database/Chat/Collections/01_conversations.js')
load('Database/Chat/Collections/02_messages.js')
load('Database/Chat/Indexes/01_conversation_indexes.js')
load('Database/Chat/Indexes/02_message_indexes.js')
load('Database/Chat/Seeds/seed_data.js')  # Optional: test data

# Repeat for other services...
```

### Quick Setup Script

```bash
#!/bin/bash
# setup_all_databases.sh

# PostgreSQL - Auth Service
psql -U postgres -c "CREATE DATABASE wechat_auth;"
psql -U postgres -d wechat_auth -f Database/Auth/Migrations/01_InitialSetup.sql

# MongoDB - All Services
for service in Chat UserProfile PostFeed Media Notification; do
    mongosh "mongodb://localhost:27017/wechat_${service,,}" --eval "
        $(cat Database/$service/Collections/*.js)
        $(cat Database/$service/Indexes/*.js)
    "
done

echo "All databases set up successfully!"
```

## Connection Strings

### Development

```bash
# PostgreSQL (Auth)
DATABASE_URL=postgresql://postgres:password@localhost:5432/wechat_auth

# MongoDB (All services)
CHAT_MONGO_URL=mongodb://localhost:27017/wechat_chat
USERPROFILE_MONGO_URL=mongodb://localhost:27017/wechat_userprofile
POSTFEED_MONGO_URL=mongodb://localhost:27017/wechat_postfeed
MEDIA_MONGO_URL=mongodb://localhost:27017/wechat_media
NOTIFICATION_MONGO_URL=mongodb://localhost:27017/wechat_notification
```

### Production (Docker Compose)

```bash
# PostgreSQL
DATABASE_URL=postgresql://postgres:password@postgres:5432/wechat_auth

# MongoDB
CHAT_MONGO_URL=mongodb://mongo:27017/wechat_chat
USERPROFILE_MONGO_URL=mongodb://mongo:27017/wechat_userprofile
POSTFEED_MONGO_URL=mongodb://mongo:27017/wechat_postfeed
MEDIA_MONGO_URL=mongodb://mongo:27017/wechat_media
NOTIFICATION_MONGO_URL=mongodb://mongo:27017/wechat_notification
```

## Backup & Restore

### PostgreSQL (Auth Service)

```bash
# Backup
pg_dump -U postgres -h localhost -d wechat_auth | gzip > auth_backup_$(date +%Y%m%d).sql.gz

# Restore
gunzip -c auth_backup_20240115.sql.gz | psql -U postgres -h localhost -d wechat_auth
```

### MongoDB (All Services)

```bash
# Backup all databases
mongodump --gzip --archive=mongodb_backup_$(date +%Y%m%d).gz

# Restore all databases
mongorestore --gzip --archive=mongodb_backup_20240115.gz

# Backup specific database
mongodump --db=wechat_chat --gzip --archive=chat_backup.gz

# Restore specific database
mongorestore --db=wechat_chat --gzip --archive=chat_backup.gz
```

## Database Features by Service

### Auth Service (PostgreSQL)

- **Tables**: users, user_sessions, verification_codes, user_login_history, password_reset_tokens
- **Features**:
  - BCrypt password hashing
  - JWT refresh tokens
  - Email/phone verification
  - Session management
  - Login history audit
  - Stored procedures for user management
  - Helper functions for validation

### Chat Service (MongoDB)

- **Collections**: conversations, messages
- **Features**:
  - One-to-one and group conversations
  - Real-time message delivery (with SignalR)
  - Read receipts
  - Message reactions
  - Media attachments (images, videos, audio, files)
  - Reply-to functionality
  - Message editing and soft deletion

### UserProfile Service (MongoDB)

- **Collections**: user_profiles, friend_requests, user_activities
- **Features**:
  - Rich user profiles with bio, location, website
  - Friends/followers/following system
  - Friend request workflow
  - Privacy settings (profile visibility, online status, messages)
  - Notification preferences
  - User activity tracking (TTL: 90 days)
  - Blocked users management

### PostFeed Service (MongoDB)

- **Collections**: posts, comments, likes, shares, hashtags
- **Features**:
  - Rich text posts with media attachments
  - Nested comments (replies)
  - Likes on posts and comments
  - Post sharing with captions
  - Hashtag trending calculation
  - @ mentions support
  - Location tagging
  - Visibility controls (Public, FriendsOnly, Private)
  - Engagement metrics (views, likes, comments, shares)

### Media Service (MongoDB)

- **Collections**: media_files
- **Features**:
  - File upload metadata tracking
  - Media type categorization (Image, Video, Audio, Document)
  - Thumbnail generation support
  - Processing status (Uploading, Processing, Ready, Failed)
  - Public/private media support
  - Tag support for organization

### Notification Service (MongoDB)

- **Collections**: notifications
- **Features**:
  - Multiple notification types (FriendRequest, Message, Like, Comment, etc.)
  - Read/unread status tracking
  - Priority levels (Low, Normal, High)
  - Auto-expiry with TTL index
  - Rich notifications with action URLs
  - Sender information tracking

## Performance Considerations

### Indexing

All collections have optimized indexes for common query patterns:

- **PostgreSQL**: Indexes on email, username, refresh_token, session tracking
- **MongoDB**: Compound indexes for filtering, sorting, and pagination

### Caching Strategy (Recommended)

Use Redis for:
- **Auth**: Active sessions, token blacklist
- **Chat**: Online users, typing indicators
- **UserProfile**: Online user list, mutual friends cache
- **PostFeed**: Public feed, trending hashtags, trending posts
- **Media**: Recently uploaded files
- **Notification**: Unread notification counts

### Monitoring

Monitor these key metrics:

1. **Database Size**: Track growth rate
2. **Query Performance**: Slow query logs
3. **Index Usage**: Ensure indexes are being used
4. **Connection Pool**: Monitor active connections
5. **Replication Lag**: For production setups

## Scaling Recommendations

### PostgreSQL (Auth)

- **Vertical Scaling**: Increase CPU/RAM for primary server
- **Read Replicas**: For read-heavy workloads
- **Connection Pooling**: Use PgBouncer
- **Partitioning**: Partition audit tables by date

### MongoDB (All Services)

- **Sharding**: For large collections (messages, posts)
  - Chat: Shard by `conversationId`
  - UserProfile: Shard by `userId`
  - PostFeed: Shard by `authorId` or hashed `_id`
- **Replica Sets**: Minimum 3-node replica set for production
- **Connection Pooling**: Configure appropriate pool sizes

## Security Best Practices

1. **Authentication**: Enable authentication for both PostgreSQL and MongoDB
2. **Encryption**: Use TLS/SSL for connections
3. **Network Security**: Restrict database ports, use firewalls
4. **Backups**: Automated daily backups with retention policy
5. **Audit Logging**: Enable audit logs for compliance
6. **Least Privilege**: Grant minimum required permissions
7. **Secrets Management**: Never commit credentials to git

## Troubleshooting

### PostgreSQL

```bash
# Check active connections
SELECT count(*) FROM pg_stat_activity;

# Find slow queries
SELECT query, calls, total_time, mean_time
FROM pg_stat_statements
ORDER BY mean_time DESC
LIMIT 10;

# Check database size
SELECT pg_size_pretty(pg_database_size('wechat_auth'));
```

### MongoDB

```bash
# Check database stats
db.stats()

# Find slow queries
db.setProfilingLevel(1, { slowms: 100 })
db.system.profile.find().sort({ ts: -1 }).limit(10)

# Check index usage
db.collection.aggregate([{ $indexStats: {} }])
```

## Development Workflow

1. **Make Schema Changes**: Update SQL/JS files in appropriate service folder
2. **Create Migration**: Add migration script if needed
3. **Test Locally**: Run on local development database
4. **Document Changes**: Update README.md
5. **Code Review**: Review schema changes carefully
6. **Deploy**: Apply to staging, then production

## Additional Resources

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [MongoDB Documentation](https://docs.mongodb.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/)
- Each service's README.md for detailed documentation

## Support

For questions or issues:
1. Check service-specific README.md files
2. Review query examples in Queries/ folders
3. Consult team documentation
4. Contact database team

---

**Last Updated**: 2024-01-15
**Maintained By**: WeChat Backend Team
