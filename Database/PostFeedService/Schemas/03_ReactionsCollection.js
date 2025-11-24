// =============================================
// WeChat.com - PostFeedService MongoDB Schema
// Collection: reactions
// Purpose: User reactions (likes, love, etc.) on posts and comments
// =============================================

const DB_NAME = 'wechat_postfeed';
const COLLECTION_NAME = 'reactions';

use(DB_NAME);

// Create collection with validation
db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["reactionId", "entityType", "entityId", "userId", "reactionType", "createdAt"],
            properties: {
                reactionId: {
                    bsonType: "string",
                    description: "Unique reaction identifier (UUID) - required"
                },
                entityType: {
                    bsonType: "string",
                    enum: ["post", "comment"],
                    description: "Type of entity being reacted to - required"
                },
                entityId: {
                    bsonType: "string",
                    description: "ID of the post or comment - required"
                },
                userId: {
                    bsonType: "string",
                    description: "User who reacted - required"
                },
                reactionType: {
                    bsonType: "string",
                    enum: ["like", "love", "haha", "wow", "sad", "angry"],
                    description: "Type of reaction (Facebook-style) - required"
                },
                createdAt: {
                    bsonType: "date",
                    description: "Reaction timestamp - required"
                }
            }
        }
    },
    validationLevel: "moderate",
    validationAction: "error"
});

print("Creating indexes for reactions collection...");

// Unique index - one user can only have one reaction per entity
db.reactions.createIndex(
    { "entityType": 1, "entityId": 1, "userId": 1 },
    { unique: true, name: "idx_entity_user_unique" }
);

// Query performance indexes
// Get all reactions for a post/comment
db.reactions.createIndex({ "entityType": 1, "entityId": 1, "createdAt": -1 }, { name: "idx_entity_reactions" });

// Get reactions by type for a post/comment
db.reactions.createIndex(
    { "entityType": 1, "entityId": 1, "reactionType": 1 },
    { name: "idx_entity_reaction_type" }
);

// Get user's reactions
db.reactions.createIndex({ "userId": 1, "createdAt": -1 }, { name: "idx_userId_createdAt" });

// Check if user liked specific post/comment
db.reactions.createIndex({ "userId": 1, "entityId": 1 }, { name: "idx_userId_entityId" });

// Get reactions by type (for analytics)
db.reactions.createIndex({ "reactionType": 1, "createdAt": -1 }, { name: "idx_reactionType_analytics" });

print("Indexes created successfully for reactions collection!");

// Sample documents
const samplePostReaction = {
    reactionId: "reaction-uuid-123",
    entityType: "post",
    entityId: "post-uuid-456",
    userId: "user-uuid-789",
    reactionType: "love",
    createdAt: new Date()
};

const sampleCommentReaction = {
    reactionId: "reaction-uuid-124",
    entityType: "comment",
    entityId: "comment-uuid-456",
    userId: "user-uuid-789",
    reactionType: "like",
    createdAt: new Date()
};

print("\n=============================================");
print("reactions collection setup completed!");
print("Supports 6 reaction types: like, love, haha, wow, sad, angry");
print("User can only have ONE reaction per post/comment (can change type)");
print("=============================================\n");
