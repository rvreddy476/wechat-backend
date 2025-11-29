# Chat Service Database Documentation

## Overview

The Chat service uses **MongoDB** as its database to store conversations and messages. MongoDB's flexible document model is ideal for chat applications due to:

- Dynamic message types (text, images, videos, audio, files)
- Nested structures (reactions, read receipts, mentions)
- Horizontal scalability for high message volume
- Flexible schema evolution

## Database Structure

### Collections

#### 1. **conversations**
Stores all chat conversations (one-to-one and group chats)

**Key Fields:**
- `_id` (string) - Unique conversation identifier
- `type` (enum) - "OneToOne" or "Group"
- `participants` (array) - List of users in the conversation
- `groupName` (string, nullable) - Name for group chats
- `groupAvatarUrl` (string, nullable) - Avatar image URL
- `groupDescription` (string, nullable) - Description text
- `createdBy` (string) - UUID of creator
- `admins` (array) - List of admin user IDs (for groups)
- `lastMessage` (object, nullable) - Preview of last message
- `isDeleted` (boolean) - Soft delete flag
- `createdAt` (date) - Creation timestamp
- `updatedAt` (date) - Last update timestamp

#### 2. **messages**
Stores all chat messages

**Key Fields:**
- `_id` (string) - Unique message identifier
- `conversationId` (string) - Reference to conversation
- `senderId` (string) - UUID of sender
- `senderUsername` (string) - Username of sender
- `content` (string) - Message text content
- `messageType` (enum) - "Text", "Image", "Video", "Audio", "File"
- `mediaUrl` (string, nullable) - URL to media file
- `mediaThumbnailUrl` (string, nullable) - Thumbnail URL
- `mediaSize` (long, nullable) - File size in bytes
- `mediaDuration` (int, nullable) - Duration in seconds (audio/video)
- `replyToMessageId` (string, nullable) - Referenced message ID
- `replyToContent` (string, nullable) - Preview of replied message
- `readBy` (array) - List of users who read the message
- `isEdited` (boolean) - Edit flag
- `editedAt` (date, nullable) - Last edit timestamp
- `isDeleted` (boolean) - Soft delete flag
- `deletedAt` (date, nullable) - Deletion timestamp
- `deletedBy` (string, nullable) - User who deleted
- `reactions` (array) - Emoji reactions
- `mentions` (array) - List of mentioned user IDs
- `createdAt` (date) - Creation timestamp
- `updatedAt` (date) - Last update timestamp

## Connection String

### Development
```bash
mongodb://localhost:27017/wechat_chat
```

### Production (with authentication)
```bash
mongodb://username:password@mongodb-host:27017/wechat_chat?authSource=admin
```

### Docker Compose
```bash
mongodb://mongo:27017/wechat_chat
```

## Setup Instructions

### 1. Create Database and Collections

```bash
# Connect to MongoDB
mongosh

# Switch to database
use wechat_chat

# Run collection creation scripts
load('Collections/01_conversations.js')
load('Collections/02_messages.js')
```

### 2. Create Indexes

```bash
# Create all indexes for optimal performance
load('Indexes/01_conversation_indexes.js')
load('Indexes/02_message_indexes.js')
```

### 3. Seed Test Data (Development Only)

```bash
# Load sample conversations and messages
load('Seeds/seed_data.js')
```

## Common Operations

### Backup

```bash
# Full database backup
mongodump --db=wechat_chat --out=/backup/chat/$(date +%Y%m%d)

# Backup conversations only
mongodump --db=wechat_chat --collection=conversations --out=/backup/chat/conversations

# Backup messages only
mongodump --db=wechat_chat --collection=messages --out=/backup/chat/messages

# Compressed backup
mongodump --db=wechat_chat --gzip --archive=/backup/chat/chat_$(date +%Y%m%d).gz
```

### Restore

```bash
# Full database restore
mongorestore --db=wechat_chat /backup/chat/20240115/wechat_chat

# Restore specific collection
mongorestore --db=wechat_chat --collection=conversations /backup/chat/conversations/conversations.bson

# Restore from compressed backup
mongorestore --gzip --archive=/backup/chat/chat_20240115.gz
```

### Maintenance

```bash
# Connect to database
mongosh wechat_chat

# Get database statistics
db.stats()

# Get collection statistics
db.conversations.stats()
db.messages.stats()

# Analyze index usage
db.messages.aggregate([{ $indexStats: {} }])

# Compact collections (reclaim disk space)
db.runCommand({ compact: 'messages' })
db.runCommand({ compact: 'conversations' })

# Validate collections
db.messages.validate()
db.conversations.validate()
```

### Cleanup Operations

```bash
# Delete soft-deleted messages older than 30 days
db.messages.deleteMany({
  isDeleted: true,
  deletedAt: { $lt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) }
})

# Delete soft-deleted conversations older than 30 days
db.conversations.deleteMany({
  isDeleted: true,
  deletedAt: { $lt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) }
})

# Remove orphaned messages (messages without valid conversation)
const validConversationIds = db.conversations.distinct('_id')
db.messages.deleteMany({
  conversationId: { $nin: validConversationIds }
})
```

## Query Examples

See `Queries/common_queries.js` for comprehensive query patterns including:

- Get user conversations
- Fetch paginated messages
- Count unread messages
- Search message content
- Filter by message type
- Get conversation analytics
- Update read receipts
- Add/remove reactions
- Edit and delete messages

## Performance Optimization

### Index Strategy

All critical queries are covered by indexes:

1. **Conversation Listing** - `participants.userId` + `isDeleted` + `lastMessage.sentAt`
2. **Message Fetching** - `conversationId` + `isDeleted` + `createdAt`
3. **Unread Count** - `conversationId` + `readBy.userId` + `isDeleted`
4. **Text Search** - Full-text index on `content` and `senderUsername`

### Query Performance Tips

1. **Always include isDeleted filter** - Leverages partial indexes
2. **Use projection** - Only fetch needed fields
3. **Limit results** - Use pagination with `.limit()`
4. **Sort on indexed fields** - Combine sort fields with index fields
5. **Avoid $where** - Use native operators instead

### Sharding Recommendations (Production)

For high-scale deployments:

```javascript
// Shard messages by conversationId
sh.enableSharding("wechat_chat")
sh.shardCollection("wechat_chat.messages", { "conversationId": "hashed" })

// Shard conversations by hashed _id
sh.shardCollection("wechat_chat.conversations", { "_id": "hashed" })
```

## Monitoring

### Key Metrics to Monitor

1. **Collection Size**
```javascript
db.messages.stats().size
db.conversations.stats().size
```

2. **Index Size**
```javascript
db.messages.stats().totalIndexSize
db.conversations.stats().totalIndexSize
```

3. **Operation Latency**
```javascript
db.currentOp({ "active": true })
```

4. **Slow Queries**
```javascript
db.setProfilingLevel(1, { slowms: 100 })
db.system.profile.find().sort({ ts: -1 }).limit(10)
```

## Schema Validation

The collections use JSON Schema validation to ensure data integrity. Validation rules are defined in the collection creation scripts.

To update validation rules:

```javascript
db.runCommand({
  collMod: "messages",
  validator: {
    $jsonSchema: {
      // Updated schema here
    }
  }
})
```

## Migration Guide

When updating the schema:

1. Test migration on development database
2. Backup production database
3. Apply migration during low-traffic period
4. Validate data integrity
5. Monitor performance

Example migration script:

```javascript
// Add new field to existing messages
db.messages.updateMany(
  { newField: { $exists: false } },
  { $set: { newField: null } }
)
```

## Troubleshooting

### Common Issues

**Issue: Slow query performance**
- Check if query uses indexes: `db.messages.find(...).explain("executionStats")`
- Ensure indexes exist: `db.messages.getIndexes()`
- Add missing indexes

**Issue: High disk usage**
- Run cleanup operations for soft-deleted records
- Compact collections
- Check for orphaned documents

**Issue: Connection errors**
- Verify MongoDB is running: `systemctl status mongod`
- Check connection string format
- Verify network connectivity and firewall rules
- Check authentication credentials

## Security Best Practices

1. **Authentication** - Always use authentication in production
2. **Authorization** - Implement role-based access control
3. **Encryption** - Use TLS/SSL for connections
4. **Network Security** - Restrict MongoDB port access
5. **Backup** - Regular automated backups
6. **Audit Logging** - Enable audit logs for compliance

## Additional Resources

- [MongoDB Manual](https://docs.mongodb.com/manual/)
- [MongoDB Performance Best Practices](https://docs.mongodb.com/manual/administration/analyzing-mongodb-performance/)
- [MongoDB Security Checklist](https://docs.mongodb.com/manual/administration/security-checklist/)
- [Sharding Guide](https://docs.mongodb.com/manual/sharding/)
