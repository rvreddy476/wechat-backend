# WeChat Social Media Platform - API Design Document

## Table of Contents

1. [Overview](#overview)
2. [Authentication & Authorization](#authentication--authorization)
3. [Common Response Format](#common-response-format)
4. [Services](#services)
   - [Auth Service](#1-auth-service)
   - [UserProfile Service](#2-userprofile-service)
   - [PostFeed Service](#3-postfeed-service)
   - [Chat Service](#4-chat-service)
   - [Video Service](#5-video-service)
   - [Media Service](#6-media-service)
   - [Notification Service](#7-notification-service)
   - [Realtime Service](#8-realtime-service)
5. [Configuration Requirements](#configuration-requirements)
6. [Error Codes](#error-codes)

---

## Overview

### Architecture

**Microservices Architecture** with API Gateway pattern
- **Gateway**: Routes requests to appropriate services
- **Services**: Independent, scalable microservices
- **Infrastructure**: PostgreSQL, MongoDB, Redis, GCP Storage

### Base URLs

| Environment | Gateway URL | Direct Service Access |
|-------------|-------------|----------------------|
| Development | `http://localhost:5000` | `http://localhost:500X` |
| Production | `https://api.wechat.com` | Internal network only |

### Authentication

All protected endpoints require JWT Bearer token in header:
```
Authorization: Bearer <access_token>
```

---

## Authentication & Authorization

### Token Types

1. **Access Token**: Short-lived (15 minutes), for API access
2. **Refresh Token**: Long-lived (7 days), for obtaining new access tokens

### Token Refresh Flow

```
Client has expired access token
  ↓
POST /api/auth/refresh with refresh_token
  ↓
Receive new access_token and refresh_token
  ↓
Use new access_token for API calls
```

---

## Common Response Format

### Success Response

```json
{
  "success": true,
  "data": <response_data>,
  "error": null,
  "errors": null,
  "timestamp": "2025-01-26T10:30:00Z"
}
```

### Error Response

```json
{
  "success": false,
  "data": null,
  "error": "Error message",
  "errors": ["Detail 1", "Detail 2"],
  "timestamp": "2025-01-26T10:30:00Z"
}
```

---

## 1. Auth Service

**Base URL**: `/api/auth`
**Port**: 5001
**Database**: PostgreSQL

### Endpoints

#### 1.1 Register New User

**POST** `/api/auth/register`

**Description**: Register a new user account

**Request Body**:
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "phoneNumber": "+1234567890"
}
```

**Validation**:
- `username`: 3-20 characters, alphanumeric + underscore only
- `email`: Valid email format
- `password`: Min 8 characters, must contain uppercase, lowercase, number, special char
- `phoneNumber`: Optional, E.164 format

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "email": "john@example.com",
    "message": "Registration successful. Please verify your email."
  }
}
```

**Errors**:
- `400`: Validation failed (weak password, invalid email, etc.)
- `409`: Email or username already exists

---

#### 1.2 Login

**POST** `/api/auth/login`

**Description**: Authenticate user and receive tokens

**Request Body**:
```json
{
  "emailOrUsername": "john@example.com",
  "password": "SecurePass123!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "email": "john@example.com",
    "emailVerified": false,
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "dGhpc2lzYXJlZnJlc2h0b2tlbg...",
    "expiresAt": "2025-01-26T11:00:00Z",
    "roles": ["User"]
  }
}
```

**Errors**:
- `401`: Invalid credentials
- `403`: Account locked (too many failed attempts)
- `403`: Email not verified

---

#### 1.3 Refresh Token

**POST** `/api/auth/refresh`

**Description**: Get new access token using refresh token

**Request Body**:
```json
{
  "refreshToken": "dGhpc2lzYXJlZnJlc2h0b2tlbg..."
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "bmV3cmVmcmVzaHRva2Vu...",
    "expiresAt": "2025-01-26T11:15:00Z"
  }
}
```

**Errors**:
- `401`: Invalid or expired refresh token
- `401`: Token has been revoked

---

#### 1.4 Logout

**POST** `/api/auth/logout`
**Auth**: Required

**Description**: Revoke all user tokens

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

#### 1.5 Get Current User

**GET** `/api/auth/me`
**Auth**: Required

**Description**: Get current authenticated user details

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "isEmailVerified": false,
    "isPhoneVerified": false,
    "isActive": true,
    "bio": null,
    "avatarUrl": null,
    "createdAt": "2025-01-20T10:00:00Z",
    "updatedAt": "2025-01-26T10:30:00Z",
    "lastLoginAt": "2025-01-26T10:30:00Z",
    "roles": ["User"]
  }
}
```

---

#### 1.6 Change Password

**POST** `/api/auth/change-password`
**Auth**: Required

**Request Body**:
```json
{
  "oldPassword": "OldPass123!",
  "newPassword": "NewSecurePass456!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

**Errors**:
- `400`: Old password incorrect
- `400`: New password doesn't meet requirements

---

#### 1.7 Forgot Password

**POST** `/api/auth/forgot-password`

**Description**: Request password reset email

**Request Body**:
```json
{
  "email": "john@example.com"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "message": "Password reset email sent"
  }
}
```

---

#### 1.8 Reset Password

**POST** `/api/auth/reset-password`

**Description**: Reset password using token from email

**Request Body**:
```json
{
  "token": "reset-token-from-email",
  "newPassword": "NewSecurePass456!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

**Errors**:
- `400`: Invalid or expired token
- `400`: Password doesn't meet requirements

---

#### 1.9 Verify Email

**POST** `/api/auth/verify-email`

**Description**: Verify email address using token

**Request Body**:
```json
{
  "token": "verification-token-from-email"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

**Errors**:
- `400`: Invalid or expired token
- `409`: Email already verified

---

### Auth Service Configuration

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=wechat_auth;Username=wechat_admin;Password=***"
  },
  "JWT": {
    "Secret": "your-super-secret-jwt-key-min-32-characters",
    "Issuer": "WeChat.AuthService",
    "Audience": "WeChat.Client",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**Environment Variables**:
- `ConnectionStrings__PostgreSQL`: PostgreSQL connection string
- `JWT__Secret`: JWT signing secret
- `JWT__Issuer`: Token issuer
- `JWT__Audience`: Token audience
- `JWT__ExpirationMinutes`: Access token lifetime (default: 15)
- `JWT__RefreshTokenExpirationDays`: Refresh token lifetime (default: 7)

---

## 2. UserProfile Service

**Base URL**: `/api/profiles`
**Port**: 5002
**Database**: MongoDB (`wechat_userprofiles`)

### Endpoints

#### 2.1 Get User Profile

**GET** `/api/profiles/{userId}`
**Auth**: Required

**Description**: Get user profile by ID

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "email": "john@example.com",
    "displayName": "John Doe",
    "bio": "Software developer and tech enthusiast",
    "avatarUrl": "https://storage.googleapis.com/wechat-media/avatars/user123.jpg",
    "coverPhotoUrl": "https://storage.googleapis.com/wechat-media/covers/user123.jpg",
    "location": "San Francisco, CA",
    "website": "https://johndoe.com",
    "birthDate": "1990-01-15",
    "gender": "Male",
    "isPrivate": false,
    "isVerified": false,
    "stats": {
      "postsCount": 42,
      "followersCount": 1250,
      "followingCount": 350,
      "videosCount": 15
    },
    "createdAt": "2025-01-20T10:00:00Z",
    "updatedAt": "2025-01-26T10:30:00Z"
  }
}
```

**Errors**:
- `404`: User not found
- `403`: Profile is private and you're not following

---

#### 2.2 Update Profile

**PUT** `/api/profiles`
**Auth**: Required

**Description**: Update current user's profile

**Request Body**:
```json
{
  "displayName": "John Doe",
  "bio": "Software developer and tech enthusiast",
  "location": "San Francisco, CA",
  "website": "https://johndoe.com",
  "birthDate": "1990-01-15",
  "gender": "Male",
  "isPrivate": false
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "displayName": "John Doe",
    "bio": "Software developer and tech enthusiast",
    ...
  }
}
```

---

#### 2.3 Upload Avatar

**POST** `/api/profiles/avatar`
**Auth**: Required
**Content-Type**: `multipart/form-data`

**Request**:
```
file: <image file> (max 5MB, jpg/png/gif)
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "avatarUrl": "https://storage.googleapis.com/wechat-media/avatars/user123_v2.jpg"
  }
}
```

---

#### 2.4 Follow User

**POST** `/api/profiles/{userId}/follow`
**Auth**: Required

**Description**: Follow another user

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "followerId": "550e8400-e29b-41d4-a716-446655440000",
    "followingId": "660e8400-e29b-41d4-a716-446655440001",
    "followedAt": "2025-01-26T10:30:00Z"
  }
}
```

**Errors**:
- `400`: Cannot follow yourself
- `404`: User not found
- `409`: Already following

---

#### 2.5 Unfollow User

**DELETE** `/api/profiles/{userId}/follow`
**Auth**: Required

**Description**: Unfollow a user

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

#### 2.6 Get Followers

**GET** `/api/profiles/{userId}/followers?page=1&pageSize=20`
**Auth**: Required

**Description**: Get list of user's followers

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "userId": "770e8400-e29b-41d4-a716-446655440002",
        "username": "janedoe",
        "displayName": "Jane Doe",
        "avatarUrl": "https://...",
        "isVerified": false,
        "followedAt": "2025-01-25T15:20:00Z"
      }
    ],
    "totalCount": 1250,
    "page": 1,
    "pageSize": 20,
    "totalPages": 63
  }
}
```

---

#### 2.7 Get Following

**GET** `/api/profiles/{userId}/following?page=1&pageSize=20`
**Auth**: Required

**Description**: Get list of users this user follows

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 350,
    "page": 1,
    "pageSize": 20,
    "totalPages": 18
  }
}
```

---

#### 2.8 Block User

**POST** `/api/profiles/{userId}/block`
**Auth**: Required

**Description**: Block a user

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

#### 2.9 Unblock User

**DELETE** `/api/profiles/{userId}/block`
**Auth**: Required

**Description**: Unblock a user

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

#### 2.10 Search Users

**GET** `/api/profiles/search?query=john&page=1&pageSize=20`
**Auth**: Required

**Description**: Search users by username or display name

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "johndoe",
        "displayName": "John Doe",
        "avatarUrl": "https://...",
        "bio": "Software developer",
        "isVerified": false,
        "followersCount": 1250
      }
    ],
    "totalCount": 15,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  }
}
```

---

#### 2.8 Friend Request Management

##### 2.8.1 Send Friend Request

**POST** `/api/friendrequest/send/{userId}`
**Auth**: Required

**Description**: Send a friend request to another user

**Request Body** (optional):
```json
{
  "message": "Hi! I'd like to connect with you."
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "507f1f77bcf86cd799439011",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "senderUsername": "johndoe",
    "senderDisplayName": "John Doe",
    "senderAvatarUrl": "https://...",
    "senderIsVerified": false,
    "receiverId": "550e8400-e29b-41d4-a716-446655440001",
    "receiverUsername": "janedoe",
    "receiverDisplayName": "Jane Doe",
    "receiverAvatarUrl": "https://...",
    "receiverIsVerified": true,
    "status": "Pending",
    "message": "Hi! I'd like to connect with you.",
    "createdAt": "2025-01-26T10:30:00Z",
    "respondedAt": null
  }
}
```

**Errors**:
- `400`: Cannot send request to yourself
- `400`: User is blocked
- `400`: Already friends
- `400`: Friend request already exists
- `404`: User not found

---

##### 2.8.2 Accept Friend Request

**POST** `/api/friendrequest/{requestId}/accept`
**Auth**: Required

**Description**: Accept a pending friend request

**Response** (200 OK):
```json
{
  "success": true,
  "data": true,
  "message": "Friend request accepted"
}
```

**Errors**:
- `400`: Friend request not found
- `403`: Not authorized to accept this request

---

##### 2.8.3 Reject Friend Request

**POST** `/api/friendrequest/{requestId}/reject`
**Auth**: Required

**Description**: Reject a pending friend request

**Response** (200 OK):
```json
{
  "success": true,
  "data": true,
  "message": "Friend request rejected"
}
```

**Errors**:
- `400`: Friend request not found
- `403`: Not authorized to reject this request

---

##### 2.8.4 Cancel Friend Request

**DELETE** `/api/friendrequest/{requestId}/cancel`
**Auth**: Required

**Description**: Cancel a friend request you sent

**Response** (200 OK):
```json
{
  "success": true,
  "data": true,
  "message": "Friend request cancelled"
}
```

**Errors**:
- `400`: Friend request not found
- `403`: Not authorized to cancel this request

---

##### 2.8.5 Get Received Friend Requests

**GET** `/api/friendrequest/received?page=1&pageSize=20`
**Auth**: Required

**Description**: Get all pending friend requests received (incoming)

**Query Parameters**:
- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 20): Items per page

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": "507f1f77bcf86cd799439011",
      "senderId": "550e8400-e29b-41d4-a716-446655440000",
      "senderUsername": "johndoe",
      "senderDisplayName": "John Doe",
      "senderAvatarUrl": "https://...",
      "senderIsVerified": false,
      "receiverId": "550e8400-e29b-41d4-a716-446655440001",
      "receiverUsername": "janedoe",
      "receiverDisplayName": "Jane Doe",
      "receiverAvatarUrl": "https://...",
      "receiverIsVerified": true,
      "status": "Pending",
      "message": "Hi! I'd like to connect.",
      "createdAt": "2025-01-26T10:30:00Z",
      "respondedAt": null
    }
  ]
}
```

---

##### 2.8.6 Get Sent Friend Requests

**GET** `/api/friendrequest/sent?page=1&pageSize=20`
**Auth**: Required

**Description**: Get all pending friend requests sent (outgoing)

**Query Parameters**:
- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 20): Items per page

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": "507f1f77bcf86cd799439011",
      "senderId": "550e8400-e29b-41d4-a716-446655440001",
      "senderUsername": "janedoe",
      "senderDisplayName": "Jane Doe",
      "senderAvatarUrl": "https://...",
      "senderIsVerified": true,
      "receiverId": "550e8400-e29b-41d4-a716-446655440000",
      "receiverUsername": "johndoe",
      "receiverDisplayName": "John Doe",
      "receiverAvatarUrl": "https://...",
      "receiverIsVerified": false,
      "status": "Pending",
      "message": null,
      "createdAt": "2025-01-26T09:00:00Z",
      "respondedAt": null
    }
  ]
}
```

---

##### 2.8.7 Get Friend Request by ID

**GET** `/api/friendrequest/{requestId}`
**Auth**: Required

**Description**: Get details of a specific friend request

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "507f1f77bcf86cd799439011",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "senderUsername": "johndoe",
    "senderDisplayName": "John Doe",
    "senderAvatarUrl": "https://...",
    "senderIsVerified": false,
    "receiverId": "550e8400-e29b-41d4-a716-446655440001",
    "receiverUsername": "janedoe",
    "receiverDisplayName": "Jane Doe",
    "receiverAvatarUrl": "https://...",
    "receiverIsVerified": true,
    "status": "Pending",
    "message": "Hi! I'd like to connect.",
    "createdAt": "2025-01-26T10:30:00Z",
    "respondedAt": null
  }
}
```

**Errors**:
- `404`: Friend request not found

---

##### 2.8.8 Check Friendship Status

**GET** `/api/friendrequest/status/{userId}`
**Auth**: Required

**Description**: Check friendship status with another user

**Response** (200 OK) - When friends:
```json
{
  "success": true,
  "data": {
    "status": "friends",
    "areFriends": true,
    "hasPendingRequest": false
  }
}
```

**Response** (200 OK) - When request sent:
```json
{
  "success": true,
  "data": {
    "status": "request_sent",
    "areFriends": false,
    "hasPendingRequest": true,
    "requestDirection": "outgoing"
  }
}
```

**Response** (200 OK) - When request received:
```json
{
  "success": true,
  "data": {
    "status": "request_received",
    "areFriends": false,
    "hasPendingRequest": true,
    "requestDirection": "incoming"
  }
}
```

**Response** (200 OK) - When not friends:
```json
{
  "success": true,
  "data": {
    "status": "not_friends",
    "areFriends": false,
    "hasPendingRequest": false
  }
}
```

---

##### 2.8.9 Get Friends List

**GET** `/api/friendrequest/friends?page=1&pageSize=20`
**Auth**: Required

**Description**: Get list of your friends

**Query Parameters**:
- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 20): Items per page

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "username": "johndoe",
      "displayName": "John Doe",
      "avatarUrl": "https://...",
      "isVerified": false,
      "bio": "Software developer",
      "friendshipDate": "2025-01-20T15:30:00Z"
    }
  ]
}
```

---

##### 2.8.10 Get User's Friends

**GET** `/api/friendrequest/friends/{userId}?page=1&pageSize=20`
**Auth**: Optional

**Description**: Get list of another user's friends

**Query Parameters**:
- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 20): Items per page

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "userId": "550e8400-e29b-41d4-a716-446655440002",
      "username": "bobsmith",
      "displayName": "Bob Smith",
      "avatarUrl": "https://...",
      "isVerified": true,
      "bio": "Designer and creator",
      "friendshipDate": "2025-01-15T12:00:00Z"
    }
  ]
}
```

---

##### 2.8.11 Remove Friend (Unfriend)

**DELETE** `/api/friendrequest/friends/{userId}`
**Auth**: Required

**Description**: Remove a friend from your friends list

**Response** (200 OK):
```json
{
  "success": true,
  "data": true,
  "message": "Friend removed successfully"
}
```

**Errors**:
- `400`: Cannot remove yourself
- `400`: Not friends with this user

---

##### 2.8.12 Get Friend Request Statistics

**GET** `/api/friendrequest/stats`
**Auth**: Required

**Description**: Get statistics about friend requests and friendships

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "pendingRequestsSent": 3,
    "pendingRequestsReceived": 5,
    "totalFriends": 42
  }
}
```

---

### UserProfile Service Configuration

**appsettings.json**:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "wechat_userprofiles"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "JWT": {
    "Secret": "your-super-secret-jwt-key-min-32-characters",
    "Issuer": "WeChat.AuthService",
    "Audience": "WeChat.Client"
  }
}
```

---

## 3. PostFeed Service

**Base URL**: `/api/posts`
**Port**: 5003
**Database**: MongoDB (`wechat_posts`)

### Endpoints

#### 3.1 Create Post

**POST** `/api/posts`
**Auth**: Required

**Description**: Create a new post

**Request Body**:
```json
{
  "content": "Just launched my new project! 🚀 #coding #webdev",
  "mediaUrls": [
    "https://storage.googleapis.com/wechat-media/posts/img1.jpg"
  ],
  "visibility": "Public",
  "locationName": "San Francisco, CA",
  "mentions": [
    "660e8400-e29b-41d4-a716-446655440001"
  ]
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f60001",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "displayName": "John Doe",
    "avatarUrl": "https://...",
    "content": "Just launched my new project! 🚀 #coding #webdev",
    "mediaUrls": ["https://storage.googleapis.com/wechat-media/posts/img1.jpg"],
    "hashtags": ["coding", "webdev"],
    "mentions": ["660e8400-e29b-41d4-a716-446655440001"],
    "visibility": "Public",
    "locationName": "San Francisco, CA",
    "stats": {
      "likesCount": 0,
      "commentsCount": 0,
      "sharesCount": 0,
      "viewsCount": 0
    },
    "createdAt": "2025-01-26T10:30:00Z",
    "updatedAt": "2025-01-26T10:30:00Z"
  }
}
```

---

#### 3.2 Get Post

**GET** `/api/posts/{postId}`
**Auth**: Required

**Description**: Get single post by ID

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f60001",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "displayName": "John Doe",
    "avatarUrl": "https://...",
    "content": "Just launched my new project! 🚀 #coding #webdev",
    "mediaUrls": [...],
    "hashtags": ["coding", "webdev"],
    "mentions": [...],
    "stats": {
      "likesCount": 42,
      "commentsCount": 8,
      "sharesCount": 3,
      "viewsCount": 520
    },
    "isLikedByMe": false,
    "isBookmarkedByMe": false,
    "createdAt": "2025-01-26T10:30:00Z"
  }
}
```

---

#### 3.3 Update Post

**PUT** `/api/posts/{postId}`
**Auth**: Required

**Description**: Update own post

**Request Body**:
```json
{
  "content": "Updated content",
  "visibility": "Public"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f60001",
    "content": "Updated content",
    ...
  }
}
```

---

#### 3.4 Delete Post

**DELETE** `/api/posts/{postId}`
**Auth**: Required

**Description**: Delete own post

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

#### 3.5 Get Feed

**GET** `/api/posts/feed?page=1&pageSize=20`
**Auth**: Required

**Description**: Get personalized feed (posts from followed users)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "65b4f3c2a1b2c3d4e5f60001",
        "userId": "660e8400-e29b-41d4-a716-446655440001",
        "username": "janedoe",
        "displayName": "Jane Doe",
        "avatarUrl": "https://...",
        "content": "Beautiful sunset today! 🌅",
        "mediaUrls": ["https://..."],
        "stats": {
          "likesCount": 125,
          "commentsCount": 18,
          "sharesCount": 7,
          "viewsCount": 1520
        },
        "isLikedByMe": true,
        "createdAt": "2025-01-26T09:15:00Z"
      }
    ],
    "totalCount": 500,
    "page": 1,
    "pageSize": 20,
    "totalPages": 25
  }
}
```

---

#### 3.6 Like Post

**POST** `/api/posts/{postId}/like`
**Auth**: Required

**Description**: Like a post

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "postId": "65b4f3c2a1b2c3d4e5f60001",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "reactionType": "Like",
    "createdAt": "2025-01-26T10:30:00Z"
  }
}
```

---

#### 3.7 Unlike Post

**DELETE** `/api/posts/{postId}/like`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

#### 3.8 Create Comment

**POST** `/api/posts/{postId}/comments`
**Auth**: Required

**Request Body**:
```json
{
  "content": "Great post! 👍",
  "parentCommentId": null
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f60002",
    "postId": "65b4f3c2a1b2c3d4e5f60001",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "displayName": "John Doe",
    "avatarUrl": "https://...",
    "content": "Great post! 👍",
    "parentCommentId": null,
    "repliesCount": 0,
    "likesCount": 0,
    "createdAt": "2025-01-26T10:30:00Z"
  }
}
```

---

#### 3.9 Get Comments

**GET** `/api/posts/{postId}/comments?page=1&pageSize=20`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "65b4f3c2a1b2c3d4e5f60002",
        "postId": "65b4f3c2a1b2c3d4e5f60001",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "username": "johndoe",
        "displayName": "John Doe",
        "avatarUrl": "https://...",
        "content": "Great post! 👍",
        "repliesCount": 2,
        "likesCount": 5,
        "isLikedByMe": false,
        "createdAt": "2025-01-26T10:30:00Z"
      }
    ],
    "totalCount": 8,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  }
}
```

---

### PostFeed Service Configuration

**appsettings.json**:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "wechat_posts"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "JWT": {
    "Secret": "your-super-secret-jwt-key-min-32-characters",
    "Issuer": "WeChat.AuthService",
    "Audience": "WeChat.Client"
  }
}
```

---

## 4. Chat Service

**Base URL**: `/api/chats`
**Port**: 5004
**Database**: MongoDB (`wechat_chat`)

### Endpoints

#### 4.1 Get Conversations

**GET** `/api/chats/conversations?page=1&pageSize=20`
**Auth**: Required

**Description**: Get list of user's conversations

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "65b4f3c2a1b2c3d4e5f60003",
        "type": "Direct",
        "participants": [
          {
            "userId": "550e8400-e29b-41d4-a716-446655440000",
            "username": "johndoe",
            "displayName": "John Doe",
            "avatarUrl": "https://..."
          },
          {
            "userId": "660e8400-e29b-41d4-a716-446655440001",
            "username": "janedoe",
            "displayName": "Jane Doe",
            "avatarUrl": "https://..."
          }
        ],
        "lastMessage": {
          "id": "65b4f3c2a1b2c3d4e5f60004",
          "senderId": "660e8400-e29b-41d4-a716-446655440001",
          "content": "See you tomorrow!",
          "messageType": "Text",
          "sentAt": "2025-01-26T09:45:00Z"
        },
        "unreadCount": 2,
        "createdAt": "2025-01-25T14:00:00Z",
        "updatedAt": "2025-01-26T09:45:00Z"
      }
    ],
    "totalCount": 15,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  }
}
```

---

#### 4.2 Create Conversation

**POST** `/api/chats/conversations`
**Auth**: Required

**Description**: Start new conversation

**Request Body**:
```json
{
  "participantIds": [
    "660e8400-e29b-41d4-a716-446655440001"
  ],
  "type": "Direct"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f60005",
    "type": "Direct",
    "participants": [...],
    "createdAt": "2025-01-26T10:30:00Z"
  }
}
```

---

#### 4.3 Send Message

**POST** `/api/chats/conversations/{conversationId}/messages`
**Auth**: Required

**Description**: Send message in conversation

**Request Body**:
```json
{
  "content": "Hello! How are you?",
  "messageType": "Text",
  "mediaUrl": null,
  "replyToMessageId": null
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f60006",
    "conversationId": "65b4f3c2a1b2c3d4e5f60005",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "content": "Hello! How are you?",
    "messageType": "Text",
    "mediaUrl": null,
    "replyToMessageId": null,
    "sentAt": "2025-01-26T10:30:00Z",
    "deliveredAt": null,
    "readAt": null,
    "isEdited": false,
    "isDeleted": false
  }
}
```

---

#### 4.4 Get Messages

**GET** `/api/chats/conversations/{conversationId}/messages?page=1&pageSize=50`
**Auth**: Required

**Description**: Get conversation messages (paginated, newest first)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "65b4f3c2a1b2c3d4e5f60006",
        "conversationId": "65b4f3c2a1b2c3d4e5f60005",
        "senderId": "550e8400-e29b-41d4-a716-446655440000",
        "senderUsername": "johndoe",
        "senderDisplayName": "John Doe",
        "senderAvatarUrl": "https://...",
        "content": "Hello! How are you?",
        "messageType": "Text",
        "sentAt": "2025-01-26T10:30:00Z",
        "deliveredAt": "2025-01-26T10:30:05Z",
        "readAt": "2025-01-26T10:30:10Z",
        "isEdited": false
      }
    ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 50,
    "totalPages": 3
  }
}
```

---

#### 4.5 Mark as Read

**PUT** `/api/chats/conversations/{conversationId}/read`
**Auth**: Required

**Description**: Mark all messages as read

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

#### 4.6 Delete Message

**DELETE** `/api/chats/messages/{messageId}`
**Auth**: Required

**Description**: Delete own message

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

### Chat Service Configuration

**appsettings.json**:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "wechat_chat"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "JWT": {
    "Secret": "your-super-secret-jwt-key-min-32-characters",
    "Issuer": "WeChat.AuthService",
    "Audience": "WeChat.Client"
  }
}
```

---

## 5. Video Service

**Base URL**: `/api/videos`
**Port**: 5005
**Database**: MongoDB (`wechat_videos`)

### Endpoints

#### 5.1 Upload Video

**POST** `/api/videos`
**Auth**: Required

**Description**: Create video record and upload to GCS

**Request Body**:
```json
{
  "title": "My awesome video",
  "description": "Check out this amazing content!",
  "videoType": "LongForm",
  "visibility": "Public",
  "category": "Technology",
  "tags": ["coding", "tutorial"],
  "duration": 300,
  "originalFileName": "video.mp4",
  "fileSize": 52428800,
  "format": "mp4",
  "resolution": {
    "width": 1920,
    "height": 1080,
    "aspectRatio": "16:9"
  },
  "sourceUrl": "https://storage.googleapis.com/wechat-videos/uploads/video123.mp4"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f60007",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "title": "My awesome video",
    "description": "Check out this amazing content!",
    "videoType": "LongForm",
    "duration": 300,
    "processingStatus": "Uploaded",
    "processingProgress": 0,
    "visibility": "Public",
    "stats": {
      "viewsCount": 0,
      "likesCount": 0,
      "commentsCount": 0,
      "sharesCount": 0
    },
    "createdAt": "2025-01-26T10:30:00Z"
  }
}
```

**Notes**:
- Video is automatically enqueued for background processing
- Processing generates multiple quality variants and thumbnails
- Status updates: Uploaded → Processing → Ready → Failed

---

#### 5.2 Get Video

**GET** `/api/videos/{videoId}`
**Auth**: Optional (public videos) / Required (private/unlisted)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f60007",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "title": "My awesome video",
    "description": "Check out this amazing content!",
    "videoType": "LongForm",
    "duration": 300,
    "processingStatus": "Ready",
    "processingProgress": 100,
    "streamingUrl": "https://storage.googleapis.com/wechat-videos/user123/video456/hls/master.m3u8",
    "thumbnailUrls": [
      "https://storage.googleapis.com/wechat-videos/user123/video456/thumbnails/thumbnail_1.jpg",
      "https://storage.googleapis.com/wechat-videos/user123/video456/thumbnails/thumbnail_2.jpg",
      "https://storage.googleapis.com/wechat-videos/user123/video456/thumbnails/thumbnail_3.jpg",
      "https://storage.googleapis.com/wechat-videos/user123/video456/thumbnails/thumbnail_4.jpg",
      "https://storage.googleapis.com/wechat-videos/user123/video456/thumbnails/thumbnail_5.jpg"
    ],
    "selectedThumbnailIndex": 0,
    "qualityVariants": [
      {
        "quality": "1080p",
        "url": "https://storage.googleapis.com/wechat-videos/user123/video456/variants/1080p.mp4",
        "fileSize": 52428800,
        "bitrate": 4500,
        "codec": "h264"
      },
      {
        "quality": "720p",
        "url": "https://storage.googleapis.com/wechat-videos/user123/video456/variants/720p.mp4",
        "fileSize": 31457280,
        "bitrate": 2500,
        "codec": "h264"
      }
    ],
    "visibility": "Public",
    "category": "Technology",
    "tags": ["coding", "tutorial"],
    "stats": {
      "viewsCount": 1520,
      "uniqueViewsCount": 890,
      "likesCount": 125,
      "commentsCount": 18,
      "sharesCount": 7,
      "watchTimeSeconds": 125000,
      "averageWatchPercentage": 75.5,
      "completionRate": 42.3
    },
    "createdAt": "2025-01-26T10:30:00Z",
    "publishedAt": "2025-01-26T10:35:00Z"
  }
}
```

---

#### 5.3 Get User Videos

**GET** `/api/videos/user/{userId}?page=1&pageSize=20&videoType=LongForm`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 15,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  }
}
```

---

#### 5.4 Update Video

**PUT** `/api/videos/{videoId}`
**Auth**: Required

**Request Body**:
```json
{
  "title": "Updated title",
  "description": "Updated description",
  "visibility": "Public",
  "category": "Technology",
  "tags": ["coding", "tutorial", "webdev"]
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f60007",
    "title": "Updated title",
    ...
  }
}
```

---

#### 5.5 Delete Video

**DELETE** `/api/videos/{videoId}`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

### Video Service Configuration

**appsettings.json**:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "wechat_videos"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "JWT": {
    "Secret": "your-super-secret-jwt-key-min-32-characters",
    "Issuer": "WeChat.AuthService",
    "Audience": "WeChat.Client"
  },
  "GCP": {
    "Storage": {
      "BucketName": "wechat-videos"
    },
    "CredentialsPath": "/app/config/gcp-credentials.json"
  }
}
```

---

## 6. Media Service

**Base URL**: `/api/media`
**Port**: 5006
**Database**: MongoDB (`wechat_media`)

### Endpoints

#### 6.1 Upload Image

**POST** `/api/media/upload`
**Auth**: Required
**Content-Type**: `multipart/form-data`

**Request**:
```
file: <image file> (max 10MB, jpg/png/gif/webp)
mediaType: Image
purpose: Post | Avatar | Cover
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f60008",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "mediaType": "Image",
    "purpose": "Post",
    "originalUrl": "https://storage.googleapis.com/wechat-media/posts/img_original_123.jpg",
    "thumbnailUrl": "https://storage.googleapis.com/wechat-media/posts/img_thumb_123.jpg",
    "mediumUrl": "https://storage.googleapis.com/wechat-media/posts/img_medium_123.jpg",
    "fileSize": 2048000,
    "width": 1920,
    "height": 1080,
    "format": "jpg",
    "uploadedAt": "2025-01-26T10:30:00Z"
  }
}
```

---

#### 6.2 Get Media

**GET** `/api/media/{mediaId}`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f60008",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "mediaType": "Image",
    "purpose": "Post",
    "originalUrl": "https://storage.googleapis.com/wechat-media/posts/img_original_123.jpg",
    "thumbnailUrl": "https://storage.googleapis.com/wechat-media/posts/img_thumb_123.jpg",
    "mediumUrl": "https://storage.googleapis.com/wechat-media/posts/img_medium_123.jpg",
    "fileSize": 2048000,
    "width": 1920,
    "height": 1080,
    "format": "jpg",
    "uploadedAt": "2025-01-26T10:30:00Z"
  }
}
```

---

#### 6.3 Delete Media

**DELETE** `/api/media/{mediaId}`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

### Media Service Configuration

**appsettings.json**:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "wechat_media"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "JWT": {
    "Secret": "your-super-secret-jwt-key-min-32-characters",
    "Issuer": "WeChat.AuthService",
    "Audience": "WeChat.Client"
  },
  "GCP": {
    "Storage": {
      "BucketName": "wechat-media"
    },
    "CredentialsPath": "/app/config/gcp-credentials.json"
  },
  "Upload": {
    "MaxFileSizeMB": 10,
    "AllowedExtensions": ["jpg", "jpeg", "png", "gif", "webp"],
    "GenerateThumbnails": true,
    "ThumbnailWidth": 200,
    "MediumWidth": 800
  }
}
```

---

## 7. Notification Service

**Base URL**: `/api/notifications`
**Port**: 5007
**Database**: MongoDB (`wechat_notifications`)

### Endpoints

#### 7.1 Get Notifications

**GET** `/api/notifications?page=1&pageSize=20&unreadOnly=false&type=All`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "65b4f3c2a1b2c3d4e5f60009",
        "type": "Like",
        "title": "New like",
        "message": "johndoe liked your post",
        "senderId": "660e8400-e29b-41d4-a716-446655440001",
        "senderUsername": "johndoe",
        "senderAvatarUrl": "https://...",
        "entityId": "65b4f3c2a1b2c3d4e5f60001",
        "entityType": "Post",
        "actionUrl": "/posts/65b4f3c2a1b2c3d4e5f60001",
        "imageUrl": "https://...",
        "priority": "Normal",
        "isRead": false,
        "deliveryStatus": "Delivered",
        "deliveryChannels": ["InApp", "Push"],
        "createdAt": "2025-01-26T10:30:00Z"
      }
    ],
    "totalCount": 50,
    "page": 1,
    "pageSize": 20,
    "totalPages": 3
  }
}
```

**Query Parameters**:
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20, max: 100)
- `unreadOnly`: Filter unread only (default: false)
- `type`: Filter by type (All | Like | Comment | Follow | Mention | System)

---

#### 7.2 Get Unread Count

**GET** `/api/notifications/unread-count`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": 15
}
```

---

#### 7.3 Mark as Read

**PUT** `/api/notifications/{notificationId}/read`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

#### 7.4 Mark All as Read

**PUT** `/api/notifications/read-all`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

#### 7.5 Delete Notification

**DELETE** `/api/notifications/{notificationId}`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": true
}
```

---

#### 7.6 Get Preferences

**GET** `/api/notifications/preferences`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f6000a",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "channels": {
      "likes": {
        "inApp": true,
        "push": true,
        "email": false,
        "sms": false
      },
      "comments": {
        "inApp": true,
        "push": true,
        "email": true,
        "sms": false
      },
      "follows": {
        "inApp": true,
        "push": true,
        "email": false,
        "sms": false
      },
      "mentions": {
        "inApp": true,
        "push": true,
        "email": true,
        "sms": false
      }
    },
    "muteUntil": null,
    "updatedAt": "2025-01-26T10:30:00Z"
  }
}
```

---

#### 7.7 Update Preferences

**PUT** `/api/notifications/preferences`
**Auth**: Required

**Request Body**:
```json
{
  "channels": {
    "likes": {
      "inApp": true,
      "push": false,
      "email": false,
      "sms": false
    },
    "comments": {
      "inApp": true,
      "push": true,
      "email": true,
      "sms": false
    }
  }
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f6000a",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "channels": {...},
    "updatedAt": "2025-01-26T10:35:00Z"
  }
}
```

---

#### 7.8 Register Device Token

**POST** `/api/notifications/device-tokens`
**Auth**: Required

**Description**: Register device for push notifications

**Request Body**:
```json
{
  "token": "firebase-cloud-messaging-token-here",
  "platform": "iOS",
  "deviceId": "unique-device-id",
  "deviceName": "John's iPhone"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "65b4f3c2a1b2c3d4e5f6000b",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "token": "firebase-cloud-messaging-token-here",
    "platform": "iOS",
    "deviceId": "unique-device-id",
    "deviceName": "John's iPhone",
    "isActive": true,
    "createdAt": "2025-01-26T10:30:00Z"
  }
}
```

---

### Notification Service Configuration

**appsettings.json**:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "wechat_notifications"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "JWT": {
    "Secret": "your-super-secret-jwt-key-min-32-characters",
    "Issuer": "WeChat.AuthService",
    "Audience": "WeChat.Client"
  },
  "NotificationSettings": {
    "EnablePushNotifications": true,
    "EnableEmailNotifications": true,
    "FirebaseCredentialsPath": "/app/config/firebase-credentials.json"
  }
}
```

---

## 8. Realtime Service

**Base URL**: `/api/realtime` (HTTP) + `/hubs/...` (SignalR)
**Port**: 5008
**Technology**: SignalR WebSocket

### SignalR Hubs

#### 8.1 Notification Hub

**Hub URL**: `/hubs/notifications`
**Auth**: Required (JWT in query string or header)

**Client Methods (Receive from server)**:
- `ReceiveNotification(notification)`: New notification received
- `NotificationRead(notificationId)`: Notification marked as read
- `UnreadCountUpdated(count)`: Unread count changed

**Server Methods (Send from client)**:
- `JoinUserGroup()`: Join personal notification group
- `LeaveUserGroup()`: Leave personal notification group

---

#### 8.2 Chat Hub

**Hub URL**: `/hubs/chat`
**Auth**: Required

**Client Methods**:
- `ReceiveMessage(message)`: New message in conversation
- `MessageDelivered(messageId, deliveredAt)`: Message delivered
- `MessageRead(messageId, readAt)`: Message read
- `UserTyping(conversationId, userId, username)`: User typing indicator
- `UserStoppedTyping(conversationId, userId)`: User stopped typing

**Server Methods**:
- `JoinConversation(conversationId)`: Join conversation group
- `LeaveConversation(conversationId)`: Leave conversation group
- `SendTypingIndicator(conversationId)`: Send typing indicator
- `StopTypingIndicator(conversationId)`: Stop typing indicator

---

#### 8.3 Presence Hub

**Hub URL**: `/hubs/presence`
**Auth**: Required

**Client Methods**:
- `UserOnline(userId, username)`: User came online
- `UserOffline(userId)`: User went offline
- `UserStatusChanged(userId, status)`: User status changed

**Server Methods**:
- `SetStatus(status)`: Set online status (Online, Away, Busy, Invisible)
- `GetOnlineUsers()`: Get list of online users

---

### HTTP Endpoints

#### 8.4 Get Presence

**GET** `/api/realtime/presence/{userId}`
**Auth**: Required

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "status": "Online",
    "lastSeen": "2025-01-26T10:30:00Z",
    "isOnline": true
  }
}
```

---

#### 8.5 Get Multiple Presence

**POST** `/api/realtime/presence/bulk`
**Auth**: Required

**Request Body**:
```json
{
  "userIds": [
    "550e8400-e29b-41d4-a716-446655440000",
    "660e8400-e29b-41d4-a716-446655440001"
  ]
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "username": "johndoe",
      "status": "Online",
      "isOnline": true,
      "lastSeen": "2025-01-26T10:30:00Z"
    },
    {
      "userId": "660e8400-e29b-41d4-a716-446655440001",
      "username": "janedoe",
      "status": "Away",
      "isOnline": true,
      "lastSeen": "2025-01-26T10:25:00Z"
    }
  ]
}
```

---

### Realtime Service Configuration

**appsettings.json**:
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "JWT": {
    "Secret": "your-super-secret-jwt-key-min-32-characters",
    "Issuer": "WeChat.AuthService",
    "Audience": "WeChat.Client"
  },
  "SignalR": {
    "KeepAliveInterval": "00:00:15",
    "ClientTimeoutInterval": "00:00:30",
    "HandshakeTimeout": "00:00:15"
  }
}
```

---

## Configuration Requirements

### Global Configuration

All services require these common settings:

**JWT Configuration**:
- `JWT__Secret`: Minimum 32 characters, same across all services
- `JWT__Issuer`: "WeChat.AuthService"
- `JWT__Audience`: "WeChat.Client"

**Database Configuration**:
- **PostgreSQL** (AuthService): Connection string with credentials
- **MongoDB** (All other services): Connection string + database name per service
- **Redis** (All services): Connection string with password

**GCP Configuration** (VideoService, MediaService):
- `GCP__Storage__BucketName`: GCS bucket name
- `GCP__CredentialsPath`: Path to service account JSON file

**Firebase Configuration** (NotificationService):
- `NotificationSettings__FirebaseCredentialsPath`: Path to Firebase credentials JSON

---

### Service-Specific Ports

| Service | Development Port | Production Port |
|---------|-----------------|----------------|
| Gateway | 5000 | 80/443 |
| Auth | 5001 | Internal |
| UserProfile | 5002 | Internal |
| PostFeed | 5003 | Internal |
| Chat | 5004 | Internal |
| Video | 5005 | Internal |
| Media | 5006 | Internal |
| Notification | 5007 | Internal |
| Realtime | 5008 | Internal |

**Production**: Only Gateway exposed to internet, all services internal

---

### Environment Variables Summary

```bash
# Database
POSTGRES_PASSWORD=<strong-password>
MONGO_PASSWORD=<strong-password>
REDIS_PASSWORD=<strong-password>

# JWT
JWT_SECRET=<min-32-chars-secret-key>

# GCP
GCP_BUCKET_NAME=wechat-videos
GCP_PROJECT_ID=your-project-id

# Firebase
FIREBASE_PROJECT_ID=your-firebase-project

# Service URLs (for Gateway routing)
AUTH_SERVICE_URL=http://auth-service:8080
USERPROFILE_SERVICE_URL=http://userprofile-service:8080
# ... etc
```

---

## Error Codes

### HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET, PUT, DELETE |
| 201 | Created | Successful POST (resource created) |
| 204 | No Content | Successful DELETE (no response body) |
| 400 | Bad Request | Validation failed, malformed request |
| 401 | Unauthorized | Missing/invalid/expired token |
| 403 | Forbidden | Valid token but insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Resource already exists, state conflict |
| 422 | Unprocessable Entity | Semantic validation failed |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |
| 503 | Service Unavailable | Service down/maintenance |

---

### Common Error Messages

**Authentication Errors**:
- `Invalid credentials`
- `Token has expired`
- `Token has been revoked`
- `Email not verified`
- `Account locked`

**Validation Errors**:
- `{field} is required`
- `{field} has invalid format`
- `{field} must be at least {min} characters`
- `{field} cannot exceed {max} characters`

**Not Found Errors**:
- `{EntityName} with ID '{id}' not found`
- `User not found`
- `Post not found`
- `Video not found`

**Conflict Errors**:
- `Email already registered`
- `Username already taken`
- `{EntityName} with this {field} already exists`
- `Already following this user`

---

## Rate Limiting

### Default Limits

| Endpoint Type | Limit | Window |
|--------------|-------|--------|
| Authentication | 5 requests | per minute |
| Read Operations | 100 requests | per minute |
| Write Operations | 30 requests | per minute |
| File Uploads | 10 uploads | per hour |

### Rate Limit Headers

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 75
X-RateLimit-Reset: 1706263200
```

---

## Pagination

### Query Parameters

- `page`: Page number (1-based, default: 1)
- `pageSize`: Items per page (default: 20, max: 100)

### Response Format

```json
{
  "items": [...],
  "totalCount": 500,
  "page": 1,
  "pageSize": 20,
  "totalPages": 25
}
```

---

## File Upload Limits

| Media Type | Max Size | Allowed Formats |
|-----------|----------|-----------------|
| Avatar | 5 MB | jpg, png, gif |
| Cover Photo | 10 MB | jpg, png |
| Post Image | 10 MB | jpg, png, gif, webp |
| Video (Short) | 100 MB | mp4, mov, avi |
| Video (Long) | 2 GB | mp4, mov, avi, mkv |

---

## Webhook Events (Future)

### Available Events

- `user.created`
- `post.created`
- `video.ready`
- `comment.created`
- `message.sent`

### Webhook Payload

```json
{
  "event": "post.created",
  "timestamp": "2025-01-26T10:30:00Z",
  "data": {
    "postId": "65b4f3c2a1b2c3d4e5f60001",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    ...
  }
}
```

---

## Support

For API support:
- Documentation: `/swagger` on each service
- Issues: GitHub repository
- Email: api-support@wechat.com

**Last Updated**: January 26, 2025
**API Version**: 1.0
