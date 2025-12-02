# Auth API - Complete Request & Response Documentation

> **Purpose**: Complete API documentation with request/response examples for authentication and authorization
> **Last Updated**: 2025-12-02
> **Base URL**: `https://api.yourapp.com/api/v1`

---

## Table of Contents
1. [Authentication](#authentication)
2. [User Registration](#user-registration)
3. [Email Verification](#email-verification)
4. [User Login](#user-login)
5. [Token Management](#token-management)
6. [Password Management](#password-management)
7. [User Profile](#user-profile)
8. [Account Management](#account-management)
9. [Session Management](#session-management)
10. [Error Responses](#error-responses)

---

## Authentication

Most endpoints in this service require JWT Bearer authentication.

**Headers Required**:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

---

## User Registration

### 1. Check Email Availability

**Endpoint**: `GET /api/v1/auth/check-email`

**Description**: Check if an email is already registered

**Request**:
```http
GET /api/v1/auth/check-email?email=john@example.com HTTP/1.1
Host: api.yourapp.com
Content-Type: application/json
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "email": "john@example.com",
    "available": true
  }
}
```

**Email Already Exists**:
```json
{
  "success": true,
  "data": {
    "email": "john@example.com",
    "available": false,
    "message": "Email is already registered"
  }
}
```

---

### 2. Check Username Availability

**Endpoint**: `GET /api/v1/auth/check-username`

**Description**: Check if a username is already taken

**Request**:
```http
GET /api/v1/auth/check-username?username=john_doe HTTP/1.1
Host: api.yourapp.com
Content-Type: application/json
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "username": "john_doe",
    "available": true
  }
}
```

**Username Already Taken**:
```json
{
  "success": true,
  "data": {
    "username": "john_doe",
    "available": false,
    "message": "Username is already taken"
  }
}
```

---

### 3. Register New User

**Endpoint**: `POST /api/v1/auth/register`

**Description**: Register a new user account

**Request**:
```http
POST /api/v1/auth/register HTTP/1.1
Host: api.yourapp.com
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "phoneNumber": "+1234567890",
  "firstName": "John",
  "lastName": "Doe",
  "agreeToTerms": true
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Registration successful. Please check your email to verify your account.",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "email": "john@example.com",
    "emailVerificationSent": true,
    "verificationCodeExpiresAt": "2025-12-02T11:15:00Z",
    "createdAt": "2025-12-02T11:00:00Z"
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "statusCode": 400,
    "details": [
      {
        "field": "password",
        "message": "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character"
      },
      {
        "field": "email",
        "message": "Invalid email format"
      }
    ]
  }
}
```

**Error Response** (409 Conflict):
```json
{
  "success": false,
  "error": {
    "code": "EMAIL_ALREADY_EXISTS",
    "message": "An account with this email already exists",
    "statusCode": 409
  }
}
```

---

## Email Verification

### 4. Verify Email

**Endpoint**: `POST /api/v1/auth/verify-email`

**Description**: Verify user email with verification code

**Request**:
```http
POST /api/v1/auth/verify-email HTTP/1.1
Host: api.yourapp.com
Content-Type: application/json

{
  "email": "john@example.com",
  "verificationCode": "123456"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Email verified successfully",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john@example.com",
    "isEmailVerified": true,
    "verifiedAt": "2025-12-02T11:10:00Z"
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "INVALID_CODE",
    "message": "Invalid or expired verification code",
    "statusCode": 400
  }
}
```

---

### 5. Resend Verification Code

**Endpoint**: `POST /api/v1/auth/resend-verification`

**Description**: Resend email verification code

**Request**:
```http
POST /api/v1/auth/resend-verification HTTP/1.1
Host: api.yourapp.com
Content-Type: application/json

{
  "email": "john@example.com"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Verification code sent successfully",
  "data": {
    "email": "john@example.com",
    "codeSentAt": "2025-12-02T11:15:00Z",
    "expiresAt": "2025-12-02T11:30:00Z"
  }
}
```

**Error Response** (429 Too Many Requests):
```json
{
  "success": false,
  "error": {
    "code": "TOO_MANY_ATTEMPTS",
    "message": "Too many verification attempts. Please try again in 5 minutes.",
    "statusCode": 429,
    "retryAfter": 300
  }
}
```

---

## User Login

### 6. Login

**Endpoint**: `POST /api/v1/auth/login`

**Description**: Authenticate user and receive access/refresh tokens

**Request**:
```http
POST /api/v1/auth/login HTTP/1.1
Host: api.yourapp.com
Content-Type: application/json

{
  "login": "john@example.com",
  "password": "SecurePassword123!",
  "deviceInfo": {
    "deviceType": "Web",
    "browser": "Chrome 120",
    "os": "Windows 10",
    "ipAddress": "192.168.1.100"
  }
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "email": "john@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "isEmailVerified": true,
      "profilePictureUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg"
    },
    "tokens": {
      "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDAiLCJ1c2VybmFtZSI6ImpvaG5fZG9lIiwiaWF0IjoxNzAxNTE4NDAwLCJleHAiOjE3MDE1MjIwMDB9.signature",
      "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDAiLCJ0b2tlbklkIjoiYWJjZGVmZ2gifQ.signature",
      "accessTokenExpiresAt": "2025-12-02T12:00:00Z",
      "refreshTokenExpiresAt": "2025-12-09T11:00:00Z",
      "tokenType": "Bearer"
    },
    "session": {
      "sessionId": "session-67890abcdef1234567890abc",
      "deviceInfo": {
        "deviceType": "Web",
        "browser": "Chrome 120",
        "os": "Windows 10",
        "ipAddress": "192.168.1.100"
      },
      "createdAt": "2025-12-02T11:00:00Z"
    }
  }
}
```

**Error Response** (401 Unauthorized):
```json
{
  "success": false,
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "Invalid email or password",
    "statusCode": 401
  }
}
```

**Error Response** (403 Forbidden - Email Not Verified):
```json
{
  "success": false,
  "error": {
    "code": "EMAIL_NOT_VERIFIED",
    "message": "Please verify your email before logging in",
    "statusCode": 403,
    "data": {
      "email": "john@example.com",
      "resendVerificationUrl": "/api/v1/auth/resend-verification"
    }
  }
}
```

**Error Response** (403 Forbidden - Account Locked):
```json
{
  "success": false,
  "error": {
    "code": "ACCOUNT_LOCKED",
    "message": "Your account has been locked due to multiple failed login attempts. Please try again in 30 minutes.",
    "statusCode": 403,
    "lockExpiresAt": "2025-12-02T11:30:00Z"
  }
}
```

---

### 7. Login with Username

**Endpoint**: `POST /api/v1/auth/login`

**Description**: Login using username instead of email

**Request**:
```http
POST /api/v1/auth/login HTTP/1.1
Host: api.yourapp.com
Content-Type: application/json

{
  "login": "john_doe",
  "password": "SecurePassword123!",
  "deviceInfo": {
    "deviceType": "Mobile",
    "device": "iPhone 15 Pro",
    "os": "iOS 17.1"
  }
}
```

**Success Response**: Same as email login (200 OK)

---

## Token Management

### 8. Refresh Access Token

**Endpoint**: `POST /api/v1/auth/refresh-token`

**Description**: Get a new access token using refresh token

**Request**:
```http
POST /api/v1/auth/refresh-token HTTP/1.1
Host: api.yourapp.com
Content-Type: application/json

{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDAiLCJ0b2tlbklkIjoiYWJjZGVmZ2gifQ.signature"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.newAccessToken.signature",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.newRefreshToken.signature",
    "accessTokenExpiresAt": "2025-12-02T13:00:00Z",
    "refreshTokenExpiresAt": "2025-12-09T12:00:00Z",
    "tokenType": "Bearer"
  }
}
```

**Error Response** (401 Unauthorized):
```json
{
  "success": false,
  "error": {
    "code": "INVALID_TOKEN",
    "message": "Invalid or expired refresh token",
    "statusCode": 401
  }
}
```

---

### 9. Validate Token

**Endpoint**: `GET /api/v1/auth/validate-token`

**Description**: Validate if current access token is valid

**Request**:
```http
GET /api/v1/auth/validate-token HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "valid": true,
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "expiresAt": "2025-12-02T12:00:00Z"
  }
}
```

**Error Response** (401 Unauthorized):
```json
{
  "success": false,
  "error": {
    "code": "INVALID_TOKEN",
    "message": "Token is invalid or expired",
    "statusCode": 401
  }
}
```

---

### 10. Logout

**Endpoint**: `POST /api/v1/auth/logout`

**Description**: Logout and invalidate current session

**Request**:
```http
POST /api/v1/auth/logout HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "allDevices": false
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Logged out successfully",
  "data": {
    "loggedOutAt": "2025-12-02T14:00:00Z"
  }
}
```

---

### 11. Logout All Devices

**Endpoint**: `POST /api/v1/auth/logout`

**Description**: Logout from all devices and invalidate all sessions

**Request**:
```http
POST /api/v1/auth/logout HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "allDevices": true
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Logged out from all devices successfully",
  "data": {
    "sessionsRevoked": 5,
    "loggedOutAt": "2025-12-02T14:05:00Z"
  }
}
```

---

## Password Management

### 12. Request Password Reset

**Endpoint**: `POST /api/v1/auth/forgot-password`

**Description**: Request a password reset code

**Request**:
```http
POST /api/v1/auth/forgot-password HTTP/1.1
Host: api.yourapp.com
Content-Type: application/json

{
  "email": "john@example.com"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Password reset code sent to your email",
  "data": {
    "email": "john@example.com",
    "resetCodeSentAt": "2025-12-02T14:10:00Z",
    "expiresAt": "2025-12-02T14:25:00Z"
  }
}
```

**Note**: For security, always return success even if email doesn't exist

---

### 13. Verify Reset Code

**Endpoint**: `POST /api/v1/auth/verify-reset-code`

**Description**: Verify password reset code is valid

**Request**:
```http
POST /api/v1/auth/verify-reset-code HTTP/1.1
Host: api.yourapp.com
Content-Type: application/json

{
  "email": "john@example.com",
  "resetCode": "789012"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Reset code is valid",
  "data": {
    "valid": true,
    "resetToken": "temp-reset-token-abc123",
    "expiresAt": "2025-12-02T14:25:00Z"
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "INVALID_RESET_CODE",
    "message": "Invalid or expired reset code",
    "statusCode": 400
  }
}
```

---

### 14. Reset Password

**Endpoint**: `POST /api/v1/auth/reset-password`

**Description**: Reset password with verified reset token

**Request**:
```http
POST /api/v1/auth/reset-password HTTP/1.1
Host: api.yourapp.com
Content-Type: application/json

{
  "resetToken": "temp-reset-token-abc123",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Password reset successfully",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "passwordChangedAt": "2025-12-02T14:20:00Z",
    "allSessionsRevoked": true
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "statusCode": 400,
    "details": [
      {
        "field": "newPassword",
        "message": "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character"
      }
    ]
  }
}
```

---

### 15. Change Password

**Endpoint**: `PUT /api/v1/auth/change-password`

**Description**: Change password for authenticated user

**Request**:
```http
PUT /api/v1/auth/change-password HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "currentPassword": "SecurePassword123!",
  "newPassword": "NewSecurePassword456!",
  "confirmPassword": "NewSecurePassword456!"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Password changed successfully",
  "data": {
    "passwordChangedAt": "2025-12-02T14:30:00Z",
    "otherSessionsRevoked": true
  }
}
```

**Error Response** (401 Unauthorized):
```json
{
  "success": false,
  "error": {
    "code": "INVALID_PASSWORD",
    "message": "Current password is incorrect",
    "statusCode": 401
  }
}
```

---

## User Profile

### 16. Get Current User

**Endpoint**: `GET /api/v1/auth/me`

**Description**: Get authenticated user's information

**Request**:
```http
GET /api/v1/auth/me HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1234567890",
    "isEmailVerified": true,
    "isPhoneVerified": false,
    "profilePictureUrl": "https://cdn.yourapp.com/avatars/john_doe.jpg",
    "role": "User",
    "accountStatus": "Active",
    "lastLoginAt": "2025-12-02T11:00:00Z",
    "createdAt": "2023-01-15T08:00:00Z",
    "updatedAt": "2025-12-02T11:00:00Z"
  }
}
```

---

### 17. Update User Profile

**Endpoint**: `PUT /api/v1/auth/me`

**Description**: Update authenticated user's basic information

**Request**:
```http
PUT /api/v1/auth/me HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe Jr.",
  "phoneNumber": "+1234567890"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "firstName": "John",
    "lastName": "Doe Jr.",
    "phoneNumber": "+1234567890",
    "updatedAt": "2025-12-02T14:40:00Z"
  }
}
```

---

### 18. Update Email

**Endpoint**: `PUT /api/v1/auth/update-email`

**Description**: Request to update email (requires verification)

**Request**:
```http
PUT /api/v1/auth/update-email HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "newEmail": "newemail@example.com",
  "password": "SecurePassword123!"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Verification code sent to new email address",
  "data": {
    "newEmail": "newemail@example.com",
    "verificationCodeSentAt": "2025-12-02T14:45:00Z",
    "expiresAt": "2025-12-02T15:00:00Z"
  }
}
```

---

### 19. Confirm Email Update

**Endpoint**: `POST /api/v1/auth/confirm-email-update`

**Description**: Confirm email update with verification code

**Request**:
```http
POST /api/v1/auth/confirm-email-update HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "verificationCode": "456789"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Email updated successfully",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "newemail@example.com",
    "isEmailVerified": true,
    "updatedAt": "2025-12-02T14:50:00Z"
  }
}
```

---

## Account Management

### 20. Deactivate Account

**Endpoint**: `POST /api/v1/auth/deactivate-account`

**Description**: Temporarily deactivate user account

**Request**:
```http
POST /api/v1/auth/deactivate-account HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "password": "SecurePassword123!",
  "reason": "Taking a break"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Account deactivated successfully",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "deactivatedAt": "2025-12-02T15:00:00Z",
    "reactivationInfo": "You can reactivate your account anytime by logging in"
  }
}
```

---

### 21. Delete Account

**Endpoint**: `DELETE /api/v1/auth/delete-account`

**Description**: Permanently delete user account (30-day grace period)

**Request**:
```http
DELETE /api/v1/auth/delete-account HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "password": "SecurePassword123!",
  "confirmationText": "DELETE MY ACCOUNT"
}
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Account deletion scheduled. Your account will be permanently deleted in 30 days.",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "scheduledDeletionDate": "2026-01-01T15:00:00Z",
    "cancellationDeadline": "2026-01-01T15:00:00Z",
    "cancellationInfo": "You can cancel account deletion by logging in before the deadline"
  }
}
```

---

## Session Management

### 22. Get Active Sessions

**Endpoint**: `GET /api/v1/auth/sessions`

**Description**: Get all active sessions for authenticated user

**Request**:
```http
GET /api/v1/auth/sessions HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "sessions": [
      {
        "sessionId": "session-67890abcdef1234567890abc",
        "deviceInfo": {
          "deviceType": "Web",
          "browser": "Chrome 120",
          "os": "Windows 10",
          "ipAddress": "192.168.1.100"
        },
        "location": {
          "city": "San Francisco",
          "state": "CA",
          "country": "United States"
        },
        "isCurrentSession": true,
        "createdAt": "2025-12-02T11:00:00Z",
        "lastActivityAt": "2025-12-02T15:00:00Z"
      },
      {
        "sessionId": "session-67890abcdef1234567890def",
        "deviceInfo": {
          "deviceType": "Mobile",
          "device": "iPhone 15 Pro",
          "os": "iOS 17.1",
          "ipAddress": "192.168.1.105"
        },
        "location": {
          "city": "San Francisco",
          "state": "CA",
          "country": "United States"
        },
        "isCurrentSession": false,
        "createdAt": "2025-12-01T09:00:00Z",
        "lastActivityAt": "2025-12-01T22:00:00Z"
      }
    ],
    "count": 2
  }
}
```

---

### 23. Revoke Session

**Endpoint**: `DELETE /api/v1/auth/sessions/{sessionId}`

**Description**: Revoke a specific session

**Request**:
```http
DELETE /api/v1/auth/sessions/session-67890abcdef1234567890def HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "message": "Session revoked successfully",
  "data": {
    "sessionId": "session-67890abcdef1234567890def",
    "revokedAt": "2025-12-02T15:10:00Z"
  }
}
```

---

### 24. Get Login History

**Endpoint**: `GET /api/v1/auth/login-history`

**Description**: Get login history for authenticated user

**Request**:
```http
GET /api/v1/auth/login-history?page=1&limit=20 HTTP/1.1
Host: api.yourapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "history": [
      {
        "loginId": "login-67890abcdef1234567890001",
        "loginStatus": "Success",
        "deviceInfo": {
          "deviceType": "Web",
          "browser": "Chrome 120",
          "os": "Windows 10",
          "ipAddress": "192.168.1.100"
        },
        "location": {
          "city": "San Francisco",
          "state": "CA",
          "country": "United States"
        },
        "loginAt": "2025-12-02T11:00:00Z"
      },
      {
        "loginId": "login-67890abcdef1234567890002",
        "loginStatus": "Success",
        "deviceInfo": {
          "deviceType": "Mobile",
          "device": "iPhone 15 Pro",
          "os": "iOS 17.1",
          "ipAddress": "192.168.1.105"
        },
        "location": {
          "city": "San Francisco",
          "state": "CA",
          "country": "United States"
        },
        "loginAt": "2025-12-01T09:00:00Z"
      },
      {
        "loginId": "login-67890abcdef1234567890003",
        "loginStatus": "Failed",
        "failureReason": "Invalid password",
        "deviceInfo": {
          "ipAddress": "192.168.1.120"
        },
        "loginAt": "2025-11-30T14:30:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "totalPages": 3,
      "totalCount": 52,
      "hasNextPage": true,
      "hasPreviousPage": false
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
    "statusCode": 400,
    "details": []
  }
}
```

### Common Error Codes

#### Authentication Errors

**Invalid Credentials (401)**:
```json
{
  "success": false,
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "Invalid email or password",
    "statusCode": 401
  }
}
```

**Token Expired (401)**:
```json
{
  "success": false,
  "error": {
    "code": "TOKEN_EXPIRED",
    "message": "Your session has expired. Please login again.",
    "statusCode": 401
  }
}
```

**Account Locked (403)**:
```json
{
  "success": false,
  "error": {
    "code": "ACCOUNT_LOCKED",
    "message": "Your account has been locked due to multiple failed login attempts",
    "statusCode": 403,
    "lockExpiresAt": "2025-12-02T15:30:00Z"
  }
}
```

**Email Not Verified (403)**:
```json
{
  "success": false,
  "error": {
    "code": "EMAIL_NOT_VERIFIED",
    "message": "Please verify your email before logging in",
    "statusCode": 403
  }
}
```

#### Validation Errors

**Password Requirements (400)**:
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "statusCode": 400,
    "details": [
      {
        "field": "password",
        "message": "Password must be at least 8 characters"
      },
      {
        "field": "password",
        "message": "Password must contain at least one uppercase letter"
      },
      {
        "field": "password",
        "message": "Password must contain at least one number"
      },
      {
        "field": "password",
        "message": "Password must contain at least one special character"
      }
    ]
  }
}
```

---

## Password Requirements

**Minimum Requirements**:
- At least 8 characters long
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one number (0-9)
- At least one special character (!@#$%^&*()_+-=[]{}|;:,.<>?)

**Example Valid Passwords**:
- `SecurePass123!`
- `MyP@ssw0rd`
- `Welcome2024#`

---

## Rate Limiting

**Rate Limits**:
- **Login**: 5 attempts per 15 minutes per IP
- **Registration**: 3 attempts per hour per IP
- **Password Reset**: 3 attempts per hour per email
- **Verification Code**: 5 attempts per hour per email
- **Other endpoints**: 100 requests per hour per user

**Rate Limit Response (429)**:
```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests. Please try again later.",
    "statusCode": 429,
    "retryAfter": 300
  }
}
```

---

## Security Best Practices

1. **Token Storage**: Store tokens securely (httpOnly cookies or secure storage)
2. **HTTPS Only**: Always use HTTPS in production
3. **Password Strength**: Enforce strong password requirements
4. **Session Management**: Implement proper session timeout
5. **2FA**: Implement two-factor authentication (future feature)
6. **Audit Logs**: Monitor suspicious login activities
7. **Rate Limiting**: Implement aggressive rate limiting on auth endpoints
8. **CORS**: Configure proper CORS policies
9. **Input Validation**: Always validate and sanitize user input
10. **Error Messages**: Don't expose sensitive information in errors

---

**End of Documentation**
