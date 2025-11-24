# 7Media Backend – GitHub Copilot Architecture & Scaffolding Guide

> Copilot, you are my senior backend engineer for **7Media**.  
> Implement a **microservices backend** that can scale from **Phase 1 (MVP)** to **Phase 2 (millions of users)** without rewriting everything.

The platform combines:

- **Feed / Posts** (text + media, comments, reactions)
- **Chat** (1:1 + groups)
- **Notifications** (for comments, reactions, messages, follows, etc.)
- **Media** upload via GCP Storage + Media CDN
- A **Realtime layer** using SignalR + Redis

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
  - **Redis** for:
    - Unread counters (notifications, chat)
    - Presence (who is online)
    - SignalR backplane
  - **SQL (PostgreSQL / SQL Server)** for:
    - Auth / Identity (users, credentials, tokens)
    - Any strict relational/billing data later

- **Media & CDN**
  - **GCP Cloud Storage** for media (images, videos)
  - **GCP Media CDN** for global delivery
  - Uploads are done via **signed URLs**

- **Messaging / Events**
  - **Phase 1**: services may call each other via REST directly
  - **Phase 2**: introduce **event bus** (Kafka / Pub/Sub / Service Bus) for domain events

---

## 2. High-Level Solution Layout

Copilot, create a backend solution with this *top-level layout*:

```text
/7Media.Backend
  /src
    /Gateway.Api/                # API Gateway / BFF (YARP-based or reverse proxy)
    /Realtime.Api/               # SignalR hubs (FeedHub, ChatHub, NotificationsHub)

    /AuthService.Api/
    /UserProfileService.Api/
    /PostFeedService.Api/
    /ChatService.Api/
    /NotificationService.Api/
    /MediaService.Api/

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

### 3.7 Realtime.Api (SignalR Gateway)

**Responsibilities**

- Provide WebSocket endpoints via SignalR:
  - `/hubs/feed` → **FeedHub**
  - `/hubs/chat` → **ChatHub**
  - `/hubs/notifications` → **NotificationsHub**
- Use **Redis backplane** for multi-instance scale-out.
- Authenticate users via JWT.

**Tech & Config**

- `.NET 10`, `Microsoft.AspNetCore.SignalR`.
- `AddStackExchangeRedis` for backplane.
- Hubs:
  - `FeedHub`: `JoinPostGroup`, `LeavePostGroup`, server methods to broadcast `CommentAdded`, `ReactionUpdated`.
  - `ChatHub`: `JoinConversation`, `LeaveConversation`, `TypingStarted`, `TypingStopped`, `MessageReceived`.
  - `NotificationsHub`: manages user connections and sends `NewNotification`.

---

### 3.8 Gateway.Api (API Gateway / BFF)

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
- `/api/media/*` → MediaService
- `/hubs/*` → Realtime.Api

---

## 4. Cross-Cutting Infrastructure (Phase 1)

> Copilot: Create Shared infrastructure to keep services consistent.

### 4.1 Shared.Contracts

- DTOs and contracts shared across services:
  - `CommentDto`, `ReactionDto`, `PostDto`, `PostCountersDto`
  - `NotificationDto`, `NotificationType`
  - `ChatMessageDto`, `ConversationSummaryDto`
  - Simple event DTOs for Phase 1 (HTTP-based events), e.g. `CommentCreatedEvent`.

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
   - Create `/7Media.Backend` solution.
   - Add projects:
     - `Gateway.Api`
     - `Realtime.Api`
     - `AuthService.Api`
     - `UserProfileService.Api`
     - `PostFeedService.Api`
     - `ChatService.Api`
     - `NotificationService.Api`
     - `MediaService.Api`
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
     - `FeedHub`, `ChatHub`, `NotificationsHub`.
   - Configure Redis backplane.
   - Implement simple methods for joining groups, broadcasting events.

6. **Wire domain services to hubs**
   - In PostFeedService, after saving a comment/reaction:
     - Use `IHubContext<FeedHub>` to send `CommentAdded` or `ReactionUpdated`.
   - In NotificationService, after saving notifications:
     - Use `IHubContext<NotificationsHub>` to send `NewNotification`.
   - In ChatService, after saving a message:
     - Use `IHubContext<ChatHub>` to send `MessageReceived`.

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
- Add read replicas for heavy-read services.

### 6.3 Dedicated Read Models

- Build read-optimized projections for:
  - Feed timelines (denormalized documents per user).
  - Notification lists / summaries.
- Use background workers to update read models from events.

### 6.4 More Realtime Scaling

- Run multiple instances of Realtime.Api behind a load balancer.
- Consider splitting Realtime services if needed:
  - `Realtime.Feed`, `Realtime.Chat`, `Realtime.Notifications`.

### 6.5 Observability

- Add OpenTelemetry:
  - Distributed traces gateway → services → DB.
- Add structured logging and metrics.
- Configure dashboards for:
  - Request latency
  - Error rates
  - Message throughput (comments, likes, messages)

---

## 7. Final Instructions to Copilot

> Copilot, in this repo you should:
>
> 1. Scaffold the **solution and all projects** listed in section 2.
> 2. For **Phase 1**, fully implement:
>    - `AuthService.Api`, `UserProfileService.Api`, `PostFeedService.Api`, `ChatService.Api`, `NotificationService.Api`, `MediaService.Api`, `Realtime.Api`, `Gateway.Api`.
> 3. Use **MongoDB** and **Redis** consistently via `Shared.Infrastructure`.
> 4. Implement **Realtime** using SignalR with Redis backplane and the three hubs:
>    - FeedHub (comments/reactions)
>    - ChatHub (messages/typing)
>    - NotificationsHub (notifications + unread counts)
> 5. Keep every service **independently deployable** and **stateless**, so that we can scale them horizontally.
> 6. Structure the code so that **Phase 2** changes (event bus, sharding, read models) can be added without breaking the API contracts or rewriting the services.
>
> Always keep domain logic in the respective services; hubs and gateways must stay thin translation layers to the outside world.
"# wechat-backend" 
