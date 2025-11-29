// ========================================
// PostFeed Service - Seed Data for Development/Testing
// WARNING: Do not run in production!
// ========================================

// Clear existing data (DEVELOPMENT ONLY!)
db.hashtags.deleteMany({});
db.shares.deleteMany({});
db.likes.deleteMany({});
db.comments.deleteMany({});
db.posts.deleteMany({});

print("Existing data cleared");

// ========================================
// Sample Posts
// ========================================

const posts = [
  {
    _id: "post-550e8400-e29b-41d4-a716-446655440001",
    authorId: "550e8400-e29b-41d4-a716-446655440000",
    authorUsername: "admin",
    authorAvatarUrl: "https://example.com/avatars/admin.jpg",
    content: "Excited to announce our new platform features! Check out the amazing updates we've been working on. #technology #innovation #excited",
    mediaAttachments: [
      {
        mediaType: "Image",
        mediaUrl: "https://example.com/media/feature-showcase.jpg",
        thumbnailUrl: "https://example.com/media/feature-showcase_thumb.jpg",
        width: 1920,
        height: 1080,
        duration: null,
        size: 2048000
      }
    ],
    mentions: [],
    hashtags: ["technology", "innovation", "excited"],
    location: {
      name: "San Francisco, CA",
      latitude: 37.7749,
      longitude: -122.4194
    },
    visibility: "Public",
    likesCount: 15,
    commentsCount: 3,
    sharesCount: 2,
    viewsCount: 150,
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-15T10:00:00Z"),
    updatedAt: new Date("2024-01-15T10:00:00Z")
  },
  {
    _id: "post-550e8400-e29b-41d4-a716-446655440002",
    authorId: "550e8400-e29b-41d4-a716-446655440001",
    authorUsername: "testuser1",
    authorAvatarUrl: "https://example.com/avatars/john.jpg",
    content: "Just completed my first open source contribution! Feeling proud 🎉 Thanks to @admin for the guidance. #opensource #coding #learning",
    mediaAttachments: [],
    mentions: ["550e8400-e29b-41d4-a716-446655440000"],
    hashtags: ["opensource", "coding", "learning"],
    location: {
      name: "New York, NY",
      latitude: 40.7128,
      longitude: -74.0060
    },
    visibility: "Public",
    likesCount: 23,
    commentsCount: 5,
    sharesCount: 1,
    viewsCount: 200,
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-14T14:30:00Z"),
    updatedAt: new Date("2024-01-14T14:30:00Z")
  },
  {
    _id: "post-550e8400-e29b-41d4-a716-446655440003",
    authorId: "550e8400-e29b-41d4-a716-446655440002",
    authorUsername: "testuser2",
    authorAvatarUrl: "https://example.com/avatars/jane.jpg",
    content: "New artwork I've been working on! Digital painting is such a rewarding medium. What do you think? 🎨",
    mediaAttachments: [
      {
        mediaType: "Image",
        mediaUrl: "https://example.com/media/artwork-001.jpg",
        thumbnailUrl: "https://example.com/media/artwork-001_thumb.jpg",
        width: 2400,
        height: 3000,
        duration: null,
        size: 4096000
      },
      {
        mediaType: "Image",
        mediaUrl: "https://example.com/media/artwork-002.jpg",
        thumbnailUrl: "https://example.com/media/artwork-002_thumb.jpg",
        width: 2400,
        height: 3000,
        duration: null,
        size: 3584000
      }
    ],
    mentions: [],
    hashtags: ["art", "digitalart", "painting", "creative"],
    location: {
      name: "Los Angeles, CA",
      latitude: 34.0522,
      longitude: -118.2437
    },
    visibility: "Public",
    likesCount: 42,
    commentsCount: 8,
    sharesCount: 5,
    viewsCount: 350,
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-13T16:00:00Z"),
    updatedAt: new Date("2024-01-13T16:00:00Z")
  },
  {
    _id: "post-550e8400-e29b-41d4-a716-446655440004",
    authorId: "550e8400-e29b-41d4-a716-446655440000",
    authorUsername: "admin",
    authorAvatarUrl: "https://example.com/avatars/admin.jpg",
    content: "Coffee and code ☕💻 Perfect way to start the week! #MondayMotivation #developer",
    mediaAttachments: [],
    mentions: [],
    hashtags: ["MondayMotivation", "developer"],
    location: null,
    visibility: "FriendsOnly",
    likesCount: 8,
    commentsCount: 2,
    sharesCount: 0,
    viewsCount: 45,
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-12T09:00:00Z"),
    updatedAt: new Date("2024-01-12T09:00:00Z")
  },
  {
    _id: "post-550e8400-e29b-41d4-a716-446655440005",
    authorId: "550e8400-e29b-41d4-a716-446655440001",
    authorUsername: "testuser1",
    authorAvatarUrl: "https://example.com/avatars/john.jpg",
    content: "Check out this amazing tutorial on Clean Architecture! Really helpful for anyone building scalable applications. #architecture #software #tutorial",
    mediaAttachments: [
      {
        mediaType: "Video",
        mediaUrl: "https://example.com/media/tutorial-video.mp4",
        thumbnailUrl: "https://example.com/media/tutorial-video_thumb.jpg",
        width: 1920,
        height: 1080,
        duration: 450,
        size: 52428800
      }
    ],
    mentions: [],
    hashtags: ["architecture", "software", "tutorial"],
    location: null,
    visibility: "Public",
    likesCount: 31,
    commentsCount: 6,
    sharesCount: 8,
    viewsCount: 280,
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-11T11:30:00Z"),
    updatedAt: new Date("2024-01-11T11:30:00Z")
  }
];

db.posts.insertMany(posts);
print(`${posts.length} posts inserted`);

// ========================================
// Sample Comments
// ========================================

const comments = [
  {
    _id: "comment-001",
    postId: "post-550e8400-e29b-41d4-a716-446655440001",
    parentCommentId: null,
    authorId: "550e8400-e29b-41d4-a716-446655440001",
    authorUsername: "testuser1",
    authorAvatarUrl: "https://example.com/avatars/john.jpg",
    content: "This looks amazing! Can't wait to try it out!",
    mentions: [],
    likesCount: 5,
    repliesCount: 1,
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-15T10:15:00Z"),
    updatedAt: new Date("2024-01-15T10:15:00Z")
  },
  {
    _id: "comment-002",
    postId: "post-550e8400-e29b-41d4-a716-446655440001",
    parentCommentId: "comment-001",
    authorId: "550e8400-e29b-41d4-a716-446655440000",
    authorUsername: "admin",
    authorAvatarUrl: "https://example.com/avatars/admin.jpg",
    content: "Thanks! Let me know what you think after trying it!",
    mentions: [],
    likesCount: 2,
    repliesCount: 0,
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-15T10:20:00Z"),
    updatedAt: new Date("2024-01-15T10:20:00Z")
  },
  {
    _id: "comment-003",
    postId: "post-550e8400-e29b-41d4-a716-446655440001",
    parentCommentId: null,
    authorId: "550e8400-e29b-41d4-a716-446655440002",
    authorUsername: "testuser2",
    authorAvatarUrl: "https://example.com/avatars/jane.jpg",
    content: "Great work team! 🎉",
    mentions: [],
    likesCount: 3,
    repliesCount: 0,
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-15T11:00:00Z"),
    updatedAt: new Date("2024-01-15T11:00:00Z")
  },
  {
    _id: "comment-004",
    postId: "post-550e8400-e29b-41d4-a716-446655440002",
    parentCommentId: null,
    authorId: "550e8400-e29b-41d4-a716-446655440000",
    authorUsername: "admin",
    authorAvatarUrl: "https://example.com/avatars/admin.jpg",
    content: "Proud of you! Your PR was excellent!",
    mentions: [],
    likesCount: 8,
    repliesCount: 1,
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-14T15:00:00Z"),
    updatedAt: new Date("2024-01-14T15:00:00Z")
  },
  {
    _id: "comment-005",
    postId: "post-550e8400-e29b-41d4-a716-446655440002",
    parentCommentId: "comment-004",
    authorId: "550e8400-e29b-41d4-a716-446655440001",
    authorUsername: "testuser1",
    authorAvatarUrl: "https://example.com/avatars/john.jpg",
    content: "Thank you so much! Your mentorship has been invaluable! 🙏",
    mentions: ["550e8400-e29b-41d4-a716-446655440000"],
    likesCount: 4,
    repliesCount: 0,
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-14T15:30:00Z"),
    updatedAt: new Date("2024-01-14T15:30:00Z")
  },
  {
    _id: "comment-006",
    postId: "post-550e8400-e29b-41d4-a716-446655440003",
    parentCommentId: null,
    authorId: "550e8400-e29b-41d4-a716-446655440000",
    authorUsername: "admin",
    authorAvatarUrl: "https://example.com/avatars/admin.jpg",
    content: "Absolutely stunning! The colors are incredible! 🎨✨",
    mentions: [],
    likesCount: 6,
    repliesCount: 0,
    isEdited: false,
    editedAt: null,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-13T16:30:00Z"),
    updatedAt: new Date("2024-01-13T16:30:00Z")
  }
];

db.comments.insertMany(comments);
print(`${comments.length} comments inserted`);

// ========================================
// Sample Likes
// ========================================

const likes = [
  // Likes on posts
  { _id: "like-001", entityId: "post-550e8400-e29b-41d4-a716-446655440001", entityType: "Post", userId: "550e8400-e29b-41d4-a716-446655440001", username: "testuser1", createdAt: new Date("2024-01-15T10:05:00Z") },
  { _id: "like-002", entityId: "post-550e8400-e29b-41d4-a716-446655440001", entityType: "Post", userId: "550e8400-e29b-41d4-a716-446655440002", username: "testuser2", createdAt: new Date("2024-01-15T10:10:00Z") },
  { _id: "like-003", entityId: "post-550e8400-e29b-41d4-a716-446655440002", entityType: "Post", userId: "550e8400-e29b-41d4-a716-446655440000", username: "admin", createdAt: new Date("2024-01-14T14:35:00Z") },
  { _id: "like-004", entityId: "post-550e8400-e29b-41d4-a716-446655440002", entityType: "Post", userId: "550e8400-e29b-41d4-a716-446655440002", username: "testuser2", createdAt: new Date("2024-01-14T15:00:00Z") },
  { _id: "like-005", entityId: "post-550e8400-e29b-41d4-a716-446655440003", entityType: "Post", userId: "550e8400-e29b-41d4-a716-446655440000", username: "admin", createdAt: new Date("2024-01-13T16:10:00Z") },
  { _id: "like-006", entityId: "post-550e8400-e29b-41d4-a716-446655440003", entityType: "Post", userId: "550e8400-e29b-41d4-a716-446655440001", username: "testuser1", createdAt: new Date("2024-01-13T16:15:00Z") },

  // Likes on comments
  { _id: "like-007", entityId: "comment-001", entityType: "Comment", userId: "550e8400-e29b-41d4-a716-446655440000", username: "admin", createdAt: new Date("2024-01-15T10:16:00Z") },
  { _id: "like-008", entityId: "comment-001", entityType: "Comment", userId: "550e8400-e29b-41d4-a716-446655440002", username: "testuser2", createdAt: new Date("2024-01-15T10:17:00Z") },
  { _id: "like-009", entityId: "comment-004", entityType: "Comment", userId: "550e8400-e29b-41d4-a716-446655440001", username: "testuser1", createdAt: new Date("2024-01-14T15:05:00Z") },
  { _id: "like-010", entityId: "comment-006", entityType: "Comment", userId: "550e8400-e29b-41d4-a716-446655440002", username: "testuser2", createdAt: new Date("2024-01-13T16:35:00Z") }
];

db.likes.insertMany(likes);
print(`${likes.length} likes inserted`);

// ========================================
// Sample Shares
// ========================================

const shares = [
  {
    _id: "share-001",
    postId: "post-550e8400-e29b-41d4-a716-446655440001",
    userId: "550e8400-e29b-41d4-a716-446655440001",
    username: "testuser1",
    caption: "This is exactly what we needed!",
    visibility: "Public",
    createdAt: new Date("2024-01-15T12:00:00Z")
  },
  {
    _id: "share-002",
    postId: "post-550e8400-e29b-41d4-a716-446655440001",
    userId: "550e8400-e29b-41d4-a716-446655440002",
    username: "testuser2",
    caption: null,
    visibility: "FriendsOnly",
    createdAt: new Date("2024-01-15T13:30:00Z")
  },
  {
    _id: "share-003",
    postId: "post-550e8400-e29b-41d4-a716-446655440003",
    userId: "550e8400-e29b-41d4-a716-446655440000",
    username: "admin",
    caption: "Amazing talent! 🎨",
    visibility: "Public",
    createdAt: new Date("2024-01-13T18:00:00Z")
  },
  {
    _id: "share-004",
    postId: "post-550e8400-e29b-41d4-a716-446655440005",
    userId: "550e8400-e29b-41d4-a716-446655440000",
    username: "admin",
    caption: "Great tutorial! Everyone should watch this.",
    visibility: "Public",
    createdAt: new Date("2024-01-11T14:00:00Z")
  }
];

db.shares.insertMany(shares);
print(`${shares.length} shares inserted`);

// ========================================
// Sample Hashtags
// ========================================

const hashtags = [
  { _id: "technology", tag: "technology", displayTag: "Technology", usageCount: 15, trendingScore: 45.5, lastUsedAt: new Date("2024-01-15T10:00:00Z"), createdAt: new Date("2024-01-01T00:00:00Z"), updatedAt: new Date("2024-01-15T10:00:00Z") },
  { _id: "innovation", tag: "innovation", displayTag: "Innovation", usageCount: 8, trendingScore: 22.0, lastUsedAt: new Date("2024-01-15T10:00:00Z"), createdAt: new Date("2024-01-01T00:00:00Z"), updatedAt: new Date("2024-01-15T10:00:00Z") },
  { _id: "opensource", tag: "opensource", displayTag: "OpenSource", usageCount: 12, trendingScore: 38.5, lastUsedAt: new Date("2024-01-14T14:30:00Z"), createdAt: new Date("2024-01-01T00:00:00Z"), updatedAt: new Date("2024-01-14T14:30:00Z") },
  { _id: "coding", tag: "coding", displayTag: "Coding", usageCount: 20, trendingScore: 55.0, lastUsedAt: new Date("2024-01-14T14:30:00Z"), createdAt: new Date("2024-01-01T00:00:00Z"), updatedAt: new Date("2024-01-14T14:30:00Z") },
  { _id: "art", tag: "art", displayTag: "Art", usageCount: 25, trendingScore: 68.0, lastUsedAt: new Date("2024-01-13T16:00:00Z"), createdAt: new Date("2024-01-01T00:00:00Z"), updatedAt: new Date("2024-01-13T16:00:00Z") },
  { _id: "digitalart", tag: "digitalart", displayTag: "DigitalArt", usageCount: 18, trendingScore: 50.5, lastUsedAt: new Date("2024-01-13T16:00:00Z"), createdAt: new Date("2024-01-01T00:00:00Z"), updatedAt: new Date("2024-01-13T16:00:00Z") },
  { _id: "developer", tag: "developer", displayTag: "Developer", usageCount: 30, trendingScore: 75.0, lastUsedAt: new Date("2024-01-12T09:00:00Z"), createdAt: new Date("2024-01-01T00:00:00Z"), updatedAt: new Date("2024-01-12T09:00:00Z") },
  { _id: "mondaymotivation", tag: "mondaymotivation", displayTag: "MondayMotivation", usageCount: 45, trendingScore: 120.0, lastUsedAt: new Date("2024-01-12T09:00:00Z"), createdAt: new Date("2024-01-01T00:00:00Z"), updatedAt: new Date("2024-01-12T09:00:00Z") },
  { _id: "architecture", tag: "architecture", displayTag: "Architecture", usageCount: 10, trendingScore: 28.0, lastUsedAt: new Date("2024-01-11T11:30:00Z"), createdAt: new Date("2024-01-01T00:00:00Z"), updatedAt: new Date("2024-01-11T11:30:00Z") },
  { _id: "software", tag: "software", displayTag: "Software", usageCount: 22, trendingScore: 60.0, lastUsedAt: new Date("2024-01-11T11:30:00Z"), createdAt: new Date("2024-01-01T00:00:00Z"), updatedAt: new Date("2024-01-11T11:30:00Z") }
];

db.hashtags.insertMany(hashtags);
print(`${hashtags.length} hashtags inserted`);

// ========================================
// Verification
// ========================================

print("\n=== Database Seeding Complete ===");
print(`Total posts: ${db.posts.countDocuments({})}`);
print(`Total comments: ${db.comments.countDocuments({})}`);
print(`Total likes: ${db.likes.countDocuments({})}`);
print(`Total shares: ${db.shares.countDocuments({})}`);
print(`Total hashtags: ${db.hashtags.countDocuments({})}`);
print(`Public posts: ${db.posts.countDocuments({ visibility: "Public" })}`);
print(`Trending hashtags: ${db.hashtags.countDocuments({ trendingScore: { $gte: 50 } })}`);
