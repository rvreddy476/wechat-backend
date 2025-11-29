// ========================================
// Chat Service - Message Indexes
// ========================================

// Primary index for fetching messages by conversation
db.messages.createIndex(
  { "conversationId": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_conversationId_isDeleted_createdAt" }
);

// Index for finding messages by sender
db.messages.createIndex(
  { "senderId": 1, "createdAt": -1 },
  { name: "idx_senderId_createdAt" }
);

// Index for unread messages
db.messages.createIndex(
  { "conversationId": 1, "readBy.userId": 1, "isDeleted": 1 },
  { name: "idx_conversationId_readBy_userId_isDeleted" }
);

// Index for message mentions
db.messages.createIndex(
  { "mentions": 1, "createdAt": -1 },
  { name: "idx_mentions_createdAt" }
);

// Index for finding reply threads
db.messages.createIndex(
  { "replyToMessageId": 1, "conversationId": 1 },
  { name: "idx_replyToMessageId_conversationId" }
);

// Index for message type filtering
db.messages.createIndex(
  { "conversationId": 1, "messageType": 1, "isDeleted": 1, "createdAt": -1 },
  { name: "idx_conversationId_messageType_isDeleted_createdAt" }
);

// Text search index for message content
db.messages.createIndex(
  { "content": "text", "senderUsername": "text" },
  { name: "idx_message_text_search", default_language: "english" }
);

// Index for edited messages
db.messages.createIndex(
  { "isEdited": 1, "editedAt": -1 },
  { name: "idx_isEdited_editedAt" }
);

// Index for reactions
db.messages.createIndex(
  { "reactions.userId": 1, "conversationId": 1 },
  { name: "idx_reactions_userId_conversationId" }
);

// Compound index for pagination and filtering
db.messages.createIndex(
  { "conversationId": 1, "isDeleted": 1, "createdAt": -1, "_id": -1 },
  { name: "idx_message_pagination" }
);

print("Message indexes created successfully");
