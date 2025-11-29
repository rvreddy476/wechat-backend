# WeChat Backend API - UI Implementation Guide

## 🎯 Overview

This document provides all the information needed to implement the UI for the WeChat Backend. All services use **JWT Bearer Authentication** and follow **RESTful** principles.

## 🔐 Authentication

All endpoints (except `/api/auth/register` and `/api/auth/login`) require a **JWT Bearer token** in the Authorization header:

```
Authorization: Bearer <access_token>
```

---

## 📡 API Endpoints

### Base URLs (Development)

| Service | URL | Port |
|---------|-----|------|
| Auth | `http://localhost:5001` | 5001 |
| Chat | `http://localhost:5004` | 5004 |
| UserProfile | `http://localhost:5002` | 5002 |
| PostFeed | `http://localhost:5003` | 5003 |
| Media | `http://localhost:5005` | 5005 |
| Notification | `http://localhost:5007` | 5007 |

---

## 🔑 Auth Service Endpoints

### 1. Register User
**POST** `/api/auth/register`

**Request Body:**
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "Password123!",
  "phoneNumber": "+1234567890"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "expiresAt": "2024-01-15T10:30:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "johndoe",
      "email": "john@example.com",
      "phoneNumber": "+1234567890",
      "roles": ["User"]
    }
  },
  "error": null,
  "timestamp": "2024-01-15T09:30:00Z"
}
```

**Error (400 Bad Request):**
```json
{
  "success": false,
  "data": null,
  "error": "Email already registered",
  "timestamp": "2024-01-15T09:30:00Z"
}
```

---

### 2. Login
**POST** `/api/auth/login`

**Request Body:**
```json
{
  "emailOrUsername": "johndoe",
  "password": "Password123!"
}
```

**Response:** Same as Register

---

### 3. Refresh Token
**POST** `/api/auth/refresh`

**Request Body:**
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

**Response:** Same as Register (new tokens)

---

### 4. Get Current User
**GET** `/api/auth/me`

**Headers:**
```
Authorization: Bearer <access_token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "roles": ["User"]
  }
}
```

---

## 💬 Chat Service Endpoints

### 1. Create Conversation
**POST** `/api/conversations`

**Headers:**
```
Authorization: Bearer <access_token>
```

**Request Body (One-to-One):**
```json
{
  "type": 0,
  "participants": [
    {
      "userId": "550e8400-e29b-41d4-a716-446655440001",
      "username": "janedoe"
    }
  ]
}
```

**Request Body (Group):**
```json
{
  "type": 1,
  "groupName": "Team Chat",
  "groupAvatarUrl": "https://example.com/avatar.jpg",
  "participants": [
    {
      "userId": "550e8400-e29b-41d4-a716-446655440001",
      "username": "janedoe"
    },
    {
      "userId": "550e8400-e29b-41d4-a716-446655440002",
      "username": "bobsmith"
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "507f1f77bcf86cd799439011",
    "type": 1,
    "participants": [
      {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "johndoe",
        "joinedAt": "2024-01-15T09:30:00Z"
      },
      {
        "userId": "550e8400-e29b-41d4-a716-446655440001",
        "username": "janedoe",
        "joinedAt": "2024-01-15T09:30:00Z"
      }
    ],
    "groupName": "Team Chat",
    "createdAt": "2024-01-15T09:30:00Z"
  }
}
```

---

### 2. Get Conversations
**GET** `/api/conversations?page=1&pageSize=20`

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "507f1f77bcf86cd799439011",
      "type": 0,
      "participants": [...],
      "lastMessage": {
        "messageId": "507f1f77bcf86cd799439012",
        "senderId": "550e8400-e29b-41d4-a716-446655440001",
        "senderUsername": "janedoe",
        "content": "Hey! How are you?",
        "sentAt": "2024-01-15T09:25:00Z"
      },
      "createdAt": "2024-01-15T09:00:00Z",
      "updatedAt": "2024-01-15T09:25:00Z"
    }
  ]
}
```

---

### 3. Send Message
**POST** `/api/messages`

**Request Body:**
```json
{
  "conversationId": "507f1f77bcf86cd799439011",
  "content": "Hello! This is a message",
  "messageType": 0,
  "mediaUrl": null,
  "replyToMessageId": null
}
```

**Message Types:**
- `0` - Text
- `1` - Image
- `2` - Video
- `3` - Audio
- `4` - File

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "507f1f77bcf86cd799439012",
    "conversationId": "507f1f77bcf86cd799439011",
    "content": "Hello! This is a message",
    "createdAt": "2024-01-15T09:30:00Z"
  }
}
```

---

### 4. Get Messages
**GET** `/api/messages?conversationId=507f1f77bcf86cd799439011&page=1&pageSize=50`

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "507f1f77bcf86cd799439012",
      "conversationId": "507f1f77bcf86cd799439011",
      "senderId": "550e8400-e29b-41d4-a716-446655440001",
      "senderUsername": "janedoe",
      "content": "Hello!",
      "messageType": 0,
      "mediaUrl": null,
      "replyToMessageId": null,
      "readBy": [
        {
          "userId": "550e8400-e29b-41d4-a716-446655440000",
          "readAt": "2024-01-15T09:31:00Z"
        }
      ],
      "isEdited": false,
      "createdAt": "2024-01-15T09:30:00Z",
      "updatedAt": "2024-01-15T09:30:00Z"
    }
  ]
}
```

---

### 5. Mark Message as Read
**POST** `/api/messages/{messageId}/read?conversationId={conversationId}`

**Response (200 OK):**
```json
{
  "success": true,
  "data": true
}
```

---

## 🔴 SignalR Real-Time Chat

### Hub URL
**WebSocket:** `ws://localhost:5004/hubs/chat`
**HTTP:** `http://localhost:5004/hubs/chat`

### Connection

```typescript
// Using @microsoft/signalr
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5004/hubs/chat", {
    accessTokenFactory: () => localStorage.getItem("accessToken")
  })
  .withAutomaticReconnect()
  .build();

await connection.start();
```

### Hub Methods (Client → Server)

#### 1. Join Conversation
```typescript
await connection.invoke("JoinConversation", conversationId);
```

#### 2. Leave Conversation
```typescript
await connection.invoke("LeaveConversation", conversationId);
```

#### 3. Send Message (via SignalR)
```typescript
await connection.invoke("SendMessage", conversationId, messageId, content, senderUsername);
```

#### 4. Typing Indicator
```typescript
// User started typing
await connection.invoke("TypingIndicator", conversationId, true);

// User stopped typing
await connection.invoke("TypingIndicator", conversationId, false);
```

#### 5. Message Read
```typescript
await connection.invoke("MessageRead", conversationId, messageId);
```

#### 6. Message Deleted
```typescript
await connection.invoke("MessageDeleted", conversationId, messageId);
```

---

### Hub Events (Server → Client)

#### 1. Receive Message
```typescript
connection.on("ReceiveMessage", (message) => {
  console.log("New message:", message);
  // {
  //   conversationId: "507f1f77bcf86cd799439011",
  //   messageId: "507f1f77bcf86cd799439012",
  //   senderId: "550e8400-e29b-41d4-a716-446655440001",
  //   senderUsername: "janedoe",
  //   content: "Hello!",
  //   messageType: 0,
  //   createdAt: "2024-01-15T09:30:00Z"
  // }
});
```

#### 2. User Online/Offline
```typescript
connection.on("UserOnline", (data) => {
  console.log(`${data.username} is online`);
  // { userId: "...", username: "janedoe" }
});

connection.on("UserOffline", (data) => {
  console.log(`${data.username} is offline`);
  // { userId: "...", username: "janedoe" }
});
```

#### 3. Online Users List
```typescript
connection.on("OnlineUsers", (users) => {
  console.log("Online users:", users);
  // [
  //   { userId: "...", username: "janedoe" },
  //   { userId: "...", username: "bobsmith" }
  // ]
});
```

#### 4. Typing Indicators
```typescript
connection.on("UserTyping", (conversationId, userId, username) => {
  console.log(`${username} is typing in ${conversationId}`);
  // Show "janedoe is typing..." in UI
});

connection.on("UserStoppedTyping", (conversationId, userId) => {
  console.log(`User ${userId} stopped typing`);
  // Hide typing indicator
});
```

#### 5. Message Read
```typescript
connection.on("MessageRead", (data) => {
  console.log("Message read:", data);
  // {
  //   conversationId: "...",
  //   messageId: "...",
  //   userId: "...",
  //   readAt: "2024-01-15T09:31:00Z"
  // }
});
```

#### 6. Message Deleted
```typescript
connection.on("MessageDeleted", (conversationId, messageId) => {
  console.log(`Message ${messageId} deleted from ${conversationId}`);
  // Remove message from UI
});
```

#### 7. User Joined/Left Conversation
```typescript
connection.on("UserJoinedConversation", (conversationId, userId) => {
  console.log(`User ${userId} joined conversation`);
});

connection.on("UserLeftConversation", (conversationId, userId) => {
  console.log(`User ${userId} left conversation`);
});
```

---

## 🎨 UI Implementation Example (React)

### Chat Component Example

```typescript
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

const ChatComponent = ({ conversationId, currentUserId }) => {
  const [messages, setMessages] = useState([]);
  const [connection, setConnection] = useState(null);
  const [typingUsers, setTypingUsers] = useState(new Set());
  const [onlineUsers, setOnlineUsers] = useState([]);

  useEffect(() => {
    // Establish SignalR connection
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5004/hubs/chat", {
        accessTokenFactory: () => localStorage.getItem("accessToken")
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          console.log('Connected to SignalR hub');

          // Join conversation
          connection.invoke("JoinConversation", conversationId);

          // Listen for new messages
          connection.on("ReceiveMessage", (message) => {
            setMessages(prev => [...prev, message]);
          });

          // Listen for typing indicators
          connection.on("UserTyping", (convId, userId, username) => {
            if (userId !== currentUserId) {
              setTypingUsers(prev => new Set(prev).add(username));
            }
          });

          connection.on("UserStoppedTyping", (convId, userId) => {
            setTypingUsers(prev => {
              const newSet = new Set(prev);
              newSet.delete(userId);
              return newSet;
            });
          });

          // Listen for online users
          connection.on("OnlineUsers", (users) => {
            setOnlineUsers(users);
          });

          connection.on("UserOnline", (data) => {
            setOnlineUsers(prev => [...prev, data]);
          });

          connection.on("UserOffline", (data) => {
            setOnlineUsers(prev => prev.filter(u => u.userId !== data.userId));
          });

          // Listen for message read
          connection.on("MessageRead", (data) => {
            setMessages(prev => prev.map(msg =>
              msg.messageId === data.messageId
                ? { ...msg, readBy: [...(msg.readBy || []), { userId: data.userId, readAt: data.readAt }] }
                : msg
            ));
          });
        })
        .catch(err => console.error('SignalR connection error:', err));
    }

    return () => {
      if (connection) {
        connection.invoke("LeaveConversation", conversationId);
        connection.stop();
      }
    };
  }, [connection, conversationId]);

  const sendMessage = async (content) => {
    // Send via REST API
    const response = await fetch('http://localhost:5004/api/messages', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      },
      body: JSON.stringify({
        conversationId,
        content,
        messageType: 0
      })
    });

    const result = await response.json();

    // Message will be received via SignalR "ReceiveMessage" event
    // No need to manually add to messages array
  };

  const handleTyping = (isTyping) => {
    connection?.invoke("TypingIndicator", conversationId, isTyping);
  };

  return (
    <div>
      {/* Messages */}
      <div className="messages">
        {messages.map(msg => (
          <div key={msg.messageId}>
            <strong>{msg.senderUsername}:</strong> {msg.content}
            {msg.readBy?.length > 0 && <span> ✓✓</span>}
          </div>
        ))}
      </div>

      {/* Typing indicators */}
      {typingUsers.size > 0 && (
        <div className="typing">
          {Array.from(typingUsers).join(', ')} {typingUsers.size === 1 ? 'is' : 'are'} typing...
        </div>
      )}

      {/* Online users */}
      <div className="online-users">
        Online: {onlineUsers.map(u => u.username).join(', ')}
      </div>

      {/* Input */}
      <input
        type="text"
        onFocus={() => handleTyping(true)}
        onBlur={() => handleTyping(false)}
        onKeyPress={(e) => {
          if (e.key === 'Enter') {
            sendMessage(e.target.value);
            e.target.value = '';
          }
        }}
      />
    </div>
  );
};
```

---

## 📦 Required npm Packages

```bash
npm install @microsoft/signalr
npm install axios  # for HTTP requests
```

---

## 🔧 Environment Variables (Frontend)

```env
REACT_APP_AUTH_API=http://localhost:5001
REACT_APP_CHAT_API=http://localhost:5004
REACT_APP_CHAT_HUB=http://localhost:5004/hubs/chat
REACT_APP_USERPROFILE_API=http://localhost:5002
REACT_APP_POSTFEED_API=http://localhost:5003
REACT_APP_MEDIA_API=http://localhost:5005
REACT_APP_NOTIFICATION_API=http://localhost:5007
```

---

## 🚀 Getting Started

### 1. Start Backend Services

```bash
docker-compose up -d
```

### 2. Test Auth
```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Password123!"
  }'
```

### 3. Connect to SignalR

```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5004/hubs/chat", {
    accessTokenFactory: () => "your-jwt-token-here"
  })
  .build();

await connection.start();
await connection.invoke("JoinConversation", "conversationId");
```

---

## 📝 Common Patterns

### Authentication Flow

1. User registers → Receive `accessToken` and `refreshToken`
2. Store tokens in localStorage/secure storage
3. Include `accessToken` in Authorization header for all requests
4. When `accessToken` expires (401 error), use `/api/auth/refresh` with `refreshToken`
5. Update stored `accessToken`

### Chat Flow

1. Get conversations → `/api/conversations`
2. Select conversation → Navigate to chat view
3. Connect to SignalR → `connection.start()`
4. Join conversation → `connection.invoke("JoinConversation", conversationId)`
5. Load messages → `/api/messages?conversationId=...`
6. Listen for new messages → `connection.on("ReceiveMessage", ...)`
7. Send message → POST to `/api/messages`
8. Leave conversation → `connection.invoke("LeaveConversation", conversationId)`

### Typing Indicator Flow

1. User focuses on input → Send `TypingIndicator(conversationId, true)`
2. User blurs input or stops typing for 3s → Send `TypingIndicator(conversationId, false)`
3. Listen for others typing → `connection.on("UserTyping", ...)`
4. Show "X is typing..." in UI

---

## ⚠️ Important Notes

1. **CORS:** Frontend must be running on `http://localhost:3000` or `http://localhost:5173` for SignalR to work
2. **JWT:** Tokens expire after 60 minutes. Implement auto-refresh logic
3. **SignalR Reconnection:** Use `.withAutomaticReconnect()` for resilience
4. **Message Ordering:** Messages from SignalR may arrive before REST API response. Handle duplicates
5. **Typing Timeout:** Clear typing indicator after 3 seconds of inactivity

---

## 🐛 Troubleshooting

### SignalR Connection Failed

- Check if Chat service is running: `http://localhost:5004/hubs/chat`
- Verify JWT token is valid
- Check CORS settings in backend
- Ensure you're using correct protocol (http/https, ws/wss)

### 401 Unauthorized

- Check if token is included in headers
- Verify token hasn't expired
- Try refreshing token with `/api/auth/refresh`

### Messages Not Appearing

- Verify you've joined the conversation via SignalR
- Check if message was sent successfully (check API response)
- Verify SignalR connection is active

---

## 📚 Additional Resources

- SignalR Client Documentation: https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client
- JWT Best Practices: https://jwt.io/introduction
- React Hooks for SignalR: https://www.npmjs.com/package/@microsoft/signalr

---

**Happy Coding! 🚀**

For issues or questions, check the backend logs or API documentation.
