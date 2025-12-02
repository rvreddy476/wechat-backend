# PostFeed API - Complete Request & Response Documentation

> **Purpose**: Complete API documentation with request/response examples for social media posts and feeds
> **Last Updated**: 2025-12-02
> **Base URL**: `https://api.yourapp.com/api/v1`

---

## Table of Contents
1. [Authentication](#authentication)
2. [Posts](#posts)
3. [Comments](#comments)
4. [Likes](#likes)
5. [Shares](#shares)
6. [Hashtags](#hashtags)
7. [Feed](#feed)
8. [Error Responses](#error-responses)

---

## Authentication

All API requests require JWT Bearer authentication.

**Headers Required**:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

---

## Posts

### 1. Create Post

**Endpoint**: `POST /api/v1/posts`

**Description**: Create a new post

**Request**:
```http
POST /api/v1/posts HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "content": "Just launched my new project! 🚀 Check it out #coding #project",
  "mediaUrls": [
    "https://cdn.yourapp.com/posts/image1.jpg",
    "https://cdn.yourapp.com/posts/image2.jpg"
  ],
  "mentions": [
    "660e8400-e29b-41d4-a716-446655440001"
  ],
  "visibility": "Public",
  "location": {
    "name": "San Francisco, CA",
    "latitude": 37.7749,
    "longitude": -122.4194
  }
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Post created successfully",
  "data": {
    "_id": "post-67890abcdef1234567890001",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "displayName": "John Doe",
    "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
    "content": "Just launched my new project! 🚀 Check it out #coding #project",
    "mediaUrls": [
      "https://cdn.yourapp.com/posts/image1.jpg",
      "https://cdn.yourapp.com/posts/image2.jpg"
    ],
    "hashtags": ["coding", "project"],
    "mentions": [
      {
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane_smith",
        "displayName": "Jane Smith"
      }
    ],
    "visibility": "Public",
    "location": {
      "name": "San Francisco, CA",
      "latitude": 37.7749,
      "longitude": -122.4194
    },
    "likesCount": 0,
    "commentsCount": 0,
    "sharesCount": 0,
    "isDeleted": false,
    "createdAt": "2025-12-02T12:00:00Z",
    "updatedAt": "2025-12-02T12:00:00Z"
  }
}
```

---

### 2. Get Post by ID

**Endpoint**: `GET /api/v1/posts/{postId}`

**Description**: Get a specific post with full details

**Request**:
```http
GET /api/v1/posts/post-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "_id": "post-67890abcdef1234567890001",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "displayName": "John Doe",
    "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
    "isVerified": true,
    "content": "Just launched my new project! 🚀 Check it out #coding #project",
    "mediaUrls": [
      "https://cdn.yourapp.com/posts/image1.jpg",
      "https://cdn.yourapp.com/posts/image2.jpg"
    ],
    "hashtags": ["coding", "project"],
    "mentions": [
      {
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane_smith",
        "displayName": "Jane Smith"
      }
    ],
    "visibility": "Public",
    "location": {
      "name": "San Francisco, CA",
      "latitude": 37.7749,
      "longitude": -122.4194
    },
    "likesCount": 234,
    "commentsCount": 42,
    "sharesCount": 18,
    "isLikedByMe": true,
    "isBookmarkedByMe": false,
    "isDeleted": false,
    "createdAt": "2025-12-02T12:00:00Z",
    "updatedAt": "2025-12-02T12:00:00Z"
  }
}
```

---

### 3. Update Post

**Endpoint**: `PUT /api/v1/posts/{postId}`

**Description**: Update an existing post (only content and visibility can be updated)

**Request**:
```http
PUT /api/v1/posts/post-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "content": "Just launched my new project! 🚀 Check it out at https://myproject.com #coding #project #launch",
  "visibility": "Friends"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Post updated successfully",
  "data": {
    "_id": "post-67890abcdef1234567890001",
    "content": "Just launched my new project! 🚀 Check it out at https://myproject.com #coding #project #launch",
    "hashtags": ["coding", "project", "launch"],
    "visibility": "Friends",
    "updatedAt": "2025-12-02T12:30:00Z"
  }
}
```

---

### 4. Delete Post

**Endpoint**: `DELETE /api/v1/posts/{postId}`

**Description**: Delete a post (soft delete)

**Request**:
```http
DELETE /api/v1/posts/post-67890abcdef1234567890001 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Post deleted successfully",
  "data": {
    "postId": "post-67890abcdef1234567890001",
    "deletedAt": "2025-12-02T13:00:00Z"
  }
}
```

---

### 5. Get User Posts

**Endpoint**: `GET /api/v1/users/{userId}/posts`

**Description**: Get all posts from a specific user

**Request**:
```http
GET /api/v1/users/550e8400-e29b-41d4-a716-446655440000/posts?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "posts": [
      {
        "_id": "post-67890abcdef1234567890001",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "displayName": "John Doe",
        "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
        "content": "Just launched my new project! 🚀",
        "mediaUrls": ["https://cdn.yourapp.com/posts/image1.jpg"],
        "likesCount": 234,
        "commentsCount": 42,
        "sharesCount": 18,
        "isLikedByMe": true,
        "createdAt": "2025-12-02T12:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 8,
      "totalCount": 156,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

## Comments

### 6. Get Comments

**Endpoint**: `GET /api/v1/posts/{postId}/comments`

**Description**: Get all comments for a post

**Request**:
```http
GET /api/v1/posts/post-67890abcdef1234567890001/comments?page=1&limit=20&sortBy=recent HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters**:
- `sortBy`: `recent` or `popular` (default: recent)

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "comments": [
      {
        "_id": "comment-67890abcdef1234567890001",
        "postId": "post-67890abcdef1234567890001",
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane_smith",
        "displayName": "Jane Smith",
        "avatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
        "isVerified": true,
        "content": "Congratulations! This looks amazing! 🎉",
        "likesCount": 15,
        "repliesCount": 3,
        "isLikedByMe": false,
        "parentCommentId": null,
        "isDeleted": false,
        "createdAt": "2025-12-02T12:05:00Z",
        "updatedAt": "2025-12-02T12:05:00Z"
      },
      {
        "_id": "comment-67890abcdef1234567890002",
        "postId": "post-67890abcdef1234567890001",
        "userId": "770e8400-e29b-41d4-a716-446655440001",
        "username": "alice_johnson",
        "displayName": "Alice Johnson",
        "avatarUrl": "https://cdn.yourapp.com/avatars/alice_johnson.jpg",
        "isVerified": false,
        "content": "Great work! Can't wait to try it out.",
        "likesCount": 8,
        "repliesCount": 0,
        "isLikedByMe": true,
        "parentCommentId": null,
        "isDeleted": false,
        "createdAt": "2025-12-02T12:10:00Z",
        "updatedAt": "2025-12-02T12:10:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 3,
      "totalCount": 42,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### 7. Add Comment

**Endpoint**: `POST /api/v1/posts/{postId}/comments`

**Description**: Add a comment to a post

**Request**:
```http
POST /api/v1/posts/post-67890abcdef1234567890001/comments HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "content": "This is amazing! Congratulations! 🎉",
  "mentions": [
    "550e8400-e29b-41d4-a716-446655440000"
  ]
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Comment added successfully",
  "data": {
    "_id": "comment-67890abcdef1234567890010",
    "postId": "post-67890abcdef1234567890001",
    "userId": "660e8400-e29b-41d4-a716-446655440001",
    "username": "jane_smith",
    "displayName": "Jane Smith",
    "avatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
    "content": "This is amazing! Congratulations! 🎉",
    "mentions": [
      {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "displayName": "John Doe"
      }
    ],
    "likesCount": 0,
    "repliesCount": 0,
    "isLikedByMe": false,
    "parentCommentId": null,
    "createdAt": "2025-12-02T13:00:00Z"
  }
}
```

---

### 8. Reply to Comment

**Endpoint**: `POST /api/v1/posts/{postId}/comments`

**Description**: Reply to an existing comment

**Request**:
```http
POST /api/v1/posts/post-67890abcdef1234567890001/comments HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "content": "Thank you so much! @jane_smith",
  "parentCommentId": "comment-67890abcdef1234567890001",
  "mentions": [
    "660e8400-e29b-41d4-a716-446655440001"
  ]
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Reply added successfully",
  "data": {
    "_id": "comment-67890abcdef1234567890011",
    "postId": "post-67890abcdef1234567890001",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "displayName": "John Doe",
    "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
    "content": "Thank you so much! @jane_smith",
    "parentCommentId": "comment-67890abcdef1234567890001",
    "mentions": [
      {
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane_smith",
        "displayName": "Jane Smith"
      }
    ],
    "likesCount": 0,
    "repliesCount": 0,
    "createdAt": "2025-12-02T13:05:00Z"
  }
}
```

---

### 9. Get Comment Replies

**Endpoint**: `GET /api/v1/comments/{commentId}/replies`

**Description**: Get all replies to a specific comment

**Request**:
```http
GET /api/v1/comments/comment-67890abcdef1234567890001/replies?page=1&limit=10 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "replies": [
      {
        "_id": "comment-67890abcdef1234567890011",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "displayName": "John Doe",
        "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
        "content": "Thank you so much! @jane_smith",
        "likesCount": 2,
        "createdAt": "2025-12-02T13:05:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 10,
      "totalCount": 3,
      "hasNextPage": false
    }
  }
}
```

---

### 10. Update Comment

**Endpoint**: `PUT /api/v1/comments/{commentId}`

**Description**: Edit a comment

**Request**:
```http
PUT /api/v1/comments/comment-67890abcdef1234567890010 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "content": "This is amazing! Congratulations on the launch! 🎉🚀"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Comment updated successfully",
  "data": {
    "_id": "comment-67890abcdef1234567890010",
    "content": "This is amazing! Congratulations on the launch! 🎉🚀",
    "updatedAt": "2025-12-02T13:10:00Z"
  }
}
```

---

### 11. Delete Comment

**Endpoint**: `DELETE /api/v1/comments/{commentId}`

**Description**: Delete a comment

**Request**:
```http
DELETE /api/v1/comments/comment-67890abcdef1234567890010 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Comment deleted successfully",
  "data": {
    "commentId": "comment-67890abcdef1234567890010",
    "deletedAt": "2025-12-02T13:15:00Z"
  }
}
```

---

## Likes

### 12. Like Post

**Endpoint**: `POST /api/v1/posts/{postId}/like`

**Description**: Like a post

**Request**:
```http
POST /api/v1/posts/post-67890abcdef1234567890001/like HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Post liked successfully",
  "data": {
    "postId": "post-67890abcdef1234567890001",
    "likesCount": 235,
    "isLiked": true,
    "likedAt": "2025-12-02T13:20:00Z"
  }
}
```

**Error Response** (400 Bad Request - Already Liked):
```json
{
  "success": false,
  "error": {
    "code": "ALREADY_LIKED",
    "message": "You have already liked this post",
    "statusCode": 400
  }
}
```

---

### 13. Unlike Post

**Endpoint**: `DELETE /api/v1/posts/{postId}/like`

**Description**: Remove like from a post

**Request**:
```http
DELETE /api/v1/posts/post-67890abcdef1234567890001/like HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Like removed successfully",
  "data": {
    "postId": "post-67890abcdef1234567890001",
    "likesCount": 234,
    "isLiked": false
  }
}
```

---

### 14. Get Post Likes

**Endpoint**: `GET /api/v1/posts/{postId}/likes`

**Description**: Get list of users who liked a post

**Request**:
```http
GET /api/v1/posts/post-67890abcdef1234567890001/likes?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "likes": [
      {
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane_smith",
        "displayName": "Jane Smith",
        "avatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
        "isVerified": true,
        "likedAt": "2025-12-02T12:05:00Z"
      },
      {
        "userId": "770e8400-e29b-41d4-a716-446655440001",
        "username": "alice_johnson",
        "displayName": "Alice Johnson",
        "avatarUrl": "https://cdn.yourapp.com/avatars/alice_johnson.jpg",
        "isVerified": false,
        "likedAt": "2025-12-02T12:10:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 12,
      "totalCount": 234,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### 15. Like Comment

**Endpoint**: `POST /api/v1/comments/{commentId}/like`

**Description**: Like a comment

**Request**:
```http
POST /api/v1/comments/comment-67890abcdef1234567890001/like HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Comment liked successfully",
  "data": {
    "commentId": "comment-67890abcdef1234567890001",
    "likesCount": 16,
    "isLiked": true,
    "likedAt": "2025-12-02T13:25:00Z"
  }
}
```

---

### 16. Unlike Comment

**Endpoint**: `DELETE /api/v1/comments/{commentId}/like`

**Description**: Remove like from a comment

**Request**:
```http
DELETE /api/v1/comments/comment-67890abcdef1234567890001/like HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Like removed successfully",
  "data": {
    "commentId": "comment-67890abcdef1234567890001",
    "likesCount": 15,
    "isLiked": false
  }
}
```

---

## Shares

### 17. Share Post

**Endpoint**: `POST /api/v1/posts/{postId}/share`

**Description**: Share a post to your timeline

**Request**:
```http
POST /api/v1/posts/post-67890abcdef1234567890001/share HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "caption": "This is really cool! Check it out 👇",
  "visibility": "Public"
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Post shared successfully",
  "data": {
    "_id": "share-67890abcdef1234567890001",
    "userId": "660e8400-e29b-41d4-a716-446655440001",
    "username": "jane_smith",
    "originalPostId": "post-67890abcdef1234567890001",
    "caption": "This is really cool! Check it out 👇",
    "visibility": "Public",
    "originalPost": {
      "_id": "post-67890abcdef1234567890001",
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "displayName": "John Doe",
      "content": "Just launched my new project! 🚀",
      "mediaUrls": ["https://cdn.yourapp.com/posts/image1.jpg"]
    },
    "sharedAt": "2025-12-02T13:30:00Z"
  }
}
```

---

### 18. Get Post Shares

**Endpoint**: `GET /api/v1/posts/{postId}/shares`

**Description**: Get list of users who shared a post

**Request**:
```http
GET /api/v1/posts/post-67890abcdef1234567890001/shares?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "shares": [
      {
        "_id": "share-67890abcdef1234567890001",
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane_smith",
        "displayName": "Jane Smith",
        "avatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
        "caption": "This is really cool! Check it out 👇",
        "sharedAt": "2025-12-02T13:30:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 1,
      "totalCount": 18,
      "hasNextPage": false
    }
  }
}
```

---

## Hashtags

### 19. Get Trending Hashtags

**Endpoint**: `GET /api/v1/hashtags/trending`

**Description**: Get currently trending hashtags

**Request**:
```http
GET /api/v1/hashtags/trending?limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "hashtags": [
      {
        "tag": "coding",
        "count": 1234,
        "trendScore": 98.5,
        "lastUsed": "2025-12-02T13:35:00Z"
      },
      {
        "tag": "ai",
        "count": 987,
        "trendScore": 95.2,
        "lastUsed": "2025-12-02T13:30:00Z"
      },
      {
        "tag": "technology",
        "count": 856,
        "trendScore": 92.8,
        "lastUsed": "2025-12-02T13:32:00Z"
      }
    ],
    "count": 20
  }
}
```

---

### 20. Get Posts by Hashtag

**Endpoint**: `GET /api/v1/hashtags/{tag}/posts`

**Description**: Get all posts with a specific hashtag

**Request**:
```http
GET /api/v1/hashtags/coding/posts?page=1&limit=20&sortBy=recent HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters**:
- `sortBy`: `recent` or `popular` (default: recent)

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "hashtag": "coding",
    "posts": [
      {
        "_id": "post-67890abcdef1234567890001",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "john_doe",
        "displayName": "John Doe",
        "avatarUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
        "content": "Just launched my new project! 🚀 #coding #project",
        "likesCount": 234,
        "commentsCount": 42,
        "sharesCount": 18,
        "createdAt": "2025-12-02T12:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 62,
      "totalCount": 1234,
      "hasNextPage": true
    }
  }
}
```

---

## Feed

### 21. Get Public Feed

**Endpoint**: `GET /api/v1/feed/public`

**Description**: Get global public feed

**Request**:
```http
GET /api/v1/feed/public?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "posts": [
      {
        "_id": "post-67890abcdef1234567890005",
        "userId": "880e8400-e29b-41d4-a716-446655440001",
        "username": "bob_wilson",
        "displayName": "Bob Wilson",
        "avatarUrl": "https://cdn.yourapp.com/avatars/bob_wilson.jpg",
        "isVerified": true,
        "content": "Amazing sunset today! 🌅 #nature #photography",
        "mediaUrls": ["https://cdn.yourapp.com/posts/sunset.jpg"],
        "likesCount": 567,
        "commentsCount": 89,
        "sharesCount": 34,
        "isLikedByMe": false,
        "createdAt": "2025-12-02T18:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "hasNextPage": true
    }
  }
}
```

---

### 22. Get Personalized Feed

**Endpoint**: `GET /api/v1/feed`

**Description**: Get personalized feed based on following and interests

**Request**:
```http
GET /api/v1/feed?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "posts": [
      {
        "_id": "post-67890abcdef1234567890001",
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "jane_smith",
        "displayName": "Jane Smith",
        "avatarUrl": "https://cdn.yourapp.com/avatars/jane_smith.jpg",
        "isVerified": true,
        "isFriend": true,
        "content": "Working on something exciting! Stay tuned 🚀",
        "likesCount": 123,
        "commentsCount": 28,
        "sharesCount": 5,
        "isLikedByMe": true,
        "createdAt": "2025-12-02T14:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "hasNextPage": true
    }
  }
}
```

---

### 23. Get Trending Posts

**Endpoint**: `GET /api/v1/feed/trending`

**Description**: Get trending posts based on engagement

**Request**:
```http
GET /api/v1/feed/trending?timeframe=24h&page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Query Parameters**:
- `timeframe`: `1h`, `6h`, `24h`, `7d` (default: 24h)

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "timeframe": "24h",
    "posts": [
      {
        "_id": "post-67890abcdef1234567890010",
        "userId": "990e8400-e29b-41d4-a716-446655440001",
        "username": "tech_news",
        "displayName": "Tech News Daily",
        "avatarUrl": "https://cdn.yourapp.com/avatars/tech_news.jpg",
        "isVerified": true,
        "content": "Breaking: Major tech announcement! #tech #news",
        "likesCount": 5678,
        "commentsCount": 892,
        "sharesCount": 1234,
        "trendScore": 99.8,
        "createdAt": "2025-12-02T10:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "hasNextPage": true
    }
  }
}
```

---

## Error Responses

### Standard Error Format

```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "statusCode": 400
  }
}
```

### Common Error Codes

**Post Not Found (404)**:
```json
{
  "success": false,
  "error": {
    "code": "POST_NOT_FOUND",
    "message": "Post not found",
    "statusCode": 404
  }
}
```

**Already Liked (400)**:
```json
{
  "success": false,
  "error": {
    "code": "ALREADY_LIKED",
    "message": "You have already liked this post",
    "statusCode": 400
  }
}
```

**Content Too Long (400)**:
```json
{
  "success": false,
  "error": {
    "code": "CONTENT_TOO_LONG",
    "message": "Post content exceeds maximum length of 5000 characters",
    "statusCode": 400
  }
}
```

---

**End of Documentation**
