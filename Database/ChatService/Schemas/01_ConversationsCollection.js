// =============================================
// WeChat.com - ChatService MongoDB Schema
// Collection: conversations
// Purpose: Chat conversations (1:1 and group chats)
// =============================================

const DB_NAME = 'wechat_chat';
const COLLECTION_NAME = 'conversations';

use(DB_NAME);

// Create collection with validation
db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["conversationId", "type", "participants", "createdAt", "updatedAt"],
            properties: {
                conversationId: {
                    bsonType: "string",
                    description: "Unique conversation identifier (UUID) - required"
                },
                type: {
                    bsonType: "string",
                    enum: ["direct", "group"],
                    description: "Conversation type - required"
                },
                participants: {
                    bsonType: "array",
                    minItems: 2,
                    items: {
                        bsonType: "object",
                        required: ["userId"],
                        properties: {
                            userId: {
                                bsonType: "string",
                                description: "Participant user ID"
                            },
                            joinedAt: {
                                bsonType: "date",
                                description: "When user joined conversation"
                            },
                            leftAt: {
                                bsonType: ["date", "null"],
                                description: "When user left (for group chats)"
                            },
                            role: {
                                bsonType: "string",
                                enum: ["admin", "member"],
                                description: "User role in group (admin can add/remove users)"
                            },
                            isActive: {
                                bsonType: "bool",
                                description: "Whether user is currently in conversation"
                            },
                            notificationsMuted: {
                                bsonType: "bool",
                                description: "Whether user muted notifications"
                            },
                            lastReadMessageId: {
                                bsonType: ["string", "null"],
                                description: "Last message read by this user"
                            },
                            lastReadAt: {
                                bsonType: ["date", "null"],
                                description: "When user last read messages"
                            }
                        }
                    },
                    description: "Array of participant objects - required, min 2"
                },
                // Group chat specific fields
                groupName: {
                    bsonType: ["string", "null"],
                    maxLength: 100,
                    description: "Group chat name (null for direct messages)"
                },
                groupDescription: {
                    bsonType: ["string", "null"],
                    maxLength: 500,
                    description: "Group chat description"
                },
                groupIconUrl: {
                    bsonType: ["string", "null"],
                    description: "Group chat icon/avatar URL"
                },
                createdBy: {
                    bsonType: ["string", "null"],
                    description: "User ID who created the conversation"
                },
                // Last message info (denormalized for performance)
                lastMessage: {
                    bsonType: ["object", "null"],
                    properties: {
                        messageId: { bsonType: "string" },
                        senderId: { bsonType: "string" },
                        content: { bsonType: "string" },
                        messageType: { bsonType: "string" },
                        createdAt: { bsonType: "date" }
                    },
                    description: "Last message in conversation (for preview)"
                },
                lastMessageAt: {
                    bsonType: ["date", "null"],
                    description: "Timestamp of last message"
                },
                // Metadata
                messageCount: {
                    bsonType: "int",
                    minimum: 0,
                    description: "Total number of messages in conversation"
                },
                isArchived: {
                    bsonType: "bool",
                    description: "Whether conversation is archived"
                },
                isPinned: {
                    bsonType: "bool",
                    description: "Whether conversation is pinned"
                },
                isBlocked: {
                    bsonType: "bool",
                    description: "Whether conversation is blocked (spam)"
                },
                // Timestamps
                createdAt: {
                    bsonType: "date",
                    description: "Conversation creation timestamp - required"
                },
                updatedAt: {
                    bsonType: "date",
                    description: "Last update timestamp - required"
                },
                isDeleted: {
                    bsonType: "bool",
                    description: "Soft delete flag"
                },
                deletedAt: {
                    bsonType: ["date", "null"],
                    description: "Deletion timestamp"
                }
            }
        }
    },
    validationLevel: "moderate",
    validationAction: "error"
});

print("Creating indexes for conversations collection...");

// Unique index
db.conversations.createIndex({ "conversationId": 1 }, { unique: true, name: "idx_conversationId_unique" });

// Participant queries (most critical for chat)
db.conversations.createIndex(
    { "participants.userId": 1, "lastMessageAt": -1 },
    { name: "idx_participants_lastMessage" }
);

db.conversations.createIndex(
    { "participants.userId": 1, "isDeleted": 1, "lastMessageAt": -1 },
    { name: "idx_participants_active" }
);

// Direct message lookup (find conversation between two users)
db.conversations.createIndex(
    { "type": 1, "participants.userId": 1 },
    { name: "idx_type_participants" }
);

// Get user's conversations sorted by last activity
db.conversations.createIndex(
    { "participants.userId": 1, "isPinned": -1, "lastMessageAt": -1 },
    { name: "idx_user_conversations" }
);

// Archived conversations
db.conversations.createIndex(
    { "participants.userId": 1, "isArchived": 1 },
    { name: "idx_archived" }
);

// Group chats
db.conversations.createIndex(
    { "type": 1, "createdAt": -1 },
    { partialFilterExpression: { type: "group" }, name: "idx_group_chats" }
);

// Created by user (for group management)
db.conversations.createIndex({ "createdBy": 1, "createdAt": -1 }, { name: "idx_createdBy" });

// Text search for group names
db.conversations.createIndex(
    { "groupName": "text", "groupDescription": "text" },
    { name: "idx_group_text_search" }
);

print("Indexes created successfully for conversations collection!");

// Sample documents
const sampleDirectConversation = {
    conversationId: "conv-uuid-123",
    type: "direct",
    participants: [
        {
            userId: "user-uuid-1",
            joinedAt: new Date(),
            leftAt: null,
            role: "member",
            isActive: true,
            notificationsMuted: false,
            lastReadMessageId: "msg-uuid-100",
            lastReadAt: new Date()
        },
        {
            userId: "user-uuid-2",
            joinedAt: new Date(),
            leftAt: null,
            role: "member",
            isActive: true,
            notificationsMuted: false,
            lastReadMessageId: "msg-uuid-99",
            lastReadAt: new Date("2024-01-15T10:30:00Z")
        }
    ],
    groupName: null,
    groupDescription: null,
    groupIconUrl: null,
    createdBy: "user-uuid-1",
    lastMessage: {
        messageId: "msg-uuid-100",
        senderId: "user-uuid-1",
        content: "Hey, how are you?",
        messageType: "text",
        createdAt: new Date()
    },
    lastMessageAt: new Date(),
    messageCount: 42,
    isArchived: false,
    isPinned: false,
    isBlocked: false,
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date(),
    isDeleted: false,
    deletedAt: null
};

const sampleGroupConversation = {
    conversationId: "conv-uuid-456",
    type: "group",
    participants: [
        {
            userId: "user-uuid-1",
            joinedAt: new Date("2024-01-01T00:00:00Z"),
            leftAt: null,
            role: "admin",
            isActive: true,
            notificationsMuted: false,
            lastReadMessageId: "msg-uuid-200",
            lastReadAt: new Date()
        },
        {
            userId: "user-uuid-2",
            joinedAt: new Date("2024-01-01T00:00:00Z"),
            leftAt: null,
            role: "member",
            isActive: true,
            notificationsMuted: true,
            lastReadMessageId: "msg-uuid-198",
            lastReadAt: new Date("2024-01-15T09:00:00Z")
        },
        {
            userId: "user-uuid-3",
            joinedAt: new Date("2024-01-01T00:00:00Z"),
            leftAt: null,
            role: "member",
            isActive: true,
            notificationsMuted: false,
            lastReadMessageId: "msg-uuid-200",
            lastReadAt: new Date()
        }
    ],
    groupName: "Project Team",
    groupDescription: "Discussion about the new project",
    groupIconUrl: "https://cdn.wechat.com/groups/project-team.jpg",
    createdBy: "user-uuid-1",
    lastMessage: {
        messageId: "msg-uuid-200",
        senderId: "user-uuid-3",
        content: "Meeting at 3pm today",
        messageType: "text",
        createdAt: new Date()
    },
    lastMessageAt: new Date(),
    messageCount: 156,
    isArchived: false,
    isPinned: true,
    isBlocked: false,
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date(),
    isDeleted: false,
    deletedAt: null
};

print("\n=============================================");
print("conversations collection setup completed!");
print("Supports both direct (1:1) and group chats");
print("=============================================\n");
