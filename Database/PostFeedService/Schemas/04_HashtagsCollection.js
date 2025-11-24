// =============================================
// WeChat.com - PostFeedService MongoDB Schema
// Collection: hashtags
// Purpose: Track trending hashtags and their usage
// =============================================

const DB_NAME = 'wechat_postfeed';
const COLLECTION_NAME = 'hashtags';

use(DB_NAME);

// Create collection with validation
db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["tag", "stats", "updatedAt"],
            properties: {
                tag: {
                    bsonType: "string",
                    description: "Hashtag text (lowercase, without #) - required"
                },
                stats: {
                    bsonType: "object",
                    required: ["totalCount", "todayCount", "weekCount"],
                    properties: {
                        totalCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Total usage count"
                        },
                        todayCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Usage count today"
                        },
                        weekCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Usage count this week"
                        },
                        monthCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Usage count this month"
                        }
                    }
                },
                trendingScore: {
                    bsonType: "double",
                    description: "Calculated trending score (based on velocity and recency)"
                },
                category: {
                    bsonType: ["string", "null"],
                    enum: ["technology", "sports", "entertainment", "news", "lifestyle", "business", "other", null],
                    description: "Hashtag category (can be auto-detected or manual)"
                },
                isTrending: {
                    bsonType: "bool",
                    description: "Whether hashtag is currently trending"
                },
                recentPosts: {
                    bsonType: "array",
                    items: {
                        bsonType: "object",
                        properties: {
                            postId: { bsonType: "string" },
                            createdAt: { bsonType: "date" }
                        }
                    },
                    description: "Recent posts using this hashtag (max 50, for quick preview)"
                },
                firstSeenAt: {
                    bsonType: "date",
                    description: "When hashtag was first used"
                },
                lastUsedAt: {
                    bsonType: "date",
                    description: "Most recent usage"
                },
                updatedAt: {
                    bsonType: "date",
                    description: "Stats last updated - required"
                },
                isBanned: {
                    bsonType: "bool",
                    description: "Whether hashtag is banned/blocked"
                },
                bannedReason: {
                    bsonType: ["string", "null"],
                    description: "Reason for ban (spam, inappropriate, etc.)"
                }
            }
        }
    },
    validationLevel: "moderate",
    validationAction: "error"
});

print("Creating indexes for hashtags collection...");

// Unique index
db.hashtags.createIndex({ "tag": 1 }, { unique: true, name: "idx_tag_unique" });

// Trending hashtags
db.hashtags.createIndex({ "isTrending": 1, "trendingScore": -1 }, { name: "idx_trending" });
db.hashtags.createIndex({ "trendingScore": -1 }, { name: "idx_trendingScore_desc" });

// Popular hashtags (by count)
db.hashtags.createIndex({ "stats.todayCount": -1 }, { name: "idx_todayCount_desc" });
db.hashtags.createIndex({ "stats.weekCount": -1 }, { name: "idx_weekCount_desc" });
db.hashtags.createIndex({ "stats.totalCount": -1 }, { name: "idx_totalCount_desc" });

// Category-based queries
db.hashtags.createIndex({ "category": 1, "trendingScore": -1 }, { name: "idx_category_trending" });

// Recently active hashtags
db.hashtags.createIndex({ "lastUsedAt": -1 }, { name: "idx_lastUsedAt_desc" });

// Text search for hashtag discovery
db.hashtags.createIndex({ "tag": "text" }, { name: "idx_tag_text_search" });

// Banned hashtags filter
db.hashtags.createIndex({ "isBanned": 1 }, { name: "idx_banned" });

print("Indexes created successfully for hashtags collection!");

// Sample document
const sampleHashtag = {
    tag: "technology",
    stats: {
        totalCount: 15420,
        todayCount: 342,
        weekCount: 2145,
        monthCount: 8934
    },
    trendingScore: 0.87,
    category: "technology",
    isTrending: true,
    recentPosts: [
        {
            postId: "post-uuid-123",
            createdAt: new Date("2024-01-15T10:30:00Z")
        },
        {
            postId: "post-uuid-124",
            createdAt: new Date("2024-01-15T09:15:00Z")
        }
        // ... up to 50 recent posts
    ],
    firstSeenAt: new Date("2023-01-01T00:00:00Z"),
    lastUsedAt: new Date(),
    updatedAt: new Date(),
    isBanned: false,
    bannedReason: null
};

print("\n=============================================");
print("hashtags collection setup completed!");
print("Tracks trending hashtags with stats and scores");
print("=============================================\n");
