# Notification API - Complete Request & Response Documentation

> **Purpose**: Complete API documentation with request/response examples for notification management
> **Last Updated**: 2025-12-02
> **Base URL**: `https://api.yourapp.com/api/v1`

---

## Table of Contents
1. [Authentication](#authentication)
2. [Get Notifications](#get-notifications)
3. [Notification Actions](#notification-actions)
4. [Notification Settings](#notification-settings)
5. [Error Responses](#error-responses)

---

## Authentication

All API requests require JWT Bearer authentication.

**Headers Required**:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

---

## Get Notifications

### 1. Get All Notifications

**Endpoint**: `GET /api/v1/notifications`

**Description**: Get all notifications for authenticated user

**Request**:
```http
GET /api/v1/notifications?page=1&limit=20&type=All&isRead=false HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters**:
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 20, max: 100)
- `type`: Filter by type - `All`, `FriendRequest`, `Message`, `Post`, `Comment`, `Like`, `Mention`, `Follow` (default: All)
- `isRead`: Filter by read status - `true`, `false`, or omit for all (optional)

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "notifications": [
      {
        "_id": "notif-67890abcdef1234567890001",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "type": "FriendRequest",
        "title": "New Friend Request",
        "message": "Jane Smith sent you a friend request",
        "actorId": "660e8400-e29b-41d4-a716-446655440001",
        "actorUsername": "jane_smith",
        "actorDisplayName": "Jane Smith",
        "actorAvatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
        "relatedEntityId": "freq-67890abcdef1234567890001",
        "relatedEntityType": "FriendRequest",
        "actionUrl": "/friend-requests/freq-67890abcdef1234567890001",
        "isRead": false,
        "createdAt": "2025-12-02T10:00:00Z",
        "expiresAt": "2025-12-09T10:00:00Z"
      },
      {
        "_id": "notif-67890abcdef1234567890002",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "type": "Like",
        "title": "Post Liked",
        "message": "Alice Johnson and 12 others liked your post",
        "actorId": "770e8400-e29b-41d4-a716-446655440001",
        "actorUsername": "alice_johnson",
        "actorDisplayName": "Alice Johnson",
        "actorAvatarUrl": "https://cdn.yourapp.com/avatars/alice_johnson.jpg",
        "relatedEntityId": "post-67890abcdef1234567890001",
        "relatedEntityType": "Post",
        "actionUrl": "/posts/post-67890abcdef1234567890001",
        "metadata": {
          "totalLikes": 13,
          "postPreview": "Just launched my new project! 🚀"
        },
        "isRead": false,
        "createdAt": "2025-12-02T09:45:00Z",
        "expiresAt": "2025-12-09T09:45:00Z"
      },
      {
        "_id": "notif-67890abcdef1234567890003",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "type": "Comment",
        "title": "New Comment",
        "message": "Bob Wilson commented on your post",
        "actorId": "880e8400-e29b-41d4-a716-446655440001",
        "actorUsername": "bob_wilson",
        "actorDisplayName": "Bob Wilson",
        "actorAvatarUrl": "https://cdn.yourapp.com/avatars/bob_wilson.jpg",
        "relatedEntityId": "post-67890abcdef1234567890001",
        "relatedEntityType": "Post",
        "actionUrl": "/posts/post-67890abcdef1234567890001#comment-67890abcdef1234567890010",
        "metadata": {
          "commentPreview": "This is amazing! Congratulations!",
          "postPreview": "Just launched my new project! 🚀"
        },
        "isRead": true,
        "readAt": "2025-12-02T09:30:00Z",
        "createdAt": "2025-12-02T09:15:00Z",
        "expiresAt": "2025-12-09T09:15:00Z"
      },
      {
        "_id": "notif-67890abcdef1234567890004",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "type": "Message",
        "title": "New Message",
        "message": "Jane Smith sent you a message",
        "actorId": "660e8400-e29b-41d4-a716-446655440001",
        "actorUsername": "jane_smith",
        "actorDisplayName": "Jane Smith",
        "actorAvatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
        "relatedEntityId": "conv-67890abcdef1234567890001",
        "relatedEntityType": "Conversation",
        "actionUrl": "/messages/conv-67890abcdef1234567890001",
        "metadata": {
          "messagePreview": "Hey! How are you doing?"
        },
        "isRead": false,
        "createdAt": "2025-12-02T08:30:00Z",
        "expiresAt": "2025-12-09T08:30:00Z"
      },
      {
        "_id": "notif-67890abcdef1234567890005",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "type": "Follow",
        "title": "New Follower",
        "message": "Charlie Brown started following you",
        "actorId": "990e8400-e29b-41d4-a716-446655440001",
        "actorUsername": "charlie_brown",
        "actorDisplayName": "Charlie Brown",
        "actorAvatarUrl": "https://cdn.yourapp.com/avatars/charlie_brown.jpg",
        "relatedEntityId": "990e8400-e29b-41d4-a716-446655440001",
        "relatedEntityType": "User",
        "actionUrl": "/profiles/990e8400-e29b-41d4-a716-446655440001",
        "isRead": false,
        "createdAt": "2025-12-02T07:00:00Z",
        "expiresAt": "2025-12-09T07:00:00Z"
      },
      {
        "_id": "notif-67890abcdef1234567890006",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "type": "Mention",
        "title": "You Were Mentioned",
        "message": "David Lee mentioned you in a post",
        "actorId": "aa0e8400-e29b-41d4-a716-446655440001",
        "actorUsername": "david_lee",
        "actorDisplayName": "David Lee",
        "actorAvatarUrl": "https://cdn.yourapp.com/avatars/david_lee.jpg",
        "relatedEntityId": "post-67890abcdef1234567890020",
        "relatedEntityType": "Post",
        "actionUrl": "/posts/post-67890abcdef1234567890020",
        "metadata": {
          "postPreview": "Great collaboration with @john_doe on this project!"
        },
        "isRead": false,
        "createdAt": "2025-12-02T06:15:00Z",
        "expiresAt": "2025-12-09T06:15:00Z"
      }
    ],
    "unreadCount": 5,
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 3,
      "totalCount": 47,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### 2. Get Unread Notifications Count

**Endpoint**: `GET /api/v1/notifications/unread/count`

**Description**: Get count of unread notifications

**Request**:
```http
GET /api/v1/notifications/unread/count HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "totalUnread": 5,
    "byType": {
      "FriendRequest": 1,
      "Like": 1,
      "Comment": 0,
      "Message": 1,
      "Follow": 1,
      "Mention": 1,
      "Post": 0
    }
  }
}
```

---

### 3. Get Notification by ID

**Endpoint**: `GET /api/v1/notifications/{notificationId}`

**Description**: Get detailed information about a specific notification

**Request**:
```http
GET /api/v1/notifications/notif-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "_id": "notif-67890abcdef1234567890001",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "type": "FriendRequest",
    "title": "New Friend Request",
    "message": "Jane Smith sent you a friend request",
    "actorId": "660e8400-e29b-41d4-a716-446655440001",
    "actorUsername": "jane_smith",
    "actorDisplayName": "Jane Smith",
    "actorAvatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
    "relatedEntityId": "freq-67890abcdef1234567890001",
    "relatedEntityType": "FriendRequest",
    "actionUrl": "/friend-requests/freq-67890abcdef1234567890001",
    "isRead": false,
    "createdAt": "2025-12-02T10:00:00Z",
    "expiresAt": "2025-12-09T10:00:00Z"
  }
}
```

---

## Notification Actions

### 4. Mark Notification as Read

**Endpoint**: `PUT /api/v1/notifications/{notificationId}/read`

**Description**: Mark a notification as read

**Request**:
```http
PUT /api/v1/notifications/notif-67890abcdef1234567890001/read HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Notification marked as read",
  "data": {
    "notificationId": "notif-67890abcdef1234567890001",
    "isRead": true,
    "readAt": "2025-12-02T11:00:00Z"
  }
}
```

---

### 5. Mark All Notifications as Read

**Endpoint**: `PUT /api/v1/notifications/read-all`

**Description**: Mark all notifications as read

**Request**:
```http
PUT /api/v1/notifications/read-all HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "type": "All"
}
```

**Request Body** (optional):
- `type`: Mark only specific type as read - `All`, `FriendRequest`, `Message`, etc. (default: All)

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "All notifications marked as read",
  "data": {
    "markedAsRead": 5,
    "readAt": "2025-12-02T11:05:00Z"
  }
}
```

---

### 6. Delete Notification

**Endpoint**: `DELETE /api/v1/notifications/{notificationId}`

**Description**: Delete a specific notification

**Request**:
```http
DELETE /api/v1/notifications/notif-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Notification deleted successfully",
  "data": {
    "notificationId": "notif-67890abcdef1234567890001",
    "deletedAt": "2025-12-02T11:10:00Z"
  }
}
```

---

### 7. Clear All Read Notifications

**Endpoint**: `DELETE /api/v1/notifications/clear-read`

**Description**: Delete all read notifications

**Request**:
```http
DELETE /api/v1/notifications/clear-read HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Read notifications cleared successfully",
  "data": {
    "deletedCount": 15,
    "deletedAt": "2025-12-02T11:15:00Z"
  }
}
```

---

## Notification Settings

### 8. Get Notification Preferences

**Endpoint**: `GET /api/v1/notifications/preferences`

**Description**: Get user's notification preferences

**Request**:
```http
GET /api/v1/notifications/preferences HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "preferences": {
      "emailNotifications": true,
      "pushNotifications": true,
      "messageNotifications": true,
      "friendRequestNotifications": true,
      "postNotifications": true,
      "commentNotifications": true,
      "likeNotifications": false,
      "mentionNotifications": true,
      "followerNotifications": true
    }
  }
}
```

---

### 9. Update Notification Preferences

**Endpoint**: `PUT /api/v1/notifications/preferences`

**Description**: Update notification preferences

**Request**:
```http
PUT /api/v1/notifications/preferences HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "emailNotifications": true,
  "pushNotifications": true,
  "messageNotifications": true,
  "friendRequestNotifications": true,
  "postNotifications": false,
  "commentNotifications": true,
  "likeNotifications": false,
  "mentionNotifications": true,
  "followerNotifications": false
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Notification preferences updated successfully",
  "data": {
    "preferences": {
      "emailNotifications": true,
      "pushNotifications": true,
      "messageNotifications": true,
      "friendRequestNotifications": true,
      "postNotifications": false,
      "commentNotifications": true,
      "likeNotifications": false,
      "mentionNotifications": true,
      "followerNotifications": false
    },
    "updatedAt": "2025-12-02T11:20:00Z"
  }
}
```

---

## Real-Time Notifications

### 10. Connect to SignalR for Real-Time Notifications

**WebSocket Connection**:
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/notifications", {
    accessTokenFactory: () => authToken
  })
  .build();

await connection.start();
```

---

### 11. Receive Real-Time Notifications

**SignalR Event**:
```javascript
connection.on("NotificationReceived", (notification) => {
  console.log("New notification:", notification);
  // {
  //   "_id": "notif-67890abcdef1234567890010",
  //   "type": "Like",
  //   "title": "Post Liked",
  //   "message": "Emma Davis liked your post",
  //   "actorId": "bb0e8400-e29b-41d4-a716-446655440001",
  //   "actorUsername": "emma_davis",
  //   "actorDisplayName": "Emma Davis",
  //   "actorAvatarUrl": "https://cdn.yourapp.com/avatars/emma_davis.jpg",
  //   "actionUrl": "/posts/post-67890abcdef1234567890001",
  //   "createdAt": "2025-12-02T11:25:00Z"
  // }
});

// Update unread count
connection.on("UnreadCountChanged", (count) => {
  console.log("Unread notifications:", count);
});
```

---

## Error Responses

### Standard Error Format

```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "statusCode": 400
  }
}
```

### Common Error Codes

**Notification Not Found (404)**:
```json
{
  "success": false,
  "error": {
    "code": "NOTIFICATION_NOT_FOUND",
    "message": "Notification not found",
    "statusCode": 404
  }
}
```

**Already Read (400)**:
```json
{
  "success": false,
  "error": {
    "code": "ALREADY_READ",
    "message": "Notification has already been marked as read",
    "statusCode": 400
  }
}
```

---

## Notification Types

### All Supported Types

1. **FriendRequest**: New friend request received
2. **FriendAccepted**: Friend request accepted
3. **Message**: New message received
4. **Post**: Friend posted something new
5. **Comment**: Someone commented on your post
6. **CommentReply**: Someone replied to your comment
7. **Like**: Someone liked your post/comment
8. **Share**: Someone shared your post
9. **Mention**: You were mentioned in a post/comment
10. **Follow**: New follower
11. **ProfileView**: Someone viewed your profile (premium)

---

## Notification Expiry

- Notifications automatically expire after **7 days**
- Expired notifications are automatically deleted
- TTL (Time To Live) index ensures automatic cleanup
- No action required from client side

---

**End of Documentation**
