// =============================================
// WeChat.com - UserProfileService MongoDB Schema
// Collection: follows
// Purpose: User follow relationships (followers/following)
// =============================================

const DB_NAME = 'wechat_profiles';
const COLLECTION_NAME = 'follows';

use(DB_NAME);

// Create collection with validation
db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["followerId", "followingId", "createdAt"],
            properties: {
                followerId: {
                    bsonType: "string",
                    description: "User ID of the follower - required"
                },
                followingId: {
                    bsonType: "string",
                    description: "User ID being followed - required"
                },
                isAccepted: {
                    bsonType: "bool",
                    description: "Whether follow request is accepted (for private accounts)"
                },
                createdAt: {
                    bsonType: "date",
                    description: "When the follow relationship was created - required"
                },
                acceptedAt: {
                    bsonType: ["date", "null"],
                    description: "When the follow request was accepted"
                },
                unfollowedAt: {
                    bsonType: ["date", "null"],
                    description: "When the user unfollowed (for soft tracking)"
                },
                notificationsEnabled: {
                    bsonType: "bool",
                    description: "Whether follower receives notifications from this user"
                }
            }
        }
    }
});

print("Creating indexes for follows collection...");

// Composite unique index to prevent duplicate follows
db.follows.createIndex(
    { "followerId": 1, "followingId": 1 },
    { unique: true, name: "idx_follower_following_unique" }
);

// Query performance indexes
db.follows.createIndex({ "followerId": 1, "isAccepted": 1 }, { name: "idx_followerId_accepted" });
db.follows.createIndex({ "followingId": 1, "isAccepted": 1 }, { name: "idx_followingId_accepted" });
db.follows.createIndex({ "followerId": 1, "createdAt": -1 }, { name: "idx_followerId_createdAt" });
db.follows.createIndex({ "followingId": 1, "createdAt": -1 }, { name: "idx_followingId_createdAt" });

// For follow requests (pending follows)
db.follows.createIndex(
    { "followingId": 1, "isAccepted": 1 },
    { partialFilterExpression: { isAccepted: false }, name: "idx_pending_follow_requests" }
);

// For notification settings
db.follows.createIndex({ "followerId": 1, "notificationsEnabled": 1 }, { name: "idx_notifications" });

print("Indexes created successfully for follows collection!");

// Sample document structure
const sampleFollow = {
    followerId: "123e4567-e89b-12d3-a456-426614174001",
    followingId: "123e4567-e89b-12d3-a456-426614174002",
    isAccepted: true,
    createdAt: new Date(),
    acceptedAt: new Date(),
    unfollowedAt: null,
    notificationsEnabled: true
};

print("\n=============================================");
print("follows collection setup completed!");
print("=============================================\n");
