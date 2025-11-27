# Chat API - Integration Guide

Complete guide for integrating Chat and Messaging endpoints into your UI application.

---

## Table of Contents

1. [Overview](#overview)
2. [Base URL](#base-url)
3. [Response Format](#response-format)
4. [Chat Flow](#chat-flow)
5. [API Endpoints](#api-endpoints)
   - **Conversation Management**
     - [Create Conversation](#1-create-conversation)
     - [Get Conversation](#2-get-conversation)
     - [Get User Conversations](#3-get-user-conversations)
     - [Update Conversation](#4-update-conversation)
     - [Delete Conversation](#5-delete-conversation)
     - [Add Participant](#6-add-participant)
     - [Remove Participant](#7-remove-participant)
     - [Mute Conversation](#8-mute-conversation)
     - [Unmute Conversation](#9-unmute-conversation)
     - [Get Unread Count](#10-get-unread-count)
   - **Message Management**
     - [Send Message](#11-send-message)
     - [Get Message](#12-get-message)
     - [Edit Message](#13-edit-message)
     - [Delete Message](#14-delete-message)
     - [Get Conversation Messages](#15-get-conversation-messages)
     - [Get Messages Before](#16-get-messages-before)
     - [Get Messages After](#17-get-messages-after)
     - [Mark Message as Read](#18-mark-message-as-read)
     - [Search Messages](#19-search-messages)
6. [Real-time Communication](#real-time-communication)
7. [Message Types](#message-types)
8. [Error Handling](#error-handling)
9. [Integration Examples](#integration-examples)

---

## Overview

The WeChat Chat API provides complete messaging functionality for one-to-one and group conversations. It supports:

- One-to-one conversations (private chat)
- Group conversations with multiple participants
- Multiple message types (text, image, video, audio, file, location, etc.)
- Message editing and deletion
- Read receipts and delivery status
- Conversation muting
- Message search
- Real-time messaging via SignalR

**Authentication Method**: Bearer Token (JWT) - Required for all endpoints

**Message Storage**: MongoDB for scalability and performance

---

## Base URL

| Environment | URL |
|-------------|-----|
| Development | `http://localhost:5004` |
| Production  | `https://api.wechat.com` |

**Chat Service Port**: `5004`

---

## Response Format

All API responses follow a consistent format:

### Success Response

```json
{
  "success": true,
  "data": {
    // Response data here
  },
  "error": null,
  "errors": null,
  "timestamp": "2025-11-27T10:30:00Z"
}
```

### Error Response

```json
{
  "success": false,
  "data": null,
  "error": "Error message here",
  "errors": ["Detailed error 1", "Detailed error 2"],
  "timestamp": "2025-11-27T10:30:00Z"
}
```

---

## Chat Flow

```
┌─────────────────────────────────────────────────────────────┐
│                   One-to-One Chat Flow                       │
└─────────────────────────────────────────────────────────────┘

User A → Create Conversation → User B
         (Type: OneToOne)
              ↓
    Conversation Created
    (Both users are participants)
              ↓
    User A sends message → Message delivered → User B receives
              ↓
    User B marks as read → Read receipt → User A sees "Read"

┌─────────────────────────────────────────────────────────────┐
│                    Group Chat Flow                           │
└─────────────────────────────────────────────────────────────┘

User A (Creator) → Create Conversation → Users B, C, D
                   (Type: Group)
                   (User A is admin)
                        ↓
              Group conversation created
              (All users are participants)
                        ↓
    Any participant sends message → All participants receive
                        ↓
              Admin can add/remove participants
              Admin can update group info
                        ↓
    Participants can leave (remove themselves)
    Participants can mute notifications

┌─────────────────────────────────────────────────────────────┐
│                    Message Flow                              │
└─────────────────────────────────────────────────────────────┘

Sender → Send Message API → Server
                ↓
         Message saved to DB
                ↓
         SignalR Hub broadcasts
                ↓
    All participants receive (real-time)
                ↓
    Recipients mark as read
                ↓
    Read receipts sent to sender
```

---

## API Endpoints

## Conversation Management

### 1. Create Conversation

Create a new one-to-one or group conversation.

**Endpoint**: `POST /api/chats`
**Authentication**: Required (Bearer token)

#### Request Body - One-to-One Conversation

```json
{
  "type": "OneToOne",
  "participantIds": [
    "660e8400-e29b-41d4-a716-446655440001"
  ],
  "participantUsernames": ["jane"]
}
```

#### Request Body - Group Conversation

```json
{
  "type": "Group",
  "participantIds": [
    "660e8400-e29b-41d4-a716-446655440001",
    "770e8400-e29b-41d4-a716-446655440002",
    "880e8400-e29b-41d4-a716-446655440003"
  ],
  "participantUsernames": ["jane", "bob", "alice"],
  "groupName": "Project Team",
  "groupAvatarUrl": "https://cdn.wechat.com/groups/team-avatar.jpg",
  "groupDescription": "Discussion about the new project"
}
```

#### Request Fields

| Field | Type | Required | Description | Constraints |
|-------|------|----------|-------------|-------------|
| type | string | Yes | Conversation type | "OneToOne" or "Group" |
| participantIds | array | Yes | Array of user GUIDs | At least 1 participant |
| participantUsernames | array | No | Array of usernames (for display) | Same length as participantIds |
| groupName | string | No | Name of the group (for Group type) | Max 100 characters |
| groupAvatarUrl | string | No | URL of group avatar | Valid URL |
| groupDescription | string | No | Description of the group | Max 500 characters |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "673c0e8f8c4d5e1a2b3c4d5e",
    "type": "Group",
    "participants": [
      {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john",
        "joinedAt": "2025-11-27T10:30:00Z",
        "lastReadAt": null,
        "lastReadMessageId": null,
        "isMuted": false,
        "mutedUntil": null
      },
      {
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane",
        "joinedAt": "2025-11-27T10:30:00Z",
        "lastReadAt": null,
        "lastReadMessageId": null,
        "isMuted": false,
        "mutedUntil": null
      }
    ],
    "groupName": "Project Team",
    "groupAvatarUrl": "https://cdn.wechat.com/groups/team-avatar.jpg",
    "groupDescription": "Discussion about the new project",
    "createdBy": "550e8400-e29b-41d4-a716-446655440000",
    "admins": [
      "550e8400-e29b-41d4-a716-446655440000"
    ],
    "lastMessage": null,
    "createdAt": "2025-11-27T10:30:00Z",
    "updatedAt": "2025-11-27T10:30:00Z",
    "isDeleted": false,
    "deletedAt": null
  }
}
```

#### Notes

- For OneToOne conversations: If a conversation already exists between the two users, the existing conversation will be returned
- The creator is automatically added as the first participant and admin (for groups)
- Use the returned conversation ID for all future message operations

---

### 2. Get Conversation

Get details of a specific conversation.

**Endpoint**: `GET /api/chats/{conversationId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "673c0e8f8c4d5e1a2b3c4d5e",
    "type": "OneToOne",
    "participants": [
      {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john",
        "joinedAt": "2025-11-27T10:30:00Z",
        "lastReadAt": "2025-11-27T11:00:00Z",
        "lastReadMessageId": "673c0f1a8c4d5e1a2b3c4d5f",
        "isMuted": false,
        "mutedUntil": null
      },
      {
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane",
        "joinedAt": "2025-11-27T10:30:00Z",
        "lastReadAt": "2025-11-27T10:55:00Z",
        "lastReadMessageId": "673c0f0a8c4d5e1a2b3c4d5d",
        "isMuted": false,
        "mutedUntil": null
      }
    ],
    "groupName": null,
    "groupAvatarUrl": null,
    "groupDescription": null,
    "createdBy": "550e8400-e29b-41d4-a716-446655440000",
    "admins": [],
    "lastMessage": {
      "messageId": "673c0f1a8c4d5e1a2b3c4d5f",
      "senderId": "550e8400-e29b-41d4-a716-446655440000",
      "senderUsername": "john",
      "content": "Hey! How are you?",
      "messageType": "Text",
      "sentAt": "2025-11-27T11:00:00Z"
    },
    "createdAt": "2025-11-27T10:30:00Z",
    "updatedAt": "2025-11-27T11:00:00Z",
    "isDeleted": false,
    "deletedAt": null
  }
}
```

---

### 3. Get User Conversations

Get all conversations for the current user.

**Endpoint**: `GET /api/chats`
**Authentication**: Required (Bearer token)

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| page | integer | No | 1 | Page number (1-based) |
| pageSize | integer | No | 20 | Number of items per page (max 100) |

#### Request

```
GET /api/chats?page=1&pageSize=20
Authorization: Bearer {access_token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": "673c0e8f8c4d5e1a2b3c4d5e",
      "type": "OneToOne",
      "participants": [...],
      "lastMessage": {
        "messageId": "673c0f1a8c4d5e1a2b3c4d5f",
        "senderId": "660e8400-e29b-41d4-a716-446655440001",
        "senderUsername": "jane",
        "content": "See you tomorrow!",
        "messageType": "Text",
        "sentAt": "2025-11-27T11:30:00Z"
      },
      "createdAt": "2025-11-27T10:30:00Z",
      "updatedAt": "2025-11-27T11:30:00Z"
    },
    {
      "id": "673c0e8f8c4d5e1a2b3c4d60",
      "type": "Group",
      "groupName": "Family Group",
      "groupAvatarUrl": "https://cdn.wechat.com/groups/family.jpg",
      "participants": [...],
      "lastMessage": {
        "messageId": "673c0f2a8c4d5e1a2b3c4d61",
        "senderId": "770e8400-e29b-41d4-a716-446655440002",
        "senderUsername": "bob",
        "content": "Dinner at 7?",
        "messageType": "Text",
        "sentAt": "2025-11-27T11:45:00Z"
      },
      "createdAt": "2025-11-20T09:00:00Z",
      "updatedAt": "2025-11-27T11:45:00Z"
    }
  ]
}
```

#### Notes

- Conversations are sorted by most recent activity (lastMessage.sentAt)
- Only returns conversations where the user is a participant

---

### 4. Update Conversation

Update group conversation details (name, avatar, description). Only admins can update.

**Endpoint**: `PUT /api/chats/{conversationId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |

#### Request Body

```json
{
  "groupName": "Updated Team Name",
  "groupAvatarUrl": "https://cdn.wechat.com/groups/new-avatar.jpg",
  "groupDescription": "Updated description"
}
```

#### Request Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| groupName | string | No | New group name |
| groupAvatarUrl | string | No | New group avatar URL |
| groupDescription | string | No | New group description |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "673c0e8f8c4d5e1a2b3c4d5e",
    "type": "Group",
    "groupName": "Updated Team Name",
    "groupAvatarUrl": "https://cdn.wechat.com/groups/new-avatar.jpg",
    "groupDescription": "Updated description",
    "updatedAt": "2025-11-27T12:00:00Z"
  }
}
```

#### Error Responses

| Status | Error | Description |
|--------|-------|-------------|
| 403 | Forbidden | User is not an admin of the group |
| 404 | "Conversation not found" | Invalid conversationId |

---

### 5. Delete Conversation

Delete a conversation (soft delete).

**Endpoint**: `DELETE /api/chats/{conversationId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |

#### Response (200 OK)

```json
{
  "success": true,
  "data": true
}
```

#### Notes

- This is a soft delete (sets isDeleted flag)
- Messages are preserved but conversation is hidden from user's list

---

### 6. Add Participant

Add a new participant to a group conversation. Only admins can add participants.

**Endpoint**: `POST /api/chats/{conversationId}/participants`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |

#### Request Body

```json
{
  "userId": "880e8400-e29b-41d4-a716-446655440003",
  "username": "alice"
}
```

#### Request Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| userId | GUID | Yes | The user ID to add |
| username | string | Yes | The username of the user |

#### Response (200 OK)

```json
{
  "success": true,
  "data": true
}
```

#### Error Responses

| Status | Error | Description |
|--------|-------|-------------|
| 403 | Forbidden | User is not an admin of the group |
| 404 | "Conversation not found" | Invalid conversationId |

---

### 7. Remove Participant

Remove a participant from a group conversation. Admins can remove others, any user can remove themselves.

**Endpoint**: `DELETE /api/chats/{conversationId}/participants/{participantUserId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |
| participantUserId | GUID | Yes | The user ID to remove |

#### Response (200 OK)

```json
{
  "success": true,
  "data": true
}
```

#### Error Responses

| Status | Error | Description |
|--------|-------|-------------|
| 403 | Forbidden | User is not an admin and trying to remove someone else |
| 404 | "Conversation not found" | Invalid conversationId |

---

### 8. Mute Conversation

Mute notifications for a conversation.

**Endpoint**: `POST /api/chats/{conversationId}/mute`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |

#### Request Body

```json
{
  "mutedUntil": "2025-11-28T10:30:00Z"
}
```

#### Request Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| mutedUntil | datetime | No | Mute until this time (null = mute forever) |

#### Response (200 OK)

```json
{
  "success": true,
  "data": true
}
```

#### Notes

- If mutedUntil is null or omitted, conversation is muted indefinitely
- Each participant has their own mute settings

---

### 9. Unmute Conversation

Unmute notifications for a conversation.

**Endpoint**: `POST /api/chats/{conversationId}/unmute`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |

#### Response (200 OK)

```json
{
  "success": true,
  "data": true
}
```

---

### 10. Get Unread Count

Get the number of unread messages in a conversation.

**Endpoint**: `GET /api/chats/{conversationId}/unread-count`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |

#### Response (200 OK)

```json
{
  "success": true,
  "data": 5
}
```

---

## Message Management

### 11. Send Message

Send a message in a conversation.

**Endpoint**: `POST /api/messages`
**Authentication**: Required (Bearer token)

#### Request Body - Text Message

```json
{
  "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
  "messageType": "Text",
  "content": "Hello! How are you?",
  "replyToMessageId": null,
  "mentions": []
}
```

#### Request Body - Image Message

```json
{
  "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
  "messageType": "Image",
  "content": "Check out this photo!",
  "mediaUrl": "https://cdn.wechat.com/images/photo123.jpg",
  "mediaThumbnailUrl": "https://cdn.wechat.com/images/thumb/photo123.jpg"
}
```

#### Request Body - Video Message

```json
{
  "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
  "messageType": "Video",
  "content": "Awesome video!",
  "mediaUrl": "https://cdn.wechat.com/videos/video123.mp4",
  "mediaThumbnailUrl": "https://cdn.wechat.com/videos/thumb/video123.jpg",
  "mediaDuration": 120
}
```

#### Request Body - File Message

```json
{
  "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
  "messageType": "File",
  "content": "Project documentation",
  "mediaUrl": "https://cdn.wechat.com/files/document.pdf",
  "fileName": "project_plan.pdf",
  "fileSize": 2048576
}
```

#### Request Body - Location Message

```json
{
  "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
  "messageType": "Location",
  "content": "Meet me here!",
  "location": {
    "latitude": 37.7749,
    "longitude": -122.4194,
    "name": "Golden Gate Bridge",
    "address": "Golden Gate Bridge, San Francisco, CA"
  }
}
```

#### Request Body - Reply Message

```json
{
  "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
  "messageType": "Text",
  "content": "Yes, I agree!",
  "replyToMessageId": "673c0f1a8c4d5e1a2b3c4d5f"
}
```

#### Request Body - Message with Mentions

```json
{
  "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
  "messageType": "Text",
  "content": "@jane @bob Let's meet tomorrow!",
  "mentions": [
    "660e8400-e29b-41d4-a716-446655440001",
    "770e8400-e29b-41d4-a716-446655440002"
  ]
}
```

#### Request Fields

| Field | Type | Required | Description | Constraints |
|-------|------|----------|-------------|-------------|
| conversationId | string | Yes | The conversation ID | Valid MongoDB ObjectId |
| messageType | string | Yes | Type of message | See [Message Types](#message-types) |
| content | string | Yes | Message content/text | Max 10,000 characters |
| mediaUrl | string | No | URL of media file | Required for Image/Video/Audio/File |
| mediaThumbnailUrl | string | No | URL of thumbnail | For Image/Video |
| mediaDuration | integer | No | Duration in seconds | For Video/Audio |
| fileName | string | No | Original file name | For File type |
| fileSize | long | No | File size in bytes | For File type |
| location | object | No | Location data | Required for Location type |
| replyToMessageId | string | No | ID of message being replied to | For threaded conversations |
| mentions | array | No | Array of mentioned user GUIDs | For @mentions |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "673c0f1a8c4d5e1a2b3c4d5f",
    "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "senderUsername": "john",
    "messageType": "Text",
    "content": "Hello! How are you?",
    "mediaUrl": null,
    "mediaThumbnailUrl": null,
    "mediaDuration": null,
    "fileName": null,
    "fileSize": null,
    "location": null,
    "replyToMessageId": null,
    "forwardedFromMessageId": null,
    "mentions": [],
    "readBy": [],
    "deliveredAt": "2025-11-27T11:00:00Z",
    "isEdited": false,
    "editedAt": null,
    "createdAt": "2025-11-27T11:00:00Z",
    "updatedAt": "2025-11-27T11:00:00Z",
    "isDeleted": false,
    "deletedAt": null,
    "deletedFor": []
  }
}
```

#### Notes

- Messages are delivered in real-time via SignalR to all conversation participants
- The sender's username is automatically populated from the JWT token
- Media files should be uploaded separately and URLs provided

---

### 12. Get Message

Get details of a specific message.

**Endpoint**: `GET /api/messages/{messageId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| messageId | string | Yes | The MongoDB ObjectId of the message |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "673c0f1a8c4d5e1a2b3c4d5f",
    "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "senderUsername": "john",
    "messageType": "Text",
    "content": "Hello! How are you?",
    "readBy": [
      {
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "readAt": "2025-11-27T11:05:00Z"
      }
    ],
    "createdAt": "2025-11-27T11:00:00Z"
  }
}
```

---

### 13. Edit Message

Edit a message you sent. Only the sender can edit their messages.

**Endpoint**: `PUT /api/messages/{messageId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| messageId | string | Yes | The MongoDB ObjectId of the message |

#### Request Body

```json
{
  "content": "Updated message content"
}
```

#### Request Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| content | string | Yes | New message content |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "673c0f1a8c4d5e1a2b3c4d5f",
    "content": "Updated message content",
    "isEdited": true,
    "editedAt": "2025-11-27T11:10:00Z",
    "updatedAt": "2025-11-27T11:10:00Z"
  }
}
```

#### Error Responses

| Status | Error | Description |
|--------|-------|-------------|
| 403 | Forbidden | User is not the sender of the message |
| 404 | "Message not found" | Invalid messageId |

#### Notes

- Only text content can be edited
- Media URLs and other fields cannot be changed
- isEdited flag is set to true automatically

---

### 14. Delete Message

Delete a message. Supports "delete for me" and "delete for everyone".

**Endpoint**: `DELETE /api/messages/{messageId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| messageId | string | Yes | The MongoDB ObjectId of the message |

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| deleteForEveryone | boolean | No | false | If true, delete for all participants (sender only) |

#### Request

```
DELETE /api/messages/673c0f1a8c4d5e1a2b3c4d5f?deleteForEveryone=false
Authorization: Bearer {access_token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": true
}
```

#### Notes

- **Delete for me** (deleteForEveryone=false): Adds your userId to deletedFor array
- **Delete for everyone** (deleteForEveryone=true): Sets isDeleted=true (only sender can do this)
- Deleted messages are not physically removed from the database

---

### 15. Get Conversation Messages

Get messages from a conversation with pagination.

**Endpoint**: `GET /api/messages/conversation/{conversationId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| page | integer | No | 1 | Page number (1-based) |
| pageSize | integer | No | 50 | Number of messages per page (max 100) |

#### Request

```
GET /api/messages/conversation/673c0e8f8c4d5e1a2b3c4d5e?page=1&pageSize=50
Authorization: Bearer {access_token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": "673c0f1a8c4d5e1a2b3c4d5f",
      "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
      "senderId": "550e8400-e29b-41d4-a716-446655440000",
      "senderUsername": "john",
      "messageType": "Text",
      "content": "Hello! How are you?",
      "createdAt": "2025-11-27T11:00:00Z"
    },
    {
      "id": "673c0f2a8c4d5e1a2b3c4d60",
      "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
      "senderId": "660e8400-e29b-41d4-a716-446655440001",
      "senderUsername": "jane",
      "messageType": "Text",
      "content": "I'm doing great, thanks!",
      "createdAt": "2025-11-27T11:02:00Z"
    }
  ]
}
```

#### Notes

- Messages are returned in chronological order (oldest first)
- Messages where current user is in deletedFor array are excluded

---

### 16. Get Messages Before

Get messages before a specific timestamp (for loading older messages).

**Endpoint**: `GET /api/messages/conversation/{conversationId}/before`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| before | datetime | Yes | - | Get messages before this timestamp |
| limit | integer | No | 50 | Number of messages to retrieve (max 100) |

#### Request

```
GET /api/messages/conversation/673c0e8f8c4d5e1a2b3c4d5e/before?before=2025-11-27T11:00:00Z&limit=50
Authorization: Bearer {access_token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": "673c0e8a8c4d5e1a2b3c4d5d",
      "content": "Previous message 1",
      "createdAt": "2025-11-27T10:50:00Z"
    },
    {
      "id": "673c0e8b8c4d5e1a2b3c4d5c",
      "content": "Previous message 2",
      "createdAt": "2025-11-27T10:55:00Z"
    }
  ]
}
```

#### Use Case

- Infinite scroll loading older messages
- User scrolls to top of chat and loads more history

---

### 17. Get Messages After

Get messages after a specific timestamp (for loading newer messages).

**Endpoint**: `GET /api/messages/conversation/{conversationId}/after`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| after | datetime | Yes | - | Get messages after this timestamp |
| limit | integer | No | 50 | Number of messages to retrieve (max 100) |

#### Request

```
GET /api/messages/conversation/673c0e8f8c4d5e1a2b3c4d5e/after?after=2025-11-27T11:00:00Z&limit=50
Authorization: Bearer {access_token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": "673c0f2a8c4d5e1a2b3c4d60",
      "content": "New message 1",
      "createdAt": "2025-11-27T11:05:00Z"
    },
    {
      "id": "673c0f2b8c4d5e1a2b3c4d61",
      "content": "New message 2",
      "createdAt": "2025-11-27T11:10:00Z"
    }
  ]
}
```

#### Use Case

- Catching up on messages after reconnection
- Loading messages that arrived while viewing older history

---

### 18. Mark Message as Read

Mark a message as read (creates read receipt).

**Endpoint**: `POST /api/messages/{messageId}/read`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| messageId | string | Yes | The MongoDB ObjectId of the message |

#### Response (200 OK)

```json
{
  "success": true,
  "data": true
}
```

#### Notes

- Automatically adds current user and timestamp to message's readBy array
- Sender receives read receipt notification via SignalR
- Should be called when message becomes visible on screen

---

### 19. Search Messages

Search for messages within a conversation.

**Endpoint**: `GET /api/messages/conversation/{conversationId}/search`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| conversationId | string | Yes | The MongoDB ObjectId of the conversation |

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| query | string | Yes | - | Search query text |
| page | integer | No | 1 | Page number (1-based) |
| pageSize | integer | No | 20 | Number of results per page (max 100) |

#### Request

```
GET /api/messages/conversation/673c0e8f8c4d5e1a2b3c4d5e/search?query=meeting&page=1&pageSize=20
Authorization: Bearer {access_token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": "673c0f3a8c4d5e1a2b3c4d62",
      "content": "Let's schedule a meeting for tomorrow",
      "senderId": "550e8400-e29b-41d4-a716-446655440000",
      "senderUsername": "john",
      "createdAt": "2025-11-27T09:00:00Z"
    },
    {
      "id": "673c0f4a8c4d5e1a2b3c4d63",
      "content": "The meeting went well!",
      "senderId": "660e8400-e29b-41d4-a716-446655440001",
      "senderUsername": "jane",
      "createdAt": "2025-11-27T15:00:00Z"
    }
  ]
}
```

#### Notes

- Search is case-insensitive
- Searches in message content field
- Results are ordered by relevance and recency

---

## Real-time Communication

### SignalR Hub Connection

The Chat API uses SignalR for real-time message delivery.

#### Hub URL

```
ws://localhost:5004/hubs/chat
```

#### Connection Setup

```typescript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('http://localhost:5004/hubs/chat', {
    accessTokenFactory: () => localStorage.getItem('accessToken') || ''
  })
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();

// Start connection
await connection.start();
console.log('Connected to chat hub');
```

#### Hub Methods (Server → Client)

**ReceiveMessage** - Receive a new message

```typescript
connection.on('ReceiveMessage', (message) => {
  console.log('New message:', message);
  // Update UI with new message
  addMessageToConversation(message.conversationId, message);
});
```

**MessageDeleted** - Message was deleted

```typescript
connection.on('MessageDeleted', (messageId, conversationId, deleteForEveryone) => {
  console.log('Message deleted:', messageId);
  if (deleteForEveryone) {
    // Remove from UI for everyone
    removeMessageFromUI(messageId);
  } else {
    // User deleted it for themselves
  }
});
```

**MessageEdited** - Message was edited

```typescript
connection.on('MessageEdited', (message) => {
  console.log('Message edited:', message);
  // Update message in UI
  updateMessageInUI(message);
});
```

**MessageRead** - Message was read (read receipt)

```typescript
connection.on('MessageRead', (messageId, userId, readAt) => {
  console.log(`Message ${messageId} read by ${userId}`);
  // Update read status in UI
  addReadReceipt(messageId, userId, readAt);
});
```

**UserTyping** - User is typing

```typescript
connection.on('UserTyping', (conversationId, userId, username) => {
  console.log(`${username} is typing in conversation ${conversationId}`);
  // Show typing indicator
  showTypingIndicator(conversationId, username);
});
```

**UserStoppedTyping** - User stopped typing

```typescript
connection.on('UserStoppedTyping', (conversationId, userId) => {
  console.log(`User ${userId} stopped typing`);
  // Hide typing indicator
  hideTypingIndicator(conversationId, userId);
});
```

#### Hub Methods (Client → Server)

**JoinConversation** - Join a conversation room

```typescript
await connection.invoke('JoinConversation', conversationId);
```

**LeaveConversation** - Leave a conversation room

```typescript
await connection.invoke('LeaveConversation', conversationId);
```

**SendTypingIndicator** - Notify others you're typing

```typescript
await connection.invoke('SendTypingIndicator', conversationId);
```

**StopTypingIndicator** - Notify others you stopped typing

```typescript
await connection.invoke('StopTypingIndicator', conversationId);
```

---

## Message Types

The API supports the following message types:

| Type | Value | Description | Required Fields |
|------|-------|-------------|-----------------|
| Text | "Text" | Plain text message | content |
| Image | "Image" | Image with optional caption | content, mediaUrl, mediaThumbnailUrl (optional) |
| Video | "Video" | Video with optional caption | content, mediaUrl, mediaThumbnailUrl (optional), mediaDuration (optional) |
| Audio | "Audio" | Audio message or voice note | content, mediaUrl, mediaDuration (optional) |
| File | "File" | File attachment | content, mediaUrl, fileName, fileSize |
| Location | "Location" | Location/map pin | content, location |
| Contact | "Contact" | Shared contact | content |
| Sticker | "Sticker" | Sticker or emoji | content, mediaUrl |
| Gif | "Gif" | Animated GIF | content, mediaUrl |
| System | "System" | System message (auto-generated) | content |

### Message Type Examples

```typescript
// Text
{
  messageType: "Text",
  content: "Hello!"
}

// Image
{
  messageType: "Image",
  content: "Check this out!",
  mediaUrl: "https://cdn.wechat.com/images/photo.jpg",
  mediaThumbnailUrl: "https://cdn.wechat.com/images/thumb/photo.jpg"
}

// Video
{
  messageType: "Video",
  content: "Awesome video",
  mediaUrl: "https://cdn.wechat.com/videos/video.mp4",
  mediaThumbnailUrl: "https://cdn.wechat.com/videos/thumb.jpg",
  mediaDuration: 120
}

// File
{
  messageType: "File",
  content: "Project documentation",
  mediaUrl: "https://cdn.wechat.com/files/doc.pdf",
  fileName: "project_plan.pdf",
  fileSize: 2048576
}

// Location
{
  messageType: "Location",
  content: "Meet me here",
  location: {
    latitude: 37.7749,
    longitude: -122.4194,
    name: "Golden Gate Bridge",
    address: "Golden Gate Bridge, San Francisco, CA"
  }
}
```

---

## Error Handling

### Common Error Codes

| Status Code | Meaning | Common Causes |
|-------------|---------|---------------|
| 400 | Bad Request | Invalid input, validation errors |
| 401 | Unauthorized | Missing or invalid authentication token |
| 403 | Forbidden | Not a participant, not an admin |
| 404 | Not Found | Conversation or message not found |
| 500 | Internal Server Error | Server-side error |

### Error Response Format

```json
{
  "success": false,
  "data": null,
  "error": "Main error message",
  "errors": [
    "Detailed error 1",
    "Detailed error 2"
  ],
  "timestamp": "2025-11-27T10:30:00Z"
}
```

---

## Integration Examples

### React/TypeScript Chat Component

```typescript
// services/chatService.ts
import axios from 'axios';
import * as signalR from '@microsoft/signalr';

const API_BASE_URL = 'http://localhost:5004/api';
const HUB_URL = 'http://localhost:5004/hubs/chat';

interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  messageType: string;
  content: string;
  createdAt: string;
  // ... other fields
}

interface Conversation {
  id: string;
  type: string;
  participants: any[];
  lastMessage?: any;
  // ... other fields
}

const getAuthToken = (): string => {
  return localStorage.getItem('accessToken') || '';
};

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const token = getAuthToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export class ChatService {
  private connection: signalR.HubConnection;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => getAuthToken()
      })
      .withAutomaticReconnect()
      .build();
  }

  // Start SignalR connection
  async startConnection() {
    try {
      await this.connection.start();
      console.log('Connected to chat hub');
    } catch (error) {
      console.error('Error connecting to hub:', error);
    }
  }

  // Stop SignalR connection
  async stopConnection() {
    await this.connection.stop();
  }

  // Listen for new messages
  onReceiveMessage(callback: (message: Message) => void) {
    this.connection.on('ReceiveMessage', callback);
  }

  // Listen for message edits
  onMessageEdited(callback: (message: Message) => void) {
    this.connection.on('MessageEdited', callback);
  }

  // Listen for message deletions
  onMessageDeleted(callback: (messageId: string, conversationId: string, deleteForEveryone: boolean) => void) {
    this.connection.on('MessageDeleted', callback);
  }

  // Listen for read receipts
  onMessageRead(callback: (messageId: string, userId: string, readAt: string) => void) {
    this.connection.on('MessageRead', callback);
  }

  // Listen for typing indicators
  onUserTyping(callback: (conversationId: string, userId: string, username: string) => void) {
    this.connection.on('UserTyping', callback);
  }

  // Join conversation room
  async joinConversation(conversationId: string) {
    await this.connection.invoke('JoinConversation', conversationId);
  }

  // Leave conversation room
  async leaveConversation(conversationId: string) {
    await this.connection.invoke('LeaveConversation', conversationId);
  }

  // Send typing indicator
  async sendTypingIndicator(conversationId: string) {
    await this.connection.invoke('SendTypingIndicator', conversationId);
  }

  // API Methods
  async createConversation(participantIds: string[], type: string = 'OneToOne', groupName?: string) {
    const response = await apiClient.post('/chats', {
      type,
      participantIds,
      groupName
    });
    return response.data;
  }

  async getConversations(page: number = 1, pageSize: number = 20) {
    const response = await apiClient.get('/chats', {
      params: { page, pageSize }
    });
    return response.data;
  }

  async getConversation(conversationId: string) {
    const response = await apiClient.get(`/chats/${conversationId}`);
    return response.data;
  }

  async sendMessage(conversationId: string, content: string, messageType: string = 'Text') {
    const response = await apiClient.post('/messages', {
      conversationId,
      messageType,
      content
    });
    return response.data;
  }

  async getMessages(conversationId: string, page: number = 1, pageSize: number = 50) {
    const response = await apiClient.get(`/messages/conversation/${conversationId}`, {
      params: { page, pageSize }
    });
    return response.data;
  }

  async getMessagesBefore(conversationId: string, before: Date, limit: number = 50) {
    const response = await apiClient.get(`/messages/conversation/${conversationId}/before`, {
      params: { before: before.toISOString(), limit }
    });
    return response.data;
  }

  async markAsRead(messageId: string) {
    const response = await apiClient.post(`/messages/${messageId}/read`);
    return response.data;
  }

  async editMessage(messageId: string, content: string) {
    const response = await apiClient.put(`/messages/${messageId}`, { content });
    return response.data;
  }

  async deleteMessage(messageId: string, deleteForEveryone: boolean = false) {
    const response = await apiClient.delete(`/messages/${messageId}`, {
      params: { deleteForEveryone }
    });
    return response.data;
  }

  async searchMessages(conversationId: string, query: string) {
    const response = await apiClient.get(`/messages/conversation/${conversationId}/search`, {
      params: { query }
    });
    return response.data;
  }
}

export const chatService = new ChatService();
```

```tsx
// components/ChatWindow.tsx
import React, { useState, useEffect, useRef } from 'react';
import { chatService } from '../services/chatService';

interface ChatWindowProps {
  conversationId: string;
}

export const ChatWindow: React.FC<ChatWindowProps> = ({ conversationId }) => {
  const [messages, setMessages] = useState<any[]>([]);
  const [newMessage, setNewMessage] = useState('');
  const [loading, setLoading] = useState(true);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // Initialize chat
    initializeChat();

    return () => {
      // Cleanup
      chatService.leaveConversation(conversationId);
    };
  }, [conversationId]);

  const initializeChat = async () => {
    try {
      // Start SignalR connection
      await chatService.startConnection();

      // Join conversation room
      await chatService.joinConversation(conversationId);

      // Load messages
      const response = await chatService.getMessages(conversationId);
      if (response.success) {
        setMessages(response.data);
      }

      // Listen for new messages
      chatService.onReceiveMessage((message) => {
        if (message.conversationId === conversationId) {
          setMessages((prev) => [...prev, message]);
          scrollToBottom();

          // Mark as read
          chatService.markAsRead(message.id);
        }
      });

      // Listen for message edits
      chatService.onMessageEdited((message) => {
        setMessages((prev) =>
          prev.map((m) => (m.id === message.id ? message : m))
        );
      });

      // Listen for deletions
      chatService.onMessageDeleted((messageId, convId, deleteForEveryone) => {
        if (deleteForEveryone) {
          setMessages((prev) => prev.filter((m) => m.id !== messageId));
        }
      });
    } catch (error) {
      console.error('Error initializing chat:', error);
    } finally {
      setLoading(false);
    }
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleSendMessage = async () => {
    if (!newMessage.trim()) return;

    try {
      const response = await chatService.sendMessage(
        conversationId,
        newMessage,
        'Text'
      );

      if (response.success) {
        setNewMessage('');
      }
    } catch (error) {
      console.error('Error sending message:', error);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  if (loading) return <div>Loading chat...</div>;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '600px' }}>
      {/* Messages area */}
      <div style={{ flex: 1, overflowY: 'auto', padding: '20px' }}>
        {messages.map((message) => (
          <div
            key={message.id}
            style={{
              marginBottom: '10px',
              textAlign: message.senderId === localStorage.getItem('userId') ? 'right' : 'left'
            }}
          >
            <div style={{
              display: 'inline-block',
              padding: '10px',
              borderRadius: '10px',
              backgroundColor: message.senderId === localStorage.getItem('userId') ? '#007bff' : '#e9ecef',
              color: message.senderId === localStorage.getItem('userId') ? 'white' : 'black'
            }}>
              <strong>{message.senderUsername}</strong>
              <p>{message.content}</p>
              <small>{new Date(message.createdAt).toLocaleTimeString()}</small>
            </div>
          </div>
        ))}
        <div ref={messagesEndRef} />
      </div>

      {/* Input area */}
      <div style={{ padding: '20px', borderTop: '1px solid #ccc' }}>
        <input
          type="text"
          value={newMessage}
          onChange={(e) => setNewMessage(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="Type a message..."
          style={{ width: '80%', padding: '10px', marginRight: '10px' }}
        />
        <button onClick={handleSendMessage} style={{ padding: '10px 20px' }}>
          Send
        </button>
      </div>
    </div>
  );
};
```

---

## Best Practices

### 1. Message Batching

Load messages in batches using pagination:

```typescript
const loadMoreMessages = async () => {
  const oldestMessage = messages[0];
  const response = await chatService.getMessagesBefore(
    conversationId,
    new Date(oldestMessage.createdAt),
    50
  );

  if (response.success) {
    setMessages((prev) => [...response.data, ...prev]);
  }
};
```

### 2. Optimistic Updates

Update UI immediately before API confirms:

```typescript
const handleSendMessage = async () => {
  const optimisticMessage = {
    id: 'temp-' + Date.now(),
    content: newMessage,
    senderId: currentUserId,
    senderUsername: currentUsername,
    createdAt: new Date().toISOString(),
    status: 'sending'
  };

  // Add to UI immediately
  setMessages((prev) => [...prev, optimisticMessage]);
  setNewMessage('');

  try {
    const response = await chatService.sendMessage(conversationId, newMessage);

    if (response.success) {
      // Replace optimistic message with real one
      setMessages((prev) =>
        prev.map((m) => (m.id === optimisticMessage.id ? response.data : m))
      );
    }
  } catch (error) {
    // Remove optimistic message on error
    setMessages((prev) => prev.filter((m) => m.id !== optimisticMessage.id));
    alert('Failed to send message');
  }
};
```

### 3. Typing Indicators

Implement typing indicators with debounce:

```typescript
const [typingTimeout, setTypingTimeout] = useState<NodeJS.Timeout | null>(null);

const handleInputChange = (value: string) => {
  setNewMessage(value);

  // Send typing indicator
  chatService.sendTypingIndicator(conversationId);

  // Clear previous timeout
  if (typingTimeout) {
    clearTimeout(typingTimeout);
  }

  // Stop typing after 3 seconds of inactivity
  const timeout = setTimeout(() => {
    chatService.connection.invoke('StopTypingIndicator', conversationId);
  }, 3000);

  setTypingTimeout(timeout);
};
```

### 4. Read Receipts

Mark messages as read when they become visible:

```typescript
const handleMessageVisible = (messageId: string) => {
  chatService.markAsRead(messageId);
};

// Use Intersection Observer
useEffect(() => {
  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          const messageId = entry.target.getAttribute('data-message-id');
          if (messageId) {
            handleMessageVisible(messageId);
          }
        }
      });
    },
    { threshold: 0.5 }
  );

  // Observe all messages
  document.querySelectorAll('[data-message-id]').forEach((el) => {
    observer.observe(el);
  });

  return () => observer.disconnect();
}, [messages]);
```

### 5. Connection Management

Handle reconnection gracefully:

```typescript
chatService.connection.onreconnected(() => {
  console.log('Reconnected to chat');
  // Reload messages to catch up
  loadMessages();
});

chatService.connection.onreconnecting(() => {
  console.log('Reconnecting...');
  showNotification('Reconnecting to chat...', 'info');
});
```

---

## Testing

### Test with cURL

**Create Conversation**

```bash
curl -X POST http://localhost:5004/api/chats \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "OneToOne",
    "participantIds": ["660e8400-e29b-41d4-a716-446655440001"]
  }'
```

**Send Message**

```bash
curl -X POST http://localhost:5004/api/messages \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "conversationId": "673c0e8f8c4d5e1a2b3c4d5e",
    "messageType": "Text",
    "content": "Hello!"
  }'
```

**Get Messages**

```bash
curl http://localhost:5004/api/messages/conversation/673c0e8f8c4d5e1a2b3c4d5e \
  -H "Authorization: Bearer {token}"
```

---

## Support

For issues or questions:
- Backend API: Check ChatService.Api logs
- MongoDB: Check `ChatDb` database, `Conversations` and `Messages` collections
- SignalR: Check hub connection status and console logs
- Authentication: Ensure valid JWT token in Authorization header

---

**Version**: 1.0
**Last Updated**: 2025-11-27
**Service**: ChatService.Api (Port 5004)
