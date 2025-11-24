// =============================================
// WeChat.com - PostFeedService MongoDB Queries
// Purpose: Common query patterns and aggregations for feed
// =============================================

const DB_NAME = 'wechat_postfeed';
use(DB_NAME);

print("PostFeedService - Common Query Patterns");
print("==========================================\n");

// =============================================
// 1. Create Post
// =============================================
print("1. Create Post:");
const createPost = (postData) => {
    return db.posts.insertOne({
        ...postData,
        stats: {
            likeCount: 0,
            commentCount: 0,
            shareCount: 0,
            viewCount: 0,
            engagementScore: 0
        },
        isPinned: false,
        isEdited: false,
        editHistory: [],
        isDeleted: false,
        deletedAt: null,
        reportCount: 0,
        isFlagged: false,
        createdAt: new Date(),
        updatedAt: new Date()
    });
};
print("Usage: createPost(postData)");
print("");

// =============================================
// 2. Get User Feed (Personalized)
// Aggregation: Get posts from followed users
// =============================================
print("2. Get Personalized User Feed:");
const getUserFeed = (userId, followingUserIds, limit = 20, skip = 0) => {
    return db.posts.aggregate([
        {
            $match: {
                userId: { $in: followingUserIds },
                visibility: { $in: ["public", "followers"] },
                isDeleted: false
            }
        },
        {
            $sort: { createdAt: -1 }
        },
        {
            $skip: skip
        },
        {
            $limit: limit
        },
        // Lookup user profile
        {
            $lookup: {
                from: "wechat_profiles.profiles", // Cross-database lookup
                localField: "userId",
                foreignField: "userId",
                as: "author"
            }
        },
        {
            $unwind: {
                path: "$author",
                preserveNullAndEmptyArrays: true
            }
        },
        // Check if current user liked the post
        {
            $lookup: {
                from: "reactions",
                let: { postId: "$postId" },
                pipeline: [
                    {
                        $match: {
                            $expr: {
                                $and: [
                                    { $eq: ["$entityId", "$$postId"] },
                                    { $eq: ["$userId", userId] },
                                    { $eq: ["$entityType", "post"] }
                                ]
                            }
                        }
                    }
                ],
                as: "userReaction"
            }
        },
        {
            $project: {
                _id: 0,
                postId: 1,
                userId: 1,
                content: 1,
                postType: 1,
                mediaUrls: 1,
                hashtags: 1,
                mentions: 1,
                location: 1,
                stats: 1,
                createdAt: 1,
                "author.username": 1,
                "author.displayName": 1,
                "author.avatarUrl": 1,
                "author.verified": 1,
                userLiked: { $cond: [{ $gt: [{ $size: "$userReaction" }, 0] }, true, false] },
                userReactionType: { $arrayElemAt: ["$userReaction.reactionType", 0] }
            }
        }
    ]).toArray();
};
print("Usage: getUserFeed('user-uuid', ['follower-1', 'follower-2'], 20, 0)");
print("");

// =============================================
// 3. Get Trending Posts
// =============================================
print("3. Get Trending Posts:");
const getTrendingPosts = (hours = 24, limit = 50) => {
    const cutoffDate = new Date(Date.now() - hours * 60 * 60 * 1000);

    return db.posts.aggregate([
        {
            $match: {
                createdAt: { $gte: cutoffDate },
                visibility: "public",
                isDeleted: false
            }
        },
        {
            $addFields: {
                trendingScore: {
                    $add: [
                        { $multiply: ["$stats.likeCount", 1] },
                        { $multiply: ["$stats.commentCount", 2] },
                        { $multiply: ["$stats.shareCount", 3] },
                        { $multiply: ["$stats.viewCount", 0.01] }
                    ]
                }
            }
        },
        {
            $sort: { trendingScore: -1 }
        },
        {
            $limit: limit
        },
        {
            $lookup: {
                from: "wechat_profiles.profiles",
                localField: "userId",
                foreignField: "userId",
                as: "author"
            }
        },
        {
            $unwind: "$author"
        },
        {
            $project: {
                _id: 0,
                postId: 1,
                userId: 1,
                content: 1,
                postType: 1,
                mediaUrls: 1,
                hashtags: 1,
                stats: 1,
                trendingScore: 1,
                createdAt: 1,
                "author.username": 1,
                "author.displayName": 1,
                "author.avatarUrl": 1,
                "author.verified": 1
            }
        }
    ]).toArray();
};
print("Usage: getTrendingPosts(24, 50)");
print("");

// =============================================
// 4. Get Post by ID with Comments
// =============================================
print("4. Get Post with Comments:");
const getPostWithComments = (postId, userId, commentLimit = 10) => {
    return db.posts.aggregate([
        {
            $match: { postId: postId, isDeleted: false }
        },
        // Lookup author
        {
            $lookup: {
                from: "wechat_profiles.profiles",
                localField: "userId",
                foreignField: "userId",
                as: "author"
            }
        },
        {
            $unwind: "$author"
        },
        // Lookup user's reaction
        {
            $lookup: {
                from: "reactions",
                let: { postId: "$postId" },
                pipeline: [
                    {
                        $match: {
                            $expr: {
                                $and: [
                                    { $eq: ["$entityId", "$$postId"] },
                                    { $eq: ["$userId", userId] },
                                    { $eq: ["$entityType", "post"] }
                                ]
                            }
                        }
                    }
                ],
                as: "userReaction"
            }
        },
        // Lookup top comments (root level only)
        {
            $lookup: {
                from: "comments",
                let: { postId: "$postId" },
                pipeline: [
                    {
                        $match: {
                            $expr: { $eq: ["$postId", "$$postId"] },
                            level: 0,
                            isDeleted: false
                        }
                    },
                    {
                        $sort: { "stats.likeCount": -1, createdAt: -1 }
                    },
                    {
                        $limit: commentLimit
                    },
                    // Lookup comment author
                    {
                        $lookup: {
                            from: "wechat_profiles.profiles",
                            localField: "userId",
                            foreignField: "userId",
                            as: "author"
                        }
                    },
                    {
                        $unwind: "$author"
                    },
                    {
                        $project: {
                            _id: 0,
                            commentId: 1,
                            userId: 1,
                            content: 1,
                            stats: 1,
                            createdAt: 1,
                            "author.username": 1,
                            "author.displayName": 1,
                            "author.avatarUrl": 1,
                            "author.verified": 1
                        }
                    }
                ],
                as: "topComments"
            }
        },
        {
            $project: {
                _id: 0,
                postId: 1,
                userId: 1,
                content: 1,
                postType: 1,
                mediaUrls: 1,
                hashtags: 1,
                mentions: 1,
                location: 1,
                visibility: 1,
                allowComments: 1,
                allowSharing: 1,
                stats: 1,
                createdAt: 1,
                updatedAt: 1,
                "author.username": 1,
                "author.displayName": 1,
                "author.avatarUrl": 1,
                "author.verified": 1,
                userLiked: { $cond: [{ $gt: [{ $size: "$userReaction" }, 0] }, true, false] },
                userReactionType: { $arrayElemAt: ["$userReaction.reactionType", 0] },
                topComments: 1
            }
        }
    ]).toArray();
};
print("Usage: getPostWithComments('post-uuid', 'user-uuid', 10)");
print("");

// =============================================
// 5. Get Comments for Post (with threading)
// =============================================
print("5. Get Comments for Post:");
const getPostComments = (postId, userId, limit = 20, skip = 0) => {
    return db.comments.aggregate([
        {
            $match: {
                postId: postId,
                level: 0, // Root comments only
                isDeleted: false
            }
        },
        {
            $sort: { "stats.likeCount": -1, createdAt: -1 }
        },
        {
            $skip: skip
        },
        {
            $limit: limit
        },
        // Lookup comment author
        {
            $lookup: {
                from: "wechat_profiles.profiles",
                localField: "userId",
                foreignField: "userId",
                as: "author"
            }
        },
        {
            $unwind: "$author"
        },
        // Lookup replies (first level only)
        {
            $lookup: {
                from: "comments",
                let: { commentId: "$commentId" },
                pipeline: [
                    {
                        $match: {
                            $expr: { $eq: ["$parentCommentId", "$$commentId"] },
                            isDeleted: false
                        }
                    },
                    {
                        $sort: { createdAt: 1 }
                    },
                    {
                        $limit: 3 // Show first 3 replies
                    },
                    {
                        $lookup: {
                            from: "wechat_profiles.profiles",
                            localField: "userId",
                            foreignField: "userId",
                            as: "author"
                        }
                    },
                    {
                        $unwind: "$author"
                    },
                    {
                        $project: {
                            _id: 0,
                            commentId: 1,
                            userId: 1,
                            content: 1,
                            stats: 1,
                            createdAt: 1,
                            "author.username": 1,
                            "author.displayName": 1,
                            "author.avatarUrl": 1
                        }
                    }
                ],
                as: "replies"
            }
        },
        // Check if user liked comment
        {
            $lookup: {
                from: "reactions",
                let: { commentId: "$commentId" },
                pipeline: [
                    {
                        $match: {
                            $expr: {
                                $and: [
                                    { $eq: ["$entityId", "$$commentId"] },
                                    { $eq: ["$userId", userId] },
                                    { $eq: ["$entityType", "comment"] }
                                ]
                            }
                        }
                    }
                ],
                as: "userReaction"
            }
        },
        {
            $project: {
                _id: 0,
                commentId: 1,
                userId: 1,
                content: 1,
                mediaUrls: 1,
                stats: 1,
                createdAt: 1,
                isEdited: 1,
                "author.username": 1,
                "author.displayName": 1,
                "author.avatarUrl": 1,
                "author.verified": 1,
                replies: 1,
                hasMoreReplies: { $gt: ["$stats.replyCount", 3] },
                userLiked: { $cond: [{ $gt: [{ $size: "$userReaction" }, 0] }, true, false] }
            }
        }
    ]).toArray();
};
print("Usage: getPostComments('post-uuid', 'user-uuid', 20, 0)");
print("");

// =============================================
// 6. Get Posts by Hashtag
// =============================================
print("6. Get Posts by Hashtag:");
const getPostsByHashtag = (hashtag, limit = 50, skip = 0) => {
    return db.posts.find(
        {
            hashtags: hashtag.toLowerCase(),
            visibility: "public",
            isDeleted: false
        },
        {
            projection: {
                _id: 0,
                postId: 1,
                userId: 1,
                content: 1,
                postType: 1,
                mediaUrls: 1,
                hashtags: 1,
                stats: 1,
                createdAt: 1
            }
        }
    )
    .sort({ createdAt: -1 })
    .skip(skip)
    .limit(limit)
    .toArray();
};
print("Usage: getPostsByHashtag('technology', 50, 0)");
print("");

// =============================================
// 7. Update Post Stats (Increment)
// =============================================
print("7. Update Post Stats:");
const incrementPostStat = (postId, statName, increment = 1) => {
    return db.posts.updateOne(
        { postId: postId },
        {
            $inc: { [`stats.${statName}`]: increment },
            $set: { updatedAt: new Date() }
        }
    );
};
print("Usage: incrementPostStat('post-uuid', 'likeCount', 1)");
print("");

// =============================================
// 8. Add Reaction
// =============================================
print("8. Add/Update Reaction:");
const addReaction = (entityType, entityId, userId, reactionType) => {
    return db.reactions.updateOne(
        { entityType, entityId, userId },
        {
            $set: {
                reactionType,
                createdAt: new Date()
            },
            $setOnInsert: {
                reactionId: new UUID().toString()
            }
        },
        { upsert: true }
    );
};
print("Usage: addReaction('post', 'post-uuid', 'user-uuid', 'love')");
print("");

// =============================================
// 9. Get Trending Hashtags
// =============================================
print("9. Get Trending Hashtags:");
const getTrendingHashtags = (limit = 20) => {
    return db.hashtags.find(
        { isBanned: false },
        {
            projection: {
                _id: 0,
                tag: 1,
                stats: 1,
                trendingScore: 1,
                category: 1
            }
        }
    )
    .sort({ trendingScore: -1, "stats.todayCount": -1 })
    .limit(limit)
    .toArray();
};
print("Usage: getTrendingHashtags(20)");
print("");

// =============================================
// 10. Search Posts
// =============================================
print("10. Search Posts:");
const searchPosts = (searchTerm, limit = 50) => {
    return db.posts.find(
        {
            $text: { $search: searchTerm },
            visibility: "public",
            isDeleted: false
        },
        {
            score: { $meta: "textScore" }
        }
    )
    .sort({ score: { $meta: "textScore" } })
    .limit(limit)
    .project({
        _id: 0,
        postId: 1,
        userId: 1,
        content: 1,
        postType: 1,
        mediaUrls: 1,
        hashtags: 1,
        stats: 1,
        createdAt: 1,
        score: { $meta: "textScore" }
    })
    .toArray();
};
print("Usage: searchPosts('artificial intelligence')");
print("");

// =============================================
// 11. Get User's Posts
// =============================================
print("11. Get User's Posts:");
const getUserPosts = (userId, requestingUserId, limit = 20, skip = 0) => {
    // Determine visibility based on relationship
    const visibilityFilter = userId === requestingUserId
        ? {} // Own posts - see all
        : { visibility: { $in: ["public", "followers"] } }; // Others' posts

    return db.posts.find(
        {
            userId: userId,
            ...visibilityFilter,
            isDeleted: false
        },
        {
            projection: {
                _id: 0,
                postId: 1,
                userId: 1,
                content: 1,
                postType: 1,
                mediaUrls: 1,
                hashtags: 1,
                stats: 1,
                isPinned: 1,
                createdAt: 1
            }
        }
    )
    .sort({ isPinned: -1, createdAt: -1 })
    .skip(skip)
    .limit(limit)
    .toArray();
};
print("Usage: getUserPosts('user-uuid', 'requesting-user-uuid', 20, 0)");
print("");

print("\n=============================================");
print("Query patterns defined successfully!");
print("=============================================\n");
