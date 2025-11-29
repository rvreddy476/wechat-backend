// ========================================
// UserProfile Service - User Activities Collection
// ========================================

db.createCollection("user_activities", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["userId", "activityType", "createdAt"],
      properties: {
        _id: {
          bsonType: "string",
          description: "Unique activity identifier"
        },
        userId: {
          bsonType: "string",
          description: "UUID of user performing the activity"
        },
        username: {
          bsonType: "string",
          description: "Username of user"
        },
        activityType: {
          enum: [
            "ProfileUpdated",
            "FriendAdded",
            "FriendRemoved",
            "UserFollowed",
            "UserUnfollowed",
            "PostCreated",
            "PostLiked",
            "PostCommented",
            "ProfileViewed",
            "StatusUpdated"
          ],
          description: "Type of activity"
        },
        targetUserId: {
          bsonType: ["string", "null"],
          description: "Target user ID (for social activities)"
        },
        targetUsername: {
          bsonType: ["string", "null"],
          description: "Target username"
        },
        metadata: {
          bsonType: ["object", "null"],
          description: "Additional activity data"
        },
        ipAddress: {
          bsonType: ["string", "null"],
          description: "IP address of activity"
        },
        userAgent: {
          bsonType: ["string", "null"],
          description: "User agent string"
        },
        createdAt: {
          bsonType: "date",
          description: "Activity timestamp"
        }
      }
    }
  }
});

// TTL index to auto-delete activities older than 90 days
db.user_activities.createIndex(
  { "createdAt": 1 },
  { expireAfterSeconds: 7776000, name: "ttl_user_activities" }
);

print("User activities collection created with validation schema and TTL index");
