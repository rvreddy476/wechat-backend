// ========================================
// UserProfile Service - Friend Requests Collection
// ========================================

db.createCollection("friend_requests", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["senderId", "receiverId", "status", "createdAt"],
      properties: {
        _id: {
          bsonType: "string",
          description: "Unique friend request identifier"
        },
        senderId: {
          bsonType: "string",
          description: "UUID of user sending the request"
        },
        senderUsername: {
          bsonType: "string",
          description: "Username of sender"
        },
        senderAvatarUrl: {
          bsonType: ["string", "null"],
          description: "Avatar URL of sender"
        },
        receiverId: {
          bsonType: "string",
          description: "UUID of user receiving the request"
        },
        receiverUsername: {
          bsonType: "string",
          description: "Username of receiver"
        },
        receiverAvatarUrl: {
          bsonType: ["string", "null"],
          description: "Avatar URL of receiver"
        },
        status: {
          enum: ["Pending", "Accepted", "Rejected", "Cancelled"],
          description: "Status of the friend request"
        },
        message: {
          bsonType: ["string", "null"],
          description: "Optional message with the request",
          maxLength: 200
        },
        respondedAt: {
          bsonType: ["date", "null"],
          description: "When the request was accepted/rejected"
        },
        expiresAt: {
          bsonType: ["date", "null"],
          description: "When the request expires (auto-reject)"
        },
        createdAt: {
          bsonType: "date",
          description: "Request creation timestamp"
        },
        updatedAt: {
          bsonType: "date",
          description: "Last update timestamp"
        }
      }
    }
  }
});

print("Friend requests collection created with validation schema");
