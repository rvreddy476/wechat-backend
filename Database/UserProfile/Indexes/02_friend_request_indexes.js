// ========================================
// UserProfile Service - Friend Request Indexes
// ========================================

// Index for finding requests sent by user
db.friend_requests.createIndex(
  { "senderId": 1, "status": 1, "createdAt": -1 },
  { name: "idx_senderId_status_createdAt" }
);

// Index for finding requests received by user
db.friend_requests.createIndex(
  { "receiverId": 1, "status": 1, "createdAt": -1 },
  { name: "idx_receiverId_status_createdAt" }
);

// Index for checking existing requests between two users
db.friend_requests.createIndex(
  { "senderId": 1, "receiverId": 1, "status": 1 },
  { name: "idx_sender_receiver_status" }
);

// Index for pending requests
db.friend_requests.createIndex(
  { "status": 1, "createdAt": -1 },
  { name: "idx_status_createdAt" }
);

// Index for expired requests cleanup
db.friend_requests.createIndex(
  { "status": 1, "expiresAt": 1 },
  { name: "idx_status_expiresAt" }
);

// Compound index for user's pending requests
db.friend_requests.createIndex(
  { "receiverId": 1, "status": 1, "expiresAt": 1 },
  { name: "idx_receiver_status_expires" }
);

print("Friend request indexes created successfully");
