# Chat Service - Database Operations Guide

## Table of Contents
1. [Collections Overview](#collections-overview)
2. [Collections Reference](#collections-reference)
3. [Common Operations](#common-operations)
4. [Real-Time Features](#real-time-features)
5. [Query Patterns](#query-patterns)

---

## Collections Overview

### Collections
- **conversations** - Chat conversations (1-on-1 and groups)
- **messages** - Chat messages with media support

### Indexes
- Conversation indexes for user lookups and sorting
- Message indexes for pagination and search
- Text search indexes for message content

---

## Collections Reference

### 1. conversations

**Purpose**: Store chat conversations (one-to-one and group chats)

**Key Fields**:
```javascript
{
  _id: "conv-uuid",                    // Unique conversation ID
  type: "OneToOne" | "Group",          // Conversation type
  participants: [                       // Array of participants
    {
      userId: "uuid",
      username: "john_doe",
      joinedAt: ISODate(),
      lastReadAt: ISODate()
    }
  ],
  groupName: "Project Team",           // For groups only
  groupAvatarUrl: "https://...",       // Group avatar
  groupDescription: "Team chat",       // Group description
  createdBy: "uuid",                   // Creator user ID
  admins: ["uuid1", "uuid2"],          // Admin user IDs (groups)
  lastMessage: {                        // Preview
    messageId: "msg-uuid",
    content: "Hello!",
    senderId: "uuid",
    senderUsername: "john",
    sentAt: ISODate()
  },
  isDeleted: false,
  deletedAt: null,
  createdAt: ISODate(),
  updatedAt: ISODate()
}
```

**Indexes**:
- `participants.userId` + `isDeleted` + `updatedAt` - User's conversations
- `type` + `participants.userId` + `isDeleted` - Find 1-on-1 conversations
- `lastMessage.sentAt` + `isDeleted` - Sort by activity

---

### 2. messages

**Purpose**: Store all chat messages

**Key Fields**:
```javascript
{
  _id: "msg-uuid",                     // Unique message ID
  conversationId: "conv-uuid",         // Parent conversation
  senderId: "uuid",                    // Sender user ID
  senderUsername: "john_doe",          // Sender username
  content: "Hello, how are you?",      // Message text
  messageType: "Text",                 // Text|Image|Video|Audio|File
  mediaUrl: "https://...",             // Media file URL (if applicable)
  mediaThumbnailUrl: "https://...",    // Thumbnail
  mediaSize: 1024000,                  // File size in bytes
  mediaDuration: 120,                  // Duration (audio/video)
  replyToMessageId: "msg-uuid",        // Reply to message ID
  replyToContent: "Original msg",      // Preview of replied message
  readBy: [                             // Read receipts
    {
      userId: "uuid",
      username: "jane",
      readAt: ISODate()
    }
  ],
  isEdited: false,
  editedAt: null,
  isDeleted: false,
  deletedAt: null,
  deletedBy: null,
  reactions: [                          // Emoji reactions
    {
      userId: "uuid",
      username: "jane",
      emoji: "👍",
      reactedAt: ISODate()
    }
  ],
  mentions: ["uuid1", "uuid2"],        // Mentioned user IDs
  createdAt: ISODate(),
  updatedAt: ISODate()
}
```

**Indexes**:
- `conversationId` + `isDeleted` + `createdAt` - Get conversation messages
- `senderId` + `createdAt` - Get user's messages
- `readBy.userId` + `conversationId` - Unread messages
- Text index on `content` and `senderUsername` - Search

---

## Common Operations

### 💬 Create One-to-One Conversation

**Step 1: Check if Conversation Exists**
```javascript
// Check for existing 1-on-1 conversation
db.conversations.findOne({
  type: "OneToOne",
  "participants.userId": {
    $all: [
      "550e8400-e29b-41d4-a716-446655440000",  // User 1
      "550e8400-e29b-41d4-a716-446655440001"   // User 2
    ]
  },
  isDeleted: false
});
```

**Step 2: Create if Not Exists**
```javascript
// If no conversation exists, create one
const conversation = {
  _id: `conv-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
  type: "OneToOne",
  participants: [
    {
      userId: "550e8400-e29b-41d4-a716-446655440000",
      username: "john_doe",
      joinedAt: new Date(),
      lastReadAt: null
    },
    {
      userId: "550e8400-e29b-41d4-a716-446655440001",
      username: "jane_smith",
      joinedAt: new Date(),
      lastReadAt: null
    }
  ],
  groupName: null,
  groupAvatarUrl: null,
  groupDescription: null,
  createdBy: "550e8400-e29b-41d4-a716-446655440000",
  admins: [],
  lastMessage: null,
  isDeleted: false,
  deletedAt: null,
  createdAt: new Date(),
  updatedAt: new Date()
};

db.conversations.insertOne(conversation);
```

**Application Code Example** (Node.js):
```javascript
async function createOrGetConversation(userId1, userId2, username1, username2) {
  // Check existing
  let conversation = await db.collection('conversations').findOne({
    type: "OneToOne",
    "participants.userId": { $all: [userId1, userId2] },
    isDeleted: false
  });

  // Create if not found
  if (!conversation) {
    conversation = {
      _id: `conv-${Date.now()}-${generateId()}`,
      type: "OneToOne",
      participants: [
        { userId: userId1, username: username1, joinedAt: new Date() },
        { userId: userId2, username: username2, joinedAt: new Date() }
      ],
      createdBy: userId1,
      admins: [],
      lastMessage: null,
      isDeleted: false,
      createdAt: new Date(),
      updatedAt: new Date()
    };

    await db.collection('conversations').insertOne(conversation);
  }

  return conversation;
}
```

---

### 👥 Create Group Conversation

```javascript
const groupConversation = {
  _id: `conv-${Date.now()}-${generateId()}`,
  type: "Group",
  participants: [
    {
      userId: "550e8400-e29b-41d4-a716-446655440000",
      username: "john_doe",
      joinedAt: new Date()
    },
    {
      userId: "550e8400-e29b-41d4-a716-446655440001",
      username: "jane_smith",
      joinedAt: new Date()
    },
    {
      userId: "550e8400-e29b-41d4-a716-446655440002",
      username: "bob_jones",
      joinedAt: new Date()
    }
  ],
  groupName: "Project Team",
  groupAvatarUrl: "https://example.com/avatars/team.png",
  groupDescription: "Team collaboration chat",
  createdBy: "550e8400-e29b-41d4-a716-446655440000",
  admins: ["550e8400-e29b-41d4-a716-446655440000"],  // Creator is admin
  lastMessage: null,
  isDeleted: false,
  deletedAt: null,
  createdAt: new Date(),
  updatedAt: new Date()
};

db.conversations.insertOne(groupConversation);
```

---

### 📤 Send Message

**Step 1: Create Message Document**
```javascript
const message = {
  _id: `msg-${Date.now()}-${generateId()}`,
  conversationId: "conv-12345",
  senderId: "550e8400-e29b-41d4-a716-446655440000",
  senderUsername: "john_doe",
  content: "Hello everyone!",
  messageType: "Text",
  mediaUrl: null,
  mediaThumbnailUrl: null,
  mediaSize: null,
  mediaDuration: null,
  replyToMessageId: null,
  replyToContent: null,
  readBy: [
    {
      userId: "550e8400-e29b-41d4-a716-446655440000",  // Sender auto-read
      username: "john_doe",
      readAt: new Date()
    }
  ],
  isEdited: false,
  editedAt: null,
  isDeleted: false,
  deletedAt: null,
  deletedBy: null,
  reactions: [],
  mentions: [],
  createdAt: new Date(),
  updatedAt: new Date()
};

db.messages.insertOne(message);
```

**Step 2: Update Conversation Last Message**
```javascript
db.conversations.updateOne(
  { _id: "conv-12345" },
  {
    $set: {
      lastMessage: {
        messageId: message._id,
        content: message.content,
        senderId: message.senderId,
        senderUsername: message.senderUsername,
        sentAt: message.createdAt
      },
      updatedAt: new Date()
    }
  }
);
```

**Step 3: Broadcast via SignalR** (In Application Code)
```javascript
// Send to all conversation participants via SignalR
await hubContext.clients.group(conversationId).sendAsync("ReceiveMessage", {
  messageId: message._id,
  conversationId: message.conversationId,
  senderId: message.senderId,
  senderUsername: message.senderUsername,
  content: message.content,
  messageType: message.messageType,
  sentAt: message.createdAt
});
```

**Complete Flow** (Application Code):
```javascript
async function sendMessage(conversationId, senderId, senderUsername, content) {
  // 1. Create message
  const message = {
    _id: `msg-${Date.now()}-${generateId()}`,
    conversationId,
    senderId,
    senderUsername,
    content,
    messageType: "Text",
    readBy: [{ userId: senderId, username: senderUsername, readAt: new Date() }],
    reactions: [],
    mentions: [],
    isEdited: false,
    isDeleted: false,
    createdAt: new Date(),
    updatedAt: new Date()
  };

  await db.collection('messages').insertOne(message);

  // 2. Update conversation
  await db.collection('conversations').updateOne(
    { _id: conversationId },
    {
      $set: {
        lastMessage: {
          messageId: message._id,
          content: message.content,
          senderId: message.senderId,
          senderUsername: message.senderUsername,
          sentAt: message.createdAt
        },
        updatedAt: new Date()
      }
    }
  );

  // 3. Broadcast via SignalR
  await broadcastMessage(conversationId, message);

  return message;
}
```

---

### 📎 Send Media Message (Image/Video/Audio)

```javascript
const mediaMessage = {
  _id: `msg-${Date.now()}-${generateId()}`,
  conversationId: "conv-12345",
  senderId: "550e8400-e29b-41d4-a716-446655440000",
  senderUsername: "john_doe",
  content: "Check out this photo!",      // Caption
  messageType: "Image",
  mediaUrl: "https://cdn.example.com/images/photo123.jpg",
  mediaThumbnailUrl: "https://cdn.example.com/images/photo123_thumb.jpg",
  mediaSize: 2048000,                    // 2MB
  mediaDuration: null,                   // Not applicable for images
  replyToMessageId: null,
  readBy: [
    { userId: senderId, username: "john_doe", readAt: new Date() }
  ],
  isEdited: false,
  isDeleted: false,
  reactions: [],
  mentions: [],
  createdAt: new Date(),
  updatedAt: new Date()
};

db.messages.insertOne(mediaMessage);

// Update conversation
db.conversations.updateOne(
  { _id: "conv-12345" },
  {
    $set: {
      lastMessage: {
        messageId: mediaMessage._id,
        content: "📷 Photo",  // Show icon for media
        senderId: mediaMessage.senderId,
        senderUsername: mediaMessage.senderUsername,
        sentAt: mediaMessage.createdAt
      },
      updatedAt: new Date()
    }
  }
);
```

---

### 💬 Reply to Message

```javascript
// Get original message first
const originalMessage = db.messages.findOne({
  _id: "msg-original-123"
});

// Create reply
const replyMessage = {
  _id: `msg-${Date.now()}-${generateId()}`,
  conversationId: "conv-12345",
  senderId: "550e8400-e29b-41d4-a716-446655440001",
  senderUsername: "jane_smith",
  content: "Thanks for sharing!",
  messageType: "Text",
  replyToMessageId: "msg-original-123",
  replyToContent: originalMessage.content.substring(0, 100),  // Preview
  readBy: [
    { userId: "550e8400-e29b-41d4-a716-446655440001", username: "jane_smith", readAt: new Date() }
  ],
  isEdited: false,
  isDeleted: false,
  reactions: [],
  mentions: [],
  createdAt: new Date(),
  updatedAt: new Date()
};

db.messages.insertOne(replyMessage);
```

---

### ✅ Mark Message as Read

**Single User Marking as Read**:
```javascript
db.messages.updateOne(
  {
    _id: "msg-12345",
    "readBy.userId": { $ne: "550e8400-e29b-41d4-a716-446655440001" }  // Not already read
  },
  {
    $addToSet: {
      readBy: {
        userId: "550e8400-e29b-41d4-a716-446655440001",
        username: "jane_smith",
        readAt: new Date()
      }
    }
  }
);
```

**Mark All Messages in Conversation as Read**:
```javascript
db.messages.updateMany(
  {
    conversationId: "conv-12345",
    "readBy.userId": { $ne: "550e8400-e29b-41d4-a716-446655440001" },
    isDeleted: false
  },
  {
    $addToSet: {
      readBy: {
        userId: "550e8400-e29b-41d4-a716-446655440001",
        username: "jane_smith",
        readAt: new Date()
      }
    }
  }
);
```

**Update Participant Last Read**:
```javascript
db.conversations.updateOne(
  {
    _id: "conv-12345",
    "participants.userId": "550e8400-e29b-41d4-a716-446655440001"
  },
  {
    $set: {
      "participants.$.lastReadAt": new Date()
    }
  }
);
```

---

### 👍 Add Reaction to Message

```javascript
db.messages.updateOne(
  {
    _id: "msg-12345"
  },
  {
    $addToSet: {
      reactions: {
        userId: "550e8400-e29b-41d4-a716-446655440001",
        username: "jane_smith",
        emoji: "👍",
        reactedAt: new Date()
      }
    }
  }
);
```

**Remove Reaction**:
```javascript
db.messages.updateOne(
  {
    _id: "msg-12345"
  },
  {
    $pull: {
      reactions: {
        userId: "550e8400-e29b-41d4-a716-446655440001"
      }
    }
  }
);
```

---

### ✏️ Edit Message

```javascript
db.messages.updateOne(
  {
    _id: "msg-12345",
    senderId: "550e8400-e29b-41d4-a716-446655440000"  // Only sender can edit
  },
  {
    $set: {
      content: "Updated message content",
      isEdited: true,
      editedAt: new Date(),
      updatedAt: new Date()
    }
  }
);
```

---

### 🗑️ Delete Message

**Soft Delete**:
```javascript
db.messages.updateOne(
  {
    _id: "msg-12345"
  },
  {
    $set: {
      isDeleted: true,
      deletedAt: new Date(),
      deletedBy: "550e8400-e29b-41d4-a716-446655440000",
      updatedAt: new Date()
    }
  }
);
```

---

### 👤 Add Participant to Group

```javascript
db.conversations.updateOne(
  {
    _id: "conv-12345",
    type: "Group"
  },
  {
    $addToSet: {
      participants: {
        userId: "550e8400-e29b-41d4-a716-446655440003",
        username: "new_user",
        joinedAt: new Date(),
        lastReadAt: null
      }
    },
    $set: {
      updatedAt: new Date()
    }
  }
);
```

---

### ❌ Remove Participant from Group

```javascript
db.conversations.updateOne(
  {
    _id: "conv-12345",
    type: "Group"
  },
  {
    $pull: {
      participants: {
        userId: "550e8400-e29b-41d4-a716-446655440003"
      }
    },
    $set: {
      updatedAt: new Date()
    }
  }
);
```

---

### 👑 Make User Admin

```javascript
db.conversations.updateOne(
  {
    _id: "conv-12345",
    type: "Group"
  },
  {
    $addToSet: {
      admins: "550e8400-e29b-41d4-a716-446655440001"
    },
    $set: {
      updatedAt: new Date()
    }
  }
);
```

---

## Real-Time Features

### 📡 User Online/Offline Status

**Tracked in Application Memory** (SignalR Hub):
```javascript
// In ChatHub.cs (C#)
private static ConcurrentDictionary<string, UserConnection> OnlineUsers = new();

public override async Task OnConnectedAsync()
{
    var userId = GetUserId();
    var username = GetUsername();

    OnlineUsers[userId] = new UserConnection
    {
        UserId = userId,
        Username = username,
        ConnectionId = Context.ConnectionId
    };

    // Broadcast to all
    await Clients.All.SendAsync("UserOnline", new { userId, username });

    // Send online users to caller
    await Clients.Caller.SendAsync("OnlineUsers", OnlineUsers.Values.ToList());
}
```

---

### ⌨️ Typing Indicators

**Tracked in Application Memory** (SignalR Hub):
```javascript
// In ChatHub.cs (C#)
private static ConcurrentDictionary<string, HashSet<string>> TypingUsers = new();

public async Task TypingIndicator(string conversationId, bool isTyping)
{
    var userId = GetUserId();

    if (isTyping)
    {
        if (!TypingUsers.ContainsKey(conversationId))
            TypingUsers[conversationId] = new HashSet<string>();

        TypingUsers[conversationId].Add(userId);

        await Clients.GroupExcept(conversationId, Context.ConnectionId)
            .SendAsync("UserTyping", conversationId, userId, GetUsername());
    }
    else
    {
        if (TypingUsers.ContainsKey(conversationId))
            TypingUsers[conversationId].Remove(userId);

        await Clients.GroupExcept(conversationId, Context.ConnectionId)
            .SendAsync("UserStoppedTyping", conversationId, userId);
    }
}
```

**Client Usage**:
```javascript
// User starts typing
connection.invoke("TypingIndicator", conversationId, true);

// User stops typing (after 3 seconds of inactivity)
setTimeout(() => {
  connection.invoke("TypingIndicator", conversationId, false);
}, 3000);
```

---

## Query Patterns

### 📋 Get User's Conversations (Sorted by Last Activity)

```javascript
db.conversations.find({
  "participants.userId": "550e8400-e29b-41d4-a716-446655440000",
  isDeleted: false
}).sort({ "lastMessage.sentAt": -1 });
```

---

### 💬 Get Messages for Conversation (Paginated)

```javascript
// Get latest 50 messages
db.messages.find({
  conversationId: "conv-12345",
  isDeleted: false
})
.sort({ createdAt: -1 })
.limit(50);

// Get next page (messages before a certain timestamp)
db.messages.find({
  conversationId: "conv-12345",
  isDeleted: false,
  createdAt: { $lt: ISODate("2024-01-15T10:00:00Z") }
})
.sort({ createdAt: -1 })
.limit(50);
```

---

### 🔍 Search Messages in Conversation

```javascript
db.messages.find({
  $text: { $search: "important meeting" },
  conversationId: "conv-12345",
  isDeleted: false
}).sort({ score: { $meta: "textScore" } });
```

---

### 🔢 Get Unread Message Count

```javascript
db.messages.countDocuments({
  conversationId: "conv-12345",
  "readBy.userId": { $ne: "550e8400-e29b-41d4-a716-446655440000" },
  isDeleted: false
});
```

**Get Unread Counts for All Conversations**:
```javascript
db.messages.aggregate([
  {
    $match: {
      isDeleted: false,
      "readBy.userId": { $ne: "550e8400-e29b-41d4-a716-446655440000" }
    }
  },
  {
    $group: {
      _id: "$conversationId",
      unreadCount: { $sum: 1 },
      lastMessage: { $last: "$$ROOT" }
    }
  },
  {
    $sort: { "lastMessage.createdAt": -1 }
  }
]);
```

---

### 📸 Get All Media Messages in Conversation

```javascript
db.messages.find({
  conversationId: "conv-12345",
  messageType: { $in: ["Image", "Video", "Audio", "File"] },
  isDeleted: false
}).sort({ createdAt: -1 });
```

---

### 🔗 Get Reply Thread

```javascript
// Get original message + all replies
db.messages.find({
  $or: [
    { _id: "msg-original-123" },
    { replyToMessageId: "msg-original-123" }
  ],
  isDeleted: false
}).sort({ createdAt: 1 });
```

---

### 👥 Get Group Members

```javascript
db.conversations.findOne(
  { _id: "conv-12345", type: "Group" },
  { projection: { participants: 1, admins: 1 } }
);
```

---

### ✅ Check if User is in Conversation

```javascript
const exists = db.conversations.findOne({
  _id: "conv-12345",
  "participants.userId": "550e8400-e29b-41d4-a716-446655440000",
  isDeleted: false
});

if (exists) {
  // User is participant
} else {
  // User is not participant
}
```

---

## Best Practices

1. **Always Check Permissions**: Verify user is participant before showing messages
2. **Use Pagination**: Always limit message queries to prevent loading too much data
3. **Update Last Message**: Keep conversation.lastMessage in sync for fast previews
4. **Broadcast Changes**: Use SignalR to broadcast all changes in real-time
5. **Handle Offline Users**: Store messages in database, deliver when user comes online
6. **Atomic Operations**: Use `$addToSet` and `$pull` for array operations
7. **Soft Delete**: Never hard delete messages for audit trail
8. **Index Usage**: Ensure queries use indexes (check with `.explain()`)
9. **Clean Up**: Regularly archive or delete very old soft-deleted messages
10. **Read Receipts**: Only send read receipts for messages sent by others

---

## Performance Tips

1. **Limit Message History**: Load messages in batches of 20-50
2. **Cache Online Users**: Store online users in Redis instead of querying database
3. **Optimize Indexes**: Ensure compound indexes match your query patterns
4. **Use Projections**: Only fetch fields you need
5. **Aggregate Unread Counts**: Calculate unread counts in batches, not per conversation
6. **SignalR Groups**: Use SignalR groups for conversation-based broadcasting
7. **Lazy Load Media**: Load media URLs separately from message text
8. **Archive Old Chats**: Move conversations older than 1 year to archive collection

---

This guide covers all database operations for the Chat service. For SignalR integration and real-time features, refer to the main application documentation and `INSTRUCTIONS.md`.
