// ========================================
// Chat Service - Messages Collection
// ========================================

db.createCollection("messages", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["conversationId", "senderId", "senderUsername", "content", "messageType", "createdAt"],
      properties: {
        _id: {
          bsonType: "string",
          description: "Unique message identifier (ObjectId as string)"
        },
        conversationId: {
          bsonType: "string",
          description: "Reference to the conversation this message belongs to"
        },
        senderId: {
          bsonType: "string",
          description: "UUID of the message sender (from Auth service)"
        },
        senderUsername: {
          bsonType: "string",
          description: "Username of the sender"
        },
        content: {
          bsonType: "string",
          description: "Message content (text, caption, or file description)"
        },
        messageType: {
          enum: ["Text", "Image", "Video", "Audio", "File"],
          description: "Type of message content"
        },
        mediaUrl: {
          bsonType: ["string", "null"],
          description: "URL to media file (for Image, Video, Audio, File types)"
        },
        mediaThumbnailUrl: {
          bsonType: ["string", "null"],
          description: "Thumbnail URL for video/image messages"
        },
        mediaSize: {
          bsonType: ["long", "null"],
          description: "Size of media file in bytes"
        },
        mediaDuration: {
          bsonType: ["int", "null"],
          description: "Duration in seconds (for Audio/Video)"
        },
        replyToMessageId: {
          bsonType: ["string", "null"],
          description: "ID of message being replied to"
        },
        replyToContent: {
          bsonType: ["string", "null"],
          description: "Preview of replied message content"
        },
        readBy: {
          bsonType: "array",
          description: "List of users who have read this message",
          items: {
            bsonType: "object",
            required: ["userId", "readAt"],
            properties: {
              userId: {
                bsonType: "string"
              },
              username: {
                bsonType: "string"
              },
              readAt: {
                bsonType: "date"
              }
            }
          }
        },
        isEdited: {
          bsonType: "bool",
          description: "Whether the message has been edited"
        },
        editedAt: {
          bsonType: ["date", "null"],
          description: "When the message was last edited"
        },
        isDeleted: {
          bsonType: "bool",
          description: "Soft delete flag"
        },
        deletedAt: {
          bsonType: ["date", "null"],
          description: "When the message was soft deleted"
        },
        deletedBy: {
          bsonType: ["string", "null"],
          description: "User ID who deleted the message"
        },
        reactions: {
          bsonType: "array",
          description: "Emoji reactions to the message",
          items: {
            bsonType: "object",
            required: ["userId", "emoji", "reactedAt"],
            properties: {
              userId: {
                bsonType: "string"
              },
              username: {
                bsonType: "string"
              },
              emoji: {
                bsonType: "string"
              },
              reactedAt: {
                bsonType: "date"
              }
            }
          }
        },
        mentions: {
          bsonType: "array",
          description: "List of mentioned user IDs",
          items: {
            bsonType: "string"
          }
        },
        createdAt: {
          bsonType: "date",
          description: "When the message was sent"
        },
        updatedAt: {
          bsonType: "date",
          description: "When the message was last updated"
        }
      }
    }
  }
});

print("Messages collection created with validation schema");
