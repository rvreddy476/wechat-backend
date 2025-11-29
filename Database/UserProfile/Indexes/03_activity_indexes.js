// ========================================
// UserProfile Service - User Activity Indexes
// ========================================

// Index for user's activities
db.user_activities.createIndex(
  { "userId": 1, "createdAt": -1 },
  { name: "idx_userId_createdAt" }
);

// Index for activity type filtering
db.user_activities.createIndex(
  { "activityType": 1, "createdAt": -1 },
  { name: "idx_activityType_createdAt" }
);

// Index for target user activities
db.user_activities.createIndex(
  { "targetUserId": 1, "activityType": 1, "createdAt": -1 },
  { name: "idx_targetUserId_activityType_createdAt" }
);

// Compound index for user activity feed
db.user_activities.createIndex(
  { "userId": 1, "activityType": 1, "createdAt": -1 },
  { name: "idx_user_activity_feed" }
);

print("User activity indexes created successfully");
