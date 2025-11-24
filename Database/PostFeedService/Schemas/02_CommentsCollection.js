// =============================================
// WeChat.com - PostFeedService MongoDB Schema
// Collection: comments
// Purpose: Comments on posts (supports nested/threaded comments)
// =============================================

const DB_NAME = 'wechat_postfeed';
const COLLECTION_NAME = 'comments';

use(DB_NAME);

// Create collection with validation
db.createCollection(COLLECTION_NAME, {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["commentId", "postId", "userId", "content", "stats", "createdAt", "updatedAt"],
            properties: {
                commentId: {
                    bsonType: "string",
                    description: "Unique comment identifier (UUID) - required"
                },
                postId: {
                    bsonType: "string",
                    description: "Post ID this comment belongs to - required"
                },
                userId: {
                    bsonType: "string",
                    description: "Comment author user ID - required"
                },
                parentCommentId: {
                    bsonType: ["string", "null"],
                    description: "Parent comment ID for nested/threaded comments (null for root comments)"
                },
                rootCommentId: {
                    bsonType: ["string", "null"],
                    description: "Root comment ID (for deeply nested threads, helps with queries)"
                },
                level: {
                    bsonType: "int",
                    minimum: 0,
                    maximum: 5,
                    description: "Nesting level (0 = root, 1 = reply, 2+ = nested replies, max 5)"
                },
                content: {
                    bsonType: "object",
                    required: ["text"],
                    properties: {
                        text: {
                            bsonType: "string",
                            minLength: 1,
                            maxLength: 2000,
                            description: "Comment text content - required"
                        },
                        richText: {
                            bsonType: ["object", "null"],
                            description: "Rich text formatting"
                        }
                    }
                },
                mediaUrls: {
                    bsonType: "array",
                    items: {
                        bsonType: "object",
                        properties: {
                            url: { bsonType: "string" },
                            type: { bsonType: "string", enum: ["image", "gif"] },
                            thumbnailUrl: { bsonType: ["string", "null"] }
                        }
                    },
                    description: "Media attachments (images, GIFs)"
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
                    description: "Users mentioned in comment"
                },
                stats: {
                    bsonType: "object",
                    required: ["likeCount", "replyCount"],
                    properties: {
                        likeCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Number of likes on comment"
                        },
                        replyCount: {
                            bsonType: "int",
                            minimum: 0,
                            description: "Number of replies to comment"
                        }
                    }
                },
                isEdited: {
                    bsonType: "bool",
                    description: "Whether comment has been edited"
                },
                editedAt: {
                    bsonType: ["date", "null"],
                    description: "Last edit timestamp"
                },
                createdAt: {
                    bsonType: "date",
                    description: "Creation timestamp - required"
                },
                updatedAt: {
                    bsonType: "date",
                    description: "Last update timestamp - required"
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
                    description: "Number of times comment was reported"
                },
                isFlagged: {
                    bsonType: "bool",
                    description: "Whether comment is flagged for review"
                }
            }
        }
    },
    validationLevel: "moderate",
    validationAction: "error"
});

print("Creating indexes for comments collection...");

// Unique index
db.comments.createIndex({ "commentId": 1 }, { unique: true, name: "idx_commentId_unique" });

// Query performance indexes - most critical
db.comments.createIndex({ "postId": 1, "level": 1, "createdAt": 1 }, { name: "idx_postId_level_createdAt" });
db.comments.createIndex({ "postId": 1, "parentCommentId": 1, "createdAt": 1 }, { name: "idx_postId_parent_createdAt" });

// Get comments for a post (root comments only)
db.comments.createIndex(
    { "postId": 1, "level": 1, "isDeleted": 1, "createdAt": -1 },
    { partialFilterExpression: { level: 0 }, name: "idx_root_comments" }
);

// Get replies to a comment
db.comments.createIndex({ "parentCommentId": 1, "createdAt": 1 }, { name: "idx_parentCommentId_createdAt" });

// Get all comments in a thread
db.comments.createIndex({ "rootCommentId": 1, "createdAt": 1 }, { name: "idx_rootCommentId_thread" });

// User's comments
db.comments.createIndex({ "userId": 1, "createdAt": -1 }, { name: "idx_userId_createdAt" });
db.comments.createIndex({ "userId": 1, "postId": 1 }, { name: "idx_userId_postId" });

// Mentions
db.comments.createIndex({ "mentions.userId": 1, "createdAt": -1 }, { name: "idx_mentions_createdAt" });

// Popular comments (by likes)
db.comments.createIndex({ "postId": 1, "stats.likeCount": -1 }, { name: "idx_postId_likeCount" });

// Moderation
db.comments.createIndex({ "isFlagged": 1, "createdAt": -1 }, { name: "idx_flagged_moderation" });

// Compound index for efficient thread queries
db.comments.createIndex(
    { "postId": 1, "rootCommentId": 1, "level": 1, "createdAt": 1 },
    { name: "idx_thread_structure" }
);

print("Indexes created successfully for comments collection!");

// Sample document structure
const sampleComment = {
    commentId: "comment-uuid-123",
    postId: "post-uuid-456",
    userId: "user-uuid-789",
    parentCommentId: null, // null for root comment
    rootCommentId: null, // null for root comment
    level: 0, // 0 = root comment
    content: {
        text: "Great post! Really love this @johndoe 👍",
        richText: null
    },
    mediaUrls: [],
    mentions: [
        {
            userId: "user-uuid-456",
            username: "johndoe",
            displayName: "John Doe"
        }
    ],
    stats: {
        likeCount: 45,
        replyCount: 3
    },
    isEdited: false,
    editedAt: null,
    createdAt: new Date(),
    updatedAt: new Date(),
    isDeleted: false,
    deletedAt: null,
    reportCount: 0,
    isFlagged: false
};

const sampleNestedComment = {
    commentId: "comment-uuid-124",
    postId: "post-uuid-456",
    userId: "user-uuid-999",
    parentCommentId: "comment-uuid-123", // Replying to above comment
    rootCommentId: "comment-uuid-123", // Root of this thread
    level: 1, // First level reply
    content: {
        text: "I agree! 💯",
        richText: null
    },
    mediaUrls: [],
    mentions: [],
    stats: {
        likeCount: 12,
        replyCount: 0
    },
    isEdited: false,
    editedAt: null,
    createdAt: new Date(),
    updatedAt: new Date(),
    isDeleted: false,
    deletedAt: null,
    reportCount: 0,
    isFlagged: false
};

print("\n=============================================");
print("comments collection setup completed!");
print("Supports nested/threaded comments up to 5 levels");
print("=============================================\n");
