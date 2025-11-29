// ========================================
// PostFeed Service - Posts Collection
// ========================================

db.createCollection("posts", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["authorId", "authorUsername", "content", "visibility", "createdAt"],
      properties: {
        _id: {
          bsonType: "string",
          description: "Unique post identifier"
        },
        authorId: {
          bsonType: "string",
          description: "UUID of post author (from Auth service)"
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
          description: "Post content/caption",
          maxLength: 5000
        },
        mediaAttachments: {
          bsonType: "array",
          description: "List of media attachments",
          items: {
            bsonType: "object",
            required: ["mediaType", "mediaUrl"],
            properties: {
              mediaType: {
                enum: ["Image", "Video", "Audio"],
                description: "Type of media"
              },
              mediaUrl: {
                bsonType: "string",
                description: "URL to media file"
              },
              thumbnailUrl: {
                bsonType: ["string", "null"],
                description: "Thumbnail URL"
              },
              width: {
                bsonType: ["int", "null"],
                description: "Media width in pixels"
              },
              height: {
                bsonType: ["int", "null"],
                description: "Media height in pixels"
              },
              duration: {
                bsonType: ["int", "null"],
                description: "Duration in seconds (for audio/video)"
              },
              size: {
                bsonType: ["long", "null"],
                description: "File size in bytes"
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
        hashtags: {
          bsonType: "array",
          description: "List of hashtags (without #)",
          items: {
            bsonType: "string"
          }
        },
        location: {
          bsonType: ["object", "null"],
          description: "Location information",
          properties: {
            name: {
              bsonType: "string",
              description: "Location name"
            },
            latitude: {
              bsonType: ["double", "null"],
              description: "Latitude coordinate"
            },
            longitude: {
              bsonType: ["double", "null"],
              description: "Longitude coordinate"
            }
          }
        },
        visibility: {
          enum: ["Public", "FriendsOnly", "Private"],
          description: "Post visibility setting"
        },
        likesCount: {
          bsonType: "int",
          description: "Number of likes",
          minimum: 0
        },
        commentsCount: {
          bsonType: "int",
          description: "Number of comments",
          minimum: 0
        },
        sharesCount: {
          bsonType: "int",
          description: "Number of shares",
          minimum: 0
        },
        viewsCount: {
          bsonType: "int",
          description: "Number of views",
          minimum: 0
        },
        isEdited: {
          bsonType: "bool",
          description: "Whether post has been edited"
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
          description: "Post creation timestamp"
        },
        updatedAt: {
          bsonType: "date",
          description: "Last update timestamp"
        }
      }
    }
  }
});

print("Posts collection created with validation schema");
