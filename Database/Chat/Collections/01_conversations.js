// ========================================
// Chat Service - Conversations Collection
// ========================================

db.createCollection("conversations", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["type", "participants", "createdBy", "createdAt"],
      properties: {
        _id: {
          bsonType: "string",
          description: "Unique conversation identifier (ObjectId as string)"
        },
        type: {
          enum: ["OneToOne", "Group"],
          description: "Type of conversation - OneToOne or Group"
        },
        participants: {
          bsonType: "array",
          description: "List of participants in the conversation",
          minItems: 2,
          items: {
            bsonType: "object",
            required: ["userId", "username", "joinedAt"],
            properties: {
              userId: {
                bsonType: "string",
                description: "UUID of the participant (from Auth service)"
              },
              username: {
                bsonType: "string",
                description: "Username of the participant"
              },
              joinedAt: {
                bsonType: "date",
                description: "When the participant joined the conversation"
              },
              lastReadAt: {
                bsonType: "date",
                description: "Last time participant read messages"
              }
            }
          }
        },
        groupName: {
          bsonType: ["string", "null"],
          description: "Name of the group (required for Group type)"
        },
        groupAvatarUrl: {
          bsonType: ["string", "null"],
          description: "Avatar URL for the group"
        },
        groupDescription: {
          bsonType: ["string", "null"],
          description: "Description of the group"
        },
        createdBy: {
          bsonType: "string",
          description: "UUID of user who created the conversation"
        },
        admins: {
          bsonType: "array",
          description: "List of admin user IDs (for groups)",
          items: {
            bsonType: "string"
          }
        },
        lastMessage: {
          bsonType: ["object", "null"],
          description: "Last message sent in this conversation",
          properties: {
            messageId: {
              bsonType: "string"
            },
            content: {
              bsonType: "string"
            },
            senderId: {
              bsonType: "string"
            },
            senderUsername: {
              bsonType: "string"
            },
            sentAt: {
              bsonType: "date"
            }
          }
        },
        isDeleted: {
          bsonType: "bool",
          description: "Soft delete flag"
        },
        deletedAt: {
          bsonType: ["date", "null"],
          description: "When the conversation was soft deleted"
        },
        createdAt: {
          bsonType: "date",
          description: "When the conversation was created"
        },
        updatedAt: {
          bsonType: "date",
          description: "When the conversation was last updated"
        }
      }
    }
  }
});

print("Conversations collection created with validation schema");
