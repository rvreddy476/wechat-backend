# Notification Service Database Documentation

## Overview

The Notification service uses **MongoDB** for storing user notifications with automatic expiry.

## Collections

### notifications
Stores all user notifications with TTL support.

**Key Fields:**
- `userId` - Recipient user ID
- `type` - FriendRequest, Message, Like, Comment, Mention, Follow, System
- `title` - Notification title
- `message` - Notification message
- `isRead` - Read status
- `priority` - Low, Normal, High
- `expiresAt` - Auto-delete timestamp (TTL index)

**Features:**
- TTL index auto-deletes expired notifications
- Supports rich notifications with actionUrl
- Tracks sender information (fromUserId, fromUsername, fromUserAvatar)

## Connection String

```bash
mongodb://localhost:27017/wechat_notification
```

## Setup

```bash
mongosh
use wechat_notification
load('Collections/01_notifications.js')
load('Indexes/01_notification_indexes.js')
```

## Common Operations

```bash
# Backup
mongodump --db=wechat_notification --gzip --archive=/backup/notification_$(date +%Y%m%d).gz

# Restore
mongorestore --gzip --archive=/backup/notification_20240115.gz

# Mark all as read for user
db.notifications.updateMany(
  { userId: "user123", isRead: false },
  { $set: { isRead: true, readAt: new Date() } }
)

# Delete old read notifications (optional, TTL handles this)
db.notifications.deleteMany({
  isRead: true,
  createdAt: { $lt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) }
})
```
