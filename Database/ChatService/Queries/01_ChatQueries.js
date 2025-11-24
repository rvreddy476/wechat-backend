// =============================================
// WeChat.com - ChatService MongoDB Queries
// Purpose: Common query patterns for chat functionality
// =============================================

const DB_NAME = 'wechat_chat';
use(DB_NAME);

print("ChatService - Common Query Patterns");
print("======================================\n");

// =============================================
// 1. Get User's Conversations
// =============================================
print("1. Get User's Conversations:");
const getUserConversations = (userId, limit = 50, skip = 0) => {
    return db.conversations.aggregate([
        {
            $match: {
                "participants.userId": userId,
                "participants.isActive": true,
                isDeleted: false
            }
        },
        // Calculate unread count for this user
        {
            $addFields: {
                currentUser: {
                    $arrayElemAt: [
                        {
                            $filter: {
                                input: "$participants",
                                cond: { $eq: ["$$this.userId", userId] }
                            }
                        },
                        0
                    ]
                }
            }
        },
        {
            $lookup: {
                from: "messages",
                let: {
                    convId: "$conversationId",
                    lastReadId: "$currentUser.lastReadMessageId"
                },
                pipeline: [
                    {
                        $match: {
                            $expr: {
                                $and: [
                                    { $eq: ["$conversationId", "$$convId"] },
                                    { $ne: ["$senderId", userId] }
                                ]
                            },
                            isDeleted: false
                        }
                    },
                    {
                        $match: {
                            $expr: {
                                $gt: ["$createdAt", "$currentUser.lastReadAt"]
                            }
                        }
                    },
                    { $count: "count" }
                ],
                as: "unreadInfo"
            }
        },
        {
            $addFields: {
                unreadCount: {
                    $ifNull: [{ $arrayElemAt: ["$unreadInfo.count", 0] }, 0]
                }
            }
        },
        {
            $sort: { isPinned: -1, lastMessageAt: -1 }
        },
        {
            $skip: skip
        },
        {
            $limit: limit
        },
        {
            $project: {
                _id: 0,
                conversationId: 1,
                type: 1,
                participants: 1,
                groupName: 1,
                groupIconUrl: 1,
                lastMessage: 1,
                lastMessageAt: 1,
                unreadCount: 1,
                isPinned: 1,
                isArchived: 1,
                messageCount: 1,
                currentUser: 1
            }
        }
    ]).toArray();
};
print("Usage: getUserConversations('user-uuid', 50, 0)");
print("");

// =============================================
// 2. Find or Create Direct Conversation
// =============================================
print("2. Find/Create Direct Conversation:");
const findDirectConversation = (userId1, userId2) => {
    return db.conversations.findOne({
        type: "direct",
        "participants.userId": { $all: [userId1, userId2] },
        isDeleted: false
    });
};
print("Usage: findDirectConversation('user1-uuid', 'user2-uuid')");
print("");

// =============================================
// 3. Get Messages for Conversation
// =============================================
print("3. Get Messages for Conversation:");
const getConversationMessages = (conversationId, userId, limit = 50, beforeMessageId = null) => {
    let matchStage = {
        conversationId: conversationId,
        isDeleted: false,
        deletedFor: { $ne: userId }
    };

    // For pagination: get messages before a specific message
    if (beforeMessageId) {
        // First find the timestamp of the beforeMessageId
        const beforeMsg = db.messages.findOne({ messageId: beforeMessageId }, { createdAt: 1 });
        if (beforeMsg) {
            matchStage.createdAt = { $lt: beforeMsg.createdAt };
        }
    }

    return db.messages.aggregate([
        { $match: matchStage },
        { $sort: { createdAt: -1 } },
        { $limit: limit },
        // Lookup sender profile
        {
            $lookup: {
                from: "wechat_profiles.profiles",
                localField: "senderId",
                foreignField: "userId",
                as: "sender"
            }
        },
        {
            $unwind: {
                path: "$sender",
                preserveNullAndEmptyArrays: true
            }
        },
        {
            $project: {
                _id: 0,
                messageId: 1,
                conversationId: 1,
                senderId: 1,
                messageType: 1,
                content: 1,
                mediaUrls: 1,
                location: 1,
                replyToMessageId: 1,
                replyToMessage: 1,
                mentions: 1,
                status: 1,
                readBy: 1,
                reactions: 1,
                isEdited: 1,
                editedAt: 1,
                createdAt: 1,
                "sender.username": 1,
                "sender.displayName": 1,
                "sender.avatarUrl": 1
            }
        },
        { $sort: { createdAt: 1 } } // Return in chronological order
    ]).toArray();
};
print("Usage: getConversationMessages('conv-uuid', 'user-uuid', 50, 'last-msg-uuid')");
print("");

// =============================================
// 4. Send Message
// =============================================
print("4. Send Message:");
const sendMessage = (messageData) => {
    const session = db.getMongo().startSession();
    session.startTransaction();

    try {
        // Insert message
        db.messages.insertOne({
            ...messageData,
            status: "sent",
            deliveredAt: new Date(),
            readBy: [],
            reactions: [],
            isEdited: false,
            editedAt: null,
            isDeleted: false,
            deletedAt: null,
            deletedFor: [],
            createdAt: new Date(),
            updatedAt: new Date(),
            isFlagged: false,
            reportCount: 0
        });

        // Update conversation lastMessage
        db.conversations.updateOne(
            { conversationId: messageData.conversationId },
            {
                $set: {
                    lastMessage: {
                        messageId: messageData.messageId,
                        senderId: messageData.senderId,
                        content: messageData.content.text || "[Media]",
                        messageType: messageData.messageType,
                        createdAt: new Date()
                    },
                    lastMessageAt: new Date(),
                    updatedAt: new Date()
                },
                $inc: { messageCount: 1 }
            }
        );

        session.commitTransaction();
        return { success: true, messageId: messageData.messageId };
    } catch (error) {
        session.abortTransaction();
        return { success: false, error: error.message };
    } finally {
        session.endSession();
    }
};
print("Usage: sendMessage(messageData)");
print("");

// =============================================
// 5. Mark Messages as Read
// =============================================
print("5. Mark Messages as Read:");
const markMessagesAsRead = (conversationId, userId, upToMessageId) => {
    const upToMsg = db.messages.findOne({ messageId: upToMessageId });
    if (!upToMsg) return { success: false, error: "Message not found" };

    // Update all unread messages up to this point
    const result = db.messages.updateMany(
        {
            conversationId: conversationId,
            createdAt: { $lte: upToMsg.createdAt },
            senderId: { $ne: userId },
            "readBy.userId": { $ne: userId }
        },
        {
            $push: {
                readBy: {
                    userId: userId,
                    readAt: new Date()
                }
            }
        }
    );

    // Update conversation participant lastRead
    db.conversations.updateOne(
        {
            conversationId: conversationId,
            "participants.userId": userId
        },
        {
            $set: {
                "participants.$.lastReadMessageId": upToMessageId,
                "participants.$.lastReadAt": new Date()
            }
        }
    );

    return { success: true, messagesMarked: result.modifiedCount };
};
print("Usage: markMessagesAsRead('conv-uuid', 'user-uuid', 'last-msg-uuid')");
print("");

// =============================================
// 6. Get Unread Message Count
// =============================================
print("6. Get Unread Message Count:");
const getUnreadCount = (userId) => {
    return db.conversations.aggregate([
        {
            $match: {
                "participants.userId": userId,
                isDeleted: false
            }
        },
        {
            $addFields: {
                currentUser: {
                    $arrayElemAt: [
                        {
                            $filter: {
                                input: "$participants",
                                cond: { $eq: ["$$this.userId", userId] }
                            }
                        },
                        0
                    ]
                }
            }
        },
        {
            $lookup: {
                from: "messages",
                let: { convId: "$conversationId", lastReadAt: "$currentUser.lastReadAt" },
                pipeline: [
                    {
                        $match: {
                            $expr: {
                                $and: [
                                    { $eq: ["$conversationId", "$$convId"] },
                                    { $ne: ["$senderId", userId] },
                                    { $gt: ["$createdAt", "$$lastReadAt"] }
                                ]
                            },
                            isDeleted: false
                        }
                    },
                    { $count: "count" }
                ],
                as: "unreadInfo"
            }
        },
        {
            $group: {
                _id: null,
                totalUnread: {
                    $sum: { $ifNull: [{ $arrayElemAt: ["$unreadInfo.count", 0] }, 0] }
                }
            }
        }
    ]).toArray();
};
print("Usage: getUnreadCount('user-uuid')");
print("");

// =============================================
// 7. Search Messages
// =============================================
print("7. Search Messages:");
const searchMessages = (conversationId, searchTerm, limit = 50) => {
    return db.messages.find(
        {
            conversationId: conversationId,
            $text: { $search: searchTerm },
            isDeleted: false
        },
        {
            score: { $meta: "textScore" }
        }
    )
    .sort({ score: { $meta: "textScore" } })
    .limit(limit)
    .project({
        _id: 0,
        messageId: 1,
        senderId: 1,
        content: 1,
        messageType: 1,
        createdAt: 1,
        score: { $meta: "textScore" }
    })
    .toArray();
};
print("Usage: searchMessages('conv-uuid', 'search term', 50)");
print("");

// =============================================
// 8. Get Media Messages (Gallery)
// =============================================
print("8. Get Media Messages:");
const getMediaMessages = (conversationId, mediaType = "image", limit = 50, skip = 0) => {
    return db.messages.find(
        {
            conversationId: conversationId,
            messageType: mediaType,
            isDeleted: false
        },
        {
            projection: {
                _id: 0,
                messageId: 1,
                senderId: 1,
                mediaUrls: 1,
                content: 1,
                createdAt: 1
            }
        }
    )
    .sort({ createdAt: -1 })
    .skip(skip)
    .limit(limit)
    .toArray();
};
print("Usage: getMediaMessages('conv-uuid', 'image', 50, 0)");
print("");

print("\n=============================================");
print("Query patterns defined successfully!");
print("=============================================\n");
