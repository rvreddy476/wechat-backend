// ========================================
// PostFeed Service - Comments Collection
// ========================================

db.createCollection("comments", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["postId", "authorId", "authorUsername", "content", "createdAt"],
      properties: {
        _id: {
          bsonType: "string",
          description: "Unique comment identifier"
        },
        postId: {
          bsonType: "string",
          description: "ID of the post this comment belongs to"
        },
        parentCommentId: {
          bsonType: ["string", "null"],
          description: "ID of parent comment (for nested replies)"
        },
        authorId: {
          bsonType: "string",
          description: "UUID of comment author"
        },
        authorUsername: {
          bsonType: "string",
          description: "Username of author"
        },
        authorAvatarUrl: {
          bsonType: ["string", "null"],
          description: "Avatar URL of author"
        },
        content: {
          bsonType: "string",
          description: "Comment text",
          maxLength: 1000
        },
        mentions: {
          bsonType: "array",
          description: "List of mentioned user IDs",
          items: {
            bsonType: "string"
          }
        },
        likesCount: {
          bsonType: "int",
          description: "Number of likes",
          minimum: 0
        },
        repliesCount: {
          bsonType: "int",
          description: "Number of replies",
          minimum: 0
        },
        isEdited: {
          bsonType: "bool",
          description: "Whether comment has been edited"
        },
        editedAt: {
          bsonType: ["date", "null"],
          description: "Last edit timestamp"
        },
        isDeleted: {
          bsonType: "bool",
          description: "Soft delete flag"
        },
        deletedAt: {
          bsonType: ["date", "null"],
          description: "Deletion timestamp"
        },
        createdAt: {
          bsonType: "date",
          description: "Comment creation timestamp"
        },
        updatedAt: {
          bsonType: "date",
          description: "Last update timestamp"
        }
      }
    }
  }
});

print("Comments collection created with validation schema");
