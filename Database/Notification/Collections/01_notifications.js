// ========================================
// Notification Service - Notifications Collection
// ========================================

db.createCollection("notifications", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["userId", "type", "title", "message", "isRead", "createdAt"],
      properties: {
        _id: { bsonType: "string" },
        userId: { bsonType: "string" },
        type: {
          enum: ["FriendRequest", "Message", "Like", "Comment", "Mention", "Follow", "System"]
        },
        title: { bsonType: "string" },
        message: { bsonType: "string" },
        actionUrl: { bsonType: ["string", "null"] },
        relatedEntityId: { bsonType: ["string", "null"] },
        relatedEntityType: { bsonType: ["string", "null"] },
        fromUserId: { bsonType: ["string", "null"] },
        fromUsername: { bsonType: ["string", "null"] },
        fromUserAvatar: { bsonType: ["string", "null"] },
        isRead: { bsonType: "bool" },
        readAt: { bsonType: ["date", "null"] },
        priority: { enum: ["Low", "Normal", "High"] },
        expiresAt: { bsonType: ["date", "null"] },
        createdAt: { bsonType: "date" }
      }
    }
  }
});

// TTL index to auto-delete old notifications
db.notifications.createIndex(
  { "expiresAt": 1 },
  { expireAfterSeconds: 0, name: "ttl_notifications" }
);

print("Notifications collection created");
