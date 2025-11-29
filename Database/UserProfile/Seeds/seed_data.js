// ========================================
// UserProfile Service - Seed Data for Development/Testing
// WARNING: Do not run in production!
// ========================================

// Clear existing data (DEVELOPMENT ONLY!)
db.user_activities.deleteMany({});
db.friend_requests.deleteMany({});
db.user_profiles.deleteMany({});

print("Existing data cleared");

// ========================================
// Sample User Profiles
// ========================================

const profiles = [
  {
    _id: "550e8400-e29b-41d4-a716-446655440000",
    userId: "550e8400-e29b-41d4-a716-446655440000",
    username: "admin",
    displayName: "Admin User",
    email: "admin@wechat.com",
    phoneNumber: "+1234567890",
    bio: "System administrator and platform moderator",
    avatarUrl: "https://example.com/avatars/admin.jpg",
    coverImageUrl: "https://example.com/covers/admin.jpg",
    location: "San Francisco, CA",
    website: "https://admin.wechat.com",
    dateOfBirth: new Date("1990-01-15"),
    gender: "PreferNotToSay",
    friends: [
      "550e8400-e29b-41d4-a716-446655440001",
      "550e8400-e29b-41d4-a716-446655440002"
    ],
    followers: [
      "550e8400-e29b-41d4-a716-446655440001",
      "550e8400-e29b-41d4-a716-446655440002"
    ],
    following: [
      "550e8400-e29b-41d4-a716-446655440001"
    ],
    blockedUsers: [],
    isOnline: true,
    lastSeenAt: new Date(),
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
      friendsCount: 2,
      followersCount: 2,
      followingCount: 1,
      postsCount: 15
    },
    isVerified: true,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-01T08:00:00Z"),
    updatedAt: new Date()
  },
  {
    _id: "550e8400-e29b-41d4-a716-446655440001",
    userId: "550e8400-e29b-41d4-a716-446655440001",
    username: "testuser1",
    displayName: "John Smith",
    email: "test1@example.com",
    phoneNumber: "+1234567891",
    bio: "Software developer passionate about clean code and best practices",
    avatarUrl: "https://example.com/avatars/john.jpg",
    coverImageUrl: null,
    location: "New York, NY",
    website: "https://johnsmith.dev",
    dateOfBirth: new Date("1995-06-20"),
    gender: "Male",
    friends: [
      "550e8400-e29b-41d4-a716-446655440000",
      "550e8400-e29b-41d4-a716-446655440002"
    ],
    followers: [
      "550e8400-e29b-41d4-a716-446655440000"
    ],
    following: [
      "550e8400-e29b-41d4-a716-446655440000",
      "550e8400-e29b-41d4-a716-446655440002"
    ],
    blockedUsers: [],
    isOnline: false,
    lastSeenAt: new Date(Date.now() - 3600000), // 1 hour ago
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
      postNotifications: false
    },
    statistics: {
      friendsCount: 2,
      followersCount: 1,
      followingCount: 2,
      postsCount: 8
    },
    isVerified: false,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-02T10:30:00Z"),
    updatedAt: new Date(Date.now() - 3600000)
  },
  {
    _id: "550e8400-e29b-41d4-a716-446655440002",
    userId: "550e8400-e29b-41d4-a716-446655440002",
    username: "testuser2",
    displayName: "Jane Doe",
    email: "test2@example.com",
    phoneNumber: null,
    bio: "Digital artist and creative enthusiast 🎨",
    avatarUrl: "https://example.com/avatars/jane.jpg",
    coverImageUrl: "https://example.com/covers/jane.jpg",
    location: "Los Angeles, CA",
    website: null,
    dateOfBirth: new Date("1998-03-10"),
    gender: "Female",
    friends: [
      "550e8400-e29b-41d4-a716-446655440000",
      "550e8400-e29b-41d4-a716-446655440001"
    ],
    followers: [
      "550e8400-e29b-41d4-a716-446655440001"
    ],
    following: [],
    blockedUsers: [],
    isOnline: true,
    lastSeenAt: new Date(),
    privacySettings: {
      profileVisibility: "FriendsOnly",
      showOnlineStatus: false,
      showLastSeen: false,
      allowFriendRequests: true,
      allowMessages: "FriendsOnly"
    },
    notificationSettings: {
      emailNotifications: false,
      pushNotifications: true,
      messageNotifications: true,
      friendRequestNotifications: true,
      postNotifications: true
    },
    statistics: {
      friendsCount: 2,
      followersCount: 1,
      followingCount: 0,
      postsCount: 23
    },
    isVerified: false,
    isDeleted: false,
    deletedAt: null,
    createdAt: new Date("2024-01-03T14:15:00Z"),
    updatedAt: new Date()
  },
  {
    _id: "550e8400-e29b-41d4-a716-446655440003",
    userId: "550e8400-e29b-41d4-a716-446655440003",
    username: "newuser",
    displayName: null,
    email: "newuser@example.com",
    phoneNumber: null,
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
  }
];

db.user_profiles.insertMany(profiles);
print(`${profiles.length} user profiles inserted`);

// ========================================
// Sample Friend Requests
// ========================================

const friendRequests = [
  {
    _id: "freq-550e8400-e29b-41d4-a716-446655440001",
    senderId: "550e8400-e29b-41d4-a716-446655440003",
    senderUsername: "newuser",
    senderAvatarUrl: null,
    receiverId: "550e8400-e29b-41d4-a716-446655440000",
    receiverUsername: "admin",
    receiverAvatarUrl: "https://example.com/avatars/admin.jpg",
    status: "Pending",
    message: "Hi! I'd like to connect with you.",
    respondedAt: null,
    expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000), // 7 days from now
    createdAt: new Date(),
    updatedAt: new Date()
  },
  {
    _id: "freq-550e8400-e29b-41d4-a716-446655440002",
    senderId: "550e8400-e29b-41d4-a716-446655440000",
    senderUsername: "admin",
    senderAvatarUrl: "https://example.com/avatars/admin.jpg",
    receiverId: "550e8400-e29b-41d4-a716-446655440001",
    receiverUsername: "testuser1",
    receiverAvatarUrl: "https://example.com/avatars/john.jpg",
    status: "Accepted",
    message: null,
    respondedAt: new Date("2024-01-02T12:00:00Z"),
    expiresAt: null,
    createdAt: new Date("2024-01-02T10:00:00Z"),
    updatedAt: new Date("2024-01-02T12:00:00Z")
  },
  {
    _id: "freq-550e8400-e29b-41d4-a716-446655440003",
    senderId: "550e8400-e29b-41d4-a716-446655440001",
    senderUsername: "testuser1",
    senderAvatarUrl: "https://example.com/avatars/john.jpg",
    receiverId: "550e8400-e29b-41d4-a716-446655440002",
    receiverUsername: "testuser2",
    receiverAvatarUrl: "https://example.com/avatars/jane.jpg",
    status: "Accepted",
    message: "Let's connect!",
    respondedAt: new Date("2024-01-03T16:00:00Z"),
    expiresAt: null,
    createdAt: new Date("2024-01-03T15:00:00Z"),
    updatedAt: new Date("2024-01-03T16:00:00Z")
  }
];

db.friend_requests.insertMany(friendRequests);
print(`${friendRequests.length} friend requests inserted`);

// ========================================
// Sample User Activities
// ========================================

const activities = [
  {
    _id: "act-001",
    userId: "550e8400-e29b-41d4-a716-446655440000",
    username: "admin",
    activityType: "FriendAdded",
    targetUserId: "550e8400-e29b-41d4-a716-446655440001",
    targetUsername: "testuser1",
    metadata: null,
    ipAddress: "192.168.1.100",
    userAgent: "Mozilla/5.0...",
    createdAt: new Date("2024-01-02T12:00:00Z")
  },
  {
    _id: "act-002",
    userId: "550e8400-e29b-41d4-a716-446655440000",
    username: "admin",
    activityType: "ProfileUpdated",
    targetUserId: null,
    targetUsername: null,
    metadata: { fields: ["bio", "avatarUrl"] },
    ipAddress: "192.168.1.100",
    userAgent: "Mozilla/5.0...",
    createdAt: new Date("2024-01-05T09:30:00Z")
  },
  {
    _id: "act-003",
    userId: "550e8400-e29b-41d4-a716-446655440001",
    username: "testuser1",
    activityType: "UserFollowed",
    targetUserId: "550e8400-e29b-41d4-a716-446655440000",
    targetUsername: "admin",
    metadata: null,
    ipAddress: "192.168.1.101",
    userAgent: "Mozilla/5.0...",
    createdAt: new Date("2024-01-06T14:20:00Z")
  },
  {
    _id: "act-004",
    userId: "550e8400-e29b-41d4-a716-446655440002",
    username: "testuser2",
    activityType: "ProfileUpdated",
    targetUserId: null,
    targetUsername: null,
    metadata: { fields: ["privacySettings"] },
    ipAddress: "192.168.1.102",
    userAgent: "Mozilla/5.0...",
    createdAt: new Date("2024-01-07T11:00:00Z")
  },
  {
    _id: "act-005",
    userId: "550e8400-e29b-41d4-a716-446655440001",
    username: "testuser1",
    activityType: "FriendAdded",
    targetUserId: "550e8400-e29b-41d4-a716-446655440002",
    targetUsername: "testuser2",
    metadata: null,
    ipAddress: "192.168.1.101",
    userAgent: "Mozilla/5.0...",
    createdAt: new Date("2024-01-03T16:00:00Z")
  }
];

db.user_activities.insertMany(activities);
print(`${activities.length} user activities inserted`);

// ========================================
// Verification
// ========================================

print("\n=== Database Seeding Complete ===");
print(`Total user profiles: ${db.user_profiles.countDocuments({})}`);
print(`Total friend requests: ${db.friend_requests.countDocuments({})}`);
print(`Total user activities: ${db.user_activities.countDocuments({})}`);
print(`Pending friend requests: ${db.friend_requests.countDocuments({ status: "Pending" })}`);
print(`Online users: ${db.user_profiles.countDocuments({ isOnline: true })}`);
