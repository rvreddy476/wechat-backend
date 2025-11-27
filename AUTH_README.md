# Authentication API - Integration Guide

Complete guide for integrating Login and Registration endpoints into your UI application.

---

## Table of Contents

1. [Overview](#overview)
2. [Base URL](#base-url)
3. [Response Format](#response-format)
4. [Authentication Flow](#authentication-flow)
5. [API Endpoints](#api-endpoints)
   - [Register](#1-register)
   - [Login](#2-login)
   - [Refresh Token](#3-refresh-token)
   - [Get Current User](#4-get-current-user)
   - [Change Password](#5-change-password)
   - [Forgot Password](#6-forgot-password)
   - [Reset Password](#7-reset-password)
   - [Verify Email](#8-verify-email)
   - [Logout](#9-logout)
6. [Token Management](#token-management)
7. [Error Handling](#error-handling)
8. [Integration Examples](#integration-examples)

---

## Overview

The WeChat Authentication API provides secure user authentication using JWT (JSON Web Tokens). It supports:

- User registration with email verification
- Login with username or email
- Access token and refresh token mechanism
- Password management (change, forgot, reset)
- Email verification
- Secure logout

**Authentication Method**: Bearer Token (JWT)

---

## Base URL

| Environment | URL |
|-------------|-----|
| Development | `http://localhost:5001` |
| Production  | `https://api.wechat.com` |

**Auth Service Port**: `5001`

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

## Authentication Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    Registration Flow                         │
└─────────────────────────────────────────────────────────────┘
1. POST /api/auth/register
   → Returns: userId, username, email
   → Email verification token sent

2. GET /api/auth/verify-email?token=xxx
   → Email verified
   → User can now login

┌─────────────────────────────────────────────────────────────┐
│                      Login Flow                              │
└─────────────────────────────────────────────────────────────┘
1. POST /api/auth/login
   → Returns: accessToken, refreshToken
   → Store tokens securely

2. Use accessToken in Authorization header:
   Authorization: Bearer <accessToken>

3. When accessToken expires (15 minutes):
   POST /api/auth/refresh
   → Returns: new accessToken, new refreshToken

4. Logout:
   POST /api/auth/logout
   → Revokes all tokens
```

---

## API Endpoints

### 1. Register

Create a new user account.

**Endpoint**: `POST /api/auth/register`
**Authentication**: Not required

#### Request Body

```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "phoneNumber": "+1234567890"  // Optional
}
```

#### Request Schema

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| username | string | ✅ | Min 3 characters |
| email | string | ✅ | Valid email format |
| password | string | ✅ | Min 8 characters |
| phoneNumber | string | ❌ | Valid phone format |

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "email": "john@example.com",
    "message": "Registration successful. Please verify your email."
  },
  "error": null,
  "errors": null,
  "timestamp": "2025-11-27T10:30:00Z"
}
```

#### Response Schema

| Field | Type | Description |
|-------|------|-------------|
| userId | string (GUID) | Unique user identifier |
| username | string | Username |
| email | string | Email address |
| message | string | Success message |

#### Error Responses

| Status | Error Message |
|--------|---------------|
| 400 | "Username must be at least 3 characters" |
| 400 | "Valid email is required" |
| 400 | "Password must be at least 8 characters" |
| 400 | "Email already registered" |
| 400 | "Username already taken" |

---

### 2. Login

Authenticate user and receive access tokens.

**Endpoint**: `POST /api/auth/login`
**Authentication**: Not required

#### Request Body

```json
{
  "emailOrUsername": "john@example.com",  // or "johndoe"
  "password": "SecurePassword123!"
}
```

#### Request Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| emailOrUsername | string | ✅ | Email or username |
| password | string | ✅ | User password |

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "email": "john@example.com",
    "emailVerified": true,
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "dGhpc2lzYXJlZnJlc2h0b2tlbg==",
    "expiresAt": "2025-11-27T10:45:00Z",
    "roles": ["User"]
  },
  "error": null,
  "errors": null,
  "timestamp": "2025-11-27T10:30:00Z"
}
```

#### Response Schema

| Field | Type | Description |
|-------|------|-------------|
| userId | string (GUID) | Unique user identifier |
| username | string | Username |
| email | string | Email address |
| emailVerified | boolean | Email verification status |
| accessToken | string | JWT access token (15 min validity) |
| refreshToken | string | Refresh token (7 days validity) |
| expiresAt | datetime | Access token expiration time |
| roles | string[] | User roles |

#### Error Responses

| Status | Error Message |
|--------|---------------|
| 400 | "Email/Username and password are required" |
| 401 | "Invalid credentials" |
| 401 | "Account is locked until {datetime}" |

#### Important Notes

- **Access Token Expiry**: 15 minutes
- **Refresh Token Expiry**: 7 days
- Store tokens securely (localStorage, sessionStorage, or secure cookie)
- Use accessToken for all authenticated requests

---

### 3. Refresh Token

Get new access token using refresh token.

**Endpoint**: `POST /api/auth/refresh`
**Authentication**: Not required (uses refresh token)

#### Request Body

```json
{
  "refreshToken": "dGhpc2lzYXJlZnJlc2h0b2tlbg=="
}
```

#### Request Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| refreshToken | string | ✅ | Valid refresh token |

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "bmV3cmVmcmVzaHRva2VuaGVyZQ==",
    "expiresAt": "2025-11-27T11:00:00Z"
  },
  "error": null,
  "errors": null,
  "timestamp": "2025-11-27T10:45:00Z"
}
```

#### Response Schema

| Field | Type | Description |
|-------|------|-------------|
| accessToken | string | New JWT access token |
| refreshToken | string | New refresh token (rotated) |
| expiresAt | datetime | New access token expiration |

#### Error Responses

| Status | Error Message |
|--------|---------------|
| 400 | "Refresh token is required" |
| 401 | "Token has been revoked" |
| 401 | "Token has expired" |
| 401 | "User not found" |

#### Important Notes

- **Token Rotation**: Each refresh generates NEW tokens, old ones are revoked
- Implement automatic token refresh before expiry
- Handle refresh failure by redirecting to login

---

### 4. Get Current User

Get authenticated user information.

**Endpoint**: `GET /api/auth/me`
**Authentication**: Required (Bearer Token)

#### Request Headers

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "isEmailVerified": true,
    "isPhoneVerified": false,
    "isActive": true,
    "isDeleted": false,
    "bio": "Software developer",
    "avatarUrl": "https://storage.googleapis.com/wechat-media/avatars/user123.jpg",
    "createdAt": "2025-01-20T10:00:00Z",
    "updatedAt": "2025-11-27T10:30:00Z",
    "lastLoginAt": "2025-11-27T09:00:00Z",
    "roles": ["User"]
  },
  "error": null,
  "errors": null,
  "timestamp": "2025-11-27T10:30:00Z"
}
```

#### Error Responses

| Status | Error Message |
|--------|---------------|
| 401 | "Invalid token" |
| 404 | "User not found" |

---

### 5. Change Password

Change password for authenticated user.

**Endpoint**: `POST /api/auth/change-password`
**Authentication**: Required (Bearer Token)

#### Request Headers

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Request Body

```json
{
  "oldPassword": "CurrentPassword123!",
  "newPassword": "NewSecurePassword456!"
}
```

#### Request Schema

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| oldPassword | string | ✅ | Current password |
| newPassword | string | ✅ | Min 8 characters |

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": true,
  "error": null,
  "errors": null,
  "timestamp": "2025-11-27T10:30:00Z"
}
```

#### Error Responses

| Status | Error Message |
|--------|---------------|
| 400 | "Current and new passwords are required" |
| 400 | "New password must be at least 8 characters" |
| 400 | "Current password is incorrect" |
| 401 | "Invalid token" |

#### Important Notes

- All refresh tokens are revoked after password change
- User must login again with new password

---

### 6. Forgot Password

Request password reset token.

**Endpoint**: `POST /api/auth/forgot-password`
**Authentication**: Not required

#### Request Body

```json
{
  "email": "john@example.com"
}
```

#### Request Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| email | string | ✅ | Registered email address |

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": "If the email exists, a password reset link has been sent",
  "error": null,
  "errors": null,
  "timestamp": "2025-11-27T10:30:00Z"
}
```

#### Error Responses

| Status | Error Message |
|--------|---------------|
| 400 | "Email is required" |

#### Important Notes

- Same response whether email exists or not (security best practice)
- Reset token valid for 1 hour
- Email contains reset link with token

---

### 7. Reset Password

Reset password using token from email.

**Endpoint**: `POST /api/auth/reset-password`
**Authentication**: Not required (uses reset token)

#### Request Body

```json
{
  "token": "dGhpc2lzYXJlc2V0dG9rZW4=",
  "newPassword": "NewSecurePassword456!"
}
```

#### Request Schema

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| token | string | ✅ | Reset token from email |
| newPassword | string | ✅ | Min 8 characters |

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": true,
  "error": null,
  "errors": null,
  "timestamp": "2025-11-27T10:30:00Z"
}
```

#### Error Responses

| Status | Error Message |
|--------|---------------|
| 400 | "Token and new password are required" |
| 400 | "Password must be at least 8 characters" |
| 400 | "Invalid or expired token" |

#### Important Notes

- Token is single-use only
- All refresh tokens are revoked after reset
- User must login with new password

---

### 8. Verify Email

Verify user email address using token.

**Endpoint**: `GET /api/auth/verify-email?token=xxx`
**Authentication**: Not required

#### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| token | string | ✅ | Verification token from email |

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": true,
  "error": null,
  "errors": null,
  "timestamp": "2025-11-27T10:30:00Z"
}
```

#### Error Responses

| Status | Error Message |
|--------|---------------|
| 400 | "Token is required" |
| 400 | "Invalid or expired token" |

#### Important Notes

- Token valid for 24 hours
- Token is single-use only
- Redirect user to login page after verification

---

### 9. Logout

Logout user and revoke all tokens.

**Endpoint**: `POST /api/auth/logout`
**Authentication**: Required (Bearer Token)

#### Request Headers

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": true,
  "error": null,
  "errors": null,
  "timestamp": "2025-11-27T10:30:00Z"
}
```

#### Error Responses

| Status | Error Message |
|--------|---------------|
| 401 | "Invalid token" |

#### Important Notes

- Revokes all refresh tokens for the user
- Clear all stored tokens from client
- Redirect to login page

---

## Token Management

### Storing Tokens

**Recommended Approaches**:

1. **localStorage** (Simple, but vulnerable to XSS)
```javascript
localStorage.setItem('accessToken', token);
localStorage.setItem('refreshToken', refreshToken);
```

2. **sessionStorage** (Cleared on tab close)
```javascript
sessionStorage.setItem('accessToken', token);
sessionStorage.setItem('refreshToken', refreshToken);
```

3. **HttpOnly Cookies** (Most secure, requires backend support)
```javascript
// Backend sets cookie with httpOnly, secure, sameSite flags
```

### Using Access Token

Include in every authenticated request:

```javascript
// Axios example
axios.get('/api/profile/me', {
  headers: {
    'Authorization': `Bearer ${accessToken}`
  }
});

// Fetch example
fetch('/api/profile/me', {
  headers: {
    'Authorization': `Bearer ${accessToken}`
  }
});
```

### Automatic Token Refresh

Implement automatic refresh before expiry:

```javascript
// Calculate time until expiry
const expiresAt = new Date(loginResponse.expiresAt);
const now = new Date();
const timeUntilExpiry = expiresAt - now;

// Refresh 1 minute before expiry
const refreshTime = timeUntilExpiry - (60 * 1000);

setTimeout(async () => {
  const response = await fetch('/api/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken })
  });

  if (response.ok) {
    const data = await response.json();
    // Store new tokens
    updateTokens(data.data.accessToken, data.data.refreshToken);
  } else {
    // Refresh failed, redirect to login
    redirectToLogin();
  }
}, refreshTime);
```

### Token Expiry Handling

Handle 401 responses globally:

```javascript
// Axios interceptor
axios.interceptors.response.use(
  response => response,
  async error => {
    if (error.response?.status === 401) {
      // Try to refresh token
      try {
        const refreshToken = getRefreshToken();
        const response = await refreshAccessToken(refreshToken);

        // Retry original request with new token
        error.config.headers['Authorization'] = `Bearer ${response.accessToken}`;
        return axios(error.config);
      } catch (refreshError) {
        // Refresh failed, logout user
        logout();
        redirectToLogin();
      }
    }
    return Promise.reject(error);
  }
);
```

---

## Error Handling

### HTTP Status Codes

| Code | Meaning | Action |
|------|---------|--------|
| 200 | Success | Process response data |
| 400 | Bad Request | Show validation errors to user |
| 401 | Unauthorized | Refresh token or redirect to login |
| 403 | Forbidden | Show "Access Denied" message |
| 404 | Not Found | Show "Resource not found" |
| 500 | Server Error | Show generic error message |

### Error Response Structure

```javascript
{
  success: false,
  data: null,
  error: "Main error message",
  errors: ["Detailed error 1", "Detailed error 2"],  // Optional
  timestamp: "2025-11-27T10:30:00Z"
}
```

### Example Error Handling

```javascript
async function login(emailOrUsername, password) {
  try {
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ emailOrUsername, password })
    });

    const result = await response.json();

    if (result.success) {
      // Success - store tokens
      localStorage.setItem('accessToken', result.data.accessToken);
      localStorage.setItem('refreshToken', result.data.refreshToken);
      return { success: true, data: result.data };
    } else {
      // API returned error
      return {
        success: false,
        error: result.error,
        errors: result.errors
      };
    }
  } catch (error) {
    // Network or other error
    return {
      success: false,
      error: 'Network error. Please try again.'
    };
  }
}
```

---

## Integration Examples

### React/TypeScript Example

```typescript
// types/auth.types.ts
export interface LoginRequest {
  emailOrUsername: string;
  password: string;
}

export interface LoginResponse {
  userId: string;
  username: string;
  email: string;
  emailVerified: boolean;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  roles: string[];
}

export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  error: string | null;
  errors: string[] | null;
  timestamp: string;
}

// services/authService.ts
const API_BASE_URL = 'http://localhost:5001/api/auth';

export const authService = {
  async register(username: string, email: string, password: string) {
    const response = await fetch(`${API_BASE_URL}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, email, password })
    });
    return await response.json();
  },

  async login(emailOrUsername: string, password: string):
    Promise<ApiResponse<LoginResponse>> {
    const response = await fetch(`${API_BASE_URL}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ emailOrUsername, password })
    });
    return await response.json();
  },

  async refreshToken(refreshToken: string) {
    const response = await fetch(`${API_BASE_URL}/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken })
    });
    return await response.json();
  },

  async getCurrentUser(accessToken: string) {
    const response = await fetch(`${API_BASE_URL}/me`, {
      headers: { 'Authorization': `Bearer ${accessToken}` }
    });
    return await response.json();
  },

  async logout(accessToken: string) {
    const response = await fetch(`${API_BASE_URL}/logout`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${accessToken}` }
    });
    return await response.json();
  }
};

// hooks/useAuth.ts
import { useState, useEffect } from 'react';
import { authService } from '../services/authService';

export const useAuth = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      authService.getCurrentUser(token)
        .then(result => {
          if (result.success) {
            setUser(result.data);
          }
        })
        .finally(() => setLoading(false));
    } else {
      setLoading(false);
    }
  }, []);

  const login = async (emailOrUsername: string, password: string) => {
    const result = await authService.login(emailOrUsername, password);
    if (result.success) {
      localStorage.setItem('accessToken', result.data.accessToken);
      localStorage.setItem('refreshToken', result.data.refreshToken);
      setUser(result.data);
    }
    return result;
  };

  const logout = async () => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      await authService.logout(token);
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    setUser(null);
  };

  return { user, loading, login, logout };
};

// components/LoginForm.tsx
import React, { useState } from 'react';
import { useAuth } from '../hooks/useAuth';

export const LoginForm: React.FC = () => {
  const [emailOrUsername, setEmailOrUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const { login } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    const result = await login(emailOrUsername, password);
    if (!result.success) {
      setError(result.error || 'Login failed');
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      {error && <div className="error">{error}</div>}

      <input
        type="text"
        placeholder="Email or Username"
        value={emailOrUsername}
        onChange={e => setEmailOrUsername(e.target.value)}
        required
      />

      <input
        type="password"
        placeholder="Password"
        value={password}
        onChange={e => setPassword(e.target.value)}
        required
      />

      <button type="submit">Login</button>
    </form>
  );
};
```

### JavaScript/Fetch Example

```javascript
// auth.js
class AuthManager {
  constructor(baseURL = 'http://localhost:5001/api/auth') {
    this.baseURL = baseURL;
  }

  async register(username, email, password) {
    const response = await fetch(`${this.baseURL}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, email, password })
    });
    return await response.json();
  }

  async login(emailOrUsername, password) {
    const response = await fetch(`${this.baseURL}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ emailOrUsername, password })
    });

    const result = await response.json();

    if (result.success) {
      localStorage.setItem('accessToken', result.data.accessToken);
      localStorage.setItem('refreshToken', result.data.refreshToken);
      localStorage.setItem('tokenExpiry', result.data.expiresAt);
    }

    return result;
  }

  async refreshToken() {
    const refreshToken = localStorage.getItem('refreshToken');
    const response = await fetch(`${this.baseURL}/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken })
    });

    const result = await response.json();

    if (result.success) {
      localStorage.setItem('accessToken', result.data.accessToken);
      localStorage.setItem('refreshToken', result.data.refreshToken);
      localStorage.setItem('tokenExpiry', result.data.expiresAt);
    }

    return result;
  }

  getAccessToken() {
    return localStorage.getItem('accessToken');
  }

  async logout() {
    const token = this.getAccessToken();
    if (token) {
      await fetch(`${this.baseURL}/logout`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` }
      });
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenExpiry');
  }

  isAuthenticated() {
    const token = this.getAccessToken();
    const expiry = localStorage.getItem('tokenExpiry');

    if (!token || !expiry) return false;

    return new Date(expiry) > new Date();
  }
}

// Usage
const auth = new AuthManager();

// Login
const loginResult = await auth.login('john@example.com', 'password123');
if (loginResult.success) {
  console.log('Logged in:', loginResult.data.username);
}

// Logout
await auth.logout();
```

---

## Security Best Practices

1. **HTTPS Only**: Always use HTTPS in production
2. **Token Storage**: Use httpOnly cookies or secure storage
3. **Token Expiry**: Implement automatic token refresh
4. **CORS**: Configure allowed origins properly
5. **XSS Protection**: Sanitize user inputs
6. **CSRF Protection**: Use CSRF tokens for state-changing operations
7. **Rate Limiting**: Backend implements rate limiting
8. **Password Strength**: Enforce strong passwords (min 8 chars)
9. **Account Lockout**: Automated lockout after failed attempts
10. **Secure Transmission**: Never log or expose tokens

---

## Support

For issues or questions:
- Check the API documentation: `API_DESIGN_DOCUMENT.md`
- Review error messages in the response
- Ensure proper request format and authentication headers
- Verify token expiry and refresh mechanism

---

**Last Updated**: 2025-11-27
**API Version**: 1.0
**Auth Service Port**: 5001
