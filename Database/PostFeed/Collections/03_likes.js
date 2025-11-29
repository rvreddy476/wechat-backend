// ========================================
// PostFeed Service - Likes Collection
// ========================================

db.createCollection("likes", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["entityId", "entityType", "userId", "createdAt"],
      properties: {
        _id: {
          bsonType: "string",
          description: "Unique like identifier"
        },
        entityId: {
          bsonType: "string",
          description: "ID of liked entity (post or comment)"
        },
        entityType: {
          enum: ["Post", "Comment"],
          description: "Type of entity being liked"
        },
        userId: {
          bsonType: "string",
          description: "UUID of user who liked"
        },
        username: {
          bsonType: "string",
          description: "Username of user who liked"
        },
        createdAt: {
          bsonType: "date",
          description: "When the like was created"
        }
      }
    }
  }
});

// Create compound unique index to prevent duplicate likes
db.likes.createIndex(
  { "entityId": 1, "userId": 1 },
  { unique: true, name: "idx_entity_user_unique" }
);

print("Likes collection created with validation schema and unique index");
