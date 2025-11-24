// =============================================
// WeChat.com - VideoService MongoDB Schema
// Collection: videos
// Purpose: Video uploads (long-form and shorts)
// =============================================

const DB_NAME = 'wechat_video';
const COLLECTION_NAME = 'videos';

use(DB_NAME);

db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["videoId", "userId", "title", "type", "uploadStatus", "stats", "createdAt"],
            properties: {
                videoId: { bsonType: "string" },
                userId: { bsonType: "string" },
                title: { bsonType: "string", maxLength: 200 },
                description: { bsonType: ["string", "null"], maxLength: 5000 },
                type: { bsonType: "string", enum: ["long-form", "short"] },
                tags: { bsonType: "array", items: { bsonType: "string" } },
                duration: { bsonType: ["double", "null"] },
                aspectRatio: { bsonType: ["string", "null"], enum: ["16:9", "9:16", "1:1", "4:3", null] },
                uploadStatus: {
                    bsonType: "string",
                    enum: ["uploading", "uploaded", "processing", "ready", "failed"]
                },
                rawVideoUrl: { bsonType: ["string", "null"] },
                transcodedUrls: {
                    bsonType: ["object", "null"],
                    properties: {
                        "1080p": { bsonType: ["string", "null"] },
                        "720p": { bsonType: ["string", "null"] },
                        "480p": { bsonType: ["string", "null"] },
                        "360p": { bsonType: ["string", "null"] }
                    }
                },
                hlsManifestUrl: { bsonType: ["string", "null"] },
                thumbnailUrl: { bsonType: ["string", "null"] },
                thumbnails: { bsonType: "array", items: { bsonType: "string" } },
                visibility: {
                    bsonType: "string",
                    enum: ["public", "private", "unlisted"],
                    description: "Video visibility"
                },
                stats: {
                    bsonType: "object",
                    required: ["viewCount", "likeCount", "commentCount", "shareCount"],
                    properties: {
                        viewCount: { bsonType: "int", minimum: 0 },
                        uniqueViewCount: { bsonType: "int", minimum: 0 },
                        likeCount: { bsonType: "int", minimum: 0 },
                        dislikeCount: { bsonType: "int", minimum: 0 },
                        commentCount: { bsonType: "int", minimum: 0 },
                        shareCount: { bsonType: "int", minimum: 0 },
                        averageWatchTime: { bsonType: ["double", "null"] },
                        completionRate: { bsonType: ["double", "null"] },
                        engagementScore: { bsonType: ["double", "null"] }
                    }
                },
                trendingScore: { bsonType: ["double", "null"] },
                location: {
                    bsonType: ["object", "null"],
                    properties: {
                        name: { bsonType: "string" },
                        coordinates: {
                            bsonType: ["object", "null"],
                            properties: {
                                latitude: { bsonType: "double" },
                                longitude: { bsonType: "double" }
                            }
                        }
                    }
                },
                createdAt: { bsonType: "date" },
                updatedAt: { bsonType: "date" },
                processedAt: { bsonType: ["date", "null"] },
                publishedAt: { bsonType: ["date", "null"] },
                isDeleted: { bsonType: "bool" },
                deletedAt: { bsonType: ["date", "null"] }
            }
        }
    }
});

print("Creating indexes for videos collection...");

db.videos.createIndex({ "videoId": 1 }, { unique: true, name: "idx_videoId_unique" });
db.videos.createIndex({ "userId": 1, "createdAt": -1 }, { name: "idx_userId_createdAt" });
db.videos.createIndex({ "type": 1, "visibility": 1, "createdAt": -1 }, { name: "idx_type_visibility" });
db.videos.createIndex({ "uploadStatus": 1, "createdAt": -1 }, { name: "idx_uploadStatus" });
db.videos.createIndex({ "tags": 1, "createdAt": -1 }, { name: "idx_tags" });
db.videos.createIndex({ "stats.viewCount": -1 }, { name: "idx_viewCount_desc" });
db.videos.createIndex({ "trendingScore": -1, "createdAt": -1 }, { name: "idx_trending" });
db.videos.createIndex({ "type": 1, "trendingScore": -1 }, { partialFilterExpression: { type: "short" }, name: "idx_shorts_trending" });
db.videos.createIndex({ "title": "text", "description": "text", "tags": "text" }, { name: "idx_text_search" });

print("videos collection setup completed!");
