// =============================================
// WeChat.com - ChatService MongoDB Schema
// Collection: messages
// Purpose: Chat messages in conversations
// =============================================

const DB_NAME = 'wechat_chat';
const COLLECTION_NAME = 'messages';

use(DB_NAME);

// Create collection with validation
db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["messageId", "conversationId", "senderId", "messageType", "content", "status", "createdAt"],
            properties: {
                messageId: {
                    bsonType: "string",
                    description: "Unique message identifier (UUID) - required"
                },
                conversationId: {
                    bsonType: "string",
                    description: "Conversation ID this message belongs to - required"
                },
                senderId: {
                    bsonType: "string",
                    description: "Message sender user ID - required"
                },
                messageType: {
                    bsonType: "string",
                    enum: ["text", "image", "video", "audio", "file", "location", "contact", "sticker", "gif", "system"],
                    description: "Type of message - required"
                },
                content: {
                    bsonType: "object",
                    description: "Message content - required",
                    properties: {
                        text: {
                            bsonType: ["string", "null"],
                            maxLength: 5000,
                            description: "Text content"
                        },
                        caption: {
                            bsonType: ["string", "null"],
                            maxLength: 1000,
                            description: "Caption for media messages"
                        }
                    }
                },
                // Media content
                mediaUrls: {
                    bsonType: "array",
                    items: {
                        bsonType: "object",
                        properties: {
                            url: { bsonType: "string" },
                            type: { bsonType: "string" },
                            thumbnailUrl: { bsonType: ["string", "null"] },
                            fileName: { bsonType: ["string", "null"] },
                            fileSize: { bsonType: ["int", "null"] },
                            duration: { bsonType: ["int", "null"] },
                            mimeType: { bsonType: ["string", "null"] }
                        }
                    },
                    description: "Media attachments"
                },
                // Location data
                location: {
                    bsonType: ["object", "null"],
                    properties: {
                        latitude: { bsonType: "double" },
                        longitude: { bsonType: "double" },
                        address: { bsonType: ["string", "null"] }
                    }
                },
                // Reply/Quote
                replyToMessageId: {
                    bsonType: ["string", "null"],
                    description: "ID of message being replied to"
                },
                replyToMessage: {
                    bsonType: ["object", "null"],
                    description: "Denormalized reply context for performance",
                    properties: {
                        messageId: { bsonType: "string" },
                        senderId: { bsonType: "string" },
                        content: { bsonType: "object" },
                        messageType: { bsonType: "string" }
                    }
                },
                // Forward
                forwardedFromMessageId: {
                    bsonType: ["string", "null"],
                    description: "Original message ID if forwarded"
                },
                forwardedFrom: {
                    bsonType: ["object", "null"],
                    properties: {
                        userId: { bsonType: "string" },
                        username: { bsonType: "string" },
                        conversationId: { bsonType: "string" }
                    }
                },
                // Mentions
                mentions: {
                    bsonType: "array",
                    items: {
                        bsonType: "object",
                        properties: {
                            userId: { bsonType: "string" },
                            username: { bsonType: "string" },
                            displayName: { bsonType: "string" }
                        }
                    },
                    description: "Users mentioned in message"
                },
                // Message status
                status: {
                    bsonType: "string",
                    enum: ["sending", "sent", "delivered", "read", "failed"],
                    description: "Message delivery status - required"
                },
                deliveredAt: {
                    bsonType: ["date", "null"],
                    description: "When message was delivered to server"
                },
                readBy: {
                    bsonType: "array",
                    items: {
                        bsonType: "object",
                        properties: {
                            userId: { bsonType: "string" },
                            readAt: { bsonType: "date" }
                        }
                    },
                    description: "Users who have read this message"
                },
                // Reactions
                reactions: {
                    bsonType: "array",
                    items: {
                        bsonType: "object",
                        properties: {
                            userId: { bsonType: "string" },
                            reaction: { bsonType: "string", enum: ["❤️", "👍", "😂", "😮", "😢", "🔥"] },
                            reactedAt: { bsonType: "date" }
                        }
                    },
                    description: "User reactions to message"
                },
                // Edit/Delete
                isEdited: {
                    bsonType: "bool",
                    description: "Whether message was edited"
                },
                editedAt: {
                    bsonType: ["date", "null"],
                    description: "Last edit timestamp"
                },
                isDeleted: {
                    bsonType: "bool",
                    description: "Whether message was deleted"
                },
                deletedAt: {
                    bsonType: ["date", "null"],
                    description: "Deletion timestamp"
                },
                deletedFor: {
                    bsonType: "array",
                    items: { bsonType: "string" },
                    description: "User IDs who deleted this message for themselves"
                },
                // System messages
                systemMessageType: {
                    bsonType: ["string", "null"],
                    enum: ["user_joined", "user_left", "group_created", "group_renamed", "admin_changed", null],
                    description: "Type of system message"
                },
                // Timestamps
                createdAt: {
                    bsonType: "date",
                    description: "Message creation timestamp - required"
                },
                updatedAt: {
                    bsonType: "date",
                    description: "Last update timestamp"
                },
                // Moderation
                isFlagged: {
                    bsonType: "bool",
                    description: "Whether message is flagged for review"
                },
                reportCount: {
                    bsonType: "int",
                    minimum: 0,
                    description: "Number of times message was reported"
                }
            }
        }
    },
    validationLevel: "moderate",
    validationAction: "error"
});

print("Creating indexes for messages collection...");

// Unique index
db.messages.createIndex({ "messageId": 1 }, { unique: true, name: "idx_messageId_unique" });

// Most critical: Get messages for a conversation (paginated, reverse chronological)
db.messages.createIndex(
    { "conversationId": 1, "createdAt": -1 },
    { name: "idx_conversationId_createdAt" }
);

// Get messages for conversation (chronological order)
db.messages.createIndex(
    { "conversationId": 1, "createdAt": 1 },
    { name: "idx_conversationId_createdAt_asc" }
);

// Get unread messages for user
db.messages.createIndex(
    { "conversationId": 1, "status": 1, "createdAt": 1 },
    { name: "idx_conversationId_status" }
);

// User's sent messages
db.messages.createIndex({ "senderId": 1, "createdAt": -1 }, { name: "idx_senderId_createdAt" });

// Mentions
db.messages.createIndex({ "mentions.userId": 1, "createdAt": -1 }, { name: "idx_mentions" });

// Replied messages
db.messages.createIndex({ "replyToMessageId": 1 }, { name: "idx_replyToMessageId" });

// Media messages (for gallery view)
db.messages.createIndex(
    { "conversationId": 1, "messageType": 1, "createdAt": -1 },
    { partialFilterExpression: { messageType: { $in: ["image", "video", "file"] } }, name: "idx_media_messages" }
);

// System messages
db.messages.createIndex(
    { "conversationId": 1, "messageType": 1, "createdAt": -1 },
    { partialFilterExpression: { messageType: "system" }, name: "idx_system_messages" }
);

// Flagged messages (moderation)
db.messages.createIndex({ "isFlagged": 1, "createdAt": -1 }, { name: "idx_flagged" });

// Compound index for efficient conversation queries with filters
db.messages.createIndex(
    { "conversationId": 1, "isDeleted": 1, "createdAt": -1 },
    { name: "idx_conversation_active" }
);

// Text search for message content
db.messages.createIndex(
    { "content.text": "text", "content.caption": "text" },
    { name: "idx_message_text_search" }
);

print("Indexes created successfully for messages collection!");

// Sample documents
const sampleTextMessage = {
    messageId: "msg-uuid-123",
    conversationId: "conv-uuid-456",
    senderId: "user-uuid-1",
    messageType: "text",
    content: {
        text: "Hey! How are you doing? @johndoe",
        caption: null
    },
    mediaUrls: [],
    location: null,
    replyToMessageId: null,
    replyToMessage: null,
    forwardedFromMessageId: null,
    forwardedFrom: null,
    mentions: [
        {
            userId: "user-uuid-2",
            username: "johndoe",
            displayName: "John Doe"
        }
    ],
    status: "read",
    deliveredAt: new Date("2024-01-15T10:30:10Z"),
    readBy: [
        {
            userId: "user-uuid-2",
            readAt: new Date("2024-01-15T10:31:00Z")
        }
    ],
    reactions: [
        {
            userId: "user-uuid-2",
            reaction: "👍",
            reactedAt: new Date("2024-01-15T10:31:05Z")
        }
    ],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedFor: [],
    systemMessageType: null,
    createdAt: new Date("2024-01-15T10:30:00Z"),
    updatedAt: new Date("2024-01-15T10:30:00Z"),
    isFlagged: false,
    reportCount: 0
};

const sampleImageMessage = {
    messageId: "msg-uuid-124",
    conversationId: "conv-uuid-456",
    senderId: "user-uuid-2",
    messageType: "image",
    content: {
        text: null,
        caption: "Check out this photo! 📸"
    },
    mediaUrls: [
        {
            url: "https://cdn.wechat.com/chat/images/photo123.jpg",
            type: "image",
            thumbnailUrl: "https://cdn.wechat.com/chat/images/photo123_thumb.jpg",
            fileName: "photo123.jpg",
            fileSize: 2048576,
            duration: null,
            mimeType: "image/jpeg"
        }
    ],
    location: null,
    replyToMessageId: "msg-uuid-123",
    replyToMessage: {
        messageId: "msg-uuid-123",
        senderId: "user-uuid-1",
        content: {
            text: "Hey! How are you doing?",
            caption: null
        },
        messageType: "text"
    },
    forwardedFromMessageId: null,
    forwardedFrom: null,
    mentions: [],
    status: "delivered",
    deliveredAt: new Date(),
    readBy: [],
    reactions: [],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedFor: [],
    systemMessageType: null,
    createdAt: new Date(),
    updatedAt: new Date(),
    isFlagged: false,
    reportCount: 0
};

const sampleSystemMessage = {
    messageId: "msg-uuid-125",
    conversationId: "conv-uuid-456",
    senderId: "system",
    messageType: "system",
    content: {
        text: "John Doe joined the group",
        caption: null
    },
    mediaUrls: [],
    location: null,
    replyToMessageId: null,
    replyToMessage: null,
    forwardedFromMessageId: null,
    forwardedFrom: null,
    mentions: [],
    status: "sent",
    deliveredAt: new Date(),
    readBy: [],
    reactions: [],
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    deletedFor: [],
    systemMessageType: "user_joined",
    createdAt: new Date(),
    updatedAt: new Date(),
    isFlagged: false,
    reportCount: 0
};

print("\n=============================================");
print("messages collection setup completed!");
print("Supports 10 message types including media and system messages");
print("=============================================\n");
