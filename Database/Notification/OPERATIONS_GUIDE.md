# Notification Service - Database Operations Guide

## Collection: notifications

### Create Notification

```javascript
const notification = {
  _id: `notif-${Date.now()}-${generateId()}`,
  userId: "550e8400-e29b-41d4-a716-446655440000",  // Recipient
  type: "FriendRequest",  // FriendRequest, Message, Like, Comment, Mention, Follow, System
  title: "New Friend Request",
  message: "john_doe sent you a friend request",
  actionUrl: "/friend-requests/freq-12345",
  relatedEntityId: "freq-12345",
  relatedEntityType: "FriendRequest",
  fromUserId: "550e8400-e29b-41d4-a716-446655440001",
  fromUsername: "john_doe",
  fromUserAvatar: "https://cdn.example.com/avatars/john.jpg",
  isRead: false,
  readAt: null,
  priority: "Normal",  // Low, Normal, High
  expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000),  // 30 days (auto-delete)
  createdAt: new Date()
};

db.notifications.insertOne(notification);
```

---

### Notification Types

**Friend Request**:
```javascript
{
  type: "FriendRequest",
  title: "New Friend Request",
  message: "john_doe sent you a friend request",
  actionUrl: "/friend-requests"
}
```

**New Message**:
```javascript
{
  type: "Message",
  title: "New Message",
  message: "john_doe sent you a message",
  actionUrl: "/chat/conv-12345"
}
```

**Post Like**:
```javascript
{
  type: "Like",
  title: "New Like",
  message: "john_doe liked your post",
  actionUrl: "/posts/post-12345"
}
```

**Comment**:
```javascript
{
  type: "Comment",
  title: "New Comment",
  message: "john_doe commented on your post",
  actionUrl: "/posts/post-12345#comment-456"
}
```

**Mention**:
```javascript
{
  type: "Mention",
  title: "You were mentioned",
  message: "john_doe mentioned you in a post",
  actionUrl: "/posts/post-12345"
}
```

**New Follower**:
```javascript
{
  type: "Follow",
  title: "New Follower",
  message: "john_doe started following you",
  actionUrl: "/profile/john_doe"
}
```

**System Announcement**:
```javascript
{
  type: "System",
  title: "System Maintenance",
  message: "Platform will be under maintenance on Sunday",
  actionUrl: null,
  fromUserId: null,
  priority: "High"
}
```

---

### Get Unread Notifications

```javascript
db.notifications.find({
  userId: "550e8400-e29b-41d4-a716-446655440000",
  isRead: false
}).sort({ createdAt: -1 }).limit(50);
```

---

### Get All Notifications (Paginated)

```javascript
db.notifications.find({
  userId: "550e8400-e29b-41d4-a716-446655440000"
}).sort({ createdAt: -1 }).limit(20);
```

---

### Mark as Read

**Single Notification**:
```javascript
db.notifications.updateOne(
  { _id: "notif-12345" },
  {
    $set: {
      isRead: true,
      readAt: new Date()
    }
  }
);
```

**Mark All as Read**:
```javascript
db.notifications.updateMany(
  {
    userId: "550e8400-e29b-41d4-a716-446655440000",
    isRead: false
  },
  {
    $set: {
      isRead: true,
      readAt: new Date()
    }
  }
);
```

---

### Get Unread Count

```javascript
db.notifications.countDocuments({
  userId: "550e8400-e29b-41d4-a716-446655440000",
  isRead: false
});
```

---

### Filter by Type

```javascript
// Get only friend request notifications
db.notifications.find({
  userId: "550e8400-e29b-41d4-a716-446655440000",
  type: "FriendRequest"
}).sort({ createdAt: -1 });
```

---

### Delete Notification

```javascript
db.notifications.deleteOne({
  _id: "notif-12345",
  userId: "550e8400-e29b-41d4-a716-446655440000"
});
```

---

### Get High Priority Notifications

```javascript
db.notifications.find({
  userId: "550e8400-e29b-41d4-a716-446655440000",
  priority: "High",
  isRead: false
}).sort({ createdAt: -1 });
```

---

## Auto-Expiry (TTL)

Notifications automatically expire and get deleted based on `expiresAt` field.

**Set Expiration**:
```javascript
// Expire in 30 days
expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000)

// Expire in 7 days
expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000)

// Never expire
expiresAt: null
```

---

## Best Practices

1. **Set Expiration**: Always set `expiresAt` for auto-cleanup
2. **Check Preferences**: Respect user notification settings from UserProfile
3. **Batch Reads**: Use `updateMany` to mark multiple notifications as read
4. **Real-Time**: Send push notifications via SignalR when creating notification
5. **Deduplicate**: Don't send duplicate notifications for same action
6. **Priority**: Use priority for important system notifications
7. **Action URLs**: Always provide actionUrl for navigation

---
