// ========================================
// PostFeed Service - Post Indexes
// ========================================

// Index for finding posts by author
db.posts.createIndex(
  { "authorId": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_authorId_isDeleted_createdAt" }
);

// Index for feed timeline (public posts)
db.posts.createIndex(
  { "visibility": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_visibility_isDeleted_createdAt" }
);

// Index for hashtag search
db.posts.createIndex(
  { "hashtags": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_hashtags_isDeleted_createdAt" }
);

// Index for mentioned users
db.posts.createIndex(
  { "mentions": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_mentions_isDeleted_createdAt" }
);

// Text search index for post content
db.posts.createIndex(
  { "content": "text", "authorUsername": "text" },
  { name: "idx_post_text_search", default_language: "english" }
);

// Index for location-based posts
db.posts.createIndex(
  { "location.name": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_location_name_isDeleted_createdAt" }
);

// Geospatial index for location coordinates
db.posts.createIndex(
  { "location.latitude": 1, "location.longitude": 1 },
  { name: "idx_location_coordinates" }
);

// Index for popular posts (by likes)
db.posts.createIndex(
  { "likesCount": -1, "isDeleted": 1 },
  { name: "idx_likesCount_isDeleted" }
);

// Index for trending posts (by engagement)
db.posts.createIndex(
  { "likesCount": -1, "commentsCount": -1, "sharesCount": -1, "createdAt": -1 },
  { name: "idx_trending_posts" }
);

// Compound index for feed queries
db.posts.createIndex(
  { "visibility": 1, "isDeleted": 1, "createdAt": -1, "_id": -1 },
  { name: "idx_feed_pagination" }
);

print("Post indexes created successfully");
