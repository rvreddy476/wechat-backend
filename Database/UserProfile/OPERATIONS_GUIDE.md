# UserProfile Service - Database Operations Guide

## Collections Overview

- **user_profiles** - User profile information and social connections
- **friend_requests** - Friend request workflow
- **user_activities** - Activity tracking (90-day TTL)

---

## user_profiles Collection

### Create User Profile

**After user registers in Auth service**, create their profile:

```javascript
const profile = {
  _id: userId,  // Same as Auth service user ID
  userId: userId,
  username: "john_doe",
  displayName: "John Doe",
  email: "john@example.com",
  phoneNumber: "+1234567890",
  bio: null,
  avatarUrl: null,
  coverImageUrl: null,
  location: null,
  website: null,
  dateOfBirth: null,
  gender: null,
  friends: [],
  followers: [],
  following: [],
  blockedUsers: [],
  isOnline: false,
  lastSeenAt: null,
  privacySettings: {
    profileVisibility: "Public",
    showOnlineStatus: true,
    showLastSeen: true,
    allowFriendRequests: true,
    allowMessages: "Everyone"
  },
  notificationSettings: {
    emailNotifications: true,
    pushNotifications: true,
    messageNotifications: true,
    friendRequestNotifications: true,
    postNotifications: true
  },
  statistics: {
    friendsCount: 0,
    followersCount: 0,
    followingCount: 0,
    postsCount: 0
  },
  isVerified: false,
  isDeleted: false,
  deletedAt: null,
  createdAt: new Date(),
  updatedAt: new Date()
};

db.user_profiles.insertOne(profile);
```

---

### Update Profile

```javascript
db.user_profiles.updateOne(
  { userId: "550e8400-e29b-41d4-a716-446655440000" },
  {
    $set: {
      displayName: "John Doe Jr.",
      bio: "Software developer and tech enthusiast",
      location: "San Francisco, CA",
      website: "https://johndoe.com",
      avatarUrl: "https://cdn.example.com/avatars/john.jpg",
      updatedAt: new Date()
    }
  }
);
```

---

### Update Privacy Settings

```javascript
db.user_profiles.updateOne(
  { userId: "550e8400-e29b-41d4-a716-446655440000" },
  {
    $set: {
      "privacySettings.profileVisibility": "FriendsOnly",
      "privacySettings.showOnlineStatus": false,
      "privacySettings.allowMessages": "FriendsOnly",
      updatedAt: new Date()
    }
  }
);
```

---

### Set Online Status

**User Connects (SignalR)**:
```javascript
db.user_profiles.updateOne(
  { userId: "550e8400-e29b-41d4-a716-446655440000" },
  {
    $set: {
      isOnline: true,
      lastSeenAt: new Date(),
      updatedAt: new Date()
    }
  }
);
```

**User Disconnects**:
```javascript
db.user_profiles.updateOne(
  { userId: "550e8400-e29b-41d4-a716-446655440000" },
  {
    $set: {
      isOnline: false,
      lastSeenAt: new Date(),
      updatedAt: new Date()
    }
  }
);
```

---

### Search Users

**By Username or Display Name**:
```javascript
db.user_profiles.find({
  $text: { $search: "john" },
  isDeleted: false
}).sort({ score: { $meta: "textScore" } });
```

**Get Online Users**:
```javascript
db.user_profiles.find({
  isOnline: true,
  isDeleted: false
}).sort({ lastSeenAt: -1 });
```

---

## Friend Request Flow

### Send Friend Request

**Step 1: Check if Request Already Exists**:
```javascript
const existing = db.friend_requests.findOne({
  $or: [
    {
      senderId: "user1-id",
      receiverId: "user2-id",
      status: "Pending"
    },
    {
      senderId: "user2-id",
      receiverId: "user1-id",
      status: "Pending"
    }
  ]
});

if (existing) {
  throw new Error("Friend request already exists");
}
```

**Step 2: Create Friend Request**:
```javascript
const friendRequest = {
  _id: `freq-${Date.now()}-${generateId()}`,
  senderId: "550e8400-e29b-41d4-a716-446655440000",
  senderUsername: "john_doe",
  senderAvatarUrl: "https://cdn.example.com/avatars/john.jpg",
  receiverId: "550e8400-e29b-41d4-a716-446655440001",
  receiverUsername: "jane_smith",
  receiverAvatarUrl: "https://cdn.example.com/avatars/jane.jpg",
  status: "Pending",
  message: "Hi! I'd like to connect with you.",
  respondedAt: null,
  expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000),  // 7 days
  createdAt: new Date(),
  updatedAt: new Date()
};

db.friend_requests.insertOne(friendRequest);
```

**Step 3: Send Notification** (via Notification service):
```javascript
await createNotification({
  userId: friendRequest.receiverId,
  type: "FriendRequest",
  title: "New Friend Request",
  message: `${friendRequest.senderUsername} sent you a friend request`,
  fromUserId: friendRequest.senderId,
  relatedEntityId: friendRequest._id
});
```

---

### Accept Friend Request

```javascript
// Start transaction-like operation
const session = db.getMongo().startSession();
session.startTransaction();

try {
  // 1. Update friend request status
  db.friend_requests.updateOne(
    { _id: "freq-12345" },
    {
      $set: {
        status: "Accepted",
        respondedAt: new Date(),
        updatedAt: new Date()
      }
    },
    { session }
  );

  // 2. Add to sender's friends
  db.user_profiles.updateOne(
    { userId: senderId },
    {
      $addToSet: { friends: receiverId },
      $inc: { "statistics.friendsCount": 1 },
      $set: { updatedAt: new Date() }
    },
    { session }
  );

  // 3. Add to receiver's friends
  db.user_profiles.updateOne(
    { userId: receiverId },
    {
      $addToSet: { friends: senderId },
      $inc: { "statistics.friendsCount": 1 },
      $set: { updatedAt: new Date() }
    },
    { session }
  );

  // 4. Log activity
  db.user_activities.insertOne({
    _id: `act-${Date.now()}`,
    userId: receiverId,
    username: receiverUsername,
    activityType: "FriendAdded",
    targetUserId: senderId,
    targetUsername: senderUsername,
    createdAt: new Date()
  }, { session });

  session.commitTransaction();
} catch (error) {
  session.abortTransaction();
  throw error;
} finally {
  session.endSession();
}
```

---

### Reject Friend Request

```javascript
db.friend_requests.updateOne(
  { _id: "freq-12345" },
  {
    $set: {
      status: "Rejected",
      respondedAt: new Date(),
      updatedAt: new Date()
    }
  }
);
```

---

### Get Pending Friend Requests

**Received Requests**:
```javascript
db.friend_requests.find({
  receiverId: "550e8400-e29b-41d4-a716-446655440000",
  status: "Pending"
}).sort({ createdAt: -1 });
```

**Sent Requests**:
```javascript
db.friend_requests.find({
  senderId: "550e8400-e29b-41d4-a716-446655440000",
  status: "Pending"
}).sort({ createdAt: -1 });
```

---

## Follow/Unfollow Flow

### Follow User

```javascript
// Add to following list
db.user_profiles.updateOne(
  { userId: "user1-id" },
  {
    $addToSet: { following: "user2-id" },
    $inc: { "statistics.followingCount": 1 },
    $set: { updatedAt: new Date() }
  }
);

// Add to followers list
db.user_profiles.updateOne(
  { userId: "user2-id" },
  {
    $addToSet: { followers: "user1-id" },
    $inc: { "statistics.followersCount": 1 },
    $set: { updatedAt: new Date() }
  }
);

// Log activity
db.user_activities.insertOne({
  userId: "user1-id",
  activityType: "UserFollowed",
  targetUserId: "user2-id",
  createdAt: new Date()
});
```

---

### Unfollow User

```javascript
// Remove from following list
db.user_profiles.updateOne(
  { userId: "user1-id" },
  {
    $pull: { following: "user2-id" },
    $inc: { "statistics.followingCount": -1 },
    $set: { updatedAt: new Date() }
  }
);

// Remove from followers list
db.user_profiles.updateOne(
  { userId: "user2-id" },
  {
    $pull: { followers: "user1-id" },
    $inc: { "statistics.followersCount": -1 },
    $set: { updatedAt: new Date() }
  }
);
```

---

## Block User Flow

### Block User

```javascript
db.user_profiles.updateOne(
  { userId: "550e8400-e29b-41d4-a716-446655440000" },
  {
    $addToSet: { blockedUsers: "user-to-block-id" },
    $pull: {
      friends: "user-to-block-id",
      followers: "user-to-block-id",
      following: "user-to-block-id"
    },
    $set: { updatedAt: new Date() }
  }
);

// Also remove from blocked user's lists
db.user_profiles.updateOne(
  { userId: "user-to-block-id" },
  {
    $pull: {
      friends: "550e8400-e29b-41d4-a716-446655440000",
      followers: "550e8400-e29b-41d4-a716-446655440000",
      following: "550e8400-e29b-41d4-a716-446655440000"
    },
    $set: { updatedAt: new Date() }
  }
);
```

---

### Unblock User

```javascript
db.user_profiles.updateOne(
  { userId: "550e8400-e29b-41d4-a716-446655440000" },
  {
    $pull: { blockedUsers: "user-to-unblock-id" },
    $set: { updatedAt: new Date() }
  }
);
```

---

## Query Patterns

### Get User's Friends

```javascript
// Get friend IDs
const user = db.user_profiles.findOne(
  { userId: "550e8400-e29b-41d4-a716-446655440000" },
  { projection: { friends: 1 } }
);

// Get full friend profiles
db.user_profiles.find({
  userId: { $in: user.friends },
  isDeleted: false
}, {
  projection: {
    userId: 1,
    username: 1,
    displayName: 1,
    avatarUrl: 1,
    isOnline: 1,
    lastSeenAt: 1
  }
});
```

---

### Get Mutual Friends

```javascript
db.user_profiles.aggregate([
  { $match: { userId: "user1-id" } },
  {
    $lookup: {
      from: "user_profiles",
      let: { user1Friends: "$friends" },
      pipeline: [
        { $match: { userId: "user2-id" } },
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
  { $unwind: "$result" },
  { $project: { mutualFriends: "$result.mutualFriends" } }
]);
```

---

### Check if Users are Friends

```javascript
const areFriends = db.user_profiles.findOne({
  userId: "user1-id",
  friends: "user2-id"
}) !== null;
```

---

## User Activities

**Log Activity**:
```javascript
db.user_activities.insertOne({
  _id: `act-${Date.now()}`,
  userId: "550e8400-e29b-41d4-a716-446655440000",
  username: "john_doe",
  activityType: "ProfileUpdated",
  targetUserId: null,
  targetUsername: null,
  metadata: { fields: ["bio", "avatarUrl"] },
  createdAt: new Date()
});
```

**Get User Activities**:
```javascript
db.user_activities.find({
  userId: "550e8400-e29b-41d4-a716-446655440000"
}).sort({ createdAt: -1 }).limit(50);
```

**Activity Types**:
- `ProfileUpdated`
- `FriendAdded`
- `FriendRemoved`
- `UserFollowed`
- `UserUnfollowed`
- `PostCreated`
- `PostLiked`
- `PostCommented`

---

## Best Practices

1. **Sync with Auth**: Keep username and email in sync with Auth service
2. **Use Transactions**: Use sessions for multi-document updates (friends)
3. **Privacy Checks**: Always check `privacySettings` before showing data
4. **Block Checks**: Verify user is not blocked before showing content
5. **TTL Cleanup**: Activities auto-delete after 90 days
6. **Cache Online Users**: Store online users in Redis for performance
7. **Update Counters**: Keep statistics in sync with array lengths
8. **Activity Logging**: Log important actions for audit trail

---
