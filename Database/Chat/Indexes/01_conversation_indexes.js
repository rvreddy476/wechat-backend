// ========================================
// Chat Service - Conversation Indexes
// ========================================

// Index for finding user's conversations
db.conversations.createIndex(
  { "participants.userId": 1, "isDeleted": 1, "updatedAt": -1 },
  { name: "idx_participants_userId_isDeleted_updatedAt" }
);

// Index for finding one-to-one conversations between two users
db.conversations.createIndex(
  { "type": 1, "participants.userId": 1, "isDeleted": 1 },
  { name: "idx_type_participants_userId_isDeleted" }
);

// Index for group conversations by name
db.conversations.createIndex(
  { "groupName": 1, "type": 1, "isDeleted": 1 },
  { name: "idx_groupName_type_isDeleted" }
);

// Index for finding conversations created by user
db.conversations.createIndex(
  { "createdBy": 1, "createdAt": -1 },
  { name: "idx_createdBy_createdAt" }
);

// Index for finding conversations with admins (groups)
db.conversations.createIndex(
  { "admins": 1, "type": 1 },
  { name: "idx_admins_type" }
);

// Index for sorting by last activity
db.conversations.createIndex(
  { "lastMessage.sentAt": -1, "isDeleted": 1 },
  { name: "idx_lastMessage_sentAt_isDeleted" }
);

// Compound index for efficient conversation listing
db.conversations.createIndex(
  { "participants.userId": 1, "isDeleted": 1, "lastMessage.sentAt": -1 },
  { name: "idx_conversation_listing" }
);

print("Conversation indexes created successfully");
