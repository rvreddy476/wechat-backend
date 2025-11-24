# WeChat.com - Complete Database Architecture Overview

## 📊 Database Summary

WeChat.com uses a **polyglot persistence** architecture:
- **PostgreSQL**: Authentication and user management (AuthService)
- **MongoDB**: All other services (flexible, scalable document storage)
- **Redis**: Caching, sessions, real-time data (documented separately)

---

## 🗄️ Databases and Collections

### 1. **AuthService** - PostgreSQL (`wechat_auth`)

**10 Tables:**
- `Users` - User authentication and credentials
- `Roles` - Role definitions (RBAC)
- `UserRoles` - User-role mapping
- `RefreshTokens` - JWT refresh tokens
- `EmailVerificationTokens` - Email verification
- `PasswordResetTokens` - Password reset flow
- `UserSessions` - Active sessions tracking
- `AuditLogs` - Security audit trail
- `LoginAttempts` - Login tracking
- `ExternalLoginProviders` - OAuth integrations

**Features:**
- 12 Stored Procedures
- 14 Helper Functions
- Automatic triggers for security
- Comprehensive audit logging
- Account lockout mechanism
- 2FA support

---

### 2. **UserProfileService** - MongoDB (`wechat_profiles`)

**3 Collections:**
- `profiles` - User profiles with stats and settings
- `follows` - Follow relationships
- `blockedUsers` - Blocked user tracking

**Key Features:**
- Full-text search on profiles
- Privacy and notification preferences
- Social stats (followers, posts, videos, views)
- Verification badges
- Social links integration

---

### 3. **PostFeedService** - MongoDB (`wechat_postfeed`)

**4 Collections:**
- `posts` - User posts (text, images, videos, shares, polls)
- `comments` - Threaded comments (up to 5 levels)
- `reactions` - Reactions on posts/comments (6 types)
- `hashtags` - Trending hashtags tracking

**Key Features:**
- Facebook-style feed
- Nested comments (threading)
- Rich media support (images, videos, GIFs)
- Hashtag trending algorithm
- Location-based posts
- Privacy controls (public, followers, friends, private)
- Edit history tracking
- Engagement scoring for trending
- Poll support

**Indexes:** 25+ optimized indexes

**Query Patterns:**
- Personalized user feed
- Trending posts algorithm
- Posts by hashtag
- Full-text search
- User timeline
- Comment threading

---

### 4. **ChatService** - MongoDB (`wechat_chat`)

**2 Collections:**
- `conversations` - Chat threads (direct and group)
- `messages` - Chat messages (10 types)

**Key Features:**
- 1:1 direct messaging
- Group chats (unlimited members)
- Message types: text, image, video, audio, file, location, contact, sticker, GIF, system
- Read receipts
- Message reactions (6 emojis)
- Reply/quote functionality
- Message forwarding
- Typing indicators
- Message search
- Media gallery view
- Mute notifications per conversation
- Pinned conversations
- Archived conversations

**Indexes:** 15+ optimized indexes

**Query Patterns:**
- User's conversations list
- Conversation messages (paginated)
- Unread message count
- Direct message lookup
- Media messages gallery
- Message search

---

### 5. **VideoService** - MongoDB (`wechat_video`)

**1 Collection (Core):**
- `videos` - Video uploads (long-form and shorts)

**Key Features:**
- Long-form videos (YouTube-style)
- Short-form videos/Shorts (TikTok-style, 15-60 seconds)
- Multiple quality transcoding (1080p, 720p, 480p, 360p)
- HLS streaming support
- Video analytics (views, watch time, completion rate)
- Trending shorts algorithm
- Video search (full-text)
- Tags and location
- Privacy controls (public, private, unlisted)
- Engagement tracking (likes, comments, shares)
- Thumbnail generation
- Processing status tracking

**Indexes:** 10+ optimized indexes

**Processing States:**
- uploading → uploaded → processing → ready → failed

**Trending Score Calculation:**
```javascript
trendingScore = (
    viewVelocity * 0.4 +
    completionRate * 0.3 +
    engagementRate * 0.2 +
    recencyScore * 0.1
)
```

---

### 6. **NotificationService** - MongoDB (`wechat_notifications`)

**1 Collection:**
- `notifications` - All user notifications

**Notification Types (16+):**
- Follow/Unfollow
- Post: like, comment, share, mention
- Comment: like, reply
- Video: like, comment, share
- Video: processed, published
- Message, message mention
- Group: invite, mention
- System notifications

**Key Features:**
- Real-time push notifications
- Unread count tracking
- Notification preferences per type
- Auto-expiration (TTL index)
- Rich content (title, body, image)
- Deep linking to entities
- Grouping/batching support

**Indexes:** 6 optimized indexes including TTL

---

### 7. **MediaService** - MongoDB (`wechat_media`)

**1 Collection:**
- `mediaUploads` - Media upload tracking

**Key Features:**
- Signed URL generation for uploads
- Upload status tracking
- GCS integration
- CDN URL management
- Metadata storage (dimensions, duration)
- Auto-expiration of pending uploads (TTL)

**Media Types:**
- Images (JPEG, PNG, GIF, WebP)
- Videos (MP4, MOV, AVI)
- Audio (MP3, WAV, M4A)
- Files (PDF, DOC, etc.)

---

## 📈 Database Statistics

| Service | Database | Collections/Tables | Total Indexes | Documents/Rows (Est.) |
|---------|----------|-------------------|---------------|---------------------|
| AuthService | PostgreSQL | 10 tables | 35+ | 1M+ users |
| UserProfileService | MongoDB | 3 collections | 20+ | 1M+ profiles |
| PostFeedService | MongoDB | 4 collections | 25+ | 10M+ posts |
| ChatService | MongoDB | 2 collections | 15+ | 100M+ messages |
| VideoService | MongoDB | 1 collection | 10+ | 5M+ videos |
| NotificationService | MongoDB | 1 collection | 6 | 50M+ notifications |
| MediaService | MongoDB | 1 collection | 4 | 20M+ uploads |
| **TOTAL** | **2 DBs** | **22 collections/tables** | **115+** | **~186M+ records** |

---

## 🔄 Data Flow Examples

### 1. User Registration & Profile Creation
```
1. POST /api/auth/register → AuthService
   → Insert into auth.Users (PostgreSQL)
   → Assign default "User" role

2. AuthService triggers UserProfileService
   → Insert into profiles collection (MongoDB)
   → Initialize stats: { followersCount: 0, postsCount: 0, ... }
```

### 2. Creating a Post
```
1. POST /api/posts → PostFeedService
   → Insert into posts collection (MongoDB)
   → Extract hashtags and update hashtags collection

2. Broadcast via SignalR FeedHub
   → Notify followers in real-time

3. NotificationService creates notifications
   → For mentioned users
   → For followers (if configured)
```

### 3. Video Upload & Processing
```
1. POST /api/videos/upload-url → VideoService
   → Generate signed GCS URL
   → Insert video document (status: "uploading")

2. Client uploads to GCS
   → Video document updated (status: "uploaded")

3. VideoProcessing.Worker triggered
   → Extract metadata
   → Generate thumbnails
   → Transcode to multiple qualities
   → Generate HLS manifest
   → Update video (status: "ready")

4. NotificationService notifies user
   → "Your video is ready!"

5. Broadcast via SignalR VideoHub
   → Notify followers about new video
```

### 4. Sending a Chat Message
```
1. POST /api/chat/conversations/{id}/messages → ChatService
   → Insert into messages collection (MongoDB)
   → Update conversation lastMessage

2. Broadcast via SignalR ChatHub
   → Send to conversation participants
   → Update unread counts

3. NotificationService (if user offline)
   → Create push notification
   → Store in notifications collection
```

---

## 🎯 Query Performance Optimizations

### PostFeedService
- **Feed Query**: Uses compound index `(userId, visibility, isDeleted, createdAt)`
- **Hashtag Query**: Index on `(hashtags, createdAt)`
- **Text Search**: Full-text index on `(content.text, hashtags)`
- **Trending**: Compound index `(stats.engagementScore, createdAt)`

### ChatService
- **Conversations List**: Index on `(participants.userId, lastMessageAt)`
- **Messages**: Index on `(conversationId, createdAt)`
- **Unread Count**: Calculated via aggregation pipeline
- **Search**: Full-text index on message content

### VideoService
- **Shorts Feed**: Index on `(type, trendingScore, createdAt)`
- **User Videos**: Index on `(userId, createdAt)`
- **Trending**: Compound index `(trendingScore, stats.viewCount)`
- **Search**: Full-text index on `(title, description, tags)`

---

## 🔐 Security Considerations

### 1. **Data Privacy**
- Respect `isPrivate` flags in profiles
- Check `visibility` before showing posts/videos
- Verify `participants` array before showing conversations
- Check `blockedUsers` before any interaction

### 2. **Soft Deletes**
- All collections use `isDeleted` flag
- Data retained for analytics and audit
- Never hard delete user data immediately
- Scheduled cleanup jobs for old soft-deleted data

### 3. **Input Validation**
- Schema validation enforced at MongoDB level
- Max lengths on all text fields
- Enum constraints on status fields
- Required field validation

### 4. **Rate Limiting**
- Track in Redis (not in persistent DB)
- Per-user, per-endpoint limits
- Sliding window implementation

---

## 🚀 Scaling Strategy

### Phase 1 (0-100K users)
- Single MongoDB cluster
- Single PostgreSQL instance
- Redis single node
- All indexes in place

### Phase 2 (100K-1M users)
- MongoDB replica sets (3 nodes)
- PostgreSQL with read replicas
- Redis Cluster
- Connection pooling

### Phase 3 (1M+ users)
- **Sharding Strategy:**
  - Posts: Shard by `userId`
  - Messages: Shard by `conversationId`
  - Videos: Shard by `userId`
  - Notifications: Shard by `userId`
- **Read Models:**
  - Materialized views for feeds
  - Cached trending data
  - Denormalized user stats

---

## 📝 Maintenance Tasks

### Daily
```bash
# Cleanup expired tokens
psql -d wechat_auth -c "SELECT * FROM auth.sp_CleanupExpiredTokens();"

# Cleanup expired media uploads
mongosh wechat_media --eval "db.mediaUploads.deleteMany({status:'pending', expiresAt:{$lt:new Date()}})"
```

### Weekly
```bash
# Update trending scores
mongosh wechat_postfeed --eval "db.hashtags.updateMany({}, {$set:{needsRecalculation:true}})"
mongosh wechat_video --eval "db.videos.updateMany({type:'short'}, {$set:{needsTrendingUpdate:true}})"

# Vacuum PostgreSQL
psql -d wechat_auth -c "VACUUM ANALYZE;"
```

### Monthly
```bash
# Archive old notifications (90+ days)
mongosh wechat_notifications --eval "db.notifications.updateMany({createdAt:{$lt:new Date(Date.now()-90*24*60*60*1000)}}, {$set:{expiresAt:new Date()}})"

# Compact collections
mongosh wechat_postfeed --eval "db.posts.compact()"
mongosh wechat_chat --eval "db.messages.compact()"
```

---

## 🔧 Setup Order

1. **AuthService** (PostgreSQL)
   ```bash
   psql -d wechat_auth -f Database/AuthService/Schema/01_CreateTables.sql
   psql -d wechat_auth -f Database/AuthService/Schema/02_CreateTriggers.sql
   # ... (all scripts in order)
   ```

2. **UserProfileService** (MongoDB)
   ```bash
   mongosh < Database/UserProfileService/Schemas/01_ProfilesCollection.js
   mongosh < Database/UserProfileService/Schemas/02_FollowsCollection.js
   mongosh < Database/UserProfileService/Schemas/03_BlockedUsersCollection.js
   ```

3. **PostFeedService** (MongoDB)
   ```bash
   mongosh < Database/PostFeedService/Schemas/01_PostsCollection.js
   mongosh < Database/PostFeedService/Schemas/02_CommentsCollection.js
   mongosh < Database/PostFeedService/Schemas/03_ReactionsCollection.js
   mongosh < Database/PostFeedService/Schemas/04_HashtagsCollection.js
   ```

4. **ChatService** (MongoDB)
   ```bash
   mongosh < Database/ChatService/Schemas/01_ConversationsCollection.js
   mongosh < Database/ChatService/Schemas/02_MessagesCollection.js
   ```

5. **VideoService** (MongoDB)
   ```bash
   mongosh < Database/VideoService/Schemas/01_VideosCollection.js
   ```

6. **NotificationService** (MongoDB)
   ```bash
   mongosh < Database/NotificationService/Schemas/01_NotificationsCollection.js
   ```

7. **MediaService** (MongoDB)
   ```bash
   mongosh < Database/MediaService/Schemas/01_MediaUploadsCollection.js
   ```

---

## 📚 Additional Documentation

- [AuthService & UserProfileService](./README.md) - Detailed documentation for auth and profiles
- [PostFeedService Queries](./PostFeedService/Queries/01_PostFeedQueries.js) - Query examples
- [ChatService Queries](./ChatService/Queries/01_ChatQueries.js) - Chat query patterns

---

**Created for WeChat.com Backend**
Complete Database Architecture
Version 2.0 - All Services
Last Updated: 2024
