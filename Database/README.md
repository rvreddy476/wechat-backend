# WeChat.com Database Architecture

Complete database architecture and scripts for **AuthService** and **UserProfileService**.

## 📋 Table of Contents

- [Overview](#overview)
- [Technology Stack](#technology-stack)
- [Database Structure](#database-structure)
- [Setup Instructions](#setup-instructions)
- [AuthService (PostgreSQL)](#authservice-postgresql)
- [UserProfileService (MongoDB)](#userprofileservice-mongodb)
- [Best Practices](#best-practices)
- [Security Considerations](#security-considerations)

## 🎯 Overview

This database architecture supports a scalable social media platform with two main services:

1. **AuthService**: Handles authentication, authorization, and user management using PostgreSQL
2. **UserProfileService**: Manages user profiles, follows, and social relationships using MongoDB

## 🛠️ Technology Stack

- **PostgreSQL 16+**: Relational database for authentication and user management
- **MongoDB 7+**: NoSQL database for flexible user profile data
- **Redis**: Caching and session management (documented separately)

## 📁 Database Structure

```
Database/
├── AuthService/              # PostgreSQL database
│   ├── Schema/
│   │   ├── 01_CreateTables.sql          # Core tables and indexes
│   │   └── 02_CreateTriggers.sql        # Triggers and automation
│   ├── Procedures/
│   │   ├── 01_UserManagement.sql        # User CRUD operations
│   │   └── 02_TokenManagement.sql       # Token operations
│   ├── Functions/
│   │   └── 01_HelperFunctions.sql       # Query helper functions
│   └── Seeds/
│       └── 01_SeedRoles.sql             # Initial data (roles, admin)
│
└── UserProfileService/       # MongoDB database
    ├── Schemas/
    │   ├── 01_ProfilesCollection.js     # User profiles collection
    │   ├── 02_FollowsCollection.js      # Follow relationships
    │   └── 03_BlockedUsersCollection.js # Blocked users
    ├── Indexes/
    │   └── (Indexes are in schema files)
    └── Queries/
        └── 01_ProfileQueries.js         # Common query patterns
```

## 🚀 Setup Instructions

### Prerequisites

- PostgreSQL 16+
- MongoDB 7+
- Database management tools (pgAdmin, MongoDB Compass, or CLI)

### AuthService Setup (PostgreSQL)

1. **Create Database**
   ```bash
   createdb wechat_auth
   ```

2. **Run Scripts in Order**
   ```bash
   # Connect to database
   psql -d wechat_auth

   # Run scripts
   \i Database/AuthService/Schema/01_CreateTables.sql
   \i Database/AuthService/Schema/02_CreateTriggers.sql
   \i Database/AuthService/Procedures/01_UserManagement.sql
   \i Database/AuthService/Procedures/02_TokenManagement.sql
   \i Database/AuthService/Functions/01_HelperFunctions.sql
   \i Database/AuthService/Seeds/01_SeedRoles.sql
   ```

3. **Verify Setup**
   ```sql
   -- Check tables
   SELECT table_name FROM information_schema.tables
   WHERE table_schema = 'auth';

   -- Check default roles
   SELECT * FROM auth.Roles;

   -- Check admin user
   SELECT UserId, Username, Email FROM auth.Users;
   ```

4. **Change Default Admin Password**
   ```sql
   -- IMPORTANT: Change this immediately!
   UPDATE auth.Users
   SET PasswordHash = '[your-bcrypt-hash]'
   WHERE Username = 'admin';
   ```

### UserProfileService Setup (MongoDB)

1. **Connect to MongoDB**
   ```bash
   mongosh "mongodb://localhost:27017"
   ```

2. **Run Schema Scripts**
   ```javascript
   // Run each script
   load("Database/UserProfileService/Schemas/01_ProfilesCollection.js");
   load("Database/UserProfileService/Schemas/02_FollowsCollection.js");
   load("Database/UserProfileService/Schemas/03_BlockedUsersCollection.js");
   ```

3. **Verify Setup**
   ```javascript
   use wechat_profiles

   // Check collections
   show collections

   // Check indexes
   db.profiles.getIndexes()
   db.follows.getIndexes()
   db.blockedUsers.getIndexes()

   // Test validation
   db.profiles.insertOne({
       userId: "test-123",
       username: "testuser",
       displayName: "Test User",
       stats: {
           followersCount: 0,
           followingCount: 0,
           postsCount: 0,
           videosCount: 0,
           shortsCount: 0
       },
       createdAt: new Date(),
       updatedAt: new Date()
   })
   ```

## 🗄️ AuthService (PostgreSQL)

### Database: `wechat_auth`
### Schema: `auth`

### Core Tables

#### 1. **Users** - Core Authentication
- Stores user credentials and security settings
- Supports email/username login
- 2FA support
- Account lockout mechanism
- Soft delete support

**Key Fields:**
- `UserId` (UUID, Primary Key)
- `Username` (VARCHAR, Unique)
- `Email` (VARCHAR, Unique)
- `PasswordHash` (VARCHAR)
- `SecurityStamp` (UUID) - Changes on password update
- `TwoFactorEnabled`, `TwoFactorSecret`
- `LockoutEnabled`, `LockoutEnd`, `AccessFailedCount`
- `IsActive`, `IsDeleted`

#### 2. **Roles** - Role-Based Access Control
- Defines system roles
- Supports custom roles

**Default Roles:**
- SuperAdmin
- Admin
- Moderator
- Creator
- User

#### 3. **UserRoles** - User-Role Mapping
- Many-to-many relationship
- Tracks who assigned the role

#### 4. **RefreshTokens** - JWT Refresh Tokens
- Stores refresh tokens with expiration
- Tracks device info and IP
- Implements token rotation
- Detects replay attacks

#### 5. **EmailVerificationTokens** - Email Verification
- One-time use tokens
- Expiration support
- Auto-cleanup of used tokens

#### 6. **PasswordResetTokens** - Password Recovery
- Secure password reset flow
- One-time use
- Expiration after 24 hours

#### 7. **UserSessions** - Active Session Tracking
- Tracks active user sessions
- Device and location info
- Supports "logout all devices"

#### 8. **AuditLogs** - Security Audit Trail
- Comprehensive security logging
- Tracks all authentication events
- Stores old/new values for changes

#### 9. **LoginAttempts** - Login Monitoring
- Tracks successful and failed attempts
- Security monitoring
- Supports rate limiting

#### 10. **ExternalLoginProviders** - OAuth Support
- Google, Facebook, Apple, etc.
- Stores external tokens
- Links external accounts to users

### Stored Procedures

#### User Management
```sql
-- Register new user
SELECT * FROM auth.sp_RegisterUser(
    'username', 'email@example.com', 'password_hash',
    '+1234567890', '192.168.1.1', 'Mozilla/5.0...'
);

-- Authenticate user
SELECT * FROM auth.sp_AuthenticateUser(
    'username', 'password_hash', '192.168.1.1', 'Mozilla/5.0...'
);

-- Get user by ID
SELECT * FROM auth.sp_GetUserById('user-uuid');

-- Update profile
SELECT * FROM auth.sp_UpdateUserProfile(
    'user-uuid', 'new_username', 'new@email.com', '+1234567890'
);

-- Change password
SELECT * FROM auth.sp_ChangePassword(
    'user-uuid', 'old_hash', 'new_hash', '192.168.1.1'
);

-- Delete user (soft delete)
SELECT * FROM auth.sp_DeleteUser('user-uuid', 'admin-uuid');
```

#### Token Management
```sql
-- Create refresh token
SELECT * FROM auth.sp_CreateRefreshToken(
    'user-uuid', 'token', 'token_hash',
    NOW() + INTERVAL '7 days', '192.168.1.1',
    'Mozilla/5.0...', '{"device": "iPhone"}'::jsonb
);

-- Validate refresh token
SELECT * FROM auth.sp_ValidateRefreshToken('token');

-- Revoke refresh token
SELECT * FROM auth.sp_RevokeRefreshToken('token');

-- Revoke all user tokens
SELECT * FROM auth.sp_RevokeAllUserTokens('user-uuid');

-- Create email verification token
SELECT * FROM auth.sp_CreateEmailVerificationToken(
    'user-uuid', 'email@example.com', 'token',
    'token_hash', NOW() + INTERVAL '24 hours'
);

-- Verify email
SELECT * FROM auth.sp_VerifyEmail('token');

-- Create password reset token
SELECT * FROM auth.sp_CreatePasswordResetToken(
    'email@example.com', 'token', 'token_hash',
    NOW() + INTERVAL '24 hours', '192.168.1.1'
);

-- Reset password with token
SELECT * FROM auth.sp_ResetPassword(
    'token', 'new_password_hash', '192.168.1.1'
);

-- Cleanup expired tokens
SELECT * FROM auth.sp_CleanupExpiredTokens();
```

### Helper Functions

```sql
-- Check if user has role
SELECT auth.fn_IsUserInRole('user-uuid', 'Admin');

-- Get user roles as array
SELECT auth.fn_GetUserRoles('user-uuid');

-- Check if account is locked
SELECT auth.fn_IsAccountLocked('user-uuid');

-- Get active session count
SELECT auth.fn_GetActiveSessionCount('user-uuid');

-- Get user statistics
SELECT * FROM auth.fn_GetUserStats('user-uuid');

-- Search users (admin)
SELECT * FROM auth.fn_SearchUsers('search term', TRUE, 50, 0);

-- Get login history
SELECT * FROM auth.fn_GetLoginHistory('user-uuid', 20);

-- Get active sessions
SELECT * FROM auth.fn_GetActiveSessions('user-uuid');

-- Get audit log
SELECT * FROM auth.fn_GetUserAuditLog('user-uuid', 50, 0);

-- Get system statistics (dashboard)
SELECT * FROM auth.fn_GetSystemStats();

-- Check email/username existence
SELECT auth.fn_EmailExists('email@example.com');
SELECT auth.fn_UsernameExists('username');

-- Get users by role
SELECT * FROM auth.fn_GetUsersByRole('Admin', 100, 0);

-- Get suspicious activity
SELECT * FROM auth.fn_GetSuspiciousActivity(24);
```

### Automatic Triggers

- **UpdatedAt**: Auto-updates `UpdatedAt` field on record modification
- **SecurityStamp**: Auto-generates new stamp on password change
- **Token Revocation**: Auto-revokes all tokens when password changes
- **Account Lockout**: Auto-locks account after 5 failed login attempts (15 min)
- **Audit Logging**: Auto-logs all user changes
- **Token Cleanup**: Auto-cleans expired tokens

## 📊 UserProfileService (MongoDB)

### Database: `wechat_profiles`

### Collections

#### 1. **profiles** - User Profiles

**Core Fields:**
```javascript
{
    userId: "UUID",              // From AuthService
    username: "unique_username",
    displayName: "Display Name",
    bio: "User bio...",
    avatarUrl: "https://...",
    bannerUrl: "https://...",
    location: "City, Country",
    website: "https://...",
    birthDate: ISODate("1990-01-15"),
    gender: "male|female|other|prefer_not_to_say",
    verified: true,
    verifiedType: "individual|business|government|celebrity",
    isPrivate: false,
    allowMessagesFrom: "everyone|followers|following|mutual_follows|none",

    stats: {
        followersCount: 0,
        followingCount: 0,
        postsCount: 0,
        videosCount: 0,
        shortsCount: 0,
        likesReceived: 0,
        viewsReceived: 0
    },

    privacy: {
        showEmail: false,
        showPhoneNumber: false,
        showBirthDate: false,
        showLocation: true,
        allowTagging: true,
        showOnlineStatus: true,
        indexProfile: true
    },

    notifications: {
        emailNotifications: true,
        pushNotifications: true,
        notifyOnFollow: true,
        notifyOnComment: true,
        notifyOnLike: true,
        notifyOnMention: true,
        notifyOnMessage: true,
        notifyOnVideoProcessed: true
    },

    socialLinks: {
        twitter: "@handle",
        instagram: "handle",
        youtube: "channel",
        // ... more platforms
    },

    createdAt: ISODate(),
    updatedAt: ISODate(),
    lastActiveAt: ISODate(),
    isDeleted: false,
    deletedAt: null
}
```

**Indexes:**
- Unique: `userId`, `username`
- Text search: `username`, `displayName`, `bio`, `location`
- Stats: `followersCount`, `postsCount`, `videosCount`, `likesReceived`, `viewsReceived`
- Queries: `verified`, `isPrivate`, `createdAt`, `lastActiveAt`

#### 2. **follows** - Follow Relationships

```javascript
{
    followerId: "UUID",          // User who follows
    followingId: "UUID",         // User being followed
    isAccepted: true,            // For private accounts
    createdAt: ISODate(),
    acceptedAt: ISODate(),
    unfollowedAt: null,
    notificationsEnabled: true
}
```

**Indexes:**
- Unique: `(followerId, followingId)`
- Queries: `followerId + isAccepted`, `followingId + isAccepted`
- Pending requests: `followingId + isAccepted:false`

#### 3. **blockedUsers** - User Blocks

```javascript
{
    blockerId: "UUID",           // User who blocked
    blockedUserId: "UUID",       // User who is blocked
    reason: "spam|harassment|inappropriate_content|impersonation|other",
    notes: "Additional info...",
    createdAt: ISODate(),
    unblockedAt: null,
    isActive: true
}
```

**Indexes:**
- Unique: `(blockerId, blockedUserId)`
- Queries: `blockerId + isActive`, `blockedUserId + isActive`
- Block check: `blockerId + blockedUserId + isActive`

### Common Query Patterns

See `Database/UserProfileService/Queries/01_ProfileQueries.js` for detailed examples:

- Get profile by userId/username
- Update profile stats (increment counters)
- Search profiles (full-text search)
- Get trending profiles
- Get top creators
- Get followers/following
- Check follow relationships
- Get mutual followers
- Get suggested users to follow
- Check blocked users
- Get profile statistics
- Get profiles by location

## 🔒 Security Considerations

### AuthService Security

1. **Password Security**
   - Use bcrypt with cost factor 11+ for password hashing
   - Never store plain text passwords
   - Enforce strong password policy in application

2. **Token Security**
   - Store token hashes, not plain tokens
   - Implement token rotation
   - Detect and prevent token replay attacks
   - Set appropriate expiration times

3. **Account Security**
   - Auto-lock after 5 failed attempts
   - Track login attempts and suspicious activity
   - Support 2FA (TOTP)
   - Comprehensive audit logging

4. **Session Security**
   - Track device info and IP addresses
   - Support "logout all devices"
   - Session expiration
   - Detect concurrent sessions from different locations

### UserProfileService Security

1. **Privacy Controls**
   - Respect privacy settings
   - Check `isPrivate` before showing profiles
   - Enforce `allowMessagesFrom` settings
   - Check blocked users before any interaction

2. **Data Validation**
   - Schema validation enforced at database level
   - Validate all inputs in application
   - Sanitize user-generated content

3. **Access Control**
   - Check permissions before profile updates
   - Only allow users to edit their own profiles
   - Admin functions require proper authorization

## 📈 Performance Optimization

### PostgreSQL

1. **Indexes**: All critical queries have optimized indexes
2. **Partial Indexes**: Used for filtered queries (e.g., `WHERE isDeleted = FALSE`)
3. **JSONB**: Used for flexible data (device info, audit data)
4. **Connection Pooling**: Use pgBouncer or similar
5. **Query Optimization**: All procedures use efficient queries

### MongoDB

1. **Indexes**: Comprehensive indexing strategy
2. **Text Search**: Full-text search index for profiles
3. **Aggregation Pipelines**: Optimized for common queries
4. **Compound Indexes**: For multi-field queries
5. **Covered Queries**: Projections match indexed fields

## 🔧 Maintenance

### Regular Tasks

1. **Cleanup Expired Tokens** (Daily)
   ```sql
   SELECT * FROM auth.sp_CleanupExpiredTokens();
   ```

2. **Monitor Suspicious Activity** (Daily)
   ```sql
   SELECT * FROM auth.fn_GetSuspiciousActivity(24);
   ```

3. **Vacuum PostgreSQL** (Weekly)
   ```sql
   VACUUM ANALYZE auth.Users;
   VACUUM ANALYZE auth.RefreshTokens;
   VACUUM ANALYZE auth.AuditLogs;
   ```

4. **Compact MongoDB** (Monthly)
   ```javascript
   db.profiles.compact();
   db.follows.compact();
   ```

5. **Backup Databases** (Daily)
   - PostgreSQL: `pg_dump wechat_auth > backup.sql`
   - MongoDB: `mongodump --db wechat_profiles`

## 📚 Additional Resources

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [MongoDB Documentation](https://docs.mongodb.com/)
- [WeChat.com Backend README](../README.md)

## 📝 Notes

- Change default admin password immediately after setup
- All UUIDs should be v4 UUIDs
- Timestamps are stored in UTC
- Soft deletes are used to preserve data integrity
- All scripts are idempotent (can be run multiple times)

---

**Created for WeChat.com Backend**
Version 1.0
Last Updated: 2024
