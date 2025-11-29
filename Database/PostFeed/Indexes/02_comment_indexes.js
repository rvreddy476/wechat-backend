// ========================================
// PostFeed Service - Comment Indexes
// ========================================

// Index for finding comments on a post
db.comments.createIndex(
  { "postId": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_postId_isDeleted_createdAt" }
);

// Index for finding replies to a comment
db.comments.createIndex(
  { "parentCommentId": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_parentCommentId_isDeleted_createdAt" }
);

// Index for finding comments by author
db.comments.createIndex(
  { "authorId": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_authorId_isDeleted_createdAt" }
);

// Index for mentioned users in comments
db.comments.createIndex(
  { "mentions": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_mentions_isDeleted_createdAt" }
);

// Index for popular comments (by likes)
db.comments.createIndex(
  { "postId": 1, "likesCount": -1, "isDeleted": 1 },
  { name: "idx_postId_likesCount_isDeleted" }
);

// Compound index for pagination
db.comments.createIndex(
  { "postId": 1, "parentCommentId": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_comment_pagination" }
);

print("Comment indexes created successfully");
