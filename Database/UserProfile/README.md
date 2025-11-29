# UserProfile Service Database Documentation

## Overview

The UserProfile service uses **MongoDB** as its database to store user profiles, friend requests, and social activity data. MongoDB's flexible document model supports:

- Rich user profile data with nested objects
- Dynamic social connections (friends, followers, following)
- Flexible privacy and notification settings
- Activity tracking and analytics
- Schema evolution for new features

## Database Structure

### Collections

#### 1. **user_profiles**
Stores comprehensive user profile information and social connections

**Key Fields:**
- `_id` / `userId` (string) - UUID from Auth service
- `username` (string) - Unique username
- `displayName` (string, nullable) - Display name
- `email` (string) - Email address
- `bio` (string, nullable) - User biography (max 500 chars)
- `avatarUrl` (string, nullable) - Profile picture URL
- `coverImageUrl` (string, nullable) - Cover image URL
- `location` (string, nullable) - User location
- `friends` (array) - List of friend user IDs
- `followers` (array) - List of follower user IDs
- `following` (array) - List of following user IDs
- `blockedUsers` (array) - List of blocked user IDs
- `isOnline` (boolean) - Current online status
- `lastSeenAt` (date, nullable) - Last activity timestamp
- `privacySettings` (object) - Privacy preferences
- `notificationSettings` (object) - Notification preferences
- `statistics` (object) - Counters (friends, followers, posts, etc.)
- `isVerified` (boolean) - Verified user badge
- `isDeleted` (boolean) - Soft delete flag

#### 2. **friend_requests**
Stores friend request information

**Key Fields:**
- `_id` (string) - Unique request identifier
- `senderId` (string) - UUID of sender
- `senderUsername` (string) - Sender's username
- `senderAvatarUrl` (string, nullable) - Sender's avatar
- `receiverId` (string) - UUID of receiver
- `receiverUsername` (string) - Receiver's username
- `receiverAvatarUrl` (string, nullable) - Receiver's avatar
- `status` (enum) - "Pending", "Accepted", "Rejected", "Cancelled"
- `message` (string, nullable) - Optional message (max 200 chars)
- `respondedAt` (date, nullable) - Response timestamp
- `expiresAt` (date, nullable) - Expiration timestamp
- `createdAt` (date) - Creation timestamp

#### 3. **user_activities**
Stores user activity logs with automatic cleanup (90-day TTL)

**Key Fields:**
- `_id` (string) - Unique activity identifier
- `userId` (string) - User performing the activity
- `activityType` (enum) - Type of activity (ProfileUpdated, FriendAdded, etc.)
- `targetUserId` (string, nullable) - Target user of the activity
- `metadata` (object, nullable) - Additional activity data
- `ipAddress` (string, nullable) - IP address
- `userAgent` (string, nullable) - User agent string
- `createdAt` (date) - Activity timestamp

**Activity Types:**
- ProfileUpdated
- FriendAdded
- FriendRemoved
- UserFollowed
- UserUnfollowed
- PostCreated
- PostLiked
- PostCommented
- ProfileViewed
- StatusUpdated

## Connection String

### Development
```bash
mongodb://localhost:27017/wechat_userprofile
```

### Production (with authentication)
```bash
mongodb://username:password@mongodb-host:27017/wechat_userprofile?authSource=admin
```

### Docker Compose
```bash
mongodb://mongo:27017/wechat_userprofile
```

## Setup Instructions

### 1. Create Database and Collections

```bash
# Connect to MongoDB
mongosh

# Switch to database
use wechat_userprofile

# Run collection creation scripts
load('Collections/01_user_profiles.js')
load('Collections/02_friend_requests.js')
load('Collections/03_user_activities.js')
```

### 2. Create Indexes

```bash
# Create all indexes for optimal performance
load('Indexes/01_profile_indexes.js')
load('Indexes/02_friend_request_indexes.js')
load('Indexes/03_activity_indexes.js')
```

### 3. Seed Test Data (Development Only)

```bash
# Load sample user profiles, friend requests, and activities
load('Seeds/seed_data.js')
```

## Common Operations

### Backup

```bash
# Full database backup
mongodump --db=wechat_userprofile --out=/backup/userprofile/$(date +%Y%m%d)

# Backup user profiles only
mongodump --db=wechat_userprofile --collection=user_profiles --out=/backup/userprofile/profiles

# Compressed backup
mongodump --db=wechat_userprofile --gzip --archive=/backup/userprofile/userprofile_$(date +%Y%m%d).gz
```

### Restore

```bash
# Full database restore
mongorestore --db=wechat_userprofile /backup/userprofile/20240115/wechat_userprofile

# Restore specific collection
mongorestore --db=wechat_userprofile --collection=user_profiles /backup/userprofile/profiles/user_profiles.bson

# Restore from compressed backup
mongorestore --gzip --archive=/backup/userprofile/userprofile_20240115.gz
```

### Maintenance

```bash
# Connect to database
mongosh wechat_userprofile

# Get database statistics
db.stats()

# Get collection statistics
db.user_profiles.stats()
db.friend_requests.stats()
db.user_activities.stats()

# Check TTL index for activities (should auto-delete after 90 days)
db.user_activities.getIndexes()

# Manually trigger activity cleanup (if needed)
db.user_activities.deleteMany({
  createdAt: { $lt: new Date(Date.now() - 90 * 24 * 60 * 60 * 1000) }
})
```

### Cleanup Operations

```bash
# Delete soft-deleted profiles older than 30 days
db.user_profiles.deleteMany({
  isDeleted: true,
  deletedAt: { $lt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) }
})

# Auto-reject expired pending friend requests
db.friend_requests.updateMany(
  {
    status: "Pending",
    expiresAt: { $lte: new Date() }
  },
  {
    $set: {
      status: "Rejected",
      respondedAt: new Date(),
      updatedAt: new Date()
    }
  }
)

# Delete old rejected/cancelled friend requests (older than 30 days)
db.friend_requests.deleteMany({
  status: { $in: ["Rejected", "Cancelled"] },
  updatedAt: { $lt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) }
})
```

### Synchronize Friend Counts

```bash
# Update friend counts for all users
db.user_profiles.find().forEach(function(profile) {
  db.user_profiles.updateOne(
    { _id: profile._id },
    {
      $set: {
        "statistics.friendsCount": profile.friends.length,
        "statistics.followersCount": profile.followers.length,
        "statistics.followingCount": profile.following.length
      }
    }
  );
});
```

## Query Examples

See `Queries/common_queries.js` for comprehensive query patterns including:

- Get user profile by userId/username
- Search users by text
- Get online users
- Get friends/followers/following with details
- Check friendship status
- Manage friend requests
- Get user activities
- Update profile and privacy settings
- Follow/unfollow users
- Block/unblock users
- Calculate mutual friends

## Performance Optimization

### Index Strategy

All critical queries are covered by indexes:

1. **Profile Lookup** - Unique indexes on `userId` and `username`
2. **Social Queries** - Indexes on `friends`, `followers`, `following` arrays
3. **Online Status** - `isOnline` + `lastSeenAt` compound index
4. **Text Search** - Full-text index on `username`, `displayName`, `bio`
5. **Friend Requests** - Indexes on `senderId`, `receiverId`, and `status`
6. **Activities** - Index on `userId` + `createdAt` with TTL

### Query Performance Tips

1. **Use indexes** - All queries should hit indexes
2. **Project only needed fields** - Don't fetch entire documents
3. **Leverage aggregation** - Use pipeline for complex queries
4. **Cache online users** - Use Redis for real-time online status
5. **Paginate results** - Use limit and skip for large result sets

### Caching Strategy (Recommended)

```javascript
// Cache frequently accessed data in Redis:
// - Online user list (TTL: 5 minutes)
// - User profile summaries (TTL: 15 minutes)
// - Friend counts (TTL: 30 minutes)
// - Mutual friend calculations (TTL: 1 hour)
```

## Real-time Features

### Online Status Tracking

When user connects (SignalR/WebSocket):
```javascript
db.user_profiles.updateOne(
  { userId: "<userId>" },
  {
    $set: {
      isOnline: true,
      lastSeenAt: new Date()
    }
  }
)
```

When user disconnects:
```javascript
db.user_profiles.updateOne(
  { userId: "<userId>" },
  {
    $set: {
      isOnline: false,
      lastSeenAt: new Date()
    }
  }
)
```

### Activity Logging

Log important user activities:
```javascript
db.user_activities.insertOne({
  userId: "<userId>",
  username: "<username>",
  activityType: "FriendAdded",
  targetUserId: "<friendUserId>",
  targetUsername: "<friendUsername>",
  metadata: null,
  createdAt: new Date()
})
```

## Privacy and Security

### Privacy Settings

Users can control:
- **Profile Visibility**: Public, FriendsOnly, Private
- **Online Status**: Show/hide online status
- **Last Seen**: Show/hide last seen timestamp
- **Friend Requests**: Accept or disable
- **Messages**: Who can send messages (Everyone, FriendsOnly, Nobody)

### Blocked Users

When a user blocks another:
1. Remove from friends list (both sides)
2. Remove from followers/following
3. Add to blockedUsers array
4. Prevent future interactions

### Data Access Control

Implement in application layer:
```javascript
// Check if requesting user can view profile
function canViewProfile(requestingUserId, targetProfile) {
  if (targetProfile.blockedUsers.includes(requestingUserId)) {
    return false;
  }

  if (targetProfile.privacySettings.profileVisibility === "Public") {
    return true;
  }

  if (targetProfile.privacySettings.profileVisibility === "FriendsOnly") {
    return targetProfile.friends.includes(requestingUserId);
  }

  return false; // Private
}
```

## Monitoring

### Key Metrics

```javascript
// Total users
db.user_profiles.countDocuments({ isDeleted: false })

// Online users
db.user_profiles.countDocuments({ isOnline: true })

// Pending friend requests
db.friend_requests.countDocuments({ status: "Pending" })

// Activities today
db.user_activities.countDocuments({
  createdAt: { $gte: new Date(new Date().setHours(0,0,0,0)) }
})

// Average friends per user
db.user_profiles.aggregate([
  { $match: { isDeleted: false } },
  { $group: { _id: null, avgFriends: { $avg: "$statistics.friendsCount" } } }
])
```

### Slow Query Profiling

```javascript
// Enable profiling for slow queries (>100ms)
db.setProfilingLevel(1, { slowms: 100 })

// Check slow queries
db.system.profile.find().sort({ ts: -1 }).limit(10)
```

## Migration Guide

### Adding New Fields

```javascript
// Add new field to all user profiles
db.user_profiles.updateMany(
  { newField: { $exists: false } },
  { $set: { newField: defaultValue } }
)
```

### Updating Privacy Settings Structure

```javascript
// Add new privacy setting
db.user_profiles.updateMany(
  { "privacySettings.newSetting": { $exists: false } },
  { $set: { "privacySettings.newSetting": true } }
)
```

## Troubleshooting

### Common Issues

**Issue: Duplicate username**
- Check if username index exists
- Ensure case-insensitive collation is set
- Validate uniqueness in application layer before insert

**Issue: Inconsistent friend counts**
- Run friend count synchronization script
- Implement atomic operations for friend add/remove
- Use transactions for critical operations

**Issue: Slow friend lookup**
- Verify indexes on friends array
- Consider denormalization for frequent queries
- Cache friend lists in Redis

**Issue: Activities growing too large**
- Verify TTL index is working: `db.user_activities.getIndexes()`
- Check MongoDB version supports TTL
- Manually clean old activities if needed

## Best Practices

1. **Atomic Operations** - Use `$addToSet` and `$pull` for array modifications
2. **Transactions** - Use transactions for multi-document operations (friend add requires updating both users)
3. **Denormalization** - Store frequently accessed data (username, avatar) in related documents
4. **Validation** - Use JSON Schema validation for data integrity
5. **Soft Deletes** - Never hard delete user profiles
6. **Activity Retention** - Keep activity logs for limited time (90 days with TTL)
7. **Caching** - Cache online users and frequent profile lookups

## Additional Resources

- [MongoDB Manual](https://docs.mongodb.com/manual/)
- [MongoDB Schema Design Best Practices](https://docs.mongodb.com/manual/core/data-modeling-introduction/)
- [Indexing Strategies](https://docs.mongodb.com/manual/applications/indexes/)
- [MongoDB Transactions](https://docs.mongodb.com/manual/core/transactions/)
