// =============================================
// WeChat.com - PostFeedService MongoDB Schema
// Collection: posts
// Purpose: User posts/feed content (text, images, videos, shares)
// =============================================

const DB_NAME = 'wechat_postfeed';
const COLLECTION_NAME = 'posts';

use(DB_NAME);

// Create collection with validation
db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["postId", "userId", "content", "visibility", "stats", "createdAt", "updatedAt"],
            properties: {
                postId: {
                    bsonType: "string",
                    description: "Unique post identifier (UUID) - required"
                },
                userId: {
                    bsonType: "string",
                    description: "Author user ID (UUID) - required"
                },
                content: {
                    bsonType: "object",
                    required: ["text"],
                    properties: {
                        text: {
                            bsonType: "string",
                            maxLength: 10000,
                            description: "Post text content"
                        },
                        richText: {
                            bsonType: ["object", "null"],
                            description: "Rich text formatting (JSON)"
                        }
                    }
                },
                postType: {
                    bsonType: "string",
                    enum: ["text", "image", "video", "shared_post", "poll"],
                    description: "Type of post"
                },
                mediaUrls: {
                    bsonType: "array",
                    items: {
                        bsonType: "object",
                        required: ["url", "type"],
                        properties: {
                            url: { bsonType: "string" },
                            type: { bsonType: "string", enum: ["image", "video", "gif"] },
                            thumbnailUrl: { bsonType: ["string", "null"] },
                            width: { bsonType: ["int", "null"] },
                            height: { bsonType: ["int", "null"] },
                            duration: { bsonType: ["int", "null"] }
                        }
                    },
                    description: "Array of media attachments"
                },
                sharedPostId: {
                    bsonType: ["string", "null"],
                    description: "If this is a shared post, original post ID"
                },
                sharedPost: {
                    bsonType: ["object", "null"],
                    description: "Denormalized shared post data for performance"
                },
                poll: {
                    bsonType: ["object", "null"],
                    properties: {
                        question: { bsonType: "string" },
                        options: {
                            bsonType: "array",
                            items: {
                                bsonType: "object",
                                properties: {
                                    optionId: { bsonType: "string" },
                                    text: { bsonType: "string" },
                                    voteCount: { bsonType: "int" }
                                }
                            }
                        },
                        expiresAt: { bsonType: ["date", "null"] },
                        allowMultipleChoices: { bsonType: "bool" }
                    }
                },
                mentions: {
                    bsonType: "array",
                    items: {
                        bsonType: "object",
                        required: ["userId", "username"],
                        properties: {
                            userId: { bsonType: "string" },
                            username: { bsonType: "string" },
                            displayName: { bsonType: "string" }
                        }
                    },
                    description: "Users mentioned in post"
                },
                hashtags: {
                    bsonType: "array",
                    items: { bsonType: "string" },
                    description: "Hashtags in post (lowercase, without #)"
                },
                location: {
                    bsonType: ["object", "null"],
                    properties: {
                        name: { bsonType: "string" },
                        placeId: { bsonType: ["string", "null"] },
                        coordinates: {
                            bsonType: ["object", "null"],
                            properties: {
                                latitude: { bsonType: "double" },
                                longitude: { bsonType: "double" }
                            }
                        }
                    },
                    description: "Location where post was created"
                },
                visibility: {
                    bsonType: "string",
                    enum: ["public", "followers", "friends", "private"],
                    description: "Post visibility - required"
                },
                allowComments: {
                    bsonType: "bool",
                    description: "Whether comments are allowed"
                },
                allowSharing: {
                    bsonType: "bool",
                    description: "Whether post can be shared"
                },
                stats: {
                    bsonType: "object",
                    required: ["likeCount", "commentCount", "shareCount", "viewCount"],
                    properties: {
                        likeCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Number of likes"
                        },
                        commentCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Number of comments"
                        },
                        shareCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Number of shares"
                        },
                        viewCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Number of views"
                        },
                        engagementScore: {
                            bsonType: ["double", "null"],
                            description: "Calculated engagement score for ranking"
                        }
                    }
                },
                isPinned: {
                    bsonType: "bool",
                    description: "Whether post is pinned to profile"
                },
                isEdited: {
                    bsonType: "bool",
                    description: "Whether post has been edited"
                },
                editHistory: {
                    bsonType: "array",
                    items: {
                        bsonType: "object",
                        properties: {
                            content: { bsonType: "object" },
                            editedAt: { bsonType: "date" }
                        }
                    },
                    description: "Edit history"
                },
                createdAt: {
                    bsonType: "date",
                    description: "Creation timestamp - required"
                },
                updatedAt: {
                    bsonType: "date",
                    description: "Last update timestamp - required"
                },
                publishedAt: {
                    bsonType: ["date", "null"],
                    description: "Scheduled publish time (null if immediate)"
                },
                isDeleted: {
                    bsonType: "bool",
                    description: "Soft delete flag"
                },
                deletedAt: {
                    bsonType: ["date", "null"],
                    description: "Deletion timestamp"
                },
                reportCount: {
                    bsonType: "int",
                    minimum: 0,
                    description: "Number of times post was reported"
                },
                isFlagged: {
                    bsonType: "bool",
                    description: "Whether post is flagged for review"
                }
            }
        }
    },
    validationLevel: "moderate",
    validationAction: "error"
});

print("Creating indexes for posts collection...");

// Unique index
db.posts.createIndex({ "postId": 1 }, { unique: true, name: "idx_postId_unique" });

// Query performance indexes
db.posts.createIndex({ "userId": 1, "createdAt": -1 }, { name: "idx_userId_createdAt" });
db.posts.createIndex({ "userId": 1, "visibility": 1, "isDeleted": 1, "createdAt": -1 }, { name: "idx_userId_visibility_deleted" });
db.posts.createIndex({ "visibility": 1, "isDeleted": 1, "createdAt": -1 }, { name: "idx_visibility_public_feed" });
db.posts.createIndex({ "createdAt": -1 }, { name: "idx_createdAt_desc" });

// Hashtag and mention indexes
db.posts.createIndex({ "hashtags": 1, "createdAt": -1 }, { name: "idx_hashtags_createdAt" });
db.posts.createIndex({ "mentions.userId": 1, "createdAt": -1 }, { name: "idx_mentions_createdAt" });

// Engagement indexes for trending/popular content
db.posts.createIndex({ "stats.engagementScore": -1, "createdAt": -1 }, { name: "idx_engagement_trending" });
db.posts.createIndex({ "stats.likeCount": -1 }, { name: "idx_likeCount_desc" });
db.posts.createIndex({ "stats.commentCount": -1 }, { name: "idx_commentCount_desc" });
db.posts.createIndex({ "stats.shareCount": -1 }, { name: "idx_shareCount_desc" });

// Shared posts
db.posts.createIndex({ "sharedPostId": 1 }, { name: "idx_sharedPostId" });

// Location-based queries
db.posts.createIndex({ "location.coordinates": "2dsphere" }, { name: "idx_location_geo" });

// Flagged content (moderation)
db.posts.createIndex({ "isFlagged": 1, "createdAt": -1 }, { name: "idx_flagged_moderation" });
db.posts.createIndex({ "reportCount": -1 }, { name: "idx_reportCount_desc" });

// Text search index
db.posts.createIndex(
    {
        "content.text": "text",
        "hashtags": "text"
    },
    {
        name: "idx_text_search",
        weights: {
            "content.text": 10,
            "hashtags": 5
        },
        default_language: "english"
    }
);

// Post type index
db.posts.createIndex({ "postType": 1, "createdAt": -1 }, { name: "idx_postType_createdAt" });

// Pinned posts
db.posts.createIndex({ "userId": 1, "isPinned": 1 }, { partialFilterExpression: { isPinned: true }, name: "idx_pinned_posts" });

print("Indexes created successfully for posts collection!");

// Sample document structure
const samplePost = {
    postId: "123e4567-e89b-12d3-a456-426614174000",
    userId: "user-uuid-123",
    content: {
        text: "Just launched my new project! 🚀 Check it out #tech #startup @johndoe",
        richText: null
    },
    postType: "image",
    mediaUrls: [
        {
            url: "https://cdn.wechat.com/images/post123.jpg",
            type: "image",
            thumbnailUrl: "https://cdn.wechat.com/images/post123_thumb.jpg",
            width: 1920,
            height: 1080,
            duration: null
        }
    ],
    sharedPostId: null,
    sharedPost: null,
    poll: null,
    mentions: [
        {
            userId: "user-uuid-456",
            username: "johndoe",
            displayName: "John Doe"
        }
    ],
    hashtags: ["tech", "startup"],
    location: {
        name: "San Francisco, CA",
        placeId: "ChIJIQBpAG2ahYAR_6128GcTUEo",
        coordinates: {
            latitude: 37.7749,
            longitude: -122.4194
        }
    },
    visibility: "public",
    allowComments: true,
    allowSharing: true,
    stats: {
        likeCount: 150,
        commentCount: 23,
        shareCount: 12,
        viewCount: 1543,
        engagementScore: 0.125
    },
    isPinned: false,
    isEdited: false,
    editHistory: [],
    createdAt: new Date(),
    updatedAt: new Date(),
    publishedAt: null,
    isDeleted: false,
    deletedAt: null,
    reportCount: 0,
    isFlagged: false
};

print("\n=============================================");
print("posts collection setup completed!");
print("Sample document structure available in code");
print("=============================================\n");
