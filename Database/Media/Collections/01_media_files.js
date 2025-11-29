// ========================================
// Media Service - Media Files Collection
// ========================================

db.createCollection("media_files", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["uploadedBy", "fileName", "fileUrl", "mediaType", "status", "createdAt"],
      properties: {
        _id: {
          bsonType: "string",
          description: "Unique media file identifier"
        },
        uploadedBy: {
          bsonType: "string",
          description: "UUID of user who uploaded"
        },
        uploaderUsername: {
          bsonType: "string"
        },
        fileName: {
          bsonType: "string"
        },
        fileUrl: {
          bsonType: "string"
        },
        thumbnailUrl: {
          bsonType: ["string", "null"]
        },
        mediaType: {
          enum: ["Image", "Video", "Audio", "Document", "Other"]
        },
        mimeType: {
          bsonType: "string"
        },
        fileSize: {
          bsonType: "long"
        },
        width: {
          bsonType: ["int", "null"]
        },
        height: {
          bsonType: ["int", "null"]
        },
        duration: {
          bsonType: ["int", "null"]
        },
        status: {
          enum: ["Uploading", "Processing", "Ready", "Failed"]
        },
        isPublic: {
          bsonType: "bool"
        },
        tags: {
          bsonType: "array"
        },
        metadata: {
          bsonType: ["object", "null"]
        },
        isDeleted: {
          bsonType: "bool"
        },
        createdAt: {
          bsonType: "date"
        },
        updatedAt: {
          bsonType: "date"
        }
      }
    }
  }
});

print("Media files collection created");
