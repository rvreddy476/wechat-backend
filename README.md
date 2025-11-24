# WeChat.com Backend – Complete Social Media Platform Architecture Guide

> This is the complete backend architecture for **WeChat.com** - a comprehensive social media platform.
> Implement a **microservices backend** that can scale from **Phase 1 (MVP)** to **Phase 2 (millions of users)** without rewriting everything.

The platform combines **THREE CORE MODULES**:

## 🎯 Core Modules

### 1. **Feed / Posts Module** (Like Facebook)
- Text posts with rich media (images, videos)
- Comments and nested replies
- Reactions (likes, love, wow, etc.)
- Post sharing and tagging
- User timeline and global feed

### 2. **Chat Module** (Real-time Messaging)
- 1:1 private conversations
- Group chats (up to 500+ members)
- Real-time typing indicators
- Message read receipts
- Media sharing in chats (images, videos, files)
- Voice and video call signaling (Phase 2)

### 3. **Video Sharing & Shorts Module** (Like YouTube + TikTok)
- **Long-form videos**: Upload, transcode, stream
- **Short-form videos (Shorts)**: Vertical videos (15-60 seconds)
- Video player with quality selection
- Video thumbnails and previews
- Video search and discovery
- Video analytics (views, watch time)
- Video comments and reactions
- Trending shorts algorithm

### Supporting Services
- **Notifications** (for comments, reactions, messages, follows, video uploads, etc.)
- **Media Processing** via GCP Storage + Media CDN + Cloud Run (transcoding)
- **Realtime layer** using SignalR + Redis

You must follow the structure, names, and phases described here.

---

## 1. Global Tech Stack (Backend)

Copilot, assume the following stack and conventions:

- **Runtime / Framework**
  - `.NET 10`
  - `ASP.NET Core Web API`
  - `SignalR` for realtime

- **Architecture**
  - **Microservices** with **Clean Architecture + CQRS** for domain-heavy services
  - Each service is a **separate project** and **separate container**, deployable and scalable independently

- **Data Stores**
  - **MongoDB** for:
    - `posts`, `comments`, `reactions`
    - `messages`, `conversations`
    - `notifications`
    - `videos`, `video_metadata`, `video_shorts`
    - `video_analytics` (views, watch time, engagement)
  - **Redis** for:
    - Unread counters (notifications, chat)
    - Presence (who is online)
    - SignalR backplane
    - Video view counters and trending scores
    - Video processing job statuses
  - **SQL (PostgreSQL / SQL Server)** for:
    - Auth / Identity (users, credentials, tokens)
    - Any strict relational/billing data later

- **Media & CDN**
  - **GCP Cloud Storage** for media (images, videos, thumbnails)
  - **GCP Media CDN** for global delivery
  - **Cloud Run** for video transcoding jobs (FFmpeg-based)
  - **Cloud Tasks** or **Pub/Sub** for async video processing
  - Uploads are done via **signed URLs**
  - **HLS/DASH** streaming for adaptive bitrate videos

- **Messaging / Events**
  - **Phase 1**: services may call each other via REST directly
  - **Phase 2**: introduce **event bus** (Kafka / Pub/Sub / Service Bus) for domain events

---

## 2. High-Level Solution Layout

Copilot, create a backend solution with this *top-level layout*:

```text
/WeChat.Backend
  /src
    /Gateway.Api/                # API Gateway / BFF (YARP-based or reverse proxy)
    /Realtime.Api/               # SignalR hubs (FeedHub, ChatHub, NotificationsHub, VideoHub)

    # Core Services
    /AuthService.Api/
    /UserProfileService.Api/

    # Feed Module
    /PostFeedService.Api/

    # Chat Module
    /ChatService.Api/

    # Video Module
    /VideoService.Api/           # Video uploads, shorts, streaming, analytics
    /VideoProcessing.Worker/     # Background worker for video transcoding

    # Supporting Services
    /NotificationService.Api/
    /MediaService.Api/           # General media (images, files)
    /SearchService.Api/          # Video search, feed search (Phase 2)

    /Shared/
      /Shared.Contracts/         # DTOs, event contracts
      /Shared.Infrastructure/    # Common infra helpers (Mongo, Redis, logging, etc.)
      /Shared.Domain/            # Base abstractions (Result, IEntity, etc.)

  /deploy/                       # Docker, Kubernetes manifests, infra docs (later)
  /tests/                        # Integration and unit tests
  README.md
```

**Rules:**

- Each `*.Api` project is a **standalone Web API** with its own `Program.cs`.
- Use **appsettings.json** + environment variables for connection strings and secrets.
- Use **JWT auth** everywhere; downstream services trust tokens from `AuthService`.

---

## 3. Phase 1 – Core Services & Responsibilities

Phase 1 is a solid MVP that can already scale to hundreds of thousands of users.

> Copilot: Implement these services first, as separate projects.

### 3.1 AuthService.Api

**Responsibilities**

- User registration, login, logout.
- Issue **JWT access tokens** + refresh tokens.
- Validate credentials and manage security.

**Tech & Data**

- `.NET 10`, ASP.NET Core Web API.
- Use **SQL** DB (PostgreSQL or SQL Server).
- Use ASP.NET Core Identity (or custom) as you see fit, but:
  - Expose a clear `UserId` (GUID/UUID) claim.
  - Use `sub` / `nameid` claim for user identifier in JWT.

**Endpoints (examples)**

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`

---

### 3.2 UserProfileService.Api

**Responsibilities**

- Public profile data:
  - Display name, bio, avatar, banner.
  - Followers/following, user stats.
- Provide profile info for Feed, Chat, Notifications.

**Tech & Data**

- `.NET 10` Web API.
- Data store: **MongoDB** or **SQL** (either is acceptable, lean toward Mongo for flexibility).
- `profiles` collection/table:
  - `userId`, `displayName`, `avatarUrl`, `bio`, `followersCount`, etc.

**Key endpoints**

- `GET /api/profiles/{userId}`
- `GET /api/profiles/by-username/{username}`
- `PUT /api/profiles/{userId}`

---

### 3.3 PostFeedService.Api

**Responsibilities**

- Posts, comments, reactions, and feed queries.
- Integration with **MediaService** for media URLs.
- Provide clean feed endpoints for the frontend.

**Tech & Data**

- `.NET 10` Web API with **Clean Architecture + CQRS**.
- **MongoDB** collections:
  - `posts` – post metadata + media references.
  - `comments` – comments per post.
  - `reactions` – likes/emoji per post (or embedded in posts as sub-docs).
- **Redis**:
  - `post_counters:{postId}` (commentCount, reactionCount)
  - Optional feed caching (hot pages).

**Endpoints**

- `POST /api/posts`
- `GET /api/feed` (paged, supports filters, personalization later)
- `GET /api/users/{userId}/posts`
- `POST /api/posts/{postId}/comments`
- `POST /api/posts/{postId}/reactions`

**Realtime**

- After saving comments/reactions:
  - Use `IHubContext<FeedHub>` (in Realtime.Api) to broadcast:
    - `CommentAdded`
    - `ReactionUpdated`
    - Updated counters

---

### 3.4 ChatService.Api

**Responsibilities**

- Manage conversations: 1:1 or group.
- Store messages and read states.
- Provide REST APIs for history and conversation lists.
- Cooperate with **ChatHub** for realtime.

**Tech & Data**

- `.NET 10` Web API.
- **MongoDB**:
  - `conversations` – participants, metadata.
  - `messages` – conversationId, senderId, text, media, timestamps.
- **Redis**:
  - `chat_unread:{userId}:{conversationId}` for quick unread counts.

**Endpoints**

- `GET /api/chat/conversations`
- `GET /api/chat/conversations/{conversationId}/messages`
- `POST /api/chat/conversations/{conversationId}/messages`

**Realtime**

- After persisting messages:
  - Use `IHubContext<ChatHub>` to broadcast `MessageReceived` to `conv:{conversationId}` group.

---

### 3.5 NotificationService.Api

**Responsibilities**

- Convert domain events (comments, reactions, follows, messages) into **user notifications**.
- Maintain **unread notification counts**.
- Expose notification APIs for frontend.
- Push live notifications via **NotificationsHub** when users are online.

**Tech & Data**

- `.NET 10` Web API + background workers (hosted services).
- **MongoDB**:
  - `notifications` collection.
- **Redis**:
  - `notif_unread:{userId}` integer.

**Phase 1 communication (simpler)**

- For now, domain services call NotificationService via HTTP:
  - Example: `PostFeedService` POSTs `CommentCreated` payload to NotificationService.
  - Later (Phase 2), this becomes an event bus message.

**Endpoints**

- `GET /api/notifications`
- `GET /api/notifications/unread-count`
- `POST /api/notifications/mark-read`
- `POST /api/notifications/mark-all-read`

**Realtime**

- After notification is created + unread count updated:
  - `IHubContext<NotificationsHub>.Clients.User(userId).SendAsync("NewNotification", { notification, unreadCount });`

---

### 3.6 MediaService.Api

**Responsibilities**

- Handle media uploads:
  - Generate **signed URLs** for GCP Cloud Storage.
  - Validate content type/size.
- Return **CDN-ready URLs** to be stored in `posts`, `messages`, `profiles`.

**Tech & Data**

- `.NET 10` Web API.
- GCP Cloud Storage SDK.
- No heavy DB (metadata can live in Post/Chat/Profile services).

**Endpoints**

- `POST /api/media/upload-url`
  (returns signed URL, path, contentType, expiry)

---

### 3.7 VideoService.Api

**Responsibilities**

- Manage video uploads (long-form and shorts)
- Trigger video transcoding and processing
- Provide video streaming URLs (HLS/DASH manifests)
- Track video analytics (views, watch time, completion rate)
- Manage video metadata (title, description, tags, thumbnails)
- Serve video feeds (shorts feed, trending, recommended)
- Handle video comments and reactions (integrated with PostFeedService patterns)

**Tech & Data**

- `.NET 10` Web API with **Clean Architecture + CQRS**.
- **MongoDB** collections:
  - `videos` – video metadata, upload status, processing status, URLs
  - `video_shorts` – short-form videos with additional metadata (trending score, swipe position)
  - `video_analytics` – views, watch time, user engagement per video
  - `video_comments` – comments specific to videos (or reuse PostFeedService)
  - `video_playlists` – user-created playlists
- **Redis**:
  - `video_views:{videoId}` – view counters
  - `video_processing:{videoId}` – processing job status
  - `trending_shorts` – sorted set for trending algorithm
  - `video_watch_time:{videoId}` – aggregated watch time
- **GCP Cloud Storage** buckets:
  - `raw-videos/` – original uploads
  - `transcoded-videos/` – processed videos (multiple qualities)
  - `thumbnails/` – auto-generated and custom thumbnails
  - `hls-manifests/` – HLS streaming manifests

**Video Processing Pipeline**

1. Client requests upload URL from `/api/videos/upload-url`
2. Client uploads to signed GCS URL
3. VideoService receives webhook/notification from GCS
4. VideoService publishes `VideoUploadCompleted` event
5. `VideoProcessing.Worker` picks up event:
   - Extracts metadata (duration, resolution, codec)
   - Generates thumbnails (at 0s, 25%, 50%, 75%)
   - Transcodes to multiple qualities (1080p, 720p, 480p, 360p)
   - Generates HLS/DASH manifests
   - Updates video status to `Ready`
6. VideoService notifies user via NotificationService

**Shorts-Specific Features**

- Maximum duration: 60 seconds
- Vertical aspect ratio (9:16)
- Infinite scroll feed
- Auto-play on view
- Swipe gesture navigation
- Trending algorithm based on:
  - View velocity (views per hour)
  - Completion rate
  - Engagement (likes, comments, shares)
  - Recency

**Endpoints**

**Video Upload & Management**
- `POST /api/videos/upload-url` – Get signed URL for upload
- `POST /api/videos` – Create video metadata
- `GET /api/videos/{videoId}` – Get video details
- `PUT /api/videos/{videoId}` – Update video metadata
- `DELETE /api/videos/{videoId}` – Delete video
- `GET /api/users/{userId}/videos` – Get user's videos

**Video Shorts**
- `POST /api/videos/shorts/upload-url` – Get signed URL for short video
- `POST /api/videos/shorts` – Create short video
- `GET /api/videos/shorts/feed` – Get shorts feed (infinite scroll, paginated)
- `GET /api/videos/shorts/trending` – Get trending shorts
- `GET /api/videos/shorts/{shortId}` – Get specific short
- `GET /api/users/{userId}/shorts` – Get user's shorts

**Video Streaming**
- `GET /api/videos/{videoId}/stream` – Get HLS manifest URL
- `GET /api/videos/{videoId}/qualities` – Get available quality options
- `POST /api/videos/{videoId}/view` – Track video view (updates analytics)

**Video Analytics**
- `GET /api/videos/{videoId}/analytics` – Get video analytics
- `POST /api/videos/{videoId}/watch-time` – Update watch time (periodic heartbeat)
- `GET /api/users/{userId}/analytics` – Get creator analytics

**Video Engagement**
- `POST /api/videos/{videoId}/reactions` – Add reaction (like, love, etc.)
- `POST /api/videos/{videoId}/comments` – Add comment
- `GET /api/videos/{videoId}/comments` – Get comments
- `POST /api/videos/{videoId}/share` – Track share

**Video Search & Discovery**
- `GET /api/videos/search?q={query}` – Search videos
- `GET /api/videos/recommended` – Get recommended videos for user
- `GET /api/videos/trending` – Get trending videos
- `GET /api/videos/following` – Videos from followed users

**Realtime**

- After video processing completes:
  - Use `IHubContext<VideoHub>` to notify uploader: `VideoProcessingCompleted`
- When new shorts are added:
  - Broadcast to `VideoHub` for feed updates
- Live view counters:
  - Broadcast view count updates to `video:{videoId}` group

**Data Models (MongoDB)**

```json
// videos collection
{
  "_id": "ObjectId",
  "videoId": "uuid",
  "userId": "uuid",
  "title": "string",
  "description": "string",
  "tags": ["array"],
  "type": "long-form | short",
  "duration": 125.5,
  "aspectRatio": "16:9",
  "uploadStatus": "uploading | uploaded | processing | ready | failed",
  "rawVideoUrl": "gs://bucket/path",
  "transcodedUrls": {
    "1080p": "https://cdn/path/1080p.mp4",
    "720p": "https://cdn/path/720p.mp4",
    "480p": "https://cdn/path/480p.mp4",
    "360p": "https://cdn/path/360p.mp4"
  },
  "hlsManifestUrl": "https://cdn/path/master.m3u8",
  "thumbnailUrl": "https://cdn/path/thumb.jpg",
  "thumbnails": ["url1", "url2", "url3"],
  "viewCount": 1000,
  "likeCount": 150,
  "commentCount": 45,
  "shareCount": 20,
  "visibility": "public | private | unlisted",
  "createdAt": "ISODate",
  "updatedAt": "ISODate",
  "processedAt": "ISODate"
}

// video_analytics collection
{
  "_id": "ObjectId",
  "videoId": "uuid",
  "views": 1000,
  "uniqueViews": 850,
  "totalWatchTime": 98750, // seconds
  "averageWatchTime": 97.75,
  "completionRate": 0.65,
  "engagementRate": 0.25,
  "peakConcurrentViewers": 150,
  "geographicData": {},
  "deviceData": {},
  "referralSources": {},
  "hourlyViews": [],
  "date": "ISODate"
}
```

---

### 3.8 VideoProcessing.Worker

**Responsibilities**

- Background service that processes video transcoding jobs
- Polls GCP Pub/Sub or internal queue for `VideoUploadCompleted` events
- Uses FFmpeg for video processing
- Updates video status in VideoService

**Tech & Implementation**

- `.NET 10` Worker Service (hosted service)
- Docker container with FFmpeg installed
- Runs as **Cloud Run Job** or **Kubernetes Job**
- For Phase 1: Can be part of VideoService.Api as background service
- For Phase 2: Separate scalable worker pool

**Processing Steps**

```csharp
// Pseudo-code workflow
1. Receive VideoUploadCompleted event
2. Download raw video from GCS
3. Extract metadata (FFprobe)
4. Generate thumbnails (FFmpeg)
5. Transcode to multiple qualities in parallel:
   - 1080p @ 5Mbps
   - 720p @ 2.5Mbps
   - 480p @ 1Mbps
   - 360p @ 500Kbps
6. Generate HLS manifest
7. Upload all outputs to GCS
8. Update video record in MongoDB
9. Publish VideoProcessingCompleted event
10. Trigger notification to user
```

---

### 3.9 Realtime.Api (SignalR Gateway)

**Responsibilities**

- Provide WebSocket endpoints via SignalR:
  - `/hubs/feed` → **FeedHub**
  - `/hubs/chat` → **ChatHub**
  - `/hubs/notifications` → **NotificationsHub**
  - `/hubs/video` → **VideoHub**
- Use **Redis backplane** for multi-instance scale-out.
- Authenticate users via JWT.

**Tech & Config**

- `.NET 10`, `Microsoft.AspNetCore.SignalR`.
- `AddStackExchangeRedis` for backplane.
- Hubs:
  - `FeedHub`: `JoinPostGroup`, `LeavePostGroup`, server methods to broadcast `CommentAdded`, `ReactionUpdated`.
  - `ChatHub`: `JoinConversation`, `LeaveConversation`, `TypingStarted`, `TypingStopped`, `MessageReceived`.
  - `NotificationsHub`: manages user connections and sends `NewNotification`.
  - `VideoHub`: `JoinVideoRoom`, `LeaveVideoRoom`, `VideoProcessingCompleted`, `VideoViewCountUpdated`, `NewShortAvailable`.

---

### 3.10 Gateway.Api (API Gateway / BFF)

**Responsibilities**

- Single entry point for frontend.
- Terminates TLS, validates JWT, routes to services.
- Simple aggregation endpoints if needed.

**Tech**

- `.NET 10` with **YARP** (Yet Another Reverse Proxy), or a separate gateway (Nginx/Envoy) – prefer YARP for tight integration.

**Routing examples**

- `/api/auth/*` → AuthService
- `/api/profiles/*` → UserProfileService
- `/api/posts/*`, `/api/feed/*` → PostFeedService
- `/api/chat/*` → ChatService
- `/api/notifications/*` → NotificationService
- `/api/media/*` → MediaService (images, general files)
- `/api/videos/*` → VideoService (video uploads, shorts, streaming, analytics)
- `/hubs/*` → Realtime.Api

---

## 4. Cross-Cutting Infrastructure (Phase 1)

> Copilot: Create Shared infrastructure to keep services consistent.

### 4.1 Shared.Contracts

- DTOs and contracts shared across services:
  - **Feed Module**: `CommentDto`, `ReactionDto`, `PostDto`, `PostCountersDto`
  - **Chat Module**: `ChatMessageDto`, `ConversationSummaryDto`, `TypingIndicatorDto`
  - **Video Module**: `VideoDto`, `VideoShortDto`, `VideoAnalyticsDto`, `VideoQualityDto`, `VideoCommentDto`
  - **Notifications**: `NotificationDto`, `NotificationType`
  - **Common**: `UserProfileDto`, `PaginationDto`, `ApiResponse<T>`
  - Simple event DTOs for Phase 1 (HTTP-based events):
    - `CommentCreatedEvent`, `ReactionAddedEvent`
    - `MessageSentEvent`, `TypingStartedEvent`
    - `VideoUploadCompletedEvent`, `VideoProcessingCompletedEvent`
    - `VideoViewedEvent`, `VideoSharedEvent`

### 4.2 Shared.Infrastructure

- Mongo helper:
  - Register `IMongoClient`, `IMongoDatabase`.
  - Generic repository patterns if needed.
- Redis helper:
  - Connection multiplexer registrations.
  - Helper methods for `INCRBY`, `GET`, `SET`.
- Logging & correlation IDs.
- JWT validation helper.

---

## 5. Phase 1 – What Copilot Should Scaffold

> Copilot, follow these steps to scaffold Phase 1:

1. **Create Solution & Projects**
   - Create `/WeChat.Backend` solution.
   - Add projects:
     - `Gateway.Api`
     - `Realtime.Api`
     - `AuthService.Api`
     - `UserProfileService.Api`
     - `PostFeedService.Api`
     - `ChatService.Api`
     - `NotificationService.Api`
     - `MediaService.Api`
     - `VideoService.Api`
     - `VideoProcessing.Worker`
     - `SearchService.Api` (Phase 2, optional for Phase 1)
     - `Shared.Contracts`
     - `Shared.Infrastructure`
     - `Shared.Domain` (if needed).

2. **Configure Auth**
   - In `AuthService.Api`, implement JWT issuing.
   - Ensure all other services and Realtime.Api use the same JWT validation config.

3. **Configure Mongo & Redis**
   - In `Shared.Infrastructure`, build reusable Mongo & Redis registration extensions.
   - In each service, use these to set up connections and register repositories.

4. **Implement basic CRUD & endpoints**
   - For each service, implement minimal endpoints described above.
   - Use DTOs from `Shared.Contracts`.

5. **Implement SignalR Realtime.Api**
   - Set up:
     - `FeedHub`, `ChatHub`, `NotificationsHub`, `VideoHub`.
   - Configure Redis backplane.
   - Implement simple methods for joining groups, broadcasting events.

6. **Wire domain services to hubs**
   - In PostFeedService, after saving a comment/reaction:
     - Use `IHubContext<FeedHub>` to send `CommentAdded` or `ReactionUpdated`.
   - In NotificationService, after saving notifications:
     - Use `IHubContext<NotificationsHub>` to send `NewNotification`.
   - In ChatService, after saving a message:
     - Use `IHubContext<ChatHub>` to send `MessageReceived`.
   - In VideoService, after video processing completes:
     - Use `IHubContext<VideoHub>` to send `VideoProcessingCompleted`.
   - In VideoService, when view counts update:
     - Use `IHubContext<VideoHub>` to send `VideoViewCountUpdated`.

7. **Create Dockerfiles**
   - Each `*.Api` project should have a basic Dockerfile.
   - Prepare for future Kubernetes deployment.

---

## 6. Phase 2 – Scale to Millions (Extensions for Copilot)

Phase 2 reuses this architecture but adds more robustness.

> Copilot, do **not** implement Phase 2 immediately; instead, keep Phase 1 code modular and ready for the following additions.

### 6.1 Introduce Event Bus

- Add a `Messaging` package or module:
  - For Kafka / Pub/Sub / Service Bus.
- Replace direct HTTP calls from PostFeedService → NotificationService with events:
  - `CommentCreated`, `ReactionAdded`, `MessageSent`, `UserFollowed`.
- NotificationService consumes events asynchronously.

### 6.2 Sharding & DB Scaling

- Prepare Mongo for sharding:
  - Shard keys:
    - Posts: `postId` or `userId`.
    - Comments: `postId`.
    - Messages: `conversationId`.
    - Notifications: `userId`.
    - Videos: `videoId` or `userId`.
    - Video Analytics: `videoId` and date-based.
- Add read replicas for heavy-read services (especially VideoService and PostFeedService).

### 6.3 Video Processing at Scale

- **Distributed Video Transcoding**:
  - Move VideoProcessing.Worker to a scalable worker pool (Cloud Run Jobs, Kubernetes Jobs).
  - Use a job queue (GCP Pub/Sub, Cloud Tasks) for video processing jobs.
  - Implement priority queues (shorts get processed faster than long-form).

- **CDN & Caching**:
  - Use GCP Media CDN for video delivery.
  - Implement edge caching for popular videos.
  - Cache HLS manifests and thumbnails aggressively.

- **Video Analytics Pipeline**:
  - Stream video view events to BigQuery for analytics.
  - Use batch processing for trending algorithm calculations.
  - Cache trending scores in Redis (update hourly).

- **Storage Optimization**:
  - Implement lifecycle policies (delete raw videos after 30 days).
  - Use cold storage for old, rarely-watched videos.
  - Implement adaptive bitrate streaming (HLS/DASH).

### 6.4 Dedicated Read Models

- Build read-optimized projections for:
  - Feed timelines (denormalized documents per user).
  - Notification lists / summaries.
  - Video feeds (trending, recommended, following).
  - Video search indices (Elasticsearch or Algolia).
- Use background workers to update read models from events.

### 6.5 More Realtime Scaling

- Run multiple instances of Realtime.Api behind a load balancer.
- Consider splitting Realtime services if needed:
  - `Realtime.Feed`, `Realtime.Chat`, `Realtime.Notifications`, `Realtime.Video`.
- Use Redis Cluster for SignalR backplane at scale.
- Implement connection throttling and rate limiting.

### 6.6 Observability

- Add OpenTelemetry:
  - Distributed traces gateway → services → DB.
- Add structured logging and metrics.
- Configure dashboards for:
  - Request latency
  - Error rates
  - Message throughput (comments, likes, messages)
  - Video metrics:
    - Videos uploaded per hour
    - Video processing time
    - Video views and watch time
    - CDN bandwidth usage
    - Trending algorithm performance

---

## 7. Final Instructions to Copilot

> Copilot, in this repo you should:
>
> 1. Scaffold the **solution and all projects** listed in section 2 for **WeChat.com Backend**.
> 2. For **Phase 1**, fully implement:
>    - **Core Services**: `AuthService.Api`, `UserProfileService.Api`
>    - **Feed Module**: `PostFeedService.Api`
>    - **Chat Module**: `ChatService.Api`
>    - **Video Module**: `VideoService.Api`, `VideoProcessing.Worker`
>    - **Supporting Services**: `NotificationService.Api`, `MediaService.Api`
>    - **Infrastructure**: `Realtime.Api`, `Gateway.Api`
> 3. Use **MongoDB** and **Redis** consistently via `Shared.Infrastructure`.
> 4. Implement **Realtime** using SignalR with Redis backplane and the **four hubs**:
>    - **FeedHub** (comments/reactions on posts)
>    - **ChatHub** (messages/typing indicators)
>    - **NotificationsHub** (notifications + unread counts)
>    - **VideoHub** (video processing status, view counts, new shorts)
> 5. Keep every service **independently deployable** and **stateless**, so that we can scale them horizontally.
> 6. Structure the code so that **Phase 2** changes (event bus, sharding, read models, video transcoding at scale) can be added without breaking the API contracts or rewriting the services.
> 7. For **VideoService**:
>    - Implement video upload via signed URLs
>    - Support both long-form videos and shorts (with different validation)
>    - Implement basic video processing workflow
>    - Track video analytics (views, watch time)
>    - Provide trending shorts algorithm
>    - Integrate with GCP Cloud Storage and prepare for CDN integration
>
> Always keep domain logic in the respective services; hubs and gateways must stay thin translation layers to the outside world.

---

## 8. Implementation Priorities

### Priority 1: Core Foundation (Week 1-2)
1. Setup solution structure and shared libraries
2. Implement `AuthService.Api` with JWT
3. Implement `UserProfileService.Api`
4. Setup MongoDB and Redis infrastructure
5. Implement `Gateway.Api` with YARP routing

### Priority 2: Feed Module (Week 2-3)
1. Implement `PostFeedService.Api` (posts, comments, reactions)
2. Setup `FeedHub` in `Realtime.Api`
3. Wire PostFeed → FeedHub for real-time updates
4. Implement `NotificationService.Api` for post notifications

### Priority 3: Chat Module (Week 3-4)
1. Implement `ChatService.Api` (conversations, messages)
2. Setup `ChatHub` in `Realtime.Api`
3. Wire ChatService → ChatHub for real-time messaging
4. Implement read receipts and typing indicators

### Priority 4: Video Module (Week 4-6)
1. Implement `MediaService.Api` for general media uploads
2. Implement `VideoService.Api`:
   - Video upload endpoints (signed URLs)
   - Video metadata management
   - Video streaming endpoints
   - Shorts feed and trending algorithm
3. Implement `VideoProcessing.Worker` (basic version):
   - Video metadata extraction
   - Thumbnail generation
   - Basic transcoding (can be enhanced in Phase 2)
4. Setup `VideoHub` in `Realtime.Api`
5. Wire VideoService → VideoHub for processing updates
6. Implement video analytics tracking

### Priority 5: Polish & Testing (Week 6-7)
1. Integration testing across services
2. Performance testing
3. Documentation
4. Docker and deployment preparation

---

## 9. Database Schema Summary

### MongoDB Collections

#### **Feed Module**
- `posts` – Post content, media references, counters
- `comments` – Comments on posts
- `reactions` – User reactions to posts

#### **Chat Module**
- `conversations` – Conversation metadata, participants
- `messages` – Chat messages

#### **Video Module**
- `videos` – Video metadata, upload/processing status, URLs
- `video_shorts` – Short-form videos with trending data
- `video_analytics` – Aggregated video analytics
- `video_comments` – Comments on videos (or reuse feed comments)

#### **Notifications Module**
- `notifications` – User notifications

#### **User Module**
- `profiles` – User profile data (or can be in PostgreSQL)

### PostgreSQL Tables (SQL)

#### **Auth Module**
- `users` – User credentials, email, password hash
- `refresh_tokens` – JWT refresh tokens

### Redis Keys

#### **Counters**
- `post_counters:{postId}` – Post metrics (likes, comments)
- `video_views:{videoId}` – Video view counts
- `chat_unread:{userId}:{conversationId}` – Unread message counts
- `notif_unread:{userId}` – Unread notification counts

#### **Trending & Analytics**
- `trending_shorts` – Sorted set for trending shorts
- `video_watch_time:{videoId}` – Aggregated watch time

#### **Processing**
- `video_processing:{videoId}` – Video processing job status

#### **Presence & Realtime**
- `online_users` – Set of online user IDs
- SignalR backplane keys (managed by StackExchange.Redis)

---

## 10. API Endpoint Summary

### AuthService.Api
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`

### UserProfileService.Api
- `GET /api/profiles/{userId}`
- `GET /api/profiles/by-username/{username}`
- `PUT /api/profiles/{userId}`
- `POST /api/profiles/{userId}/follow`
- `DELETE /api/profiles/{userId}/follow`

### PostFeedService.Api
- `POST /api/posts`
- `GET /api/feed` (paginated)
- `GET /api/posts/{postId}`
- `GET /api/users/{userId}/posts`
- `POST /api/posts/{postId}/comments`
- `GET /api/posts/{postId}/comments`
- `POST /api/posts/{postId}/reactions`
- `DELETE /api/posts/{postId}/reactions`

### ChatService.Api
- `GET /api/chat/conversations`
- `GET /api/chat/conversations/{conversationId}`
- `POST /api/chat/conversations` (create/start)
- `GET /api/chat/conversations/{conversationId}/messages`
- `POST /api/chat/conversations/{conversationId}/messages`
- `PUT /api/chat/conversations/{conversationId}/read`

### VideoService.Api
- `POST /api/videos/upload-url`
- `POST /api/videos`
- `GET /api/videos/{videoId}`
- `PUT /api/videos/{videoId}`
- `DELETE /api/videos/{videoId}`
- `GET /api/users/{userId}/videos`
- `POST /api/videos/shorts/upload-url`
- `POST /api/videos/shorts`
- `GET /api/videos/shorts/feed`
- `GET /api/videos/shorts/trending`
- `GET /api/videos/{videoId}/stream`
- `POST /api/videos/{videoId}/view`
- `GET /api/videos/{videoId}/analytics`
- `POST /api/videos/{videoId}/reactions`
- `POST /api/videos/{videoId}/comments`
- `GET /api/videos/search`
- `GET /api/videos/recommended`

### NotificationService.Api
- `GET /api/notifications`
- `GET /api/notifications/unread-count`
- `POST /api/notifications/mark-read`
- `POST /api/notifications/mark-all-read`

### MediaService.Api
- `POST /api/media/upload-url`

### Realtime.Api (SignalR Hubs)
- `/hubs/feed` – FeedHub
- `/hubs/chat` – ChatHub
- `/hubs/notifications` – NotificationsHub
- `/hubs/video` – VideoHub

---

"# wechat-backend" 
