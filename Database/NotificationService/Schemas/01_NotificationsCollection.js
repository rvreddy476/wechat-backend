// =============================================
// WeChat.com - NotificationService MongoDB Schema
// Collection: notifications
// Purpose: User notifications for all activities
// =============================================

const DB_NAME = 'wechat_notifications';
const COLLECTION_NAME = 'notifications';

use(DB_NAME);

db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["notificationId", "userId", "type", "createdAt"],
            properties: {
                notificationId: { bsonType: "string" },
                userId: { bsonType: "string", description: "Recipient user ID" },
                actorId: { bsonType: ["string", "null"], description: "Who triggered the notification" },
                type: {
                    bsonType: "string",
                    enum: [
                        "follow", "unfollow",
                        "post_like", "post_comment", "post_share", "post_mention",
                        "comment_like", "comment_reply",
                        "video_like", "video_comment", "video_share",
                        "video_processed", "video_published",
                        "message", "message_mention",
                        "group_invite", "group_mention",
                        "system"
                    ]
                },
                entityType: {
                    bsonType: ["string", "null"],
                    enum: ["post", "comment", "video", "message", "user", "group", null]
                },
                entityId: { bsonType: ["string", "null"] },
                content: {
                    bsonType: "object",
                    properties: {
                        title: { bsonType: "string" },
                        body: { bsonType: "string" },
                        imageUrl: { bsonType: ["string", "null"] }
                    }
                },
                metadata: { bsonType: ["object", "null"] },
                isRead: { bsonType: "bool" },
                readAt: { bsonType: ["date", "null"] },
                createdAt: { bsonType: "date" },
                expiresAt: { bsonType: ["date", "null"] }
            }
        }
    }
});

print("Creating indexes for notifications collection...");

db.notifications.createIndex({ "notificationId": 1 }, { unique: true, name: "idx_notificationId_unique" });
db.notifications.createIndex({ "userId": 1, "isRead": 1, "createdAt": -1 }, { name: "idx_userId_isRead_createdAt" });
db.notifications.createIndex({ "userId": 1, "type": 1, "createdAt": -1 }, { name: "idx_userId_type" });
db.notifications.createIndex({ "actorId": 1, "createdAt": -1 }, { name: "idx_actorId" });
db.notifications.createIndex({ "entityType": 1, "entityId": 1 }, { name: "idx_entity" });
db.notifications.createIndex({ "expiresAt": 1 }, { name: "idx_expiresAt_ttl", expireAfterSeconds: 0 });

print("notifications collection setup completed!");
