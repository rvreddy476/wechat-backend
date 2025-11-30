# PostFeed Service - Database Operations Guide

## Collections Overview

- **posts** - User posts with media and engagement
- **comments** - Comments on posts (with nested replies)
- **likes** - Likes on posts and comments
- **shares** - Post shares
- **hashtags** - Hashtag tracking and trending

---

## Create Post

```javascript
const post = {
  _id: `post-${Date.now()}-${generateId()}`,
  authorId: "550e8400-e29b-41d4-a716-446655440000",
  authorUsername: "john_doe",
  authorAvatarUrl: "https://cdn.example.com/avatars/john.jpg",
  content: "Just had an amazing day! #blessed #grateful",
  mediaAttachments: [
    {
      mediaType: "Image",
      mediaUrl: "https://cdn.example.com/images/photo123.jpg",
      thumbnailUrl: "https://cdn.example.com/images/photo123_thumb.jpg",
      width: 1920,
      height: 1080,
      size: 2048000
    }
  ],
  mentions: [],  // Extract from content: @username
  hashtags: ["blessed", "grateful"],  // Extract from content: #tag
  location: {
    name: "San Francisco, CA",
    latitude: 37.7749,
    longitude: -122.4194
  },
  visibility: "Public",  // Public, FriendsOnly, Private
  likesCount: 0,
  commentsCount: 0,
  sharesCount: 0,
  viewsCount: 0,
  isEdited: false,
  editedAt: null,
  isDeleted: false,
  deletedAt: null,
  createdAt: new Date(),
  updatedAt: new Date()
};

db.posts.insertOne(post);

// Update user's post count
db.user_profiles.updateOne(
  { userId: post.authorId },
  { $inc: { "statistics.postsCount": 1 } }
);

// Update/create hashtags
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
  );
});
```

---

## Like Post

```javascript
// 1. Create like record (with unique constraint)
try {
  db.likes.insertOne({
    _id: `like-${Date.now()}`,
    entityId: "post-12345",
    entityType: "Post",
    userId: "550e8400-e29b-41d4-a716-446655440000",
    username: "john_doe",
    createdAt: new Date()
  });

  // 2. Increment post likes count
  db.posts.updateOne(
    { _id: "post-12345" },
    { $inc: { likesCount: 1 } }
  );
} catch (error) {
  if (error.code === 11000) {
    // Duplicate key - user already liked
    throw new Error("Already liked");
  }
  throw error;
}
```

---

## Unlike Post

```javascript
// 1. Delete like record
const result = db.likes.deleteOne({
  entityId: "post-12345",
  entityType: "Post",
  userId: "550e8400-e29b-41d4-a716-446655440000"
});

if (result.deletedCount > 0) {
  // 2. Decrement post likes count
  db.posts.updateOne(
    { _id: "post-12345" },
    { $inc: { likesCount: -1 } }
  );
}
```

---

## Comment on Post

```javascript
const comment = {
  _id: `comment-${Date.now()}`,
  postId: "post-12345",
  parentCommentId: null,  // null for top-level comments
  authorId: "550e8400-e29b-41d4-a716-446655440000",
  authorUsername: "john_doe",
  authorAvatarUrl: "https://cdn.example.com/avatars/john.jpg",
  content: "Great post!",
  mentions: [],
  likesCount: 0,
  repliesCount: 0,
  isEdited: false,
  editedAt: null,
  isDeleted: false,
  deletedAt: null,
  createdAt: new Date(),
  updatedAt: new Date()
};

db.comments.insertOne(comment);

// Increment post comments count
db.posts.updateOne(
  { _id: "post-12345" },
  { $inc: { commentsCount: 1 } }
);
```

---

## Reply to Comment

```javascript
const reply = {
  _id: `comment-${Date.now()}`,
  postId: "post-12345",
  parentCommentId: "comment-123",  // Parent comment
  authorId: "550e8400-e29b-41d4-a716-446655440001",
  authorUsername: "jane_smith",
  authorAvatarUrl: "https://cdn.example.com/avatars/jane.jpg",
  content: "Thanks!",
  mentions: [],
  likesCount: 0,
  repliesCount: 0,
  isEdited: false,
  isDeleted: false,
  createdAt: new Date(),
  updatedAt: new Date()
};

db.comments.insertOne(reply);

// Increment parent comment replies count
db.comments.updateOne(
  { _id: "comment-123" },
  { $inc: { repliesCount: 1 } }
);

// Also increment post comments count
db.posts.updateOne(
  { _id: "post-12345" },
  { $inc: { commentsCount: 1 } }
);
```

---

## Share Post

```javascript
const share = {
  _id: `share-${Date.now()}`,
  postId: "post-12345",
  userId: "550e8400-e29b-41d4-a716-446655440000",
  username: "john_doe",
  caption: "Check this out!",
  visibility: "Public",
  createdAt: new Date()
};

db.shares.insertOne(share);

// Increment post shares count
db.posts.updateOne(
  { _id: "post-12345" },
  { $inc: { sharesCount: 1 } }
);
```

---

## Get Feed

### Public Feed (Discovery)
```javascript
db.posts.find({
  visibility: "Public",
  isDeleted: false
}).sort({ createdAt: -1 }).limit(20);
```

### Personalized Feed (Friends + Public)
```javascript
// Get user's friend IDs from UserProfile service
const friendIds = ["friend1-id", "friend2-id", "friend3-id"];

db.posts.find({
  $or: [
    { authorId: { $in: friendIds }, visibility: { $in: ["Public", "FriendsOnly"] } },
    { visibility: "Public" }
  ],
  isDeleted: false
}).sort({ createdAt: -1 }).limit(20);
```

### Trending Posts (Last 24 Hours)
```javascript
db.posts.find({
  visibility: "Public",
  isDeleted: false,
  createdAt: { $gte: new Date(Date.now() - 24 * 60 * 60 * 1000) }
}).sort({
  likesCount: -1,
  commentsCount: -1,
  sharesCount: -1
}).limit(20);
```

---

## Get Comments with Replies

```javascript
// Get top-level comments
db.comments.find({
  postId: "post-12345",
  parentCommentId: null,
  isDeleted: false
}).sort({ createdAt: -1 }).limit(20);

// For each comment, get replies
db.comments.find({
  parentCommentId: "comment-123",
  isDeleted: false
}).sort({ createdAt: 1 });  // Chronological order for replies
```

---

## Search Posts

**By Content**:
```javascript
db.posts.find({
  $text: { $search: "artificial intelligence" },
  visibility: "Public",
  isDeleted: false
}).sort({ score: { $meta: "textScore" } });
```

**By Hashtag**:
```javascript
db.posts.find({
  hashtags: "technology",
  visibility: "Public",
  isDeleted: false
}).sort({ createdAt: -1 });
```

**By Location**:
```javascript
db.posts.find({
  "location.name": { $regex: "San Francisco", $options: "i" },
  visibility: "Public",
  isDeleted: false
}).sort({ createdAt: -1 });
```

---

## Trending Hashtags

**Get Top Trending**:
```javascript
db.hashtags.find()
  .sort({ trendingScore: -1, usageCount: -1 })
  .limit(20);
```

**Calculate Trending Scores** (Run every 15 minutes as background job):
```javascript
db.hashtags.find().forEach(function(hashtag) {
  // Count recent posts (last 24 hours)
  const recentPosts = db.posts.countDocuments({
    hashtags: hashtag.tag,
    createdAt: { $gte: new Date(Date.now() - 24 * 60 * 60 * 1000) },
    isDeleted: false
  });

  // Calculate recency factor
  const hoursSinceLastUse = (Date.now() - hashtag.lastUsedAt.getTime()) / (1000 * 60 * 60);
  const recencyFactor = Math.max(1, 24 - hoursSinceLastUse);

  // Calculate trending score
  const trendingScore = recentPosts * recencyFactor;

  // Update hashtag
  db.hashtags.updateOne(
    { _id: hashtag._id },
    {
      $set: {
        trendingScore: trendingScore,
        updatedAt: new Date()
      }
    }
  );
});
```

---

## Best Practices

1. **Extract Hashtags**: Parse content for #hashtags and store in array
2. **Extract Mentions**: Parse content for @mentions and store user IDs
3. **Atomic Counters**: Use $inc for engagement metrics
4. **Unique Likes**: Rely on unique index to prevent duplicate likes
5. **Nested Comments**: Limit reply depth to 1 level for simplicity
6. **Visibility**: Always check visibility before showing posts
7. **Trending Updates**: Run trending calculation as background job
8. **Pagination**: Always paginate feed and comments
9. **Soft Delete**: Preserve deleted content for audit

---
