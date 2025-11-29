// ========================================
// UserProfile Service - Common Query Patterns
// ========================================

// ========================================
// USER PROFILE QUERIES
// ========================================

// 1. Get user profile by userId
db.user_profiles.findOne({
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "isDeleted": false
});

// 2. Get user profile by username (case-insensitive)
db.user_profiles.findOne({
  "username": "admin"
}).collation({ locale: "en", strength: 2 });

// 3. Search users by username or display name
db.user_profiles.find({
  $text: { $search: "john" },
  "isDeleted": false
}).sort({ score: { $meta: "textScore" } });

// 4. Get all online users
db.user_profiles.find({
  "isOnline": true,
  "isDeleted": false
}).sort({ "lastSeenAt": -1 });

// 5. Get user's friends with details
db.user_profiles.aggregate([
  {
    $match: {
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "isDeleted": false
    }
  },
  {
    $lookup: {
      from: "user_profiles",
      localField: "friends",
      foreignField: "userId",
      as: "friendProfiles"
    }
  },
  {
    $project: {
      friends: {
        $map: {
          input: "$friendProfiles",
          as: "friend",
          in: {
            userId: "$$friend.userId",
            username: "$$friend.username",
            displayName: "$$friend.displayName",
            avatarUrl: "$$friend.avatarUrl",
            isOnline: "$$friend.isOnline",
            lastSeenAt: "$$friend.lastSeenAt"
          }
        }
      }
    }
  }
]);

// 6. Get user's followers
db.user_profiles.find({
  "following": "550e8400-e29b-41d4-a716-446655440000",
  "isDeleted": false
},
{
  userId: 1,
  username: 1,
  displayName: 1,
  avatarUrl: 1,
  isOnline: 1
});

// 7. Get users being followed
db.user_profiles.find({
  "followers": "550e8400-e29b-41d4-a716-446655440000",
  "isDeleted": false
},
{
  userId: 1,
  username: 1,
  displayName: 1,
  avatarUrl: 1,
  isOnline: 1
});

// 8. Check if two users are friends
db.user_profiles.findOne({
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "friends": "550e8400-e29b-41d4-a716-446655440001"
}) !== null;

// 9. Get verified users
db.user_profiles.find({
  "isVerified": true,
  "isDeleted": false
}).sort({ "statistics.followersCount": -1 });

// 10. Search users by location
db.user_profiles.find({
  "location": { $regex: "New York", $options: "i" },
  "privacySettings.profileVisibility": { $in: ["Public", "FriendsOnly"] },
  "isDeleted": false
});

// 11. Get popular users (most followers)
db.user_profiles.find({
  "isDeleted": false
}).sort({ "statistics.followersCount": -1 }).limit(20);

// 12. Get users who joined recently
db.user_profiles.find({
  "isDeleted": false,
  "createdAt": { $gte: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000) }
}).sort({ "createdAt": -1 });

// ========================================
// FRIEND REQUEST QUERIES
// ========================================

// 13. Get pending friend requests for user
db.friend_requests.find({
  "receiverId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Pending"
}).sort({ "createdAt": -1 });

// 14. Get sent friend requests
db.friend_requests.find({
  "senderId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Pending"
}).sort({ "createdAt": -1 });

// 15. Check if friend request exists between two users
db.friend_requests.findOne({
  $or: [
    {
      "senderId": "550e8400-e29b-41d4-a716-446655440000",
      "receiverId": "550e8400-e29b-41d4-a716-446655440001"
    },
    {
      "senderId": "550e8400-e29b-41d4-a716-446655440001",
      "receiverId": "550e8400-e29b-41d4-a716-446655440000"
    }
  ],
  "status": "Pending"
});

// 16. Get friend request history for user
db.friend_requests.find({
  $or: [
    { "senderId": "550e8400-e29b-41d4-a716-446655440000" },
    { "receiverId": "550e8400-e29b-41d4-a716-446655440000" }
  ]
}).sort({ "createdAt": -1 });

// 17. Get expired pending requests
db.friend_requests.find({
  "status": "Pending",
  "expiresAt": { $lte: new Date() }
});

// ========================================
// USER ACTIVITY QUERIES
// ========================================

// 18. Get user's recent activities
db.user_activities.find({
  "userId": "550e8400-e29b-41d4-a716-446655440000"
}).sort({ "createdAt": -1 }).limit(50);

// 19. Get activities by type
db.user_activities.find({
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "activityType": "FriendAdded"
}).sort({ "createdAt": -1 });

// 20. Get activities related to a specific user
db.user_activities.find({
  "targetUserId": "550e8400-e29b-41d4-a716-446655440000"
}).sort({ "createdAt": -1 });

// 21. Get activity timeline (aggregated)
db.user_activities.aggregate([
  {
    $match: {
      "userId": "550e8400-e29b-41d4-a716-446655440000"
    }
  },
  {
    $group: {
      _id: {
        date: { $dateToString: { format: "%Y-%m-%d", date: "$createdAt" } },
        activityType: "$activityType"
      },
      count: { $sum: 1 }
    }
  },
  {
    $sort: { "_id.date": -1 }
  }
]);

// ========================================
// UPDATE QUERIES
// ========================================

// 22. Update profile information
db.user_profiles.updateOne(
  {
    "userId": "550e8400-e29b-41d4-a716-446655440000"
  },
  {
    $set: {
      "displayName": "John Doe",
      "bio": "Software developer and tech enthusiast",
      "location": "San Francisco, CA",
      "website": "https://johndoe.com",
      "updatedAt": new Date()
    }
  }
);

// 23. Update online status
db.user_profiles.updateOne(
  {
    "userId": "550e8400-e29b-41d4-a716-446655440000"
  },
  {
    $set: {
      "isOnline": true,
      "lastSeenAt": new Date(),
      "updatedAt": new Date()
    }
  }
);

// 24. Add friend to user's friend list
db.user_profiles.updateOne(
  {
    "userId": "550e8400-e29b-41d4-a716-446655440000"
  },
  {
    $addToSet: {
      "friends": "550e8400-e29b-41d4-a716-446655440001"
    },
    $inc: {
      "statistics.friendsCount": 1
    },
    $set: {
      "updatedAt": new Date()
    }
  }
);

// 25. Remove friend from user's friend list
db.user_profiles.updateOne(
  {
    "userId": "550e8400-e29b-41d4-a716-446655440000"
  },
  {
    $pull: {
      "friends": "550e8400-e29b-41d4-a716-446655440001"
    },
    $inc: {
      "statistics.friendsCount": -1
    },
    $set: {
      "updatedAt": new Date()
    }
  }
);

// 26. Follow a user
db.user_profiles.updateOne(
  {
    "userId": "550e8400-e29b-41d4-a716-446655440000"
  },
  {
    $addToSet: {
      "following": "550e8400-e29b-41d4-a716-446655440001"
    },
    $inc: {
      "statistics.followingCount": 1
    },
    $set: {
      "updatedAt": new Date()
    }
  }
);

// Also update the followed user's followers
db.user_profiles.updateOne(
  {
    "userId": "550e8400-e29b-41d4-a716-446655440001"
  },
  {
    $addToSet: {
      "followers": "550e8400-e29b-41d4-a716-446655440000"
    },
    $inc: {
      "statistics.followersCount": 1
    },
    $set: {
      "updatedAt": new Date()
    }
  }
);

// 27. Block a user
db.user_profiles.updateOne(
  {
    "userId": "550e8400-e29b-41d4-a716-446655440000"
  },
  {
    $addToSet: {
      "blockedUsers": "550e8400-e29b-41d4-a716-446655440001"
    },
    $pull: {
      "friends": "550e8400-e29b-41d4-a716-446655440001",
      "followers": "550e8400-e29b-41d4-a716-446655440001",
      "following": "550e8400-e29b-41d4-a716-446655440001"
    },
    $set: {
      "updatedAt": new Date()
    }
  }
);

// 28. Update privacy settings
db.user_profiles.updateOne(
  {
    "userId": "550e8400-e29b-41d4-a716-446655440000"
  },
  {
    $set: {
      "privacySettings.profileVisibility": "FriendsOnly",
      "privacySettings.showOnlineStatus": false,
      "privacySettings.allowMessages": "FriendsOnly",
      "updatedAt": new Date()
    }
  }
);

// 29. Accept friend request
db.friend_requests.updateOne(
  {
    "_id": "req123"
  },
  {
    $set: {
      "status": "Accepted",
      "respondedAt": new Date(),
      "updatedAt": new Date()
    }
  }
);

// Then add each user to the other's friend list
// (See queries 24 above - run for both users)

// 30. Reject friend request
db.friend_requests.updateOne(
  {
    "_id": "req123"
  },
  {
    $set: {
      "status": "Rejected",
      "respondedAt": new Date(),
      "updatedAt": new Date()
    }
  }
);

// 31. Cancel sent friend request
db.friend_requests.updateOne(
  {
    "_id": "req123",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "Pending"
  },
  {
    $set: {
      "status": "Cancelled",
      "updatedAt": new Date()
    }
  }
);

// 32. Auto-reject expired requests (scheduled job)
db.friend_requests.updateMany(
  {
    "status": "Pending",
    "expiresAt": { $lte: new Date() }
  },
  {
    $set: {
      "status": "Rejected",
      "respondedAt": new Date(),
      "updatedAt": new Date()
    }
  }
);

// ========================================
// ANALYTICS QUERIES
// ========================================

// 33. Get user statistics
db.user_profiles.findOne(
  { "userId": "550e8400-e29b-41d4-a716-446655440000" },
  { statistics: 1, username: 1 }
);

// 34. Get mutual friends between two users
db.user_profiles.aggregate([
  {
    $match: {
      "userId": "550e8400-e29b-41d4-a716-446655440000"
    }
  },
  {
    $lookup: {
      from: "user_profiles",
      let: { user1Friends: "$friends" },
      pipeline: [
        {
          $match: {
            "userId": "550e8400-e29b-41d4-a716-446655440001"
          }
        },
        {
          $project: {
            mutualFriends: {
              $setIntersection: ["$friends", "$$user1Friends"]
            }
          }
        }
      ],
      as: "result"
    }
  },
  {
    $unwind: "$result"
  },
  {
    $project: {
      mutualFriends: "$result.mutualFriends"
    }
  }
]);

// 35. Get friend request statistics
db.friend_requests.aggregate([
  {
    $group: {
      _id: "$status",
      count: { $sum: 1 }
    }
  }
]);

print("Common query patterns documented");
