# PostFeed Service Database Documentation

## Overview

The PostFeed service uses **MongoDB** as its database to store posts, comments, likes, shares, and hashtags. MongoDB's flexible document model is ideal for social feeds due to:

- Dynamic content types (text, images, videos)
- Nested comments and replies
- Rich engagement data (likes, shares, views)
- Hashtag trending calculations
- Flexible media attachments

## Database Structure

### Collections

#### 1. **posts**
Stores user posts with media, mentions, hashtags, and engagement metrics

**Key Fields:**
- `_id` (string) - Unique post identifier
- `authorId` (string) - UUID of post author
- `authorUsername` (string) - Username of author
- `authorAvatarUrl` (string, nullable) - Author's avatar
- `content` (string) - Post text content (max 5000 chars)
- `mediaAttachments` (array) - List of media (images, videos, audio)
- `mentions` (array) - List of mentioned user IDs
- `hashtags` (array) - List of hashtags (without #)
- `location` (object, nullable) - Location information
- `visibility` (enum) - "Public", "FriendsOnly", "Private"
- `likesCount` (int) - Number of likes
- `commentsCount` (int) - Number of comments
- `sharesCount` (int) - Number of shares
- `viewsCount` (int) - Number of views
- `isEdited` (boolean) - Edit flag
- `isDeleted` (boolean) - Soft delete flag

#### 2. **comments**
Stores comments on posts with support for nested replies

**Key Fields:**
- `_id` (string) - Unique comment identifier
- `postId` (string) - Reference to post
- `parentCommentId` (string, nullable) - Parent comment for nested replies
- `authorId` (string) - UUID of comment author
- `content` (string) - Comment text (max 1000 chars)
- `mentions` (array) - List of mentioned user IDs
- `likesCount` (int) - Number of likes
- `repliesCount` (int) - Number of replies
- `isEdited` (boolean) - Edit flag
- `isDeleted` (boolean) - Soft delete flag

#### 3. **likes**
Stores likes on posts and comments with unique constraint per user

**Key Fields:**
- `_id` (string) - Unique like identifier
- `entityId` (string) - ID of liked entity (post or comment)
- `entityType` (enum) - "Post" or "Comment"
- `userId` (string) - UUID of user who liked
- `username` (string) - Username of user
- `createdAt` (date) - Like timestamp

**Constraints:**
- Unique index on `(entityId, userId)` prevents duplicate likes

#### 4. **shares**
Stores post shares with optional captions

**Key Fields:**
- `_id` (string) - Unique share identifier
- `postId` (string) - ID of shared post
- `userId` (string) - UUID of user who shared
- `caption` (string, nullable) - Optional caption (max 500 chars)
- `visibility` (enum) - "Public", "FriendsOnly", "Private"
- `createdAt` (date) - Share timestamp

#### 5. **hashtags**
Stores hashtag metadata and trending scores

**Key Fields:**
- `_id` (string) - Lowercase hashtag (unique identifier)
- `tag` (string) - Lowercase hashtag
- `displayTag` (string) - Original case for display
- `usageCount` (int) - Total times used
- `trendingScore` (double, nullable) - Calculated trending score
- `lastUsedAt` (date) - Last usage timestamp

## Connection String

### Development
```bash
mongodb://localhost:27017/wechat_postfeed
```

### Production (with authentication)
```bash
mongodb://username:password@mongodb-host:27017/wechat_postfeed?authSource=admin
```

### Docker Compose
```bash
mongodb://mongo:27017/wechat_postfeed
```

## Setup Instructions

### 1. Create Database and Collections

```bash
# Connect to MongoDB
mongosh

# Switch to database
use wechat_postfeed

# Run collection creation scripts
load('Collections/01_posts.js')
load('Collections/02_comments.js')
load('Collections/03_likes.js')
load('Collections/04_shares.js')
load('Collections/05_hashtags.js')
```

### 2. Create Indexes

```bash
# Create all indexes for optimal performance
load('Indexes/01_post_indexes.js')
load('Indexes/02_comment_indexes.js')
load('Indexes/03_like_indexes.js')
load('Indexes/04_share_indexes.js')
load('Indexes/05_hashtag_indexes.js')
```

### 3. Seed Test Data (Development Only)

```bash
# Load sample posts, comments, likes, shares, and hashtags
load('Seeds/seed_data.js')
```

## Common Operations

### Backup

```bash
# Full database backup
mongodump --db=wechat_postfeed --out=/backup/postfeed/$(date +%Y%m%d)

# Compressed backup
mongodump --db=wechat_postfeed --gzip --archive=/backup/postfeed/postfeed_$(date +%Y%m%d).gz
```

### Restore

```bash
# Full database restore
mongorestore --db=wechat_postfeed /backup/postfeed/20240115/wechat_postfeed

# Restore from compressed backup
mongorestore --gzip --archive=/backup/postfeed/postfeed_20240115.gz
```

### Maintenance

```bash
# Connect to database
mongosh wechat_postfeed

# Get database statistics
db.stats()

# Collection statistics
db.posts.stats()
db.comments.stats()
db.likes.stats()

# Analyze slow queries
db.setProfilingLevel(1, { slowms: 100 })
db.system.profile.find().sort({ ts: -1 }).limit(10)
```

### Cleanup Operations

```bash
# Delete soft-deleted posts older than 30 days
db.posts.deleteMany({
  isDeleted: true,
  deletedAt: { $lt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) }
})

# Delete soft-deleted comments older than 30 days
db.comments.deleteMany({
  isDeleted: true,
  deletedAt: { $lt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) }
})

# Clean up orphaned comments (comments without valid post)
const validPostIds = db.posts.distinct('_id', { isDeleted: false })
db.comments.deleteMany({
  postId: { $nin: validPostIds },
  isDeleted: true
})

# Clean up orphaned likes
db.likes.aggregate([
  {
    $lookup: {
      from: "posts",
      localField: "entityId",
      foreignField: "_id",
      as: "post"
    }
  },
  {
    $match: {
      "post": { $size: 0 },
      "entityType": "Post"
    }
  }
]).forEach(like => db.likes.deleteOne({ _id: like._id }))
```

## Query Examples

See `Queries/common_queries.js` for comprehensive query patterns including:

- Get public feed
- Get user's posts
- Search posts by hashtag
- Get trending posts
- Comment threads
- Like/unlike operations
- Share posts
- Trending hashtags
- Engagement analytics

## Performance Optimization

### Index Strategy

All critical queries are covered by indexes:

1. **Feed Queries** - `visibility` + `isDeleted` + `createdAt`
2. **Hashtag Search** - `hashtags` + `isDeleted` + `createdAt`
3. **User Posts** - `authorId` + `isDeleted` + `createdAt`
4. **Comments** - `postId` + `isDeleted` + `createdAt`
5. **Likes** - Unique compound index on `(entityId, userId)`
6. **Text Search** - Full-text index on `content` and `authorUsername`

### Caching Strategy (Recommended)

```javascript
// Cache frequently accessed data in Redis:
// - Public feed (TTL: 2 minutes)
// - Trending posts (TTL: 5 minutes)
// - Trending hashtags (TTL: 15 minutes)
// - User's feed (TTL: 1 minute)
// - Post details (TTL: 5 minutes)
```

### Query Performance Tips

1. **Pagination** - Always use limit and skip for large result sets
2. **Projection** - Only fetch needed fields
3. **Denormalization** - Store author details in posts (username, avatar)
4. **Counters** - Maintain engagement counts (likes, comments, shares)
5. **Trending Calculation** - Run as scheduled job, cache results

## Engagement Tracking

### Like a Post

```javascript
// 1. Insert like record
db.likes.insertOne({
  _id: "like" + Date.now(),
  entityId: "post123",
  entityType: "Post",
  userId: "user123",
  username: "john_doe",
  createdAt: new Date()
})

// 2. Increment post likes count
db.posts.updateOne(
  { _id: "post123" },
  { $inc: { likesCount: 1 } }
)
```

### Unlike a Post

```javascript
// 1. Delete like record
db.likes.deleteOne({
  entityId: "post123",
  entityType: "Post",
  userId: "user123"
})

// 2. Decrement post likes count
db.posts.updateOne(
  { _id: "post123" },
  { $inc: { likesCount: -1 } }
)
```

### Comment on Post

```javascript
// 1. Insert comment
db.comments.insertOne({
  _id: "comment" + Date.now(),
  postId: "post123",
  parentCommentId: null,
  authorId: "user123",
  authorUsername: "john_doe",
  content: "Great post!",
  likesCount: 0,
  repliesCount: 0,
  isEdited: false,
  isDeleted: false,
  createdAt: new Date()
})

// 2. Increment post comments count
db.posts.updateOne(
  { _id: "post123" },
  { $inc: { commentsCount: 1 } }
)
```

## Hashtag Management

### Update Hashtag Usage

```javascript
// When a post with hashtags is created
post.hashtags.forEach(tag => {
  db.hashtags.updateOne(
    { tag: tag.toLowerCase() },
    {
      $set: {
        displayTag: tag,
        lastUsedAt: new Date(),
        updatedAt: new Date()
      },
      $inc: { usageCount: 1 },
      $setOnInsert: {
        _id: tag.toLowerCase(),
        tag: tag.toLowerCase(),
        trendingScore: 0,
        createdAt: new Date()
      }
    },
    { upsert: true }
  )
})
```

### Calculate Trending Scores

```javascript
// Run this as a scheduled job (every 15 minutes)
db.hashtags.find().forEach(function(hashtag) {
  // Count posts with this hashtag in last 24 hours
  const recentPosts = db.posts.countDocuments({
    hashtags: hashtag.tag,
    createdAt: { $gte: new Date(Date.now() - 24 * 60 * 60 * 1000) },
    isDeleted: false
  })

  // Calculate recency factor (weight recent usage more)
  const hoursSinceLastUse = (Date.now() - hashtag.lastUsedAt.getTime()) / (1000 * 60 * 60)
  const recencyFactor = Math.max(1, 24 - hoursSinceLastUse)

  // Calculate trending score
  const trendingScore = recentPosts * recencyFactor

  // Update hashtag
  db.hashtags.updateOne(
    { _id: hashtag._id },
    {
      $set: {
        trendingScore: trendingScore,
        updatedAt: new Date()
      }
    }
  )
})
```

## Feed Algorithm

### Public Feed (Discovery)

```javascript
// Show recent public posts sorted by engagement
db.posts.aggregate([
  {
    $match: {
      visibility: "Public",
      isDeleted: false
    }
  },
  {
    $addFields: {
      engagementScore: {
        $add: [
          { $multiply: ["$likesCount", 1] },
          { $multiply: ["$commentsCount", 2] },
          { $multiply: ["$sharesCount", 3] }
        ]
      }
    }
  },
  {
    $sort: { engagementScore: -1, createdAt: -1 }
  },
  {
    $limit: 20
  }
])
```

### Personalized Feed

```javascript
// Show posts from friends + public posts
db.posts.find({
  $or: [
    { authorId: { $in: friendIds }, visibility: { $in: ["Public", "FriendsOnly"] } },
    { visibility: "Public" }
  ],
  isDeleted: false
}).sort({ createdAt: -1 }).limit(20)
```

## Monitoring

### Key Metrics

```javascript
// Total posts
db.posts.countDocuments({ isDeleted: false })

// Posts today
db.posts.countDocuments({
  isDeleted: false,
  createdAt: { $gte: new Date(new Date().setHours(0,0,0,0)) }
})

// Engagement rate
db.posts.aggregate([
  { $match: { isDeleted: false } },
  {
    $group: {
      _id: null,
      avgLikes: { $avg: "$likesCount" },
      avgComments: { $avg: "$commentsCount" },
      avgShares: { $avg: "$sharesCount" }
    }
  }
])

// Top hashtags
db.hashtags.find().sort({ usageCount: -1 }).limit(10)
```

## Best Practices

1. **Atomic Operations** - Use $inc for counters to avoid race conditions
2. **Denormalization** - Store frequently accessed data (author info) in posts
3. **Soft Deletes** - Never hard delete posts/comments
4. **Trending Calculation** - Run as background job, don't calculate on-demand
5. **Text Search** - Use MongoDB text indexes for content search
6. **Pagination** - Always limit results and use cursor-based pagination for large datasets
7. **Cache Popular Content** - Cache trending posts and hashtags

## Additional Resources

- [MongoDB Manual](https://docs.mongodb.com/manual/)
- [Text Search Guide](https://docs.mongodb.com/manual/text-search/)
- [Aggregation Pipeline](https://docs.mongodb.com/manual/aggregation/)
- [Performance Best Practices](https://docs.mongodb.com/manual/administration/analyzing-mongodb-performance/)
