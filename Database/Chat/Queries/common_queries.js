// ========================================
// Chat Service - Common Query Patterns
// ========================================

// ========================================
// CONVERSATION QUERIES
// ========================================

// 1. Get all conversations for a user (sorted by last activity)
db.conversations.find({
  "participants.userId": "550e8400-e29b-41d4-a716-446655440000",
  "isDeleted": false
}).sort({ "lastMessage.sentAt": -1 });

// 2. Find one-to-one conversation between two users
db.conversations.findOne({
  "type": "OneToOne",
  "participants.userId": { $all: [
    "550e8400-e29b-41d4-a716-446655440000",
    "550e8400-e29b-41d4-a716-446655440001"
  ]},
  "isDeleted": false
});

// 3. Get all group conversations where user is admin
db.conversations.find({
  "type": "Group",
  "admins": "550e8400-e29b-41d4-a716-446655440000",
  "isDeleted": false
}).sort({ "updatedAt": -1 });

// 4. Search group conversations by name
db.conversations.find({
  "type": "Group",
  "groupName": { $regex: "Project", $options: "i" },
  "isDeleted": false
});

// 5. Get conversation with participant details
db.conversations.aggregate([
  {
    $match: {
      "_id": "conv123",
      "isDeleted": false
    }
  },
  {
    $project: {
      type: 1,
      groupName: 1,
      groupAvatarUrl: 1,
      participants: 1,
      lastMessage: 1,
      createdAt: 1,
      participantCount: { $size: "$participants" }
    }
  }
]);

// ========================================
// MESSAGE QUERIES
// ========================================

// 6. Get messages for a conversation (paginated, newest first)
db.messages.find({
  "conversationId": "conv123",
  "isDeleted": false
})
.sort({ "createdAt": -1 })
.limit(50);

// 7. Get messages after a specific message ID (for infinite scroll)
db.messages.find({
  "conversationId": "conv123",
  "isDeleted": false,
  "createdAt": { $lt: ISODate("2024-01-01T00:00:00Z") }
})
.sort({ "createdAt": -1 })
.limit(50);

// 8. Get unread message count for user in conversation
db.messages.countDocuments({
  "conversationId": "conv123",
  "isDeleted": false,
  "readBy.userId": { $ne: "550e8400-e29b-41d4-a716-446655440000" }
});

// 9. Get all unread messages for a user across all conversations
db.messages.aggregate([
  {
    $match: {
      "isDeleted": false,
      "readBy.userId": { $ne: "550e8400-e29b-41d4-a716-446655440000" }
    }
  },
  {
    $group: {
      _id: "$conversationId",
      unreadCount: { $sum: 1 },
      lastMessage: { $last: "$$ROOT" }
    }
  },
  {
    $sort: { "lastMessage.createdAt": -1 }
  }
]);

// 10. Search messages by content
db.messages.find({
  $text: { $search: "important meeting" },
  "conversationId": "conv123",
  "isDeleted": false
}).sort({ score: { $meta: "textScore" } });

// 11. Get all media messages in a conversation
db.messages.find({
  "conversationId": "conv123",
  "messageType": { $in: ["Image", "Video", "Audio", "File"] },
  "isDeleted": false
}).sort({ "createdAt": -1 });

// 12. Get messages with mentions for a user
db.messages.find({
  "mentions": "550e8400-e29b-41d4-a716-446655440000",
  "isDeleted": false
}).sort({ "createdAt": -1 });

// 13. Get reply thread for a message
db.messages.find({
  "replyToMessageId": "msg123",
  "isDeleted": false
}).sort({ "createdAt": 1 });

// 14. Get messages with reactions
db.messages.find({
  "conversationId": "conv123",
  "reactions": { $exists: true, $ne: [] },
  "isDeleted": false
});

// ========================================
// ANALYTICS QUERIES
// ========================================

// 15. Get message count per user in conversation
db.messages.aggregate([
  {
    $match: {
      "conversationId": "conv123",
      "isDeleted": false
    }
  },
  {
    $group: {
      _id: "$senderId",
      username: { $first: "$senderUsername" },
      messageCount: { $sum: 1 }
    }
  },
  {
    $sort: { messageCount: -1 }
  }
]);

// 16. Get conversation statistics
db.conversations.aggregate([
  {
    $match: {
      "participants.userId": "550e8400-e29b-41d4-a716-446655440000",
      "isDeleted": false
    }
  },
  {
    $facet: {
      totalConversations: [{ $count: "count" }],
      oneToOneCount: [
        { $match: { "type": "OneToOne" }},
        { $count: "count" }
      ],
      groupCount: [
        { $match: { "type": "Group" }},
        { $count: "count" }
      ]
    }
  }
]);

// 17. Get active conversations (with messages in last 7 days)
db.conversations.find({
  "participants.userId": "550e8400-e29b-41d4-a716-446655440000",
  "lastMessage.sentAt": {
    $gte: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000)
  },
  "isDeleted": false
});

// ========================================
// UPDATE QUERIES
// ========================================

// 18. Mark message as read
db.messages.updateOne(
  {
    "_id": "msg123"
  },
  {
    $addToSet: {
      "readBy": {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "readAt": new Date()
      }
    }
  }
);

// 19. Update conversation last message
db.conversations.updateOne(
  {
    "_id": "conv123"
  },
  {
    $set: {
      "lastMessage": {
        "messageId": "msg456",
        "content": "Hello there!",
        "senderId": "550e8400-e29b-41d4-a716-446655440000",
        "senderUsername": "john_doe",
        "sentAt": new Date()
      },
      "updatedAt": new Date()
    }
  }
);

// 20. Add reaction to message
db.messages.updateOne(
  {
    "_id": "msg123"
  },
  {
    $addToSet: {
      "reactions": {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "emoji": "👍",
        "reactedAt": new Date()
      }
    }
  }
);

// 21. Remove reaction from message
db.messages.updateOne(
  {
    "_id": "msg123"
  },
  {
    $pull: {
      "reactions": {
        "userId": "550e8400-e29b-41d4-a716-446655440000"
      }
    }
  }
);

// 22. Edit message
db.messages.updateOne(
  {
    "_id": "msg123"
  },
  {
    $set: {
      "content": "Updated message content",
      "isEdited": true,
      "editedAt": new Date(),
      "updatedAt": new Date()
    }
  }
);

// 23. Soft delete message
db.messages.updateOne(
  {
    "_id": "msg123"
  },
  {
    $set: {
      "isDeleted": true,
      "deletedAt": new Date(),
      "deletedBy": "550e8400-e29b-41d4-a716-446655440000",
      "updatedAt": new Date()
    }
  }
);

// 24. Add participant to group conversation
db.conversations.updateOne(
  {
    "_id": "conv123",
    "type": "Group"
  },
  {
    $addToSet: {
      "participants": {
        "userId": "550e8400-e29b-41d4-a716-446655440002",
        "username": "jane_doe",
        "joinedAt": new Date()
      }
    },
    $set: {
      "updatedAt": new Date()
    }
  }
);

// 25. Remove participant from group conversation
db.conversations.updateOne(
  {
    "_id": "conv123",
    "type": "Group"
  },
  {
    $pull: {
      "participants": {
        "userId": "550e8400-e29b-41d4-a716-446655440002"
      }
    },
    $set: {
      "updatedAt": new Date()
    }
  }
);

print("Common query patterns documented");
