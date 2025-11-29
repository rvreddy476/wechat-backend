// ========================================
// PostFeed Service - Shares Collection
// ========================================

db.createCollection("shares", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["postId", "userId", "createdAt"],
      properties: {
        _id: {
          bsonType: "string",
          description: "Unique share identifier"
        },
        postId: {
          bsonType: "string",
          description: "ID of shared post"
        },
        userId: {
          bsonType: "string",
          description: "UUID of user who shared"
        },
        username: {
          bsonType: "string",
          description: "Username of user who shared"
        },
        caption: {
          bsonType: ["string", "null"],
          description: "Optional caption when sharing",
          maxLength: 500
        },
        visibility: {
          enum: ["Public", "FriendsOnly", "Private"],
          description: "Share visibility"
        },
        createdAt: {
          bsonType: "date",
          description: "When the share was created"
        }
      }
    }
  }
});

print("Shares collection created with validation schema");
