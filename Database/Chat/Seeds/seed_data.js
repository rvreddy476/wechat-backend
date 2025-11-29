// ========================================
// Chat Service - Seed Data for Development/Testing
// WARNING: Do not run in production!
// ========================================

// Clear existing data (DEVELOPMENT ONLY!)
db.messages.deleteMany({});
db.conversations.deleteMany({});

print("Existing data cleared");

// ========================================
// Sample Conversations
// ========================================

// One-to-One Conversation
const conv1 = {
  _id: "conv-550e8400-e29b-41d4-a716-446655440001",
  type: "OneToOne",
  participants: [
    {
      userId: "550e8400-e29b-41d4-a716-446655440000",
      username: "admin",
      joinedAt: new Date("2024-01-01T10:00:00Z"),
      lastReadAt: new Date("2024-01-15T14:30:00Z")
    },
    {
      userId: "550e8400-e29b-41d4-a716-446655440001",
      username: "testuser1",
      joinedAt: new Date("2024-01-01T10:00:00Z"),
      lastReadAt: new Date("2024-01-15T14:25:00Z")
    }
  ],
  groupName: null,
  groupAvatarUrl: null,
  groupDescription: null,
  createdBy: "550e8400-e29b-41d4-a716-446655440000",
  admins: [],
  lastMessage: {
    messageId: "msg-001",
    content: "Hey, how are you?",
    senderId: "550e8400-e29b-41d4-a716-446655440001",
    senderUsername: "testuser1",
    sentAt: new Date("2024-01-15T14:25:00Z")
  },
  isDeleted: false,
  deletedAt: null,
  createdAt: new Date("2024-01-01T10:00:00Z"),
  updatedAt: new Date("2024-01-15T14:25:00Z")
};

// Group Conversation - Project Team
const conv2 = {
  _id: "conv-550e8400-e29b-41d4-a716-446655440002",
  type: "Group",
  participants: [
    {
      userId: "550e8400-e29b-41d4-a716-446655440000",
      username: "admin",
      joinedAt: new Date("2024-01-05T08:00:00Z"),
      lastReadAt: new Date("2024-01-15T16:00:00Z")
    },
    {
      userId: "550e8400-e29b-41d4-a716-446655440001",
      username: "testuser1",
      joinedAt: new Date("2024-01-05T08:00:00Z"),
      lastReadAt: new Date("2024-01-15T15:45:00Z")
    },
    {
      userId: "550e8400-e29b-41d4-a716-446655440002",
      username: "testuser2",
      joinedAt: new Date("2024-01-05T09:30:00Z"),
      lastReadAt: new Date("2024-01-15T15:50:00Z")
    }
  ],
  groupName: "Project Alpha Team",
  groupAvatarUrl: "https://example.com/avatars/project-alpha.png",
  groupDescription: "Discussion group for Project Alpha development",
  createdBy: "550e8400-e29b-41d4-a716-446655440000",
  admins: [
    "550e8400-e29b-41d4-a716-446655440000",
    "550e8400-e29b-41d4-a716-446655440001"
  ],
  lastMessage: {
    messageId: "msg-010",
    content: "Meeting scheduled for tomorrow at 10 AM",
    senderId: "550e8400-e29b-41d4-a716-446655440000",
    senderUsername: "admin",
    sentAt: new Date("2024-01-15T16:00:00Z")
  },
  isDeleted: false,
  deletedAt: null,
  createdAt: new Date("2024-01-05T08:00:00Z"),
  updatedAt: new Date("2024-01-15T16:00:00Z")
};

// Group Conversation - Social
const conv3 = {
  _id: "conv-550e8400-e29b-41d4-a716-446655440003",
  type: "Group",
  participants: [
    {
      userId: "550e8400-e29b-41d4-a716-446655440000",
      username: "admin",
      joinedAt: new Date("2024-01-10T12:00:00Z"),
      lastReadAt: new Date("2024-01-14T18:00:00Z")
    },
    {
      userId: "550e8400-e29b-41d4-a716-446655440001",
      username: "testuser1",
      joinedAt: new Date("2024-01-10T12:00:00Z"),
      lastReadAt: new Date("2024-01-14T17:55:00Z")
    },
    {
      userId: "550e8400-e29b-41d4-a716-446655440002",
      username: "testuser2",
      joinedAt: new Date("2024-01-10T12:00:00Z"),
      lastReadAt: new Date("2024-01-14T17:50:00Z")
    }
  ],
  groupName: "Weekend Hangout",
  groupAvatarUrl: null,
  groupDescription: "Planning weekend activities and fun",
  createdBy: "550e8400-e29b-41d4-a716-446655440001",
  admins: ["550e8400-e29b-41d4-a716-446655440001"],
  lastMessage: {
    messageId: "msg-020",
    content: "Anyone up for a movie this weekend?",
    senderId: "550e8400-e29b-41d4-a716-446655440001",
    senderUsername: "testuser1",
    sentAt: new Date("2024-01-14T18:00:00Z")
  },
  isDeleted: false,
  deletedAt: null,
  createdAt: new Date("2024-01-10T12:00:00Z"),
  updatedAt: new Date("2024-01-14T18:00:00Z")
};

db.conversations.insertMany([conv1, conv2, conv3]);
print("3 sample conversations inserted");

// ========================================
// Sample Messages
// ========================================

const messages = [
  // Messages for One-to-One Conversation
  {
    _id: "msg-001",
    conversationId: "conv-550e8400-e29b-41d4-a716-446655440001",
    senderId: "550e8400-e29b-41d4-a716-446655440000",
    senderUsername: "admin",
    content: "Hi! Welcome to the platform!",
    messageType: "Text",
    mediaUrl: null,
    mediaThumbnailUrl: null,
    mediaSize: null,
    mediaDuration: null,
    replyToMessageId: null,
    replyToContent: null,
    readBy: [
      {
        userId: "550e8400-e29b-41d4-a716-446655440000",
        username: "admin",
        readAt: new Date("2024-01-15T10:00:00Z")
      },
      {
        userId: "550e8400-e29b-41d4-a716-446655440001",
        username: "testuser1",
        readAt: new Date("2024-01-15T10:05:00Z")
      }
    ],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedBy: null,
    reactions: [
      {
        userId: "550e8400-e29b-41d4-a716-446655440001",
        username: "testuser1",
        emoji: "👍",
        reactedAt: new Date("2024-01-15T10:05:00Z")
      }
    ],
    mentions: [],
    createdAt: new Date("2024-01-15T10:00:00Z"),
    updatedAt: new Date("2024-01-15T10:05:00Z")
  },
  {
    _id: "msg-002",
    conversationId: "conv-550e8400-e29b-41d4-a716-446655440001",
    senderId: "550e8400-e29b-41d4-a716-446655440001",
    senderUsername: "testuser1",
    content: "Thanks! Glad to be here!",
    messageType: "Text",
    mediaUrl: null,
    mediaThumbnailUrl: null,
    mediaSize: null,
    mediaDuration: null,
    replyToMessageId: "msg-001",
    replyToContent: "Hi! Welcome to the platform!",
    readBy: [
      {
        userId: "550e8400-e29b-41d4-a716-446655440001",
        username: "testuser1",
        readAt: new Date("2024-01-15T10:10:00Z")
      },
      {
        userId: "550e8400-e29b-41d4-a716-446655440000",
        username: "admin",
        readAt: new Date("2024-01-15T10:15:00Z")
      }
    ],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedBy: null,
    reactions: [],
    mentions: [],
    createdAt: new Date("2024-01-15T10:10:00Z"),
    updatedAt: new Date("2024-01-15T10:10:00Z")
  },
  {
    _id: "msg-003",
    conversationId: "conv-550e8400-e29b-41d4-a716-446655440001",
    senderId: "550e8400-e29b-41d4-a716-446655440000",
    senderUsername: "admin",
    content: "Check out this screenshot",
    messageType: "Image",
    mediaUrl: "https://example.com/media/screenshot123.png",
    mediaThumbnailUrl: "https://example.com/media/screenshot123_thumb.png",
    mediaSize: 1024000,
    mediaDuration: null,
    replyToMessageId: null,
    replyToContent: null,
    readBy: [
      {
        userId: "550e8400-e29b-41d4-a716-446655440000",
        username: "admin",
        readAt: new Date("2024-01-15T11:00:00Z")
      }
    ],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedBy: null,
    reactions: [],
    mentions: [],
    createdAt: new Date("2024-01-15T11:00:00Z"),
    updatedAt: new Date("2024-01-15T11:00:00Z")
  },

  // Messages for Group Conversation - Project Alpha
  {
    _id: "msg-010",
    conversationId: "conv-550e8400-e29b-41d4-a716-446655440002",
    senderId: "550e8400-e29b-41d4-a716-446655440000",
    senderUsername: "admin",
    content: "Welcome to Project Alpha team chat!",
    messageType: "Text",
    mediaUrl: null,
    mediaThumbnailUrl: null,
    mediaSize: null,
    mediaDuration: null,
    replyToMessageId: null,
    replyToContent: null,
    readBy: [
      {
        userId: "550e8400-e29b-41d4-a716-446655440000",
        username: "admin",
        readAt: new Date("2024-01-05T08:00:00Z")
      },
      {
        userId: "550e8400-e29b-41d4-a716-446655440001",
        username: "testuser1",
        readAt: new Date("2024-01-05T08:05:00Z")
      },
      {
        userId: "550e8400-e29b-41d4-a716-446655440002",
        username: "testuser2",
        readAt: new Date("2024-01-05T09:35:00Z")
      }
    ],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedBy: null,
    reactions: [
      {
        userId: "550e8400-e29b-41d4-a716-446655440001",
        username: "testuser1",
        emoji: "🎉",
        reactedAt: new Date("2024-01-05T08:05:00Z")
      },
      {
        userId: "550e8400-e29b-41d4-a716-446655440002",
        username: "testuser2",
        emoji: "🎉",
        reactedAt: new Date("2024-01-05T09:35:00Z")
      }
    ],
    mentions: [],
    createdAt: new Date("2024-01-05T08:00:00Z"),
    updatedAt: new Date("2024-01-05T09:35:00Z")
  },
  {
    _id: "msg-011",
    conversationId: "conv-550e8400-e29b-41d4-a716-446655440002",
    senderId: "550e8400-e29b-41d4-a716-446655440001",
    senderUsername: "testuser1",
    content: "Excited to work on this project with everyone!",
    messageType: "Text",
    mediaUrl: null,
    mediaThumbnailUrl: null,
    mediaSize: null,
    mediaDuration: null,
    replyToMessageId: null,
    replyToContent: null,
    readBy: [
      {
        userId: "550e8400-e29b-41d4-a716-446655440001",
        username: "testuser1",
        readAt: new Date("2024-01-05T08:10:00Z")
      },
      {
        userId: "550e8400-e29b-41d4-a716-446655440000",
        username: "admin",
        readAt: new Date("2024-01-05T08:15:00Z")
      }
    ],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedBy: null,
    reactions: [],
    mentions: [],
    createdAt: new Date("2024-01-05T08:10:00Z"),
    updatedAt: new Date("2024-01-05T08:10:00Z")
  },
  {
    _id: "msg-012",
    conversationId: "conv-550e8400-e29b-41d4-a716-446655440002",
    senderId: "550e8400-e29b-41d4-a716-446655440000",
    senderUsername: "admin",
    content: "Hey @testuser2, can you review the latest design docs?",
    messageType: "Text",
    mediaUrl: null,
    mediaThumbnailUrl: null,
    mediaSize: null,
    mediaDuration: null,
    replyToMessageId: null,
    replyToContent: null,
    readBy: [
      {
        userId: "550e8400-e29b-41d4-a716-446655440000",
        username: "admin",
        readAt: new Date("2024-01-15T14:00:00Z")
      },
      {
        userId: "550e8400-e29b-41d4-a716-446655440002",
        username: "testuser2",
        readAt: new Date("2024-01-15T14:30:00Z")
      }
    ],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedBy: null,
    reactions: [],
    mentions: ["550e8400-e29b-41d4-a716-446655440002"],
    createdAt: new Date("2024-01-15T14:00:00Z"),
    updatedAt: new Date("2024-01-15T14:00:00Z")
  },
  {
    _id: "msg-013",
    conversationId: "conv-550e8400-e29b-41d4-a716-446655440002",
    senderId: "550e8400-e29b-41d4-a716-446655440002",
    senderUsername: "testuser2",
    content: "Sure! I'll check them out now.",
    messageType: "Text",
    mediaUrl: null,
    mediaThumbnailUrl: null,
    mediaSize: null,
    mediaDuration: null,
    replyToMessageId: "msg-012",
    replyToContent: "Hey @testuser2, can you review the latest design docs?",
    readBy: [
      {
        userId: "550e8400-e29b-41d4-a716-446655440002",
        username: "testuser2",
        readAt: new Date("2024-01-15T14:35:00Z")
      }
    ],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedBy: null,
    reactions: [],
    mentions: [],
    createdAt: new Date("2024-01-15T14:35:00Z"),
    updatedAt: new Date("2024-01-15T14:35:00Z")
  },
  {
    _id: "msg-014",
    conversationId: "conv-550e8400-e29b-41d4-a716-446655440002",
    senderId: "550e8400-e29b-41d4-a716-446655440001",
    senderUsername: "testuser1",
    content: "Project update video",
    messageType: "Video",
    mediaUrl: "https://example.com/media/update-jan15.mp4",
    mediaThumbnailUrl: "https://example.com/media/update-jan15_thumb.jpg",
    mediaSize: 15728640,
    mediaDuration: 180,
    replyToMessageId: null,
    replyToContent: null,
    readBy: [],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedBy: null,
    reactions: [],
    mentions: [],
    createdAt: new Date("2024-01-15T15:00:00Z"),
    updatedAt: new Date("2024-01-15T15:00:00Z")
  },

  // Messages for Social Group
  {
    _id: "msg-020",
    conversationId: "conv-550e8400-e29b-41d4-a716-446655440003",
    senderId: "550e8400-e29b-41d4-a716-446655440001",
    senderUsername: "testuser1",
    content: "Anyone up for a movie this weekend?",
    messageType: "Text",
    mediaUrl: null,
    mediaThumbnailUrl: null,
    mediaSize: null,
    mediaDuration: null,
    replyToMessageId: null,
    replyToContent: null,
    readBy: [
      {
        userId: "550e8400-e29b-41d4-a716-446655440001",
        username: "testuser1",
        readAt: new Date("2024-01-14T18:00:00Z")
      }
    ],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedBy: null,
    reactions: [
      {
        userId: "550e8400-e29b-41d4-a716-446655440000",
        username: "admin",
        emoji: "🎬",
        reactedAt: new Date("2024-01-14T18:05:00Z")
      }
    ],
    mentions: [],
    createdAt: new Date("2024-01-14T18:00:00Z"),
    updatedAt: new Date("2024-01-14T18:05:00Z")
  },
  {
    _id: "msg-021",
    conversationId: "conv-550e8400-e29b-41d4-a716-446655440003",
    senderId: "550e8400-e29b-41d4-a716-446655440000",
    senderUsername: "admin",
    content: "I'm in! What movie are we watching?",
    messageType: "Text",
    mediaUrl: null,
    mediaThumbnailUrl: null,
    mediaSize: null,
    mediaDuration: null,
    replyToMessageId: "msg-020",
    replyToContent: "Anyone up for a movie this weekend?",
    readBy: [],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedBy: null,
    reactions: [],
    mentions: [],
    createdAt: new Date("2024-01-14T18:10:00Z"),
    updatedAt: new Date("2024-01-14T18:10:00Z")
  }
];

db.messages.insertMany(messages);
print(`${messages.length} sample messages inserted`);

// ========================================
// Verification
// ========================================

print("\n=== Database Seeding Complete ===");
print(`Total conversations: ${db.conversations.countDocuments({})}`);
print(`Total messages: ${db.messages.countDocuments({})}`);
print(`One-to-One conversations: ${db.conversations.countDocuments({ type: "OneToOne" })}`);
print(`Group conversations: ${db.conversations.countDocuments({ type: "Group" })}`);
