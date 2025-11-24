// =============================================
// WeChat.com - MediaService MongoDB Schema
// Collection: mediaUploads
// Purpose: Track media uploads (minimal metadata)
// =============================================

const DB_NAME = 'wechat_media';
const COLLECTION_NAME = 'mediaUploads';

use(DB_NAME);

db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["mediaId", "userId", "status", "createdAt"],
            properties: {
                mediaId: { bsonType: "string" },
                userId: { bsonType: "string" },
                fileName: { bsonType: "string" },
                fileSize: { bsonType: ["int", "null"] },
                contentType: { bsonType: "string" },
                mediaType: { bsonType: "string", enum: ["image", "video", "audio", "file"] },
                uploadUrl: { bsonType: ["string", "null"], description: "Signed upload URL" },
                storageUrl: { bsonType: ["string", "null"], description: "GCS storage path" },
                cdnUrl: { bsonType: ["string", "null"], description: "CDN URL" },
                status: { bsonType: "string", enum: ["pending", "uploading", "uploaded", "failed"] },
                metadata: {
                    bsonType: ["object", "null"],
                    properties: {
                        width: { bsonType: ["int", "null"] },
                        height: { bsonType: ["int", "null"] },
                        duration: { bsonType: ["double", "null"] }
                    }
                },
                expiresAt: { bsonType: ["date", "null"] },
                createdAt: { bsonType: "date" },
                uploadedAt: { bsonType: ["date", "null"] }
            }
        }
    }
});

print("Creating indexes for mediaUploads collection...");

db.mediaUploads.createIndex({ "mediaId": 1 }, { unique: true, name: "idx_mediaId_unique" });
db.mediaUploads.createIndex({ "userId": 1, "createdAt": -1 }, { name: "idx_userId_createdAt" });
db.mediaUploads.createIndex({ "status": 1, "createdAt": -1 }, { name: "idx_status" });
db.mediaUploads.createIndex({ "expiresAt": 1 }, { name: "idx_expiresAt_ttl", expireAfterSeconds: 0 });

print("mediaUploads collection setup completed!");
