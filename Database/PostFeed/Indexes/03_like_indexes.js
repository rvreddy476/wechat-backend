// ========================================
// PostFeed Service - Like Indexes
// ========================================

// Index for finding likes on an entity (post or comment)
db.likes.createIndex(
  { "entityId": 1, "entityType": 1, "createdAt": -1 },
  { name: "idx_entityId_entityType_createdAt" }
);

// Index for finding user's likes
db.likes.createIndex(
  { "userId": 1, "entityType": 1, "createdAt": -1 },
  { name: "idx_userId_entityType_createdAt" }
);

// Index for recent likes
db.likes.createIndex(
  { "createdAt": -1 },
  { name: "idx_createdAt" }
);

print("Like indexes created successfully");
