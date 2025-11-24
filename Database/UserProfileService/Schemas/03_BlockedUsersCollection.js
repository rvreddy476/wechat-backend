// =============================================
// WeChat.com - UserProfileService MongoDB Schema
// Collection: blockedUsers
// Purpose: Track blocked user relationships
// =============================================

const DB_NAME = 'wechat_profiles';
const COLLECTION_NAME = 'blockedUsers';

use(DB_NAME);

// Create collection with validation
db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["blockerId", "blockedUserId", "createdAt"],
            properties: {
                blockerId: {
                    bsonType: "string",
                    description: "User ID who initiated the block - required"
                },
                blockedUserId: {
                    bsonType: "string",
                    description: "User ID who is blocked - required"
                },
                reason: {
                    bsonType: ["string", "null"],
                    enum: ["spam", "harassment", "inappropriate_content", "impersonation", "other", null],
                    description: "Reason for blocking"
                },
                notes: {
                    bsonType: ["string", "null"],
                    maxLength: 500,
                    description: "Additional notes about the block"
                },
                createdAt: {
                    bsonType: "date",
                    description: "When the block was created - required"
                },
                unblockedAt: {
                    bsonType: ["date", "null"],
                    description: "When the user was unblocked"
                },
                isActive: {
                    bsonType: "bool",
                    description: "Whether block is currently active"
                }
            }
        }
    }
});

print("Creating indexes for blockedUsers collection...");

// Composite unique index
db.blockedUsers.createIndex(
    { "blockerId": 1, "blockedUserId": 1 },
    { unique: true, name: "idx_blocker_blocked_unique" }
);

// Query performance indexes
db.blockedUsers.createIndex({ "blockerId": 1, "isActive": 1 }, { name: "idx_blockerId_active" });
db.blockedUsers.createIndex({ "blockedUserId": 1, "isActive": 1 }, { name: "idx_blockedUserId_active" });
db.blockedUsers.createIndex({ "createdAt": -1 }, { name: "idx_createdAt" });

// For checking if user is blocked (most common query)
db.blockedUsers.createIndex(
    { "blockerId": 1, "blockedUserId": 1, "isActive": 1 },
    { name: "idx_block_check" }
);

print("Indexes created successfully for blockedUsers collection!");

const sampleBlock = {
    blockerId: "123e4567-e89b-12d3-a456-426614174001",
    blockedUserId: "123e4567-e89b-12d3-a456-426614174003",
    reason: "spam",
    notes: "Repeated spam messages",
    createdAt: new Date(),
    unblockedAt: null,
    isActive: true
};

print("\n=============================================");
print("blockedUsers collection setup completed!");
print("=============================================\n");
