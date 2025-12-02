# Chat API - Complete Request & Response Documentation

> **Purpose**: Complete API documentation with request/response examples for real-time chat and messaging
> **Last Updated**: 2025-12-02
> **Base URL**: `https://api.yourapp.com/api/v1`

---

## Table of Contents
1. [Authentication](#authentication)
2. [Conversations](#conversations)
3. [Messages](#messages)
4. [Message Actions](#message-actions)
5. [Real-Time Features](#real-time-features)
6. [File Attachments](#file-attachments)
7. [Search](#search)
8. [Error Responses](#error-responses)

---

## Authentication

All API requests require JWT Bearer authentication.

**Headers Required**:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

---

## Conversations

### 1. Get All Conversations

**Endpoint**: `GET /api/v1/conversations`

**Description**: Get list of all conversations for authenticated user

**Request**:
```http
GET /api/v1/conversations?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "conversations": [
      {
        "_id": "conv-67890abcdef1234567890001",
        "type": "OneOnOne",
        "participants": [
          {
            "userId": "550e8400-e29b-41d4-a716-446655440000",
            "username": "john_doe",
            "displayName": "John Doe",
            "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
            "isOnline": true
          },
          {
            "userId": "660e8400-e29b-41d4-a716-446655440001",
            "username": "jane_smith",
            "displayName": "Jane Smith",
            "avatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
            "isOnline": false,
            "lastSeenAt": "2025-12-02T10:30:00Z"
          }
        ],
        "lastMessage": {
          "messageId": "msg-12345",
          "content": "Hey! How are you doing?",
          "senderId": "660e8400-e29b-41d4-a716-446655440001",
          "sentAt": "2025-12-02T10:30:00Z"
        },
        "unreadCount": 3,
        "isArchived": false,
        "isMuted": false,
        "createdAt": "2025-11-15T08:00:00Z",
        "updatedAt": "2025-12-02T10:30:00Z"
      },
      {
        "_id": "conv-67890abcdef1234567890002",
        "type": "Group",
        "name": "Project Alpha Team",
        "description": "Discussion about Project Alpha",
        "avatarUrl": "https://cdn.yourapp.com/groups/project-alpha.jpg",
        "participants": [
          {
            "userId": "550e8400-e29b-41d4-a716-446655440000",
            "username": "john_doe",
            "displayName": "John Doe",
            "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
            "role": "Admin",
            "isOnline": true
          },
          {
            "userId": "770e8400-e29b-41d4-a716-446655440001",
            "username": "alice_johnson",
            "displayName": "Alice Johnson",
            "avatarUrl": "https://cdn.yourapp.com/avatars/alice_johnson.jpg",
            "role": "Member",
            "isOnline": true
          },
          {
            "userId": "880e8400-e29b-41d4-a716-446655440001",
            "username": "bob_wilson",
            "displayName": "Bob Wilson",
            "avatarUrl": "https://cdn.yourapp.com/avatars/bob_wilson.jpg",
            "role": "Member",
            "isOnline": false
          }
        ],
        "participantsCount": 3,
        "lastMessage": {
          "messageId": "msg-67890",
          "content": "Meeting scheduled for tomorrow at 10 AM",
          "senderId": "770e8400-e29b-41d4-a716-446655440001",
          "sentAt": "2025-12-02T09:00:00Z"
        },
        "unreadCount": 0,
        "isArchived": false,
        "isMuted": false,
        "createdAt": "2025-10-01T12:00:00Z",
        "updatedAt": "2025-12-02T09:00:00Z"
      }
    ],
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

### 2. Get Conversation by ID

**Endpoint**: `GET /api/v1/conversations/{conversationId}`

**Description**: Get detailed information about a specific conversation

**Request**:
```http
GET /api/v1/conversations/conv-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "_id": "conv-67890abcdef1234567890001",
    "type": "OneOnOne",
    "participants": [
      {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "displayName": "John Doe",
        "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
        "isOnline": true
      },
      {
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane_smith",
        "displayName": "Jane Smith",
        "avatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
        "isOnline": false,
        "lastSeenAt": "2025-12-02T10:30:00Z"
      }
    ],
    "lastMessage": {
      "messageId": "msg-12345",
      "content": "Hey! How are you doing?",
      "senderId": "660e8400-e29b-41d4-a716-446655440001",
      "sentAt": "2025-12-02T10:30:00Z"
    },
    "unreadCount": 3,
    "isArchived": false,
    "isMuted": false,
    "settings": {
      "notifications": true,
      "messageSounds": true
    },
    "createdAt": "2025-11-15T08:00:00Z",
    "updatedAt": "2025-12-02T10:30:00Z"
  }
}
```

---

### 3. Create One-on-One Conversation

**Endpoint**: `POST /api/v1/conversations/direct`

**Description**: Create or get existing one-on-one conversation

**Request**:
```http
POST /api/v1/conversations/direct HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "participantId": "660e8400-e29b-41d4-a716-446655440001"
}
```

**Success Response** (201 Created or 200 OK if exists):
```json
{
  "success": true,
  "message": "Conversation created successfully",
  "data": {
    "_id": "conv-67890abcdef1234567890001",
    "type": "OneOnOne",
    "participants": [
      {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "displayName": "John Doe",
        "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg"
      },
      {
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane_smith",
        "displayName": "Jane Smith",
        "avatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg"
      }
    ],
    "createdAt": "2025-12-02T11:00:00Z"
  }
}
```

---

### 4. Create Group Conversation

**Endpoint**: `POST /api/v1/conversations/group`

**Description**: Create a new group conversation

**Request**:
```http
POST /api/v1/conversations/group HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "name": "Project Alpha Team",
  "description": "Discussion about Project Alpha",
  "participantIds": [
    "770e8400-e29b-41d4-a716-446655440001",
    "880e8400-e29b-41d4-a716-446655440001",
    "990e8400-e29b-41d4-a716-446655440001"
  ]
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Group created successfully",
  "data": {
    "_id": "conv-67890abcdef1234567890003",
    "type": "Group",
    "name": "Project Alpha Team",
    "description": "Discussion about Project Alpha",
    "createdBy": {
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "displayName": "John Doe"
    },
    "participants": [
      {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "displayName": "John Doe",
        "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
        "role": "Admin"
      },
      {
        "userId": "770e8400-e29b-41d4-a716-446655440001",
        "username": "alice_johnson",
        "displayName": "Alice Johnson",
        "avatarUrl": "https://cdn.yourapp.com/avatars/alice_johnson.jpg",
        "role": "Member"
      },
      {
        "userId": "880e8400-e29b-41d4-a716-446655440001",
        "username": "bob_wilson",
        "displayName": "Bob Wilson",
        "avatarUrl": "https://cdn.yourapp.com/avatars/bob_wilson.jpg",
        "role": "Member"
      },
      {
        "userId": "990e8400-e29b-41d4-a716-446655440001",
        "username": "charlie_brown",
        "displayName": "Charlie Brown",
        "avatarUrl": "https://cdn.yourapp.com/avatars/charlie_brown.jpg",
        "role": "Member"
      }
    ],
    "participantsCount": 4,
    "createdAt": "2025-12-02T11:15:00Z"
  }
}
```

---

### 5. Update Group Details

**Endpoint**: `PUT /api/v1/conversations/{conversationId}`

**Description**: Update group name, description, or avatar

**Request**:
```http
PUT /api/v1/conversations/conv-67890abcdef1234567890003 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "name": "Project Alpha - Core Team",
  "description": "Core team discussion for Project Alpha development"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Group updated successfully",
  "data": {
    "_id": "conv-67890abcdef1234567890003",
    "name": "Project Alpha - Core Team",
    "description": "Core team discussion for Project Alpha development",
    "updatedAt": "2025-12-02T11:30:00Z"
  }
}
```

---

### 6. Add Participants to Group

**Endpoint**: `POST /api/v1/conversations/{conversationId}/participants`

**Description**: Add new participants to group conversation

**Request**:
```http
POST /api/v1/conversations/conv-67890abcdef1234567890003/participants HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "participantIds": [
    "aa0e8400-e29b-41d4-a716-446655440001",
    "bb0e8400-e29b-41d4-a716-446655440001"
  ]
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Participants added successfully",
  "data": {
    "conversationId": "conv-67890abcdef1234567890003",
    "addedParticipants": [
      {
        "userId": "aa0e8400-e29b-41d4-a716-446655440001",
        "username": "david_lee",
        "displayName": "David Lee",
        "role": "Member"
      },
      {
        "userId": "bb0e8400-e29b-41d4-a716-446655440001",
        "username": "emma_davis",
        "displayName": "Emma Davis",
        "role": "Member"
      }
    ],
    "participantsCount": 6
  }
}
```

---

### 7. Remove Participant from Group

**Endpoint**: `DELETE /api/v1/conversations/{conversationId}/participants/{userId}`

**Description**: Remove a participant from group conversation

**Request**:
```http
DELETE /api/v1/conversations/conv-67890abcdef1234567890003/participants/bb0e8400-e29b-41d4-a716-446655440001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Participant removed successfully",
  "data": {
    "conversationId": "conv-67890abcdef1234567890003",
    "removedUserId": "bb0e8400-e29b-41d4-a716-446655440001",
    "participantsCount": 5
  }
}
```

---

### 8. Leave Group

**Endpoint**: `POST /api/v1/conversations/{conversationId}/leave`

**Description**: Leave a group conversation

**Request**:
```http
POST /api/v1/conversations/conv-67890abcdef1234567890003/leave HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "You have left the group",
  "data": {
    "conversationId": "conv-67890abcdef1234567890003",
    "leftAt": "2025-12-02T11:45:00Z"
  }
}
```

---

### 9. Archive Conversation

**Endpoint**: `POST /api/v1/conversations/{conversationId}/archive`

**Description**: Archive a conversation

**Request**:
```http
POST /api/v1/conversations/conv-67890abcdef1234567890001/archive HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Conversation archived",
  "data": {
    "conversationId": "conv-67890abcdef1234567890001",
    "isArchived": true
  }
}
```

---

### 10. Mute/Unmute Conversation

**Endpoint**: `POST /api/v1/conversations/{conversationId}/mute`

**Description**: Mute or unmute conversation notifications

**Request**:
```http
POST /api/v1/conversations/conv-67890abcdef1234567890001/mute HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "muted": true,
  "muteUntil": "2025-12-03T11:00:00Z"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Conversation muted",
  "data": {
    "conversationId": "conv-67890abcdef1234567890001",
    "isMuted": true,
    "muteUntil": "2025-12-03T11:00:00Z"
  }
}
```

---

## Messages

### 11. Get Messages

**Endpoint**: `GET /api/v1/conversations/{conversationId}/messages`

**Description**: Get messages from a conversation with pagination

**Request**:
```http
GET /api/v1/conversations/conv-67890abcdef1234567890001/messages?page=1&limit=50&before=2025-12-02T12:00:00Z HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters**:
- `page`: Page number (default: 1)
- `limit`: Messages per page (default: 50, max: 100)
- `before`: Get messages before this timestamp (ISO 8601)
- `after`: Get messages after this timestamp (ISO 8601)

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "messages": [
      {
        "_id": "msg-67890abcdef1234567890001",
        "conversationId": "conv-67890abcdef1234567890001",
        "senderId": "660e8400-e29b-41d4-a716-446655440001",
        "senderUsername": "jane_smith",
        "senderDisplayName": "Jane Smith",
        "senderAvatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
        "content": "Hey! How are you doing?",
        "messageType": "Text",
        "readBy": [
          {
            "userId": "660e8400-e29b-41d4-a716-446655440001",
            "username": "jane_smith",
            "readAt": "2025-12-02T10:30:00Z"
          }
        ],
        "deliveredTo": [
          {
            "userId": "550e8400-e29b-41d4-a716-446655440000",
            "username": "john_doe",
            "deliveredAt": "2025-12-02T10:30:05Z"
          }
        ],
        "reactions": [],
        "mentions": [],
        "isEdited": false,
        "isDeleted": false,
        "createdAt": "2025-12-02T10:30:00Z"
      },
      {
        "_id": "msg-67890abcdef1234567890002",
        "conversationId": "conv-67890abcdef1234567890001",
        "senderId": "550e8400-e29b-41d4-a716-446655440000",
        "senderUsername": "john_doe",
        "senderDisplayName": "John Doe",
        "senderAvatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
        "content": "I'm doing great! How about you?",
        "messageType": "Text",
        "readBy": [
          {
            "userId": "550e8400-e29b-41d4-a716-446655440000",
            "username": "john_doe",
            "readAt": "2025-12-02T10:35:00Z"
          },
          {
            "userId": "660e8400-e29b-41d4-a716-446655440001",
            "username": "jane_smith",
            "readAt": "2025-12-02T10:36:00Z"
          }
        ],
        "deliveredTo": [
          {
            "userId": "660e8400-e29b-41d4-a716-446655440001",
            "username": "jane_smith",
            "deliveredAt": "2025-12-02T10:35:02Z"
          }
        ],
        "reactions": [
          {
            "emoji": "👍",
            "userId": "660e8400-e29b-41d4-a716-446655440001",
            "username": "jane_smith",
            "addedAt": "2025-12-02T10:36:30Z"
          }
        ],
        "mentions": [],
        "replyTo": {
          "messageId": "msg-67890abcdef1234567890001",
          "content": "Hey! How are you doing?",
          "senderId": "660e8400-e29b-41d4-a716-446655440001",
          "senderUsername": "jane_smith"
        },
        "isEdited": false,
        "isDeleted": false,
        "createdAt": "2025-12-02T10:35:00Z"
      },
      {
        "_id": "msg-67890abcdef1234567890003",
        "conversationId": "conv-67890abcdef1234567890001",
        "senderId": "660e8400-e29b-41d4-a716-446655440001",
        "senderUsername": "jane_smith",
        "senderDisplayName": "Jane Smith",
        "senderAvatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
        "content": "Check out this cool photo!",
        "messageType": "Media",
        "mediaUrls": [
          {
            "type": "Image",
            "url": "https://cdn.yourapp.com/chat/image1.jpg",
            "thumbnailUrl": "https://cdn.yourapp.com/chat/thumbs/image1.jpg",
            "size": 2048576,
            "width": 1920,
            "height": 1080
          }
        ],
        "readBy": [
          {
            "userId": "660e8400-e29b-41d4-a716-446655440001",
            "username": "jane_smith",
            "readAt": "2025-12-02T10:40:00Z"
          }
        ],
        "deliveredTo": [],
        "reactions": [],
        "mentions": [],
        "isEdited": false,
        "isDeleted": false,
        "createdAt": "2025-12-02T10:40:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 50,
      "totalCount": 234,
      "hasMore": true
    }
  }
}
```

---

### 12. Send Text Message

**Endpoint**: `POST /api/v1/conversations/{conversationId}/messages`

**Description**: Send a text message to a conversation

**Request**:
```http
POST /api/v1/conversations/conv-67890abcdef1234567890001/messages HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "content": "Hello! This is a test message.",
  "messageType": "Text",
  "mentions": [
    "660e8400-e29b-41d4-a716-446655440001"
  ]
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Message sent successfully",
  "data": {
    "_id": "msg-67890abcdef1234567890010",
    "conversationId": "conv-67890abcdef1234567890001",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "senderUsername": "john_doe",
    "senderDisplayName": "John Doe",
    "senderAvatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
    "content": "Hello! This is a test message.",
    "messageType": "Text",
    "readBy": [
      {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "readAt": "2025-12-02T12:00:00Z"
      }
    ],
    "deliveredTo": [],
    "reactions": [],
    "mentions": [
      {
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane_smith",
        "displayName": "Jane Smith"
      }
    ],
    "isEdited": false,
    "isDeleted": false,
    "createdAt": "2025-12-02T12:00:00Z"
  }
}
```

---

### 13. Send Media Message

**Endpoint**: `POST /api/v1/conversations/{conversationId}/messages`

**Description**: Send a message with media attachments

**Request**:
```http
POST /api/v1/conversations/conv-67890abcdef1234567890001/messages HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "content": "Check out these photos from yesterday!",
  "messageType": "Media",
  "mediaUrls": [
    {
      "type": "Image",
      "url": "https://cdn.yourapp.com/chat/photo1.jpg",
      "thumbnailUrl": "https://cdn.yourapp.com/chat/thumbs/photo1.jpg",
      "size": 2048576,
      "width": 1920,
      "height": 1080
    },
    {
      "type": "Image",
      "url": "https://cdn.yourapp.com/chat/photo2.jpg",
      "thumbnailUrl": "https://cdn.yourapp.com/chat/thumbs/photo2.jpg",
      "size": 1536789,
      "width": 1920,
      "height": 1080
    }
  ]
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Message sent successfully",
  "data": {
    "_id": "msg-67890abcdef1234567890011",
    "conversationId": "conv-67890abcdef1234567890001",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "content": "Check out these photos from yesterday!",
    "messageType": "Media",
    "mediaUrls": [
      {
        "type": "Image",
        "url": "https://cdn.yourapp.com/chat/photo1.jpg",
        "thumbnailUrl": "https://cdn.yourapp.com/chat/thumbs/photo1.jpg",
        "size": 2048576,
        "width": 1920,
        "height": 1080
      },
      {
        "type": "Image",
        "url": "https://cdn.yourapp.com/chat/photo2.jpg",
        "thumbnailUrl": "https://cdn.yourapp.com/chat/thumbs/photo2.jpg",
        "size": 1536789,
        "width": 1920,
        "height": 1080
      }
    ],
    "createdAt": "2025-12-02T12:05:00Z"
  }
}
```

---

### 14. Reply to Message

**Endpoint**: `POST /api/v1/conversations/{conversationId}/messages`

**Description**: Send a reply to a specific message

**Request**:
```http
POST /api/v1/conversations/conv-67890abcdef1234567890001/messages HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "content": "That's awesome! Congratulations!",
  "messageType": "Text",
  "replyToMessageId": "msg-67890abcdef1234567890001"
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Reply sent successfully",
  "data": {
    "_id": "msg-67890abcdef1234567890012",
    "conversationId": "conv-67890abcdef1234567890001",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "content": "That's awesome! Congratulations!",
    "messageType": "Text",
    "replyTo": {
      "messageId": "msg-67890abcdef1234567890001",
      "content": "Hey! How are you doing?",
      "senderId": "660e8400-e29b-41d4-a716-446655440001",
      "senderUsername": "jane_smith"
    },
    "createdAt": "2025-12-02T12:10:00Z"
  }
}
```

---

## Message Actions

### 15. Edit Message

**Endpoint**: `PUT /api/v1/messages/{messageId}`

**Description**: Edit a previously sent message

**Request**:
```http
PUT /api/v1/messages/msg-67890abcdef1234567890010 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "content": "Hello! This is an updated message."
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Message updated successfully",
  "data": {
    "_id": "msg-67890abcdef1234567890010",
    "content": "Hello! This is an updated message.",
    "isEdited": true,
    "editedAt": "2025-12-02T12:15:00Z"
  }
}
```

**Error Response** (403 Forbidden):
```json
{
  "success": false,
  "error": {
    "code": "EDIT_TIME_EXPIRED",
    "message": "Messages can only be edited within 15 minutes of sending",
    "statusCode": 403
  }
}
```

---

### 16. Delete Message

**Endpoint**: `DELETE /api/v1/messages/{messageId}`

**Description**: Delete a message (soft delete)

**Request**:
```http
DELETE /api/v1/messages/msg-67890abcdef1234567890010?deleteFor=me HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters**:
- `deleteFor`: `me` (delete for yourself) or `everyone` (delete for all)

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Message deleted successfully",
  "data": {
    "messageId": "msg-67890abcdef1234567890010",
    "deletedFor": "me",
    "deletedAt": "2025-12-02T12:20:00Z"
  }
}
```

---

### 17. Mark Messages as Read

**Endpoint**: `POST /api/v1/conversations/{conversationId}/read`

**Description**: Mark all messages in conversation as read

**Request**:
```http
POST /api/v1/conversations/conv-67890abcdef1234567890001/read HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "lastReadMessageId": "msg-67890abcdef1234567890015"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Messages marked as read",
  "data": {
    "conversationId": "conv-67890abcdef1234567890001",
    "messagesRead": 5,
    "lastReadMessageId": "msg-67890abcdef1234567890015",
    "readAt": "2025-12-02T12:25:00Z"
  }
}
```

---

### 18. Add Reaction

**Endpoint**: `POST /api/v1/messages/{messageId}/reactions`

**Description**: Add emoji reaction to a message

**Request**:
```http
POST /api/v1/messages/msg-67890abcdef1234567890010/reactions HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "emoji": "👍"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Reaction added successfully",
  "data": {
    "messageId": "msg-67890abcdef1234567890010",
    "reaction": {
      "emoji": "👍",
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "addedAt": "2025-12-02T12:30:00Z"
    }
  }
}
```

---

### 19. Remove Reaction

**Endpoint**: `DELETE /api/v1/messages/{messageId}/reactions`

**Description**: Remove your reaction from a message

**Request**:
```http
DELETE /api/v1/messages/msg-67890abcdef1234567890010/reactions?emoji=👍 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Reaction removed successfully",
  "data": {
    "messageId": "msg-67890abcdef1234567890010",
    "removedEmoji": "👍"
  }
}
```

---

## Real-Time Features

### 20. Connect to SignalR

**WebSocket Connection**:
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/chat", {
    accessTokenFactory: () => authToken
  })
  .build();

await connection.start();
```

---

### 21. Send Typing Indicator

**SignalR Method**:
```javascript
// Start typing
await connection.invoke("StartTyping", "conv-67890abcdef1234567890001");

// Stop typing
await connection.invoke("StopTyping", "conv-67890abcdef1234567890001");
```

**Receive Typing Event**:
```javascript
connection.on("UserTyping", (conversationId, userId, username, isTyping) => {
  console.log(`${username} is ${isTyping ? 'typing' : 'stopped typing'}`);
});
```

---

### 22. Real-Time Message Received

**SignalR Event**:
```javascript
connection.on("MessageReceived", (message) => {
  console.log("New message:", message);
  // {
  //   "_id": "msg-67890abcdef1234567890020",
  //   "conversationId": "conv-67890abcdef1234567890001",
  //   "senderId": "660e8400-e29b-41d4-a716-446655440001",
  //   "content": "New real-time message!",
  //   "createdAt": "2025-12-02T12:40:00Z"
  // }
});
```

---

### 23. Real-Time Message Read Receipt

**SignalR Event**:
```javascript
connection.on("MessageRead", (data) => {
  console.log("Message read:", data);
  // {
  //   "messageId": "msg-67890abcdef1234567890020",
  //   "conversationId": "conv-67890abcdef1234567890001",
  //   "userId": "550e8400-e29b-41d4-a716-446655440000",
  //   "username": "john_doe",
  //   "readAt": "2025-12-02T12:41:00Z"
  // }
});
```

---

### 24. Online Status Updates

**SignalR Events**:
```javascript
// User went online
connection.on("UserOnline", (userId, username) => {
  console.log(`${username} is now online`);
});

// User went offline
connection.on("UserOffline", (userId, username, lastSeenAt) => {
  console.log(`${username} is now offline. Last seen: ${lastSeenAt}`);
});
```

---

## File Attachments

### 25. Upload File for Chat

**Endpoint**: `POST /api/v1/chat/upload`

**Description**: Upload a file to send in chat

**Request**:
```http
POST /api/v1/chat/upload HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary

------WebKitFormBoundary
Content-Disposition: form-data; name="file"; filename="document.pdf"
Content-Type: application/pdf

[binary file data]
------WebKitFormBoundary--
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "File uploaded successfully",
  "data": {
    "fileId": "file-67890abcdef1234567890001",
    "type": "Document",
    "url": "https://cdn.yourapp.com/chat/files/document.pdf",
    "fileName": "document.pdf",
    "fileSize": 524288,
    "mimeType": "application/pdf",
    "uploadedAt": "2025-12-02T12:50:00Z"
  }
}
```

**File Type Limits**:
- **Images**: JPEG, PNG, GIF, WebP (max 10MB)
- **Videos**: MP4, MOV, AVI (max 100MB)
- **Documents**: PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX (max 20MB)
- **Audio**: MP3, WAV, M4A (max 20MB)

---

## Search

### 26. Search Messages

**Endpoint**: `GET /api/v1/conversations/{conversationId}/search`

**Description**: Search messages within a conversation

**Request**:
```http
GET /api/v1/conversations/conv-67890abcdef1234567890001/search?q=meeting&page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "messages": [
      {
        "_id": "msg-67890abcdef1234567890025",
        "conversationId": "conv-67890abcdef1234567890001",
        "senderId": "660e8400-e29b-41d4-a716-446655440001",
        "senderUsername": "jane_smith",
        "content": "Let's schedule a meeting for tomorrow at 10 AM",
        "messageType": "Text",
        "createdAt": "2025-12-01T15:00:00Z"
      },
      {
        "_id": "msg-67890abcdef1234567890030",
        "conversationId": "conv-67890abcdef1234567890001",
        "senderId": "550e8400-e29b-41d4-a716-446655440000",
        "senderUsername": "john_doe",
        "content": "Sounds good! Meeting confirmed for tomorrow.",
        "messageType": "Text",
        "createdAt": "2025-12-01T15:05:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalCount": 2,
      "hasMore": false
    }
  }
}
```

---

### 27. Search All Conversations

**Endpoint**: `GET /api/v1/conversations/search`

**Description**: Search messages across all conversations

**Request**:
```http
GET /api/v1/conversations/search?q=project&page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "results": [
      {
        "message": {
          "_id": "msg-67890abcdef1234567890035",
          "conversationId": "conv-67890abcdef1234567890003",
          "senderId": "770e8400-e29b-41d4-a716-446655440001",
          "senderUsername": "alice_johnson",
          "content": "The project deadline is next Friday",
          "createdAt": "2025-12-01T14:00:00Z"
        },
        "conversation": {
          "_id": "conv-67890abcdef1234567890003",
          "type": "Group",
          "name": "Project Alpha Team"
        }
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalCount": 15,
      "hasMore": false
    }
  }
}
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

**Conversation Not Found (404)**:
```json
{
  "success": false,
  "error": {
    "code": "CONVERSATION_NOT_FOUND",
    "message": "Conversation not found",
    "statusCode": 404
  }
}
```

**Not a Participant (403)**:
```json
{
  "success": false,
  "error": {
    "code": "NOT_PARTICIPANT",
    "message": "You are not a participant in this conversation",
    "statusCode": 403
  }
}
```

**Message Too Long (400)**:
```json
{
  "success": false,
  "error": {
    "code": "MESSAGE_TOO_LONG",
    "message": "Message content exceeds maximum length of 5000 characters",
    "statusCode": 400
  }
}
```

**File Too Large (413)**:
```json
{
  "success": false,
  "error": {
    "code": "FILE_TOO_LARGE",
    "message": "File size exceeds maximum allowed size",
    "statusCode": 413,
    "maxSize": "10MB"
  }
}
```

---

**End of Documentation**
