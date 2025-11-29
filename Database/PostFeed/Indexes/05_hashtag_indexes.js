// ========================================
// PostFeed Service - Hashtag Indexes
// ========================================

// Index for trending hashtags
db.hashtags.createIndex(
  { "trendingScore": -1, "usageCount": -1 },
  { name: "idx_trending_hashtags" }
);

// Index for popular hashtags
db.hashtags.createIndex(
  { "usageCount": -1 },
  { name: "idx_usageCount" }
);

// Index for recently used hashtags
db.hashtags.createIndex(
  { "lastUsedAt": -1 },
  { name: "idx_lastUsedAt" }
);

print("Hashtag indexes created successfully");
