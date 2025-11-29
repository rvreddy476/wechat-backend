// ========================================
// UserProfile Service - User Profile Indexes
// ========================================

// Unique index on userId
db.user_profiles.createIndex(
  { "userId": 1 },
  { unique: true, name: "idx_userId_unique" }
);

// Unique index on username (case-insensitive)
db.user_profiles.createIndex(
  { "username": 1 },
  { unique: true, collation: { locale: "en", strength: 2 }, name: "idx_username_unique_ci" }
);

// Index for email lookup
db.user_profiles.createIndex(
  { "email": 1 },
  { name: "idx_email" }
);

// Index for online users
db.user_profiles.createIndex(
  { "isOnline": 1, "lastSeenAt": -1 },
  { name: "idx_isOnline_lastSeenAt" }
);

// Index for finding friends
db.user_profiles.createIndex(
  { "friends": 1, "isDeleted": 1 },
  { name: "idx_friends_isDeleted" }
);

// Index for finding followers
db.user_profiles.createIndex(
  { "followers": 1, "isDeleted": 1 },
  { name: "idx_followers_isDeleted" }
);

// Index for finding following
db.user_profiles.createIndex(
  { "following": 1, "isDeleted": 1 },
  { name: "idx_following_isDeleted" }
);

// Index for blocked users lookup
db.user_profiles.createIndex(
  { "blockedUsers": 1 },
  { name: "idx_blockedUsers" }
);

// Text search index for username and display name
db.user_profiles.createIndex(
  { "username": "text", "displayName": "text", "bio": "text" },
  { name: "idx_user_text_search", default_language: "english" }
);

// Index for verified users
db.user_profiles.createIndex(
  { "isVerified": 1, "isDeleted": 1 },
  { name: "idx_isVerified_isDeleted" }
);

// Index for location-based search
db.user_profiles.createIndex(
  { "location": 1, "isDeleted": 1 },
  { name: "idx_location_isDeleted" }
);

// Compound index for active users listing
db.user_profiles.createIndex(
  { "isDeleted": 1, "isOnline": 1, "lastSeenAt": -1 },
  { name: "idx_active_users_listing" }
);

// Index for privacy settings queries
db.user_profiles.createIndex(
  { "privacySettings.profileVisibility": 1, "isDeleted": 1 },
  { name: "idx_privacy_visibility" }
);

print("User profile indexes created successfully");
