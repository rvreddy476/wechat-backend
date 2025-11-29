// ========================================
// Notification Service - Indexes
// ========================================

db.notifications.createIndex({ "userId": 1, "isRead": 1, "createdAt": -1 }, { name: "idx_userId_isRead_createdAt" });
db.notifications.createIndex({ "userId": 1, "type": 1, "isRead": 1 }, { name: "idx_userId_type_isRead" });
db.notifications.createIndex({ "fromUserId": 1, "createdAt": -1 }, { name: "idx_fromUserId_createdAt" });
db.notifications.createIndex({ "priority": 1, "isRead": 1 }, { name: "idx_priority_isRead" });

print("Notification indexes created");
