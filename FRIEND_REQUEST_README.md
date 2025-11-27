# Friend Request API - Integration Guide

Complete guide for integrating Friend Request and Friendship management endpoints into your UI application.

---

## Table of Contents

1. [Overview](#overview)
2. [Base URL](#base-url)
3. [Response Format](#response-format)
4. [Friend Request Flow](#friend-request-flow)
5. [API Endpoints](#api-endpoints)
   - [Send Friend Request](#1-send-friend-request)
   - [Accept Friend Request](#2-accept-friend-request)
   - [Reject Friend Request](#3-reject-friend-request)
   - [Cancel Friend Request](#4-cancel-friend-request)
   - [Get Received Friend Requests](#5-get-received-friend-requests)
   - [Get Sent Friend Requests](#6-get-sent-friend-requests)
   - [Get Friend Request by ID](#7-get-friend-request-by-id)
   - [Check Friendship Status](#8-check-friendship-status)
   - [Get Friends List](#9-get-friends-list)
   - [Get User's Friends](#10-get-users-friends)
   - [Remove Friend](#11-remove-friend)
   - [Get Friend Request Statistics](#12-get-friend-request-statistics)
6. [Error Handling](#error-handling)
7. [Integration Examples](#integration-examples)

---

## Overview

The WeChat Friend Request API provides complete friendship management using a bi-directional friend system (like Facebook). It supports:

- Send friend requests with optional messages
- Accept, reject, or cancel friend requests
- Bi-directional friendship (mutual friends)
- Friend request notifications
- Friendship status checking
- Friends list management
- Friend request statistics

**Authentication Method**: Bearer Token (JWT) - Required for all endpoints

**Friendship Model**: Bi-directional (when user A and user B become friends, both have each other in their friends list)

---

## Base URL

| Environment | URL |
|-------------|-----|
| Development | `http://localhost:5003` |
| Production  | `https://api.wechat.com` |

**UserProfile Service Port**: `5003`

---

## Response Format

All API responses follow a consistent format:

### Success Response

```json
{
  "success": true,
  "data": {
    // Response data here
  },
  "error": null,
  "errors": null,
  "timestamp": "2025-11-27T10:30:00Z"
}
```

### Error Response

```json
{
  "success": false,
  "data": null,
  "error": "Error message here",
  "errors": ["Detailed error 1", "Detailed error 2"],
  "timestamp": "2025-11-27T10:30:00Z"
}
```

---

## Friend Request Flow

```
┌─────────────────────────────────────────────────────────────┐
│                  Friend Request Flow                         │
└─────────────────────────────────────────────────────────────┘

User A → Send Friend Request → User B
         (Status: Pending)
                 ↓
         ┌───────┴───────┐
         ↓               ↓
    User B Accepts   User B Rejects
         ↓               ↓
   Status: Accepted  Status: Rejected
   Create Friendship    (No friendship)
   (Bi-directional)
         ↓
   Both users are now friends
   (A is in B's friends list)
   (B is in A's friends list)

Alternative Flow:
User A → Send Friend Request → User B
         (Status: Pending)
                 ↓
         User A Cancels
                 ↓
         Status: Cancelled
         (No friendship)

After Friendship Established:
User A → Remove Friend → User B
         ↓
   Friendship deleted (bi-directional)
   (No longer friends)
```

---

## API Endpoints

### 1. Send Friend Request

Send a friend request to another user with an optional message.

**Endpoint**: `POST /api/friendrequest/send/{userId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| userId | GUID | Yes | The ID of the user to send friend request to |

#### Request Body

```json
{
  "message": "Hi! I'd like to connect with you."
}
```

#### Request Fields

| Field | Type | Required | Description | Constraints |
|-------|------|----------|-------------|-------------|
| message | string | No | Optional message with the friend request | Max 500 characters |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "673c0e8f8c4d5e1a2b3c4d5e",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "senderUsername": "john",
    "senderDisplayName": "John Doe",
    "senderAvatarUrl": "https://cdn.wechat.com/avatars/john.jpg",
    "senderIsVerified": true,
    "receiverId": "660e8400-e29b-41d4-a716-446655440001",
    "receiverUsername": "jane",
    "receiverDisplayName": "Jane Smith",
    "receiverAvatarUrl": "https://cdn.wechat.com/avatars/jane.jpg",
    "receiverIsVerified": false,
    "status": "Pending",
    "message": "Hi! I'd like to connect with you.",
    "createdAt": "2025-11-27T10:30:00Z",
    "respondedAt": null
  }
}
```

#### Error Responses

| Status | Error | Description |
|--------|-------|-------------|
| 400 | "Cannot send friend request to yourself" | Attempting to send request to own userId |
| 400 | "Friend request already sent" | A pending request already exists |
| 400 | "You are already friends" | Users are already friends |
| 400 | "User has blocked you" | Target user has blocked the sender |
| 404 | "User not found" | Target user doesn't exist |

---

### 2. Accept Friend Request

Accept a pending friend request. This creates a bi-directional friendship.

**Endpoint**: `POST /api/friendrequest/{requestId}/accept`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| requestId | string | Yes | The ID of the friend request to accept |

#### Request Body

None

#### Response (200 OK)

```json
{
  "success": true,
  "data": true,
  "message": "Friend request accepted"
}
```

#### Error Responses

| Status | Error | Description |
|--------|-------|-------------|
| 400 | "You are not the receiver of this request" | Only the receiver can accept |
| 400 | "Friend request not found or already processed" | Request doesn't exist or not pending |
| 404 | "Friend request not found" | Invalid requestId |

---

### 3. Reject Friend Request

Reject a pending friend request.

**Endpoint**: `POST /api/friendrequest/{requestId}/reject`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| requestId | string | Yes | The ID of the friend request to reject |

#### Request Body

None

#### Response (200 OK)

```json
{
  "success": true,
  "data": true,
  "message": "Friend request rejected"
}
```

#### Error Responses

| Status | Error | Description |
|--------|-------|-------------|
| 400 | "You are not the receiver of this request" | Only the receiver can reject |
| 400 | "Friend request not found or already processed" | Request doesn't exist or not pending |
| 404 | "Friend request not found" | Invalid requestId |

---

### 4. Cancel Friend Request

Cancel a friend request that you sent.

**Endpoint**: `DELETE /api/friendrequest/{requestId}/cancel`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| requestId | string | Yes | The ID of the friend request to cancel |

#### Request Body

None

#### Response (200 OK)

```json
{
  "success": true,
  "data": true,
  "message": "Friend request cancelled"
}
```

#### Error Responses

| Status | Error | Description |
|--------|-------|-------------|
| 400 | "You are not the sender of this request" | Only the sender can cancel |
| 400 | "Friend request not found or already processed" | Request doesn't exist or not pending |
| 404 | "Friend request not found" | Invalid requestId |

---

### 5. Get Received Friend Requests

Get all pending friend requests that you have received (incoming requests).

**Endpoint**: `GET /api/friendrequest/received`
**Authentication**: Required (Bearer token)

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| page | integer | No | 1 | Page number (1-based) |
| pageSize | integer | No | 20 | Number of items per page (max 100) |

#### Request

```
GET /api/friendrequest/received?page=1&pageSize=20
Authorization: Bearer {access_token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": "673c0e8f8c4d5e1a2b3c4d5e",
      "senderId": "550e8400-e29b-41d4-a716-446655440000",
      "senderUsername": "john",
      "senderDisplayName": "John Doe",
      "senderAvatarUrl": "https://cdn.wechat.com/avatars/john.jpg",
      "senderIsVerified": true,
      "receiverId": "660e8400-e29b-41d4-a716-446655440001",
      "receiverUsername": "jane",
      "receiverDisplayName": "Jane Smith",
      "receiverAvatarUrl": "https://cdn.wechat.com/avatars/jane.jpg",
      "receiverIsVerified": false,
      "status": "Pending",
      "message": "Hi! I'd like to connect with you.",
      "createdAt": "2025-11-27T10:30:00Z",
      "respondedAt": null
    }
  ]
}
```

---

### 6. Get Sent Friend Requests

Get all pending friend requests that you have sent (outgoing requests).

**Endpoint**: `GET /api/friendrequest/sent`
**Authentication**: Required (Bearer token)

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| page | integer | No | 1 | Page number (1-based) |
| pageSize | integer | No | 20 | Number of items per page (max 100) |

#### Request

```
GET /api/friendrequest/sent?page=1&pageSize=20
Authorization: Bearer {access_token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": "673c0e8f8c4d5e1a2b3c4d5f",
      "senderId": "660e8400-e29b-41d4-a716-446655440001",
      "senderUsername": "jane",
      "senderDisplayName": "Jane Smith",
      "senderAvatarUrl": "https://cdn.wechat.com/avatars/jane.jpg",
      "senderIsVerified": false,
      "receiverId": "770e8400-e29b-41d4-a716-446655440002",
      "receiverUsername": "bob",
      "receiverDisplayName": "Bob Wilson",
      "receiverAvatarUrl": "https://cdn.wechat.com/avatars/bob.jpg",
      "receiverIsVerified": false,
      "status": "Pending",
      "message": "Hey Bob! Let's connect.",
      "createdAt": "2025-11-27T11:00:00Z",
      "respondedAt": null
    }
  ]
}
```

---

### 7. Get Friend Request by ID

Get details of a specific friend request.

**Endpoint**: `GET /api/friendrequest/{requestId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| requestId | string | Yes | The ID of the friend request |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "673c0e8f8c4d5e1a2b3c4d5e",
    "senderId": "550e8400-e29b-41d4-a716-446655440000",
    "senderUsername": "john",
    "senderDisplayName": "John Doe",
    "senderAvatarUrl": "https://cdn.wechat.com/avatars/john.jpg",
    "senderIsVerified": true,
    "receiverId": "660e8400-e29b-41d4-a716-446655440001",
    "receiverUsername": "jane",
    "receiverDisplayName": "Jane Smith",
    "receiverAvatarUrl": "https://cdn.wechat.com/avatars/jane.jpg",
    "receiverIsVerified": false,
    "status": "Accepted",
    "message": "Hi! I'd like to connect with you.",
    "createdAt": "2025-11-27T10:30:00Z",
    "respondedAt": "2025-11-27T10:35:00Z"
  }
}
```

#### Error Responses

| Status | Error | Description |
|--------|-------|-------------|
| 404 | "Friend request not found" | Invalid requestId |

---

### 8. Check Friendship Status

Check the friendship status between you and another user.

**Endpoint**: `GET /api/friendrequest/status/{userId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| userId | GUID | Yes | The ID of the user to check friendship status with |

#### Response (200 OK) - Already Friends

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

#### Response (200 OK) - Request Sent

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

#### Response (200 OK) - Request Received

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

#### Response (200 OK) - Not Friends

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

#### Status Values

| Status | Description |
|--------|-------------|
| friends | Users are friends |
| request_sent | You have sent a friend request to this user |
| request_received | You have received a friend request from this user |
| not_friends | No friendship or pending request |

---

### 9. Get Friends List

Get your friends list with pagination.

**Endpoint**: `GET /api/friendrequest/friends`
**Authentication**: Required (Bearer token)

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| page | integer | No | 1 | Page number (1-based) |
| pageSize | integer | No | 20 | Number of items per page (max 100) |

#### Request

```
GET /api/friendrequest/friends?page=1&pageSize=20
Authorization: Bearer {access_token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john",
      "displayName": "John Doe",
      "avatarUrl": "https://cdn.wechat.com/avatars/john.jpg",
      "isVerified": true,
      "bio": "Software Engineer | Tech Enthusiast",
      "friendshipDate": "2025-11-20T10:30:00Z"
    },
    {
      "userId": "770e8400-e29b-41d4-a716-446655440002",
      "username": "bob",
      "displayName": "Bob Wilson",
      "avatarUrl": "https://cdn.wechat.com/avatars/bob.jpg",
      "isVerified": false,
      "bio": "Designer & Creative",
      "friendshipDate": "2025-11-15T14:20:00Z"
    }
  ]
}
```

---

### 10. Get User's Friends

Get the friends list of a specific user (public endpoint).

**Endpoint**: `GET /api/friendrequest/friends/{userId}`
**Authentication**: Not required

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| userId | GUID | Yes | The ID of the user whose friends list to retrieve |

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| page | integer | No | 1 | Page number (1-based) |
| pageSize | integer | No | 20 | Number of items per page (max 100) |

#### Request

```
GET /api/friendrequest/friends/550e8400-e29b-41d4-a716-446655440000?page=1&pageSize=20
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "userId": "660e8400-e29b-41d4-a716-446655440001",
      "username": "jane",
      "displayName": "Jane Smith",
      "avatarUrl": "https://cdn.wechat.com/avatars/jane.jpg",
      "isVerified": false,
      "bio": "Marketing Professional",
      "friendshipDate": "2025-11-20T10:30:00Z"
    }
  ]
}
```

---

### 11. Remove Friend

Remove a friend (unfriend). This removes the friendship bi-directionally.

**Endpoint**: `DELETE /api/friendrequest/friends/{userId}`
**Authentication**: Required (Bearer token)

#### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| userId | GUID | Yes | The ID of the friend to remove |

#### Request Body

None

#### Response (200 OK)

```json
{
  "success": true,
  "data": true,
  "message": "Friend removed successfully"
}
```

#### Error Responses

| Status | Error | Description |
|--------|-------|-------------|
| 400 | "Cannot remove yourself as a friend" | Attempting to unfriend own userId |
| 400 | "You are not friends with this user" | Users are not friends |

---

### 12. Get Friend Request Statistics

Get statistics about friend requests and friendships.

**Endpoint**: `GET /api/friendrequest/stats`
**Authentication**: Required (Bearer token)

#### Request

```
GET /api/friendrequest/stats
Authorization: Bearer {access_token}
```

#### Response (200 OK)

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

#### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| pendingRequestsSent | integer | Number of pending friend requests you have sent |
| pendingRequestsReceived | integer | Number of pending friend requests you have received |
| totalFriends | integer | Total number of friends |

---

## Error Handling

### Common Error Codes

| Status Code | Meaning | Common Causes |
|-------------|---------|---------------|
| 400 | Bad Request | Invalid input, validation errors |
| 401 | Unauthorized | Missing or invalid authentication token |
| 403 | Forbidden | Not authorized to perform this action |
| 404 | Not Found | Resource (user, request) not found |
| 409 | Conflict | Duplicate request, already friends |
| 500 | Internal Server Error | Server-side error |

### Error Response Format

```json
{
  "success": false,
  "data": null,
  "error": "Main error message",
  "errors": [
    "Detailed error 1",
    "Detailed error 2"
  ],
  "timestamp": "2025-11-27T10:30:00Z"
}
```

### Handling Errors in Your Application

```typescript
try {
  const response = await friendRequestService.sendFriendRequest(userId, message);

  if (!response.success) {
    // Handle specific errors
    if (response.error === "You are already friends") {
      showNotification("You are already friends with this user", "info");
    } else if (response.error === "User has blocked you") {
      showNotification("Unable to send friend request", "error");
    } else {
      showNotification(response.error, "error");
    }
  } else {
    showNotification("Friend request sent!", "success");
  }
} catch (error) {
  console.error("Network error:", error);
  showNotification("Network error. Please try again.", "error");
}
```

---

## Integration Examples

### React/TypeScript Integration

#### 1. Create Friend Request Service

```typescript
// services/friendRequestService.ts
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5003/api/friendrequest';

interface FriendRequestDto {
  id: string;
  senderId: string;
  senderUsername: string;
  senderDisplayName: string;
  senderAvatarUrl?: string;
  senderIsVerified: boolean;
  receiverId: string;
  receiverUsername: string;
  receiverDisplayName: string;
  receiverAvatarUrl?: string;
  receiverIsVerified: boolean;
  status: string;
  message?: string;
  createdAt: string;
  respondedAt?: string;
}

interface FriendshipDto {
  userId: string;
  username: string;
  displayName: string;
  avatarUrl?: string;
  isVerified: boolean;
  bio?: string;
  friendshipDate: string;
}

interface FriendRequestStatsDto {
  pendingRequestsSent: number;
  pendingRequestsReceived: number;
  totalFriends: number;
}

interface ApiResponse<T> {
  success: boolean;
  data: T;
  error?: string;
  errors?: string[];
  timestamp: string;
}

// Get access token from storage
const getAuthToken = (): string => {
  return localStorage.getItem('accessToken') || '';
};

// Create axios instance with default config
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to all requests
apiClient.interceptors.request.use((config) => {
  const token = getAuthToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const friendRequestService = {
  // Send friend request
  async sendFriendRequest(
    userId: string,
    message?: string
  ): Promise<ApiResponse<FriendRequestDto>> {
    const response = await apiClient.post(`/send/${userId}`, { message });
    return response.data;
  },

  // Accept friend request
  async acceptFriendRequest(requestId: string): Promise<ApiResponse<boolean>> {
    const response = await apiClient.post(`/${requestId}/accept`);
    return response.data;
  },

  // Reject friend request
  async rejectFriendRequest(requestId: string): Promise<ApiResponse<boolean>> {
    const response = await apiClient.post(`/${requestId}/reject`);
    return response.data;
  },

  // Cancel friend request
  async cancelFriendRequest(requestId: string): Promise<ApiResponse<boolean>> {
    const response = await apiClient.delete(`/${requestId}/cancel`);
    return response.data;
  },

  // Get received friend requests (incoming)
  async getReceivedFriendRequests(
    page: number = 1,
    pageSize: number = 20
  ): Promise<ApiResponse<FriendRequestDto[]>> {
    const response = await apiClient.get('/received', {
      params: { page, pageSize },
    });
    return response.data;
  },

  // Get sent friend requests (outgoing)
  async getSentFriendRequests(
    page: number = 1,
    pageSize: number = 20
  ): Promise<ApiResponse<FriendRequestDto[]>> {
    const response = await apiClient.get('/sent', {
      params: { page, pageSize },
    });
    return response.data;
  },

  // Get friend request by ID
  async getFriendRequest(requestId: string): Promise<ApiResponse<FriendRequestDto>> {
    const response = await apiClient.get(`/${requestId}`);
    return response.data;
  },

  // Check friendship status
  async getFriendshipStatus(userId: string): Promise<ApiResponse<{
    status: string;
    areFriends: boolean;
    hasPendingRequest: boolean;
    requestDirection?: string;
  }>> {
    const response = await apiClient.get(`/status/${userId}`);
    return response.data;
  },

  // Get friends list
  async getFriends(
    page: number = 1,
    pageSize: number = 20
  ): Promise<ApiResponse<FriendshipDto[]>> {
    const response = await apiClient.get('/friends', {
      params: { page, pageSize },
    });
    return response.data;
  },

  // Get specific user's friends
  async getUserFriends(
    userId: string,
    page: number = 1,
    pageSize: number = 20
  ): Promise<ApiResponse<FriendshipDto[]>> {
    const response = await apiClient.get(`/friends/${userId}`, {
      params: { page, pageSize },
    });
    return response.data;
  },

  // Remove friend
  async removeFriend(userId: string): Promise<ApiResponse<boolean>> {
    const response = await apiClient.delete(`/friends/${userId}`);
    return response.data;
  },

  // Get statistics
  async getStats(): Promise<ApiResponse<FriendRequestStatsDto>> {
    const response = await apiClient.get('/stats');
    return response.data;
  },
};
```

#### 2. React Component Examples

```tsx
// components/FriendRequestButton.tsx
import React, { useState, useEffect } from 'react';
import { friendRequestService } from '../services/friendRequestService';

interface FriendRequestButtonProps {
  userId: string;
  username: string;
}

export const FriendRequestButton: React.FC<FriendRequestButtonProps> = ({
  userId,
  username,
}) => {
  const [status, setStatus] = useState<string>('loading');
  const [requestId, setRequestId] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    checkFriendshipStatus();
  }, [userId]);

  const checkFriendshipStatus = async () => {
    try {
      const response = await friendRequestService.getFriendshipStatus(userId);
      if (response.success) {
        setStatus(response.data.status);
      }
    } catch (error) {
      console.error('Error checking friendship status:', error);
      setStatus('not_friends');
    }
  };

  const handleSendRequest = async () => {
    setLoading(true);
    try {
      const response = await friendRequestService.sendFriendRequest(
        userId,
        `Hi ${username}! Let's connect.`
      );

      if (response.success) {
        setStatus('request_sent');
        setRequestId(response.data.id);
        alert('Friend request sent!');
      } else {
        alert(response.error || 'Failed to send friend request');
      }
    } catch (error) {
      console.error('Error sending friend request:', error);
      alert('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleCancelRequest = async () => {
    if (!requestId) return;

    setLoading(true);
    try {
      const response = await friendRequestService.cancelFriendRequest(requestId);

      if (response.success) {
        setStatus('not_friends');
        setRequestId(null);
        alert('Friend request cancelled');
      }
    } catch (error) {
      console.error('Error cancelling request:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleRemoveFriend = async () => {
    if (!confirm(`Remove ${username} from friends?`)) return;

    setLoading(true);
    try {
      const response = await friendRequestService.removeFriend(userId);

      if (response.success) {
        setStatus('not_friends');
        alert('Friend removed');
      }
    } catch (error) {
      console.error('Error removing friend:', error);
    } finally {
      setLoading(false);
    }
  };

  if (status === 'loading') {
    return <button disabled>Loading...</button>;
  }

  if (status === 'friends') {
    return (
      <button onClick={handleRemoveFriend} disabled={loading}>
        {loading ? 'Removing...' : 'Remove Friend'}
      </button>
    );
  }

  if (status === 'request_sent') {
    return (
      <button onClick={handleCancelRequest} disabled={loading}>
        {loading ? 'Cancelling...' : 'Cancel Request'}
      </button>
    );
  }

  if (status === 'request_received') {
    return <span>Wants to be friends (check requests)</span>;
  }

  return (
    <button onClick={handleSendRequest} disabled={loading}>
      {loading ? 'Sending...' : 'Add Friend'}
    </button>
  );
};
```

```tsx
// components/FriendRequestsList.tsx
import React, { useState, useEffect } from 'react';
import { friendRequestService } from '../services/friendRequestService';

export const FriendRequestsList: React.FC = () => {
  const [requests, setRequests] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadRequests();
  }, []);

  const loadRequests = async () => {
    try {
      const response = await friendRequestService.getReceivedFriendRequests();
      if (response.success) {
        setRequests(response.data);
      }
    } catch (error) {
      console.error('Error loading requests:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleAccept = async (requestId: string) => {
    try {
      const response = await friendRequestService.acceptFriendRequest(requestId);
      if (response.success) {
        alert('Friend request accepted!');
        loadRequests(); // Reload the list
      }
    } catch (error) {
      console.error('Error accepting request:', error);
    }
  };

  const handleReject = async (requestId: string) => {
    try {
      const response = await friendRequestService.rejectFriendRequest(requestId);
      if (response.success) {
        alert('Friend request rejected');
        loadRequests(); // Reload the list
      }
    } catch (error) {
      console.error('Error rejecting request:', error);
    }
  };

  if (loading) return <div>Loading...</div>;

  if (requests.length === 0) {
    return <div>No pending friend requests</div>;
  }

  return (
    <div>
      <h2>Friend Requests ({requests.length})</h2>
      {requests.map((request) => (
        <div key={request.id} style={{ padding: '10px', border: '1px solid #ccc', margin: '10px 0' }}>
          <img
            src={request.senderAvatarUrl || '/default-avatar.png'}
            alt={request.senderDisplayName}
            style={{ width: 50, height: 50, borderRadius: '50%' }}
          />
          <h3>{request.senderDisplayName}</h3>
          <p>@{request.senderUsername}</p>
          {request.message && <p>"{request.message}"</p>}
          <p>Sent: {new Date(request.createdAt).toLocaleDateString()}</p>
          <button onClick={() => handleAccept(request.id)}>Accept</button>
          <button onClick={() => handleReject(request.id)}>Reject</button>
        </div>
      ))}
    </div>
  );
};
```

```tsx
// components/FriendsList.tsx
import React, { useState, useEffect } from 'react';
import { friendRequestService } from '../services/friendRequestService';

export const FriendsList: React.FC = () => {
  const [friends, setFriends] = useState<any[]>([]);
  const [stats, setStats] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadFriends();
    loadStats();
  }, []);

  const loadFriends = async () => {
    try {
      const response = await friendRequestService.getFriends(1, 50);
      if (response.success) {
        setFriends(response.data);
      }
    } catch (error) {
      console.error('Error loading friends:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadStats = async () => {
    try {
      const response = await friendRequestService.getStats();
      if (response.success) {
        setStats(response.data);
      }
    } catch (error) {
      console.error('Error loading stats:', error);
    }
  };

  if (loading) return <div>Loading...</div>;

  return (
    <div>
      {stats && (
        <div style={{ padding: '10px', backgroundColor: '#f0f0f0', marginBottom: '20px' }}>
          <h3>Statistics</h3>
          <p>Total Friends: {stats.totalFriends}</p>
          <p>Pending Requests Sent: {stats.pendingRequestsSent}</p>
          <p>Pending Requests Received: {stats.pendingRequestsReceived}</p>
        </div>
      )}

      <h2>Friends ({friends.length})</h2>
      {friends.length === 0 ? (
        <div>No friends yet. Start connecting with people!</div>
      ) : (
        friends.map((friend) => (
          <div key={friend.userId} style={{ padding: '10px', border: '1px solid #ccc', margin: '10px 0' }}>
            <img
              src={friend.avatarUrl || '/default-avatar.png'}
              alt={friend.displayName}
              style={{ width: 50, height: 50, borderRadius: '50%' }}
            />
            <h3>
              {friend.displayName}
              {friend.isVerified && <span> ✓</span>}
            </h3>
            <p>@{friend.username}</p>
            {friend.bio && <p>{friend.bio}</p>}
            <p>Friends since: {new Date(friend.friendshipDate).toLocaleDateString()}</p>
          </div>
        ))
      )}
    </div>
  );
};
```

---

### JavaScript/Fetch Integration

```javascript
// friendRequestService.js
const API_BASE_URL = 'http://localhost:5003/api/friendrequest';

// Get access token from storage
const getAuthToken = () => {
  return localStorage.getItem('accessToken') || '';
};

// Helper function for API calls
const apiCall = async (endpoint, options = {}) => {
  const token = getAuthToken();

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
      ...options.headers,
    },
  });

  return await response.json();
};

const friendRequestService = {
  // Send friend request
  async sendFriendRequest(userId, message) {
    return await apiCall(`/send/${userId}`, {
      method: 'POST',
      body: JSON.stringify({ message }),
    });
  },

  // Accept friend request
  async acceptFriendRequest(requestId) {
    return await apiCall(`/${requestId}/accept`, {
      method: 'POST',
    });
  },

  // Reject friend request
  async rejectFriendRequest(requestId) {
    return await apiCall(`/${requestId}/reject`, {
      method: 'POST',
    });
  },

  // Cancel friend request
  async cancelFriendRequest(requestId) {
    return await apiCall(`/${requestId}/cancel`, {
      method: 'DELETE',
    });
  },

  // Get received friend requests
  async getReceivedFriendRequests(page = 1, pageSize = 20) {
    return await apiCall(`/received?page=${page}&pageSize=${pageSize}`);
  },

  // Get sent friend requests
  async getSentFriendRequests(page = 1, pageSize = 20) {
    return await apiCall(`/sent?page=${page}&pageSize=${pageSize}`);
  },

  // Check friendship status
  async getFriendshipStatus(userId) {
    return await apiCall(`/status/${userId}`);
  },

  // Get friends list
  async getFriends(page = 1, pageSize = 20) {
    return await apiCall(`/friends?page=${page}&pageSize=${pageSize}`);
  },

  // Remove friend
  async removeFriend(userId) {
    return await apiCall(`/friends/${userId}`, {
      method: 'DELETE',
    });
  },

  // Get statistics
  async getStats() {
    return await apiCall('/stats');
  },
};

// Example usage
async function handleSendFriendRequest(userId) {
  try {
    const response = await friendRequestService.sendFriendRequest(
      userId,
      'Hi! Let\'s connect.'
    );

    if (response.success) {
      console.log('Friend request sent:', response.data);
      alert('Friend request sent successfully!');
    } else {
      console.error('Error:', response.error);
      alert(response.error);
    }
  } catch (error) {
    console.error('Network error:', error);
    alert('Network error. Please try again.');
  }
}
```

---

## Best Practices

### 1. Real-time Updates

Consider implementing real-time notifications for friend requests using SignalR or WebSockets:

```typescript
// Connect to notification hub
const connection = new signalR.HubConnectionBuilder()
  .withUrl('http://localhost:5009/hubs/notifications')
  .build();

// Listen for friend request notifications
connection.on('FriendRequestReceived', (notification) => {
  // Update UI
  showNotification(`${notification.senderName} sent you a friend request`);
  // Reload friend requests list
  loadFriendRequests();
});
```

### 2. Optimistic UI Updates

Update the UI immediately before the API call completes for better UX:

```typescript
const handleAcceptRequest = async (requestId: string) => {
  // Optimistically remove from list
  setRequests(prev => prev.filter(r => r.id !== requestId));

  try {
    const response = await friendRequestService.acceptFriendRequest(requestId);
    if (!response.success) {
      // Revert on error
      loadRequests();
      alert(response.error);
    }
  } catch (error) {
    // Revert on error
    loadRequests();
  }
};
```

### 3. Polling for Updates

If real-time is not available, poll for updates:

```typescript
useEffect(() => {
  const interval = setInterval(() => {
    loadStats(); // Check for new requests every 30 seconds
  }, 30000);

  return () => clearInterval(interval);
}, []);
```

### 4. Error Handling

Always handle errors gracefully:

```typescript
try {
  const response = await friendRequestService.sendFriendRequest(userId);
  if (response.success) {
    // Success
  } else {
    // Handle API error
    switch (response.error) {
      case 'You are already friends':
        showInfo('You are already friends with this user');
        break;
      case 'User has blocked you':
        showError('Unable to send friend request');
        break;
      default:
        showError(response.error);
    }
  }
} catch (error) {
  // Handle network error
  showError('Network error. Please try again.');
}
```

### 5. Caching

Cache friend lists and status to reduce API calls:

```typescript
const [friendsCache, setFriendsCache] = useState<Map<string, boolean>>(new Map());

const checkFriendship = async (userId: string): Promise<boolean> => {
  if (friendsCache.has(userId)) {
    return friendsCache.get(userId)!;
  }

  const response = await friendRequestService.getFriendshipStatus(userId);
  const areFriends = response.data.areFriends;

  setFriendsCache(prev => new Map(prev).set(userId, areFriends));
  return areFriends;
};
```

---

## Testing

### Sample Test Data

Use these sample GUIDs for testing (replace with actual user IDs in your environment):

```
User 1: 550e8400-e29b-41d4-a716-446655440000
User 2: 660e8400-e29b-41d4-a716-446655440001
User 3: 770e8400-e29b-41d4-a716-446655440002
```

### Test Flow

1. **Login as User 1**
2. **Send friend request to User 2**
   ```bash
   curl -X POST http://localhost:5003/api/friendrequest/send/660e8400-e29b-41d4-a716-446655440001 \
     -H "Authorization: Bearer {user1_token}" \
     -H "Content-Type: application/json" \
     -d '{"message": "Hi! Let'\''s connect."}'
   ```

3. **Login as User 2**
4. **Get received requests**
   ```bash
   curl http://localhost:5003/api/friendrequest/received \
     -H "Authorization: Bearer {user2_token}"
   ```

5. **Accept the request**
   ```bash
   curl -X POST http://localhost:5003/api/friendrequest/{requestId}/accept \
     -H "Authorization: Bearer {user2_token}"
   ```

6. **Verify friendship**
   ```bash
   curl http://localhost:5003/api/friendrequest/friends \
     -H "Authorization: Bearer {user2_token}"
   ```

---

## Support

For issues or questions:
- Backend API: Check UserProfileService.Api logs
- MongoDB: Check `UserProfiles` database, `FriendRequests` and `Friendships` collections
- Authentication: Ensure valid JWT token in Authorization header

---

**Version**: 1.0
**Last Updated**: 2025-11-27
**Service**: UserProfileService.Api (Port 5003)
