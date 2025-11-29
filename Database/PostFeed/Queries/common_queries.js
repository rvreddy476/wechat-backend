// ========================================
// PostFeed Service - Common Query Patterns
// ========================================

// ========================================
// POST QUERIES
// ========================================

// 1. Get public feed (global timeline)
db.posts.find({
  "visibility": "Public",
  "isDeleted": false
}).sort({ "createdAt": -1 }).limit(20);

// 2. Get user's posts
db.posts.find({
  "authorId": "550e8400-e29b-41d4-a716-446655440000",
  "isDeleted": false
}).sort({ "createdAt": -1 });

// 3. Get post by ID
db.posts.findOne({
  "_id": "post123",
  "isDeleted": false
});

// 4. Get posts with specific hashtag
db.posts.find({
  "hashtags": "technology",
  "visibility": "Public",
  "isDeleted": false
}).sort({ "createdAt": -1 });

// 5. Get posts mentioning a user
db.posts.find({
  "mentions": "550e8400-e29b-41d4-a716-446655440000",
  "isDeleted": false
}).sort({ "createdAt": -1 });

// 6. Search posts by content
db.posts.find({
  $text: { $search: "artificial intelligence" },
  "visibility": "Public",
  "isDeleted": false
}).sort({ score: { $meta: "textScore" } });

// 7. Get trending posts (high engagement in last 24 hours)
db.posts.find({
  "visibility": "Public",
  "isDeleted": false,
  "createdAt": { $gte: new Date(Date.now() - 24 * 60 * 60 * 1000) }
}).sort({
  "likesCount": -1,
  "commentsCount": -1,
  "sharesCount": -1
}).limit(20);

// 8. Get posts from specific location
db.posts.find({
  "location.name": { $regex: "San Francisco", $options: "i" },
  "visibility": "Public",
  "isDeleted": false
}).sort({ "createdAt": -1 });

// 9. Get friend's posts (friends-only visibility)
db.posts.aggregate([
  {
    $match: {
      "authorId": { $in: ["friend1", "friend2", "friend3"] },
      "visibility": { $in: ["Public", "FriendsOnly"] },
      "isDeleted": false
    }
  },
  {
    $sort: { "createdAt": -1 }
  },
  {
    $limit: 20
  }
]);

// 10. Get posts with media attachments
db.posts.find({
  "mediaAttachments": { $exists: true, $ne: [] },
  "visibility": "Public",
  "isDeleted": false
}).sort({ "createdAt": -1 });

// 11. Get posts with video attachments only
db.posts.find({
  "mediaAttachments.mediaType": "Video",
  "visibility": "Public",
  "isDeleted": false
}).sort({ "createdAt": -1 });

// 12. Get popular posts (most liked)
db.posts.find({
  "visibility": "Public",
  "isDeleted": false
}).sort({ "likesCount": -1 }).limit(20);

// ========================================
// COMMENT QUERIES
// ========================================

// 13. Get comments on a post
db.comments.find({
  "postId": "post123",
  "parentCommentId": null, // Top-level comments only
  "isDeleted": false
}).sort({ "createdAt": -1 });

// 14. Get replies to a comment
db.comments.find({
  "parentCommentId": "comment456",
  "isDeleted": false
}).sort({ "createdAt": 1 });

// 15. Get user's comments
db.comments.find({
  "authorId": "550e8400-e29b-41d4-a716-446655440000",
  "isDeleted": false
}).sort({ "createdAt": -1 });

// 16. Get comments mentioning a user
db.comments.find({
  "mentions": "550e8400-e29b-41d4-a716-446655440000",
  "isDeleted": false
}).sort({ "createdAt": -1 });

// 17. Get popular comments on a post (most liked)
db.comments.find({
  "postId": "post123",
  "isDeleted": false
}).sort({ "likesCount": -1 }).limit(10);

// 18. Get comment thread (comment + all replies)
db.comments.aggregate([
  {
    $match: {
      $or: [
        { "_id": "comment456" },
        { "parentCommentId": "comment456" }
      ],
      "isDeleted": false
    }
  },
  {
    $sort: { "createdAt": 1 }
  }
]);

// ========================================
// LIKE QUERIES
// ========================================

// 19. Get users who liked a post
db.likes.find({
  "entityId": "post123",
  "entityType": "Post"
}).sort({ "createdAt": -1 });

// 20. Check if user liked a post
db.likes.findOne({
  "entityId": "post123",
  "entityType": "Post",
  "userId": "550e8400-e29b-41d4-a716-446655440000"
}) !== null;

// 21. Get all posts liked by user
db.likes.find({
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "entityType": "Post"
}).sort({ "createdAt": -1 });

// 22. Get like count for post
db.likes.countDocuments({
  "entityId": "post123",
  "entityType": "Post"
});

// 23. Get users who liked a comment
db.likes.find({
  "entityId": "comment456",
  "entityType": "Comment"
}).sort({ "createdAt": -1 });

// ========================================
// SHARE QUERIES
// ========================================

// 24. Get users who shared a post
db.shares.find({
  "postId": "post123"
}).sort({ "createdAt": -1 });

// 25. Get user's shares
db.shares.find({
  "userId": "550e8400-e29b-41d4-a716-446655440000"
}).sort({ "createdAt": -1 });

// 26. Get share count for post
db.shares.countDocuments({
  "postId": "post123"
});

// ========================================
// HASHTAG QUERIES
// ========================================

// 27. Get trending hashtags
db.hashtags.find().sort({ "trendingScore": -1, "usageCount": -1 }).limit(20);

// 28. Get popular hashtags
db.hashtags.find().sort({ "usageCount": -1 }).limit(50);

// 29. Search hashtags
db.hashtags.find({
  "tag": { $regex: "^tech", $options: "i" }
}).sort({ "usageCount": -1 });

// 30. Get hashtag details
db.hashtags.findOne({
  "tag": "technology"
}).collation({ locale: "en", strength: 2 });

// ========================================
// UPDATE QUERIES
// ========================================

// 31. Create new post
db.posts.insertOne({
  _id: "post" + Date.now(),
  authorId: "550e8400-e29b-41d4-a716-446655440000",
  authorUsername: "admin",
  authorAvatarUrl: "https://example.com/avatars/admin.jpg",
  content: "Just had an amazing day! #blessed #grateful",
  mediaAttachments: [],
  mentions: [],
  hashtags: ["blessed", "grateful"],
  location: null,
  visibility: "Public",
  likesCount: 0,
  commentsCount: 0,
  sharesCount: 0,
  viewsCount: 0,
  isEdited: false,
  editedAt: null,
  isDeleted: false,
  deletedAt: null,
  createdAt: new Date(),
  updatedAt: new Date()
});

// 32. Edit post
db.posts.updateOne(
  { "_id": "post123" },
  {
    $set: {
      "content": "Updated content here",
      "isEdited": true,
      "editedAt": new Date(),
      "updatedAt": new Date()
    }
  }
);

// 33. Delete post (soft delete)
db.posts.updateOne(
  { "_id": "post123" },
  {
    $set: {
      "isDeleted": true,
      "deletedAt": new Date(),
      "updatedAt": new Date()
    }
  }
);

// 34. Increment post likes count
db.posts.updateOne(
  { "_id": "post123" },
  {
    $inc: { "likesCount": 1 },
    $set: { "updatedAt": new Date() }
  }
);

// 35. Increment post comments count
db.posts.updateOne(
  { "_id": "post123" },
  {
    $inc: { "commentsCount": 1 },
    $set: { "updatedAt": new Date() }
  }
);

// 36. Increment post shares count
db.posts.updateOne(
  { "_id": "post123" },
  {
    $inc: { "sharesCount": 1 },
    $set: { "updatedAt": new Date() }
  }
);

// 37. Add comment to post
db.comments.insertOne({
  _id: "comment" + Date.now(),
  postId: "post123",
  parentCommentId: null,
  authorId: "550e8400-e29b-41d4-a716-446655440000",
  authorUsername: "admin",
  authorAvatarUrl: "https://example.com/avatars/admin.jpg",
  content: "Great post!",
  mentions: [],
  likesCount: 0,
  repliesCount: 0,
  isEdited: false,
  editedAt: null,
  isDeleted: false,
  deletedAt: null,
  createdAt: new Date(),
  updatedAt: new Date()
});

// 38. Add reply to comment
db.comments.insertOne({
  _id: "reply" + Date.now(),
  postId: "post123",
  parentCommentId: "comment456",
  authorId: "550e8400-e29b-41d4-a716-446655440001",
  authorUsername: "testuser1",
  authorAvatarUrl: "https://example.com/avatars/john.jpg",
  content: "Thanks!",
  mentions: [],
  likesCount: 0,
  repliesCount: 0,
  isEdited: false,
  editedAt: null,
  isDeleted: false,
  deletedAt: null,
  createdAt: new Date(),
  updatedAt: new Date()
});

// Also increment parent comment's replies count
db.comments.updateOne(
  { "_id": "comment456" },
  {
    $inc: { "repliesCount": 1 },
    $set: { "updatedAt": new Date() }
  }
);

// 39. Like a post
db.likes.insertOne({
  _id: "like" + Date.now(),
  entityId: "post123",
  entityType: "Post",
  userId: "550e8400-e29b-41d4-a716-446655440000",
  username: "admin",
  createdAt: new Date()
});

// Also increment post's like count (see #34)

// 40. Unlike a post
db.likes.deleteOne({
  "entityId": "post123",
  "entityType": "Post",
  "userId": "550e8400-e29b-41d4-a716-446655440000"
});

// Also decrement post's like count
db.posts.updateOne(
  { "_id": "post123" },
  {
    $inc: { "likesCount": -1 },
    $set: { "updatedAt": new Date() }
  }
);

// 41. Share a post
db.shares.insertOne({
  _id: "share" + Date.now(),
  postId: "post123",
  userId: "550e8400-e29b-41d4-a716-446655440000",
  username: "admin",
  caption: "Check this out!",
  visibility: "Public",
  createdAt: new Date()
});

// Also increment post's share count (see #36)

// 42. Update or create hashtag
db.hashtags.updateOne(
  { "tag": "technology" },
  {
    $set: {
      "displayTag": "Technology",
      "lastUsedAt": new Date(),
      "updatedAt": new Date()
    },
    $inc: { "usageCount": 1 },
    $setOnInsert: {
      "_id": "technology",
      "tag": "technology",
      "trendingScore": 0,
      "createdAt": new Date()
    }
  },
  { upsert: true }
);

// 43. Calculate trending score for hashtags (run periodically)
db.hashtags.find().forEach(function(hashtag) {
  // Simple trending algorithm: usage in last 24 hours weighted by recency
  const recentPosts = db.posts.countDocuments({
    "hashtags": hashtag.tag,
    "createdAt": { $gte: new Date(Date.now() - 24 * 60 * 60 * 1000) },
    "isDeleted": false
  });

  const hoursSinceLastUse = (Date.now() - hashtag.lastUsedAt.getTime()) / (1000 * 60 * 60);
  const recencyFactor = Math.max(1, 24 - hoursSinceLastUse);
  const trendingScore = recentPosts * recencyFactor;

  db.hashtags.updateOne(
    { "_id": hashtag._id },
    {
      $set: {
        "trendingScore": trendingScore,
        "updatedAt": new Date()
      }
    }
  );
});

// ========================================
// ANALYTICS QUERIES
// ========================================

// 44. Get user's post statistics
db.posts.aggregate([
  {
    $match: {
      "authorId": "550e8400-e29b-41d4-a716-446655440000",
      "isDeleted": false
    }
  },
  {
    $group: {
      _id: "$authorId",
      totalPosts: { $sum: 1 },
      totalLikes: { $sum: "$likesCount" },
      totalComments: { $sum: "$commentsCount" },
      totalShares: { $sum: "$sharesCount" },
      totalViews: { $sum: "$viewsCount" }
    }
  }
]);

// 45. Get engagement rate per post
db.posts.aggregate([
  {
    $match: {
      "authorId": "550e8400-e29b-41d4-a716-446655440000",
      "isDeleted": false
    }
  },
  {
    $project: {
      _id: 1,
      content: 1,
      engagementRate: {
        $multiply: [
          {
            $divide: [
              { $add: ["$likesCount", "$commentsCount", "$sharesCount"] },
              { $max: ["$viewsCount", 1] }
            ]
          },
          100
        ]
      }
    }
  },
  {
    $sort: { "engagementRate": -1 }
  }
]);

// 46. Get most active commenters on user's posts
db.comments.aggregate([
  {
    $lookup: {
      from: "posts",
      localField: "postId",
      foreignField: "_id",
      as: "post"
    }
  },
  {
    $unwind: "$post"
  },
  {
    $match: {
      "post.authorId": "550e8400-e29b-41d4-a716-446655440000",
      "isDeleted": false
    }
  },
  {
    $group: {
      _id: "$authorId",
      username: { $first: "$authorUsername" },
      commentCount: { $sum: 1 }
    }
  },
  {
    $sort: { "commentCount": -1 }
  },
  {
    $limit: 10
  }
]);

print("Common query patterns documented");
