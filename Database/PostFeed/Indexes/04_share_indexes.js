// ========================================
// PostFeed Service - Share Indexes
// ========================================

// Index for finding shares of a post
db.shares.createIndex(
  { "postId": 1, "createdAt": -1 },
  { name: "idx_postId_createdAt" }
);

// Index for finding user's shares
db.shares.createIndex(
  { "userId": 1, "createdAt": -1 },
  { name: "idx_userId_createdAt" }
);

// Index for share visibility queries
db.shares.createIndex(
  { "visibility": 1, "createdAt": -1 },
  { name: "idx_visibility_createdAt" }
);

print("Share indexes created successfully");
