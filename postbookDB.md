# PostBook Database Architecture - Complete Implementation Guide

> **Purpose**: Comprehensive database implementation instructions for PostBook social media platform
> **Database Systems**: PostgreSQL (Auth), MongoDB (Chat, UserProfile, PostFeed, Media, Notification)
> **Version**: 1.0
> **Last Updated**: 2025-12-02

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Auth Service Database (PostgreSQL)](#auth-service-database-postgresql)
3. [UserProfile Service Database (MongoDB)](#userprofile-service-database-mongodb)
4. [Chat Service Database (MongoDB)](#chat-service-database-mongodb)
5. [PostFeed Service Database (MongoDB)](#postfeed-service-database-mongodb)
6. [Media Service Database (MongoDB)](#media-service-database-mongodb)
7. [Notification Service Database (MongoDB)](#notification-service-database-mongodb)
8. [Cross-Service Data Synchronization](#cross-service-data-synchronization)
9. [Performance Optimization](#performance-optimization)
10. [Backup and Recovery Strategy](#backup-and-recovery-strategy)

---

## System Overview

### Architecture Pattern
- **Microservices Architecture**: Each service has its own database
- **Database per Service**: Ensures loose coupling and independent scaling
- **Polyglot Persistence**: PostgreSQL for relational data (Auth), MongoDB for document-based data (everything else)

### Database Selection Rationale

#### PostgreSQL (Auth Service)
- **Why**: ACID transactions critical for authentication and user credentials
- **Use Cases**: User accounts, sessions, tokens, login history
- **Features Needed**: Strong consistency, referential integrity, stored procedures

#### MongoDB (All Other Services)
- **Why**: Flexible schema, horizontal scaling, high performance for reads
- **Use Cases**: Social features, messaging, posts, media, notifications
- **Features Needed**: Embedded documents, arrays, TTL indexes, text search

### Connection Strings Format

**PostgreSQL**:
```
Server=localhost;Port=5432;Database=AuthServiceDB;User Id=postgres;Password=your_password;
```

**MongoDB**:
```
mongodb://localhost:27017/ChatServiceDB
mongodb://localhost:27017/UserProfileServiceDB
mongodb://localhost:27017/PostFeedServiceDB
mongodb://localhost:27017/MediaServiceDB
mongodb://localhost:27017/NotificationServiceDB
```

---

## Auth Service Database (PostgreSQL)

### Database Name
`AuthServiceDB`

### Schema Structure

This service requires **5 tables**, **6 helper functions**, **7 stored procedures**, and **2 triggers**.

---

### Table 1: users

**Purpose**: Store user account information and credentials

**Columns**:

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| user_id | UUID | PRIMARY KEY, DEFAULT uuid_generate_v4() | Unique identifier for user |
| username | VARCHAR(50) | UNIQUE, NOT NULL, CHECK (length(username) >= 3) | Unique username, min 3 chars |
| email | VARCHAR(255) | UNIQUE, NOT NULL | User email address |
| email_normalized | VARCHAR(255) | NOT NULL, INDEX | Lowercase email for search |
| password_hash | VARCHAR(255) | NOT NULL | Bcrypt hashed password |
| first_name | VARCHAR(100) | NOT NULL | User's first name |
| last_name | VARCHAR(100) | NOT NULL | User's last name |
| phone_number | VARCHAR(20) | NULL | Optional phone number |
| is_email_verified | BOOLEAN | DEFAULT FALSE | Email verification status |
| is_phone_verified | BOOLEAN | DEFAULT FALSE | Phone verification status |
| email_verified_at | TIMESTAMP | NULL | When email was verified |
| phone_verified_at | TIMESTAMP | NULL | When phone was verified |
| profile_picture_url | TEXT | NULL | CDN URL for profile picture |
| role | VARCHAR(20) | DEFAULT 'User', CHECK (role IN ('User', 'Admin', 'Moderator')) | User role |
| account_status | VARCHAR(20) | DEFAULT 'Active', CHECK (account_status IN ('Active', 'Inactive', 'Suspended', 'Deleted')) | Account status |
| last_login_at | TIMESTAMP | NULL | Last successful login timestamp |
| failed_login_attempts | INTEGER | DEFAULT 0 | Counter for failed logins |
| account_locked_until | TIMESTAMP | NULL | Account lock expiry time |
| password_changed_at | TIMESTAMP | NULL | Last password change timestamp |
| is_deleted | BOOLEAN | DEFAULT FALSE | Soft delete flag |
| deleted_at | TIMESTAMP | NULL | Soft delete timestamp |
| created_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Account creation time |
| updated_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Last update time |

**Indexes**:
- PRIMARY KEY on `user_id`
- UNIQUE INDEX on `username`
- UNIQUE INDEX on `email`
- INDEX on `email_normalized` for case-insensitive search
- INDEX on `is_deleted, account_status` for active user queries
- INDEX on `created_at DESC` for recent users
- INDEX on `last_login_at DESC` for activity tracking

**Constraints**:
- Email must be valid format (use trigger to validate)
- Username must be alphanumeric with underscores only
- Password hash must be at least 60 characters (bcrypt format)

---

### Table 2: user_sessions

**Purpose**: Track active user sessions across devices

**Columns**:

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| session_id | UUID | PRIMARY KEY, DEFAULT uuid_generate_v4() | Unique session identifier |
| user_id | UUID | NOT NULL, FOREIGN KEY REFERENCES users(user_id) ON DELETE CASCADE | User who owns session |
| refresh_token | VARCHAR(500) | UNIQUE, NOT NULL | JWT refresh token |
| refresh_token_hash | VARCHAR(255) | NOT NULL | Hashed refresh token |
| device_type | VARCHAR(50) | NULL | Device type (Web, Mobile, Desktop) |
| device_info | TEXT | NULL | Device details (browser, OS) |
| ip_address | VARCHAR(45) | NULL | IP address (IPv4 or IPv6) |
| location | VARCHAR(255) | NULL | Geographic location |
| user_agent | TEXT | NULL | Full user agent string |
| is_active | BOOLEAN | DEFAULT TRUE | Session active status |
| expires_at | TIMESTAMP | NOT NULL | Refresh token expiry |
| created_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Session creation time |
| last_activity_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Last activity timestamp |

**Indexes**:
- PRIMARY KEY on `session_id`
- INDEX on `user_id, is_active` for active sessions
- INDEX on `refresh_token_hash` for token lookup
- INDEX on `expires_at` for cleanup queries
- INDEX on `created_at DESC` for recent sessions

**Foreign Keys**:
- `user_id` → `users.user_id` (CASCADE on delete)

---

### Table 3: verification_codes

**Purpose**: Store email and phone verification codes

**Columns**:

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| verification_id | UUID | PRIMARY KEY, DEFAULT uuid_generate_v4() | Unique verification identifier |
| user_id | UUID | NOT NULL, FOREIGN KEY REFERENCES users(user_id) ON DELETE CASCADE | User being verified |
| code | VARCHAR(10) | NOT NULL | Verification code (6 digits) |
| code_hash | VARCHAR(255) | NOT NULL | Hashed verification code |
| code_type | VARCHAR(20) | NOT NULL, CHECK (code_type IN ('Email', 'Phone', 'PasswordReset')) | Type of verification |
| is_used | BOOLEAN | DEFAULT FALSE | Whether code was used |
| used_at | TIMESTAMP | NULL | When code was used |
| expires_at | TIMESTAMP | NOT NULL | Code expiry time |
| created_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Code creation time |
| attempts | INTEGER | DEFAULT 0 | Failed verification attempts |

**Indexes**:
- PRIMARY KEY on `verification_id`
- INDEX on `user_id, code_type, is_used` for lookups
- INDEX on `code_hash` for verification
- INDEX on `expires_at` for cleanup
- INDEX on `created_at DESC`

**Foreign Keys**:
- `user_id` → `users.user_id` (CASCADE on delete)

**Auto-Cleanup**:
- Delete expired codes daily using scheduled job

---

### Table 4: user_login_history

**Purpose**: Audit trail of all login attempts

**Columns**:

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| login_id | BIGSERIAL | PRIMARY KEY | Auto-incrementing login ID |
| user_id | UUID | NULL, FOREIGN KEY REFERENCES users(user_id) ON DELETE SET NULL | User who attempted login |
| login_identifier | VARCHAR(255) | NOT NULL | Email or username used |
| login_status | VARCHAR(20) | NOT NULL, CHECK (login_status IN ('Success', 'Failed', 'Locked', 'Unverified')) | Login result |
| failure_reason | VARCHAR(255) | NULL | Reason for failed login |
| device_type | VARCHAR(50) | NULL | Device type |
| device_info | TEXT | NULL | Device details |
| ip_address | VARCHAR(45) | NOT NULL | IP address |
| location | VARCHAR(255) | NULL | Geographic location |
| user_agent | TEXT | NULL | User agent string |
| login_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Login attempt timestamp |

**Indexes**:
- PRIMARY KEY on `login_id`
- INDEX on `user_id, login_at DESC` for user history
- INDEX on `ip_address, login_at DESC` for IP tracking
- INDEX on `login_status, login_at DESC` for analytics
- INDEX on `login_at DESC` for recent activity

**Foreign Keys**:
- `user_id` → `users.user_id` (SET NULL on delete to preserve history)

**Retention Policy**:
- Keep login history for 90 days
- Archive older records to separate table

---

### Table 5: password_reset_tokens

**Purpose**: Secure password reset token management

**Columns**:

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| token_id | UUID | PRIMARY KEY, DEFAULT uuid_generate_v4() | Unique token identifier |
| user_id | UUID | NOT NULL, FOREIGN KEY REFERENCES users(user_id) ON DELETE CASCADE | User requesting reset |
| token | VARCHAR(500) | UNIQUE, NOT NULL | Reset token |
| token_hash | VARCHAR(255) | NOT NULL | Hashed reset token |
| is_used | BOOLEAN | DEFAULT FALSE | Whether token was used |
| used_at | TIMESTAMP | NULL | When token was used |
| expires_at | TIMESTAMP | NOT NULL | Token expiry (15 minutes) |
| created_at | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Token creation time |
| ip_address | VARCHAR(45) | NULL | IP that requested reset |

**Indexes**:
- PRIMARY KEY on `token_id`
- INDEX on `token_hash` for validation
- INDEX on `user_id, is_used, expires_at` for lookups
- INDEX on `expires_at` for cleanup

**Foreign Keys**:
- `user_id` → `users.user_id` (CASCADE on delete)

**Security**:
- Tokens expire after 15 minutes
- One-time use only
- Invalidate all tokens on successful password reset

---

### Functions (PostgreSQL)

#### Function 1: email_exists

**Purpose**: Check if an email already exists (case-insensitive)

**Signature**:
```sql
CREATE OR REPLACE FUNCTION email_exists(p_email VARCHAR)
RETURNS BOOLEAN
```

**Logic**:
1. Accept email parameter
2. Normalize email to lowercase
3. Query users table WHERE email_normalized = LOWER(p_email)
4. Return TRUE if found, FALSE otherwise

**Usage**: Called during registration and email update

---

#### Function 2: username_exists

**Purpose**: Check if a username already exists (case-insensitive)

**Signature**:
```sql
CREATE OR REPLACE FUNCTION username_exists(p_username VARCHAR)
RETURNS BOOLEAN
```

**Logic**:
1. Accept username parameter
2. Query users table WHERE LOWER(username) = LOWER(p_username)
3. Return TRUE if found, FALSE otherwise

**Usage**: Called during registration and username change

---

#### Function 3: get_user_by_login

**Purpose**: Get user by email or username for login

**Signature**:
```sql
CREATE OR REPLACE FUNCTION get_user_by_login(p_login_identifier VARCHAR)
RETURNS TABLE(user_id UUID, username VARCHAR, email VARCHAR, password_hash VARCHAR, is_email_verified BOOLEAN, account_status VARCHAR, failed_login_attempts INTEGER, account_locked_until TIMESTAMP)
```

**Logic**:
1. Accept login identifier (email or username)
2. Query users table WHERE email = p_login_identifier OR username = p_login_identifier
3. Return user record if found
4. Used for authentication

**Usage**: Called during login process

---

#### Function 4: generate_verification_code

**Purpose**: Generate a random 6-digit verification code

**Signature**:
```sql
CREATE OR REPLACE FUNCTION generate_verification_code()
RETURNS VARCHAR(6)
```

**Logic**:
1. Generate random 6-digit number (100000 to 999999)
2. Return as string
3. Ensure uniqueness by checking against existing codes

**Usage**: Called when creating verification codes

---

#### Function 5: cleanup_old_data

**Purpose**: Remove expired sessions, tokens, and verification codes

**Signature**:
```sql
CREATE OR REPLACE FUNCTION cleanup_old_data()
RETURNS INTEGER
```

**Logic**:
1. Delete from user_sessions WHERE expires_at < CURRENT_TIMESTAMP
2. Delete from verification_codes WHERE expires_at < CURRENT_TIMESTAMP AND is_used = FALSE
3. Delete from password_reset_tokens WHERE expires_at < CURRENT_TIMESTAMP AND is_used = FALSE
4. Delete from user_login_history WHERE login_at < CURRENT_TIMESTAMP - INTERVAL '90 days'
5. Return total rows deleted

**Usage**: Called by scheduled job (daily)

---

#### Function 6: get_user_stats

**Purpose**: Get comprehensive user statistics

**Signature**:
```sql
CREATE OR REPLACE FUNCTION get_user_stats(p_user_id UUID)
RETURNS JSON
```

**Logic**:
1. Get user info from users table
2. Count active sessions from user_sessions
3. Get last login from user_login_history
4. Count total logins
5. Return as JSON object

**Usage**: Called for user dashboard and analytics

---

### Stored Procedures (PostgreSQL)

#### Procedure 1: create_user

**Purpose**: Create a new user account with validation

**Signature**:
```sql
CREATE OR REPLACE PROCEDURE create_user(
    p_username VARCHAR,
    p_email VARCHAR,
    p_password_hash VARCHAR,
    p_first_name VARCHAR,
    p_last_name VARCHAR,
    p_phone_number VARCHAR DEFAULT NULL,
    OUT p_user_id UUID,
    OUT p_error VARCHAR
)
```

**Steps**:
1. **Validation**:
   - Check if email exists using `email_exists()` function
   - Check if username exists using `username_exists()` function
   - Validate email format (regex)
   - Validate username format (alphanumeric + underscore)
   - Validate password hash length (must be 60 chars for bcrypt)
   - If any validation fails, set p_error and ROLLBACK

2. **Create User**:
   - BEGIN TRANSACTION
   - INSERT into users table with all parameters
   - Set email_normalized = LOWER(p_email)
   - Set is_email_verified = FALSE
   - Set account_status = 'Active'
   - Set role = 'User'
   - RETURNING user_id INTO p_user_id

3. **Generate Verification Code**:
   - Call generate_verification_code()
   - INSERT into verification_codes table
   - Set code_type = 'Email'
   - Set expires_at = CURRENT_TIMESTAMP + INTERVAL '15 minutes'

4. **Commit or Rollback**:
   - COMMIT if successful
   - ROLLBACK on any error and set p_error

**Error Handling**:
- Return error messages in p_error parameter
- Possible errors: "Email already exists", "Username already taken", "Invalid email format"

---

#### Procedure 2: verify_email

**Purpose**: Verify user email with verification code

**Signature**:
```sql
CREATE OR REPLACE PROCEDURE verify_email(
    p_user_id UUID,
    p_code VARCHAR,
    OUT p_success BOOLEAN,
    OUT p_error VARCHAR
)
```

**Steps**:
1. **Validate Code**:
   - Query verification_codes WHERE user_id = p_user_id AND code_type = 'Email' AND is_used = FALSE
   - Check if code matches
   - Check if code is not expired (expires_at > CURRENT_TIMESTAMP)
   - Check attempts count (max 5 attempts)
   - If validation fails, increment attempts and set p_error

2. **Update User**:
   - BEGIN TRANSACTION
   - UPDATE users SET is_email_verified = TRUE, email_verified_at = CURRENT_TIMESTAMP WHERE user_id = p_user_id
   - UPDATE verification_codes SET is_used = TRUE, used_at = CURRENT_TIMESTAMP WHERE verification_id = matched_code
   - Set p_success = TRUE
   - COMMIT

**Error Handling**:
- "Invalid verification code"
- "Verification code expired"
- "Too many attempts"

---

#### Procedure 3: create_user_session

**Purpose**: Create a new user session after successful login

**Signature**:
```sql
CREATE OR REPLACE PROCEDURE create_user_session(
    p_user_id UUID,
    p_refresh_token VARCHAR,
    p_refresh_token_hash VARCHAR,
    p_device_type VARCHAR,
    p_device_info TEXT,
    p_ip_address VARCHAR,
    p_location VARCHAR,
    p_user_agent TEXT,
    p_expires_at TIMESTAMP,
    OUT p_session_id UUID
)
```

**Steps**:
1. **Create Session**:
   - INSERT into user_sessions with all parameters
   - RETURNING session_id INTO p_session_id

2. **Update User**:
   - UPDATE users SET last_login_at = CURRENT_TIMESTAMP, failed_login_attempts = 0, account_locked_until = NULL WHERE user_id = p_user_id

3. **Log Login**:
   - INSERT into user_login_history with login_status = 'Success'

**Usage**: Called after successful authentication

---

#### Procedure 4: revoke_user_sessions

**Purpose**: Revoke user sessions (logout)

**Signature**:
```sql
CREATE OR REPLACE PROCEDURE revoke_user_sessions(
    p_user_id UUID,
    p_session_id UUID DEFAULT NULL,
    p_revoke_all BOOLEAN DEFAULT FALSE,
    OUT p_sessions_revoked INTEGER
)
```

**Steps**:
1. **Single Session Revoke** (if p_session_id provided and p_revoke_all = FALSE):
   - UPDATE user_sessions SET is_active = FALSE WHERE session_id = p_session_id AND user_id = p_user_id
   - Set p_sessions_revoked = 1

2. **All Sessions Revoke** (if p_revoke_all = TRUE):
   - UPDATE user_sessions SET is_active = FALSE WHERE user_id = p_user_id AND is_active = TRUE
   - Get count of updated rows into p_sessions_revoked

**Usage**: Called during logout or "logout all devices"

---

#### Procedure 5: store_refresh_token

**Purpose**: Store or update refresh token for session

**Signature**:
```sql
CREATE OR REPLACE PROCEDURE store_refresh_token(
    p_session_id UUID,
    p_refresh_token VARCHAR,
    p_refresh_token_hash VARCHAR,
    p_expires_at TIMESTAMP
)
```

**Steps**:
1. UPDATE user_sessions SET refresh_token = p_refresh_token, refresh_token_hash = p_refresh_token_hash, expires_at = p_expires_at, last_activity_at = CURRENT_TIMESTAMP WHERE session_id = p_session_id

**Usage**: Called during token refresh

---

#### Procedure 6: update_user_profile

**Purpose**: Update user profile information

**Signature**:
```sql
CREATE OR REPLACE PROCEDURE update_user_profile(
    p_user_id UUID,
    p_first_name VARCHAR DEFAULT NULL,
    p_last_name VARCHAR DEFAULT NULL,
    p_phone_number VARCHAR DEFAULT NULL,
    p_profile_picture_url TEXT DEFAULT NULL,
    OUT p_success BOOLEAN,
    OUT p_error VARCHAR
)
```

**Steps**:
1. **Validate User Exists**:
   - Check if user_id exists in users table
   - If not found, set p_error and return

2. **Update Fields**:
   - UPDATE users SET fields that are not NULL in parameters
   - SET updated_at = CURRENT_TIMESTAMP
   - WHERE user_id = p_user_id

3. **Return Success**:
   - Set p_success = TRUE

**Usage**: Called when user updates profile

---

#### Procedure 7: soft_delete_user

**Purpose**: Soft delete user account (mark as deleted)

**Signature**:
```sql
CREATE OR REPLACE PROCEDURE soft_delete_user(
    p_user_id UUID,
    OUT p_success BOOLEAN,
    OUT p_error VARCHAR
)
```

**Steps**:
1. **Begin Transaction**:
   - BEGIN TRANSACTION

2. **Update User**:
   - UPDATE users SET is_deleted = TRUE, deleted_at = CURRENT_TIMESTAMP, account_status = 'Deleted' WHERE user_id = p_user_id

3. **Revoke Sessions**:
   - UPDATE user_sessions SET is_active = FALSE WHERE user_id = p_user_id

4. **Commit**:
   - COMMIT
   - Set p_success = TRUE

**Usage**: Called during account deletion (30-day grace period before permanent deletion)

---

### Triggers (PostgreSQL)

#### Trigger 1: update_users_timestamp

**Purpose**: Automatically update `updated_at` timestamp on user record changes

**Definition**:
```sql
CREATE OR REPLACE FUNCTION update_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_users_timestamp
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION update_timestamp();
```

**Logic**:
- Fires BEFORE UPDATE on users table
- Sets updated_at to current timestamp
- Applies to every row update

---

#### Trigger 2: validate_email_format

**Purpose**: Validate email format before insert/update

**Definition**:
```sql
CREATE OR REPLACE FUNCTION validate_email()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.email !~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$' THEN
        RAISE EXCEPTION 'Invalid email format: %', NEW.email;
    END IF;
    NEW.email_normalized = LOWER(NEW.email);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER validate_email_format
BEFORE INSERT OR UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION validate_email();
```

**Logic**:
- Fires BEFORE INSERT OR UPDATE on users table
- Validates email using regex
- Automatically sets email_normalized to lowercase
- Raises exception if invalid

---

## UserProfile Service Database (MongoDB)

### Database Name
`UserProfileServiceDB`

### Collections Overview

This service requires **3 collections** with embedded documents and arrays.

---

### Collection 1: user_profiles

**Purpose**: Store comprehensive user profile information including education, experience, skills, connections

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  userId: "UUID string",                  // From Auth service (indexed, unique)
  username: "string",                     // Denormalized from Auth (indexed)
  displayName: "string",                  // User's display name
  firstName: "string",                    // First name
  lastName: "string",                     // Last name
  email: "string",                        // Denormalized from Auth
  phoneNumber: "string",                  // Phone number
  dateOfBirth: ISODate,                   // Birth date
  gender: "string",                       // Male/Female/Other/PreferNotToSay

  // Bio & Description
  bio: "string",                          // Max 500 chars
  tagline: "string",                      // Max 100 chars

  // Media
  avatarUrl: "string",                    // CDN URL
  coverImageUrl: "string",                // CDN URL

  // Location
  location: {
    city: "string",
    state: "string",
    country: "string",
    coordinates: {
      latitude: NumberDecimal,
      longitude: NumberDecimal
    }
  },

  // Contact & Links
  website: "string",                      // Personal website
  socialLinks: {
    linkedin: "string",
    twitter: "string",
    github: "string",
    instagram: "string",
    facebook: "string"
  },

  // Education (Array of objects)
  education: [
    {
      id: "string",                       // Unique ID for this entry
      school: "string",                   // School/University name
      degree: "string",                   // Degree type
      fieldOfStudy: "string",             // Major/field
      startDate: ISODate,
      endDate: ISODate,                   // null if current
      grade: "string",                    // GPA/grade
      activities: "string",               // Extra-curricular
      description: "string",              // Additional details
      current: Boolean                    // Currently studying
    }
  ],

  // Professional Experience (Array of objects)
  experience: [
    {
      id: "string",                       // Unique ID for this entry
      company: "string",                  // Company name
      position: "string",                 // Job title
      employmentType: "string",           // Full-time/Part-time/Contract/Internship/Freelance
      location: "string",                 // Job location
      startDate: ISODate,
      endDate: ISODate,                   // null if current
      current: Boolean,                   // Currently working
      description: "string",              // Job responsibilities
      skills: ["string"]                  // Skills used in this role
    }
  ],

  // Skills (Array of objects)
  skills: [
    {
      name: "string",                     // Skill name (indexed)
      level: "string",                    // Beginner/Intermediate/Advanced/Expert
      yearsOfExperience: Number,          // Years of experience
      endorsements: Number                // Endorsement count
    }
  ],

  // Certifications (Array of objects)
  certifications: [
    {
      id: "string",                       // Unique ID for this entry
      name: "string",                     // Certification name
      issuingOrganization: "string",      // Issuing body
      issueDate: ISODate,
      expirationDate: ISODate,            // null if no expiry
      credentialId: "string",             // Credential ID
      credentialUrl: "string"             // Verification URL
    }
  ],

  // Languages (Array of objects)
  languages: [
    {
      language: "string",                 // Language name
      proficiency: "string"               // Elementary/Limited/Professional/Native
    }
  ],

  // Interests
  interests: ["string"],                  // Array of interests/hobbies

  // Social Connections
  friends: ["UUID string"],               // Array of user IDs (indexed)
  followers: ["UUID string"],             // Array of user IDs (indexed)
  following: ["UUID string"],             // Array of user IDs (indexed)
  blockedUsers: ["UUID string"],          // Array of blocked user IDs

  // Online Status
  isOnline: Boolean,                      // Current online status
  lastSeenAt: ISODate,                    // Last activity timestamp

  // Privacy Settings
  privacySettings: {
    profileVisibility: "string",          // Public/Friends/Private
    showEmail: Boolean,
    showPhoneNumber: Boolean,
    showDateOfBirth: Boolean,
    showLocation: Boolean,
    showOnlineStatus: Boolean,
    showLastSeen: Boolean,
    allowFriendRequests: Boolean,
    allowMessages: "string",              // Everyone/Friends/None
    showEducation: Boolean,
    showExperience: Boolean,
    showConnections: Boolean,
    allowTagging: "string",               // Everyone/Friends/None
    allowMentions: "string"               // Everyone/Friends/None
  },

  // Notification Settings
  notificationSettings: {
    emailNotifications: Boolean,
    pushNotifications: Boolean,
    messageNotifications: Boolean,
    friendRequestNotifications: Boolean,
    postNotifications: Boolean,
    commentNotifications: Boolean,
    likeNotifications: Boolean,
    mentionNotifications: Boolean,
    followerNotifications: Boolean
  },

  // Account Settings
  accountSettings: {
    language: "string",                   // ISO 639-1 code (en, es, fr)
    timezone: "string",                   // IANA timezone
    dateFormat: "string",                 // MM/DD/YYYY, DD/MM/YYYY, YYYY-MM-DD
    theme: "string"                       // Light/Dark/Auto
  },

  // Statistics
  statistics: {
    friendsCount: Number,
    followersCount: Number,
    followingCount: Number,
    postsCount: Number,
    photosCount: Number,
    videosCount: Number,
    profileViewsCount: Number
  },

  // Verification & Status
  isVerified: Boolean,                    // Verified badge
  isActive: Boolean,                      // Account active status
  isPremium: Boolean,                     // Premium subscription

  // Metadata
  isDeleted: Boolean,                     // Soft delete flag
  deletedAt: ISODate,                     // Soft delete timestamp
  createdAt: ISODate,                     // Profile creation
  updatedAt: ISODate                      // Last update
}
```

**Indexes Required**:

1. **Unique Index**: `{ userId: 1 }` (unique)
2. **Username Index**: `{ username: 1 }` (unique, case-insensitive)
3. **Search Index**: `{ displayName: "text", bio: "text" }` (text search)
4. **Friends Index**: `{ friends: 1 }`
5. **Followers Index**: `{ followers: 1 }`
6. **Following Index**: `{ following: 1 }`
7. **Location Index**: `{ "location.coordinates": "2dsphere" }` (geospatial)
8. **Skills Index**: `{ "skills.name": 1 }`
9. **Active Users Index**: `{ isDeleted: 1, isActive: 1 }`
10. **Compound Index**: `{ isOnline: 1, lastSeenAt: -1 }` (online status)

**Validation Rules** (JSON Schema):
- `userId`: Required, must be valid UUID format
- `username`: Required, 3-50 characters, alphanumeric + underscore
- `email`: Required, valid email format
- `bio`: Max 500 characters
- `tagline`: Max 100 characters
- `privacySettings.profileVisibility`: Must be one of [Public, Friends, Private]
- `privacySettings.allowMessages`: Must be one of [Everyone, Friends, None]

---

### Collection 2: friend_requests

**Purpose**: Manage friend request lifecycle

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  senderId: "UUID string",                // User sending request (indexed)
  senderUsername: "string",               // Denormalized sender username
  senderDisplayName: "string",            // Denormalized sender display name
  senderAvatarUrl: "string",              // Denormalized sender avatar
  receiverId: "UUID string",              // User receiving request (indexed)
  receiverUsername: "string",             // Denormalized receiver username
  receiverDisplayName: "string",          // Denormalized receiver display name
  receiverAvatarUrl: "string",            // Denormalized receiver avatar
  message: "string",                      // Optional message (max 200 chars)
  status: "string",                       // Pending/Accepted/Rejected/Cancelled
  sentAt: ISODate,                        // Request sent timestamp
  respondedAt: ISODate,                   // Response timestamp
  expiresAt: ISODate,                     // Auto-expire after 30 days
  createdAt: ISODate,
  updatedAt: ISODate
}
```

**Indexes Required**:

1. **Sender Index**: `{ senderId: 1, status: 1 }`
2. **Receiver Index**: `{ receiverId: 1, status: 1 }`
3. **Unique Constraint**: `{ senderId: 1, receiverId: 1 }` (unique, prevent duplicate requests)
4. **Status Index**: `{ status: 1, sentAt: -1 }`
5. **TTL Index**: `{ expiresAt: 1 }` (expireAfterSeconds: 0) - auto-delete expired

**Validation Rules**:
- `senderId` and `receiverId` must be different
- `status` must be one of [Pending, Accepted, Rejected, Cancelled]
- `message` max 200 characters

---

### Collection 3: user_activities

**Purpose**: Track user activities for timeline and analytics

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  userId: "UUID string",                  // User performing activity (indexed)
  activityType: "string",                 // ProfileUpdated/FriendAdded/PostCreated/etc.
  description: "string",                  // Human-readable description
  metadata: {                             // Activity-specific data
    // Dynamic based on activity type
    // Examples:
    // For FriendAdded: { friendId, friendUsername }
    // For ProfileUpdated: { fields: ['bio', 'avatar'] }
    // For PostCreated: { postId, content }
  },
  isPublic: Boolean,                      // Visible to others
  createdAt: ISODate,                     // Activity timestamp
  expiresAt: ISODate                      // Auto-expire after 90 days
}
```

**Indexes Required**:

1. **User Activities Index**: `{ userId: 1, createdAt: -1 }`
2. **Activity Type Index**: `{ activityType: 1, createdAt: -1 }`
3. **Public Activities Index**: `{ isPublic: 1, createdAt: -1 }`
4. **TTL Index**: `{ expiresAt: 1 }` (expireAfterSeconds: 0) - auto-delete old activities

**Activity Types**:
- `ProfileUpdated`, `FriendAdded`, `FriendRemoved`, `PostCreated`, `PhotoUploaded`, `VideoUploaded`, `EducationAdded`, `ExperienceAdded`, `SkillAdded`, `CertificationAdded`

---

## Chat Service Database (MongoDB)

### Database Name
`ChatServiceDB`

### Collections Overview

This service requires **2 collections** with real-time messaging support.

---

### Collection 1: conversations

**Purpose**: Store conversation metadata (1-on-1 and group chats)

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  type: "string",                         // OneOnOne / Group

  // For Group chats
  name: "string",                         // Group name (required for Group)
  description: "string",                  // Group description
  avatarUrl: "string",                    // Group avatar URL
  createdBy: {                            // Group creator
    userId: "UUID string",
    username: "string",
    displayName: "string"
  },

  // Participants (Array)
  participants: [
    {
      userId: "UUID string",              // User ID (indexed)
      username: "string",                 // Denormalized username
      displayName: "string",              // Denormalized display name
      avatarUrl: "string",                // Denormalized avatar
      role: "string",                     // Admin/Member (for groups)
      joinedAt: ISODate,                  // When user joined
      lastReadMessageId: "string",        // Last message read by this user
      lastReadAt: ISODate,                // When last read
      isMuted: Boolean,                   // Muted notifications
      isActive: Boolean                   // Still in conversation
    }
  ],

  // Last Message (Denormalized for performance)
  lastMessage: {
    messageId: "string",                  // Message ID
    content: "string",                    // Message content preview
    senderId: "UUID string",              // Sender ID
    senderUsername: "string",             // Sender username
    messageType: "string",                // Text/Media/File/Voice
    sentAt: ISODate                       // Message timestamp
  },

  // Metadata
  participantsCount: Number,              // Total participants
  messagesCount: Number,                  // Total messages
  isArchived: Boolean,                    // Archived status
  createdAt: ISODate,                     // Conversation creation
  updatedAt: ISODate                      // Last activity
}
```

**Indexes Required**:

1. **Participants Index**: `{ "participants.userId": 1 }`
2. **Type & Updated Index**: `{ type: 1, updatedAt: -1 }`
3. **Search Index**: `{ name: "text", description: "text" }` (for group search)
4. **Active Conversations**: `{ "participants.userId": 1, "participants.isActive": 1, updatedAt: -1 }`
5. **Compound Unique for 1-on-1**: `{ type: 1, "participants.userId": 1 }` (sparse, for OneOnOne only)

**Validation Rules**:
- `type` must be one of [OneOnOne, Group]
- OneOnOne conversations must have exactly 2 participants
- Group conversations must have 2+ participants
- Group name required if type is Group
- Participant role must be one of [Admin, Member]

---

### Collection 2: messages

**Purpose**: Store all chat messages

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  conversationId: "ObjectId string",      // Parent conversation (indexed)
  senderId: "UUID string",                // Message sender (indexed)
  senderUsername: "string",               // Denormalized sender username
  senderDisplayName: "string",            // Denormalized sender display name
  senderAvatarUrl: "string",              // Denormalized sender avatar

  // Message Content
  content: "string",                      // Message text (max 5000 chars)
  messageType: "string",                  // Text/Media/File/Voice/Location/Contact/System

  // Media/Files (for Media type)
  mediaUrls: [
    {
      type: "string",                     // Image/Video/Audio/Document
      url: "string",                      // CDN URL
      thumbnailUrl: "string",             // Thumbnail URL (for images/videos)
      fileName: "string",                 // Original file name
      fileSize: Number,                   // File size in bytes
      mimeType: "string",                 // MIME type
      duration: Number,                   // Duration in seconds (for audio/video)
      width: Number,                      // Image/video width
      height: Number                      // Image/video height
    }
  ],

  // Read Receipts
  readBy: [
    {
      userId: "UUID string",
      username: "string",
      readAt: ISODate
    }
  ],

  // Delivery Status
  deliveredTo: [
    {
      userId: "UUID string",
      username: "string",
      deliveredAt: ISODate
    }
  ],

  // Reactions
  reactions: [
    {
      emoji: "string",                    // Emoji character
      userId: "UUID string",
      username: "string",
      addedAt: ISODate
    }
  ],

  // Reply/Thread
  replyTo: {                              // If replying to another message
    messageId: "ObjectId string",
    content: "string",                    // Preview of original message
    senderId: "UUID string",
    senderUsername: "string"
  },

  // Mentions
  mentions: [
    {
      userId: "UUID string",
      username: "string",
      displayName: "string"
    }
  ],

  // Location (for Location type)
  location: {
    latitude: NumberDecimal,
    longitude: NumberDecimal,
    name: "string",                       // Place name
    address: "string"                     // Full address
  },

  // Status
  isEdited: Boolean,                      // Was message edited
  editedAt: ISODate,                      // Last edit timestamp
  isDeleted: Boolean,                     // Soft delete flag
  deletedAt: ISODate,                     // Deletion timestamp
  deletedFor: ["UUID string"],            // Users who deleted this message

  // Metadata
  createdAt: ISODate,                     // Message sent timestamp
  updatedAt: ISODate                      // Last update
}
```

**Indexes Required**:

1. **Conversation Messages Index**: `{ conversationId: 1, createdAt: -1 }`
2. **Sender Index**: `{ senderId: 1, createdAt: -1 }`
3. **Search Index**: `{ content: "text" }` (text search)
4. **Unread Messages**: `{ conversationId: 1, "readBy.userId": 1, createdAt: -1 }`
5. **Mentions Index**: `{ "mentions.userId": 1, createdAt: -1 }`
6. **Reply Index**: `{ "replyTo.messageId": 1 }`

**Validation Rules**:
- `content` max 5000 characters
- `messageType` must be one of [Text, Media, File, Voice, Location, Contact, System]
- `mediaUrls` required if messageType is Media
- `location` required if messageType is Location

---

## PostFeed Service Database (MongoDB)

### Database Name
`PostFeedServiceDB`

### Collections Overview

This service requires **5 collections** for social media posts.

---

### Collection 1: posts

**Purpose**: Store user posts/updates

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  userId: "UUID string",                  // Post author (indexed)
  username: "string",                     // Denormalized author username
  displayName: "string",                  // Denormalized author display name
  avatarUrl: "string",                    // Denormalized author avatar

  // Post Content
  content: "string",                      // Post text (max 5000 chars, indexed for search)
  mediaUrls: ["string"],                  // Array of CDN URLs

  // Post Metadata
  postType: "string",                     // Text/Photo/Video/Link/Poll/Event
  visibility: "string",                   // Public/Friends/Private/Custom

  // Location
  location: {
    name: "string",                       // Place name
    latitude: NumberDecimal,
    longitude: NumberDecimal,
    address: "string"
  },

  // Hashtags & Mentions
  hashtags: ["string"],                   // Array of hashtags (indexed)
  mentions: [                             // Array of mentioned users
    {
      userId: "UUID string",
      username: "string",
      displayName: "string"
    }
  ],

  // Link Preview (for Link posts)
  linkPreview: {
    url: "string",
    title: "string",
    description: "string",
    imageUrl: "string",
    domain: "string"
  },

  // Poll (for Poll posts)
  poll: {
    question: "string",
    options: [
      {
        id: "string",
        text: "string",
        votes: Number,
        votedBy: ["UUID string"]          // Users who voted for this option
      }
    ],
    allowMultipleAnswers: Boolean,
    expiresAt: ISODate
  },

  // Engagement Counts
  likesCount: Number,                     // Total likes
  commentsCount: Number,                  // Total comments
  sharesCount: Number,                    // Total shares
  viewsCount: Number,                     // Total views

  // Status
  isPinned: Boolean,                      // Pinned to profile
  isArchived: Boolean,                    // Archived by user
  isDeleted: Boolean,                     // Soft delete flag
  deletedAt: ISODate,                     // Deletion timestamp

  // Metadata
  createdAt: ISODate,                     // Post creation (indexed)
  updatedAt: ISODate,                     // Last update

  // For shared posts
  originalPostId: ObjectId,               // If this is a shared post
  shareCaption: "string"                  // Caption when sharing
}
```

**Indexes Required**:

1. **User Posts Index**: `{ userId: 1, createdAt: -1 }`
2. **Feed Index**: `{ visibility: 1, createdAt: -1 }`
3. **Hashtags Index**: `{ hashtags: 1, createdAt: -1 }`
4. **Mentions Index**: `{ "mentions.userId": 1, createdAt: -1 }`
5. **Search Index**: `{ content: "text" }` (full-text search)
6. **Location Index**: `{ "location.coordinates": "2dsphere" }` (geospatial)
7. **Trending Index**: `{ likesCount: -1, commentsCount: -1, sharesCount: -1, createdAt: -1 }` (compound for trending)
8. **Active Posts**: `{ isDeleted: 1, visibility: 1, createdAt: -1 }`

**Validation Rules**:
- `content` max 5000 characters
- `postType` must be one of [Text, Photo, Video, Link, Poll, Event]
- `visibility` must be one of [Public, Friends, Private, Custom]
- At least one of `content` or `mediaUrls` must be present

---

### Collection 2: comments

**Purpose**: Store post comments and replies

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  postId: ObjectId,                       // Parent post (indexed)
  userId: "UUID string",                  // Comment author (indexed)
  username: "string",                     // Denormalized author username
  displayName: "string",                  // Denormalized author display name
  avatarUrl: "string",                    // Denormalized author avatar

  // Comment Content
  content: "string",                      // Comment text (max 2000 chars)

  // Mentions
  mentions: [                             // Mentioned users
    {
      userId: "UUID string",
      username: "string",
      displayName: "string"
    }
  ],

  // Reply Structure
  parentCommentId: ObjectId,              // If this is a reply (indexed)

  // Engagement
  likesCount: Number,                     // Total likes on comment
  repliesCount: Number,                   // Total replies

  // Status
  isEdited: Boolean,                      // Was comment edited
  editedAt: ISODate,                      // Last edit timestamp
  isDeleted: Boolean,                     // Soft delete flag
  deletedAt: ISODate,                     // Deletion timestamp

  // Metadata
  createdAt: ISODate,                     // Comment creation (indexed)
  updatedAt: ISODate                      // Last update
}
```

**Indexes Required**:

1. **Post Comments Index**: `{ postId: 1, createdAt: -1 }`
2. **User Comments Index**: `{ userId: 1, createdAt: -1 }`
3. **Replies Index**: `{ parentCommentId: 1, createdAt: -1 }`
4. **Mentions Index**: `{ "mentions.userId": 1, createdAt: -1 }`
5. **Active Comments**: `{ postId: 1, isDeleted: 1, createdAt: -1 }`

**Validation Rules**:
- `content` max 2000 characters, required
- If `parentCommentId` exists, must reference valid comment

---

### Collection 3: likes

**Purpose**: Store likes on posts and comments

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  entityId: ObjectId,                     // Post or Comment ID (indexed)
  entityType: "string",                   // Post / Comment
  userId: "UUID string",                  // User who liked (indexed)
  username: "string",                     // Denormalized username
  displayName: "string",                  // Denormalized display name
  avatarUrl: "string",                    // Denormalized avatar
  reactionType: "string",                 // Like/Love/Haha/Wow/Sad/Angry (for future)
  createdAt: ISODate                      // Like timestamp
}
```

**Indexes Required**:

1. **Entity Likes Index**: `{ entityId: 1, entityType: 1, createdAt: -1 }`
2. **User Likes Index**: `{ userId: 1, createdAt: -1 }`
3. **Unique Constraint**: `{ entityId: 1, entityType: 1, userId: 1 }` (unique, one like per user per entity)
4. **Reaction Type Index**: `{ reactionType: 1 }`

**Validation Rules**:
- `entityType` must be one of [Post, Comment]
- `reactionType` must be one of [Like, Love, Haha, Wow, Sad, Angry]
- Cannot like same entity twice (enforced by unique index)

---

### Collection 4: shares

**Purpose**: Track post shares

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  originalPostId: ObjectId,               // Original post being shared (indexed)
  sharedPostId: ObjectId,                 // New post created as share (indexed)
  userId: "UUID string",                  // User who shared (indexed)
  username: "string",                     // Denormalized username
  displayName: "string",                  // Denormalized display name
  avatarUrl: "string",                    // Denormalized avatar
  caption: "string",                      // Share caption/comment (max 500 chars)
  visibility: "string",                   // Public/Friends/Private
  createdAt: ISODate                      // Share timestamp
}
```

**Indexes Required**:

1. **Original Post Shares**: `{ originalPostId: 1, createdAt: -1 }`
2. **User Shares**: `{ userId: 1, createdAt: -1 }`
3. **Shared Post Reference**: `{ sharedPostId: 1 }`

**Validation Rules**:
- `caption` max 500 characters
- `visibility` must be one of [Public, Friends, Private]

---

### Collection 5: hashtags

**Purpose**: Track hashtag usage and trending

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  tag: "string",                          // Hashtag text (indexed, unique, lowercase)
  count: Number,                          // Total usage count
  trendScore: Number,                     // Calculated trend score (indexed)
  lastUsed: ISODate,                      // Last time hashtag was used
  createdAt: ISODate,                     // First time hashtag was used
  updatedAt: ISODate                      // Last update
}
```

**Indexes Required**:

1. **Tag Index**: `{ tag: 1 }` (unique)
2. **Trending Index**: `{ trendScore: -1, count: -1 }`
3. **Recent Usage**: `{ lastUsed: -1 }`

**Trend Score Calculation**:
- Algorithm: `trendScore = (count_last_24h * 0.5) + (count_last_7d * 0.3) + (count_last_30d * 0.2)`
- Recalculate every hour via scheduled job
- Higher score = more trending

**Validation Rules**:
- `tag` must be lowercase, alphanumeric + underscore only
- `tag` max 50 characters

---

## Media Service Database (MongoDB)

### Database Name
`MediaServiceDB`

### Collections Overview

This service requires **1 collection** for file management.

---

### Collection 1: media_files

**Purpose**: Store metadata for all uploaded files

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  userId: "UUID string",                  // File owner (indexed)

  // File Information
  fileName: "string",                     // Generated unique filename
  originalFileName: "string",             // Original uploaded filename
  fileType: "string",                     // Image/Video/Audio/Document
  mimeType: "string",                     // MIME type (image/jpeg, video/mp4, etc.)
  fileSize: Number,                       // File size in bytes

  // Storage
  storageProvider: "string",              // Local/S3/Azure/GCP
  storagePath: "string",                  // Path in storage
  url: "string",                          // CDN URL to access file
  thumbnailUrl: "string",                 // Thumbnail URL (for images/videos)

  // Media-specific Metadata
  dimensions: {                           // For images/videos
    width: Number,
    height: Number,
    aspectRatio: "string"                 // 16:9, 4:3, 1:1, etc.
  },
  duration: Number,                       // Duration in seconds (for audio/video)
  bitrate: Number,                        // Bitrate (for audio/video)
  codec: "string",                        // Video/audio codec
  frameRate: Number,                      // Frame rate (for video)

  // Organization
  category: "string",                     // profile/post/chat/document/other
  tags: ["string"],                       // User-defined tags (indexed)
  description: "string",                  // File description (max 500 chars)

  // Processing Status
  processingStatus: "string",             // Pending/Processing/Completed/Failed
  processingError: "string",              // Error message if failed
  processingStartedAt: ISODate,
  processingCompletedAt: ISODate,

  // Usage Tracking
  usageCount: Number,                     // How many times file is referenced
  usedIn: [                               // Where file is used
    {
      entityType: "string",               // Post/Message/Profile/etc.
      entityId: "string"
    }
  ],

  // Security
  isPublic: Boolean,                      // Public access allowed
  accessKey: "string",                    // Private access key
  expiresAt: ISODate,                     // Temporary file expiry (for temp uploads)

  // Status
  isDeleted: Boolean,                     // Soft delete flag
  deletedAt: ISODate,                     // Deletion timestamp

  // Metadata
  uploadedAt: ISODate,                    // Upload timestamp (indexed)
  updatedAt: ISODate,                     // Last update

  // EXIF Data (for images)
  exifData: {
    camera: "string",
    lens: "string",
    iso: Number,
    aperture: "string",
    shutterSpeed: "string",
    focalLength: "string",
    flash: Boolean,
    capturedAt: ISODate,
    gps: {
      latitude: NumberDecimal,
      longitude: NumberDecimal,
      altitude: Number
    }
  }
}
```

**Indexes Required**:

1. **User Files Index**: `{ userId: 1, uploadedAt: -1 }`
2. **File Type Index**: `{ fileType: 1, uploadedAt: -1 }`
3. **Category Index**: `{ category: 1, uploadedAt: -1 }`
4. **Tags Index**: `{ tags: 1 }`
5. **Processing Status**: `{ processingStatus: 1, processingStartedAt: 1 }`
6. **Search Index**: `{ originalFileName: "text", description: "text", tags: "text" }`
7. **Active Files**: `{ isDeleted: 1, userId: 1, uploadedAt: -1 }`
8. **TTL Index**: `{ expiresAt: 1 }` (expireAfterSeconds: 0) - auto-delete temporary files

**Validation Rules**:
- `fileType` must be one of [Image, Video, Audio, Document]
- `processingStatus` must be one of [Pending, Processing, Completed, Failed]
- `category` must be one of [profile, post, chat, document, other]
- `description` max 500 characters

**File Size Limits**:
- Images: 10 MB
- Videos: 100 MB
- Audio: 20 MB
- Documents: 20 MB

---

## Notification Service Database (MongoDB)

### Database Name
`NotificationServiceDB`

### Collections Overview

This service requires **1 collection** with TTL for auto-expiry.

---

### Collection 1: notifications

**Purpose**: Store user notifications with auto-expiry

**Document Structure**:

```javascript
{
  _id: ObjectId,                          // MongoDB auto-generated ID
  userId: "UUID string",                  // Notification recipient (indexed)

  // Notification Content
  type: "string",                         // Notification type (indexed)
  title: "string",                        // Notification title (max 100 chars)
  message: "string",                      // Notification message (max 500 chars)

  // Actor (who triggered this notification)
  actorId: "UUID string",                 // User who performed action
  actorUsername: "string",                // Denormalized actor username
  actorDisplayName: "string",             // Denormalized actor display name
  actorAvatarUrl: "string",               // Denormalized actor avatar

  // Related Entity
  relatedEntityId: "string",              // ID of related entity (post, comment, etc.)
  relatedEntityType: "string",            // Post/Comment/User/Message/etc.
  actionUrl: "string",                    // Deep link URL to action

  // Additional Data
  metadata: {                             // Type-specific additional data
    // Dynamic based on notification type
    // Examples:
    // For Like: { postPreview: "text...", totalLikes: 15 }
    // For Comment: { commentPreview: "text...", postPreview: "text..." }
    // For FriendRequest: { mutualFriendsCount: 5 }
  },

  // Status
  isRead: Boolean,                        // Read status (indexed)
  readAt: ISODate,                        // When notification was read

  // Delivery
  deliveryStatus: "string",               // Pending/Sent/Failed
  channels: ["string"],                   // [Push, Email, InApp]
  sentVia: ["string"],                    // Channels successfully sent through

  // Grouping (for combining similar notifications)
  groupKey: "string",                     // Key to group similar notifications
  groupCount: Number,                     // How many notifications in group

  // Expiry
  expiresAt: ISODate,                     // Auto-delete after 7 days (indexed with TTL)

  // Metadata
  createdAt: ISODate,                     // Notification creation (indexed)
  updatedAt: ISODate                      // Last update
}
```

**Indexes Required**:

1. **User Notifications Index**: `{ userId: 1, createdAt: -1 }`
2. **Unread Notifications**: `{ userId: 1, isRead: 1, createdAt: -1 }`
3. **Type Index**: `{ type: 1, createdAt: -1 }`
4. **Actor Index**: `{ actorId: 1, createdAt: -1 }`
5. **Group Index**: `{ groupKey: 1 }`
6. **TTL Index**: `{ expiresAt: 1 }` (expireAfterSeconds: 0) - auto-delete after 7 days

**Notification Types**:
- `FriendRequest`: New friend request received
- `FriendAccepted`: Friend request accepted
- `Message`: New message received
- `Post`: Friend posted something
- `Comment`: Someone commented on your post
- `CommentReply`: Someone replied to your comment
- `Like`: Someone liked your post/comment
- `Share`: Someone shared your post
- `Mention`: You were mentioned
- `Follow`: New follower
- `ProfileView`: Someone viewed your profile

**Validation Rules**:
- `type` must be one of the predefined notification types
- `title` max 100 characters
- `message` max 500 characters
- `deliveryStatus` must be one of [Pending, Sent, Failed]
- `expiresAt` defaults to createdAt + 7 days

**Auto-Expiry**:
- TTL index automatically deletes notifications after 7 days
- No manual cleanup needed
- Reduces database size and improves performance

---

## Cross-Service Data Synchronization

### Overview
Since we're using microservices with separate databases, we need to ensure data consistency across services.

### Denormalized Data Strategy

**What to Denormalize**:
- Username, displayName, avatarUrl in all services
- Frequently accessed data that rarely changes
- Data needed for display without additional queries

**Services Requiring Denormalization**:

1. **UserProfile Service** ← from Auth Service:
   - userId, username, email (sync on user creation/update)

2. **Chat Service** ← from UserProfile Service:
   - username, displayName, avatarUrl (sync on profile update)

3. **PostFeed Service** ← from UserProfile Service:
   - username, displayName, avatarUrl (sync on profile update)

4. **Media Service** ← from UserProfile Service:
   - No denormalization needed (only stores userId)

5. **Notification Service** ← from UserProfile Service:
   - username, displayName, avatarUrl for actor (sync on profile update)

### Synchronization Methods

#### Method 1: Event-Driven (Recommended)

**Message Queue**: Use RabbitMQ or Azure Service Bus

**Events to Publish**:

1. **UserCreated Event**:
```json
{
  "eventType": "UserCreated",
  "userId": "UUID",
  "username": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "timestamp": "ISO8601"
}
```
**Subscribers**: UserProfile Service (creates initial profile)

2. **UserProfileUpdated Event**:
```json
{
  "eventType": "UserProfileUpdated",
  "userId": "UUID",
  "username": "string",
  "displayName": "string",
  "avatarUrl": "string",
  "timestamp": "ISO8601"
}
```
**Subscribers**: Chat, PostFeed, Notification Services (update denormalized data)

3. **UserDeleted Event**:
```json
{
  "eventType": "UserDeleted",
  "userId": "UUID",
  "timestamp": "ISO8601"
}
```
**Subscribers**: All services (soft delete related data)

#### Method 2: Scheduled Sync Jobs

**Fallback for Event Failures**:
- Run hourly/daily sync jobs
- Compare Auth service users with UserProfile profiles
- Sync any missing or outdated denormalized data
- Log discrepancies for manual review

#### Method 3: API Calls (Real-time)

**When to Use**:
- Critical data that must be current
- Low-volume operations
- User-initiated actions

**Example**: Before displaying user profile, call Auth service to verify user still exists and is active

### Consistency Guarantees

**Eventual Consistency**:
- Accept that denormalized data may be slightly stale (seconds to minutes)
- Most social features don't require immediate consistency
- Use cache invalidation for critical updates

**Strong Consistency Requirements**:
- User authentication (Auth service only)
- Financial transactions (if implemented)
- User permissions and roles

---

## Performance Optimization

### Database-Level Optimizations

#### PostgreSQL (Auth Service)

1. **Connection Pooling**:
   - Use connection pooler (PgBouncer or built-in .NET pooling)
   - Pool size: 20-50 connections
   - Connection timeout: 30 seconds

2. **Query Optimization**:
   - Use EXPLAIN ANALYZE for slow queries
   - Add indexes on frequently queried columns
   - Use prepared statements (parameterized queries)
   - Avoid N+1 queries (use JOINs or batch queries)

3. **Vacuum and Analyze**:
   - Run VACUUM ANALYZE weekly
   - Auto-vacuum enabled with appropriate thresholds
   - Monitor table bloat

4. **Partitioning** (for large tables):
   - Partition user_login_history by month
   - Improves query performance for recent data

#### MongoDB (All Other Services)

1. **Index Optimization**:
   - Create compound indexes for common query patterns
   - Use covered queries (all fields in index)
   - Monitor slow queries (> 100ms)
   - Avoid too many indexes (slows writes)

2. **Document Design**:
   - Embed frequently accessed data
   - Reference rarely accessed data
   - Keep documents < 16 MB (MongoDB limit)
   - Use arrays for bounded lists (< 1000 items)

3. **Sharding** (for horizontal scaling):
   - Shard key: userId (ensures even distribution)
   - Shard when collection exceeds 2-5 GB
   - Use range or hashed sharding

4. **Read Preference**:
   - Primary for writes and critical reads
   - Secondary for analytics and reporting
   - Nearest for low latency

5. **Write Concerns**:
   - w: 1 (default) for most operations
   - w: majority for critical operations
   - journal: true for durability

### Application-Level Optimizations

1. **Caching Strategy**:
   - **Redis Cache** for frequently accessed data
   - Cache user profiles (TTL: 15 minutes)
   - Cache posts feed (TTL: 5 minutes)
   - Cache trending hashtags (TTL: 1 hour)
   - Invalidate on updates

2. **Pagination**:
   - Use cursor-based pagination for large datasets
   - Limit: 20-50 items per page
   - Avoid offset-based pagination for deep pages

3. **Lazy Loading**:
   - Load comments on demand (not with post)
   - Load media thumbnails first, full size on click
   - Infinite scroll for feeds

4. **Batch Operations**:
   - Batch notifications (send every 5 minutes, not immediately)
   - Batch database writes where possible
   - Use bulk inserts for large datasets

5. **Database Connection Management**:
   - Use connection pooling
   - Close connections after use
   - Set appropriate timeouts

---

## Backup and Recovery Strategy

### Backup Frequency

#### PostgreSQL (Auth Service)
- **Full Backup**: Daily at 2 AM (off-peak)
- **Incremental Backup**: Every 6 hours
- **Transaction Log Backup**: Every 15 minutes
- **Retention**: 30 days

#### MongoDB (All Services)
- **Full Backup**: Daily at 3 AM (off-peak)
- **Oplog Backup**: Continuous (for point-in-time recovery)
- **Retention**: 30 days

### Backup Methods

#### PostgreSQL
1. **pg_dump** for logical backups:
```bash
pg_dump -h localhost -U postgres -d AuthServiceDB -F c -b -v -f /backups/auth_$(date +%Y%m%d).dump
```

2. **PITR (Point-in-Time Recovery)**:
   - Enable WAL archiving
   - Archive to S3 or Azure Blob Storage
   - Test recovery quarterly

#### MongoDB
1. **mongodump** for logical backups:
```bash
mongodump --uri="mongodb://localhost:27017" --db=ChatServiceDB --gzip --archive=/backups/chat_$(date +%Y%m%d).gz
```

2. **Oplog for continuous backup**:
   - Tail oplog for continuous backup
   - Store in S3 or Azure Blob Storage

3. **Snapshots** (if using cloud provider):
   - EBS snapshots (AWS)
   - Managed Disks snapshots (Azure)
   - Persistent Disk snapshots (GCP)

### Recovery Procedures

#### Recovery Time Objective (RTO): < 4 hours
#### Recovery Point Objective (RPO): < 15 minutes

**Disaster Recovery Steps**:

1. **Database Corruption**:
   - Restore from most recent full backup
   - Apply incremental backups
   - Apply transaction logs (PostgreSQL) or oplog (MongoDB)
   - Verify data integrity

2. **Accidental Data Deletion**:
   - Point-in-time recovery to before deletion
   - Export deleted data
   - Re-import to production

3. **Complete System Failure**:
   - Provision new infrastructure
   - Restore all databases from backups
   - Update connection strings in applications
   - Verify all services operational

### Testing Backups

**Quarterly Backup Restoration Test**:
1. Provision test environment
2. Restore all databases from latest backups
3. Run integrity checks
4. Verify application functionality
5. Document any issues
6. Update recovery procedures

---

## Database Monitoring

### Key Metrics to Monitor

#### PostgreSQL
- Active connections count
- Connection pool utilization
- Query execution time (slow queries > 1000ms)
- Transaction rate (commits/rollbacks per second)
- Cache hit ratio (> 95% target)
- Deadlocks count
- Table/index bloat

#### MongoDB
- Operations per second (reads/writes)
- Query execution time (slow queries > 100ms)
- Connection count
- Replication lag (if using replica sets)
- Memory usage (working set should fit in RAM)
- Disk I/O utilization
- Index usage statistics

### Alerting Thresholds

**Critical Alerts** (immediate action):
- Database down/unreachable
- Replication lag > 10 seconds
- Disk space < 10% free
- Connection pool exhausted
- Query execution time > 5 seconds

**Warning Alerts** (investigate within 1 hour):
- CPU usage > 80% for 15 minutes
- Memory usage > 85%
- Disk space < 20% free
- Slow query count increasing
- Failed backup job

### Monitoring Tools

**Recommended Tools**:
- **PostgreSQL**: pgAdmin, pg_stat_statements, Datadog, New Relic
- **MongoDB**: MongoDB Compass, MongoDB Atlas (managed), Datadog, New Relic
- **Application Performance**: Application Insights, Elastic APM
- **Infrastructure**: Prometheus + Grafana, CloudWatch (AWS), Azure Monitor

---

## Security Best Practices

### Database Security

1. **Access Control**:
   - Use strong passwords (min 16 characters)
   - Rotate passwords quarterly
   - Principle of least privilege (grant only needed permissions)
   - Separate read-only users for reporting

2. **Network Security**:
   - Database in private subnet (no public access)
   - Firewall rules: allow only application servers
   - Use VPN for remote access
   - SSL/TLS for all connections

3. **Encryption**:
   - Encryption at rest (disk encryption)
   - Encryption in transit (SSL/TLS)
   - Encrypt backups
   - Key rotation policy

4. **Auditing**:
   - Enable audit logging
   - Log all DDL statements
   - Log failed login attempts
   - Regular security audits

5. **Data Masking**:
   - Mask sensitive data in non-production environments
   - PII (Personally Identifiable Information) protection
   - GDPR/CCPA compliance

### Application Security

1. **SQL Injection Prevention**:
   - Always use parameterized queries
   - Never concatenate user input into queries
   - Validate and sanitize all inputs

2. **MongoDB Injection Prevention**:
   - Validate input types
   - Use schema validation
   - Avoid string-based queries

3. **Connection String Security**:
   - Store in Azure Key Vault or AWS Secrets Manager
   - Never commit to source control
   - Use environment variables
   - Rotate credentials regularly

---

## Implementation Checklist

### Phase 1: Setup Databases

- [ ] Install PostgreSQL 14+ for Auth service
- [ ] Install MongoDB 6+ for other services
- [ ] Configure connection pooling
- [ ] Set up database users with appropriate permissions
- [ ] Enable SSL/TLS connections

### Phase 2: Create Schemas

**Auth Service (PostgreSQL)**:
- [ ] Create database `AuthServiceDB`
- [ ] Enable uuid-ossp extension
- [ ] Create 5 tables (users, user_sessions, verification_codes, user_login_history, password_reset_tokens)
- [ ] Create all indexes
- [ ] Create 6 helper functions
- [ ] Create 7 stored procedures
- [ ] Create 2 triggers
- [ ] Test all functions and procedures

**UserProfile Service (MongoDB)**:
- [ ] Create database `UserProfileServiceDB`
- [ ] Create 3 collections (user_profiles, friend_requests, user_activities)
- [ ] Create all indexes
- [ ] Set up JSON schema validation
- [ ] Set up TTL indexes
- [ ] Test CRUD operations

**Chat Service (MongoDB)**:
- [ ] Create database `ChatServiceDB`
- [ ] Create 2 collections (conversations, messages)
- [ ] Create all indexes
- [ ] Set up JSON schema validation
- [ ] Test real-time message operations

**PostFeed Service (MongoDB)**:
- [ ] Create database `PostFeedServiceDB`
- [ ] Create 5 collections (posts, comments, likes, shares, hashtags)
- [ ] Create all indexes including geospatial and text search
- [ ] Set up JSON schema validation
- [ ] Test post creation and engagement features

**Media Service (MongoDB)**:
- [ ] Create database `MediaServiceDB`
- [ ] Create media_files collection
- [ ] Create all indexes including text search
- [ ] Set up TTL index for temporary files
- [ ] Set up JSON schema validation

**Notification Service (MongoDB)**:
- [ ] Create database `NotificationServiceDB`
- [ ] Create notifications collection
- [ ] Create all indexes
- [ ] Set up TTL index (7 days auto-expiry)
- [ ] Set up JSON schema validation

### Phase 3: Data Synchronization

- [ ] Set up message queue (RabbitMQ/Azure Service Bus)
- [ ] Implement UserCreated event publisher (Auth)
- [ ] Implement UserCreated event subscriber (UserProfile)
- [ ] Implement UserProfileUpdated event publisher
- [ ] Implement UserProfileUpdated event subscribers (Chat, PostFeed, Notification)
- [ ] Implement UserDeleted event publisher
- [ ] Implement UserDeleted event subscribers (all services)
- [ ] Create scheduled sync jobs as fallback

### Phase 4: Performance Optimization

- [ ] Configure Redis cache
- [ ] Set up connection pooling
- [ ] Monitor slow queries
- [ ] Optimize indexes based on query patterns
- [ ] Implement batch operations

### Phase 5: Backup & Monitoring

- [ ] Set up automated backups (daily)
- [ ] Configure backup retention (30 days)
- [ ] Test backup restoration
- [ ] Set up monitoring (Datadog/New Relic)
- [ ] Configure alerts
- [ ] Create runbooks for common issues

### Phase 6: Security Hardening

- [ ] Enable SSL/TLS
- [ ] Configure firewall rules
- [ ] Set up database audit logging
- [ ] Implement encryption at rest
- [ ] Move connection strings to Key Vault
- [ ] Conduct security audit

---

## Database Scripts Location

All database initialization scripts should be organized as follows:

```
/Database
  /Auth
    - 01_create_tables.sql
    - 02_create_indexes.sql
    - 03_create_functions.sql
    - 04_create_procedures.sql
    - 05_create_triggers.sql
    - 06_seed_data.sql (optional)

  /UserProfile
    - create_collections.js
    - create_indexes.js
    - setup_validation.js
    - seed_data.js (optional)

  /Chat
    - create_collections.js
    - create_indexes.js
    - setup_validation.js

  /PostFeed
    - create_collections.js
    - create_indexes.js
    - setup_validation.js

  /Media
    - create_collections.js
    - create_indexes.js
    - setup_validation.js
    - setup_ttl.js

  /Notification
    - create_collections.js
    - create_indexes.js
    - setup_validation.js
    - setup_ttl.js
```

---

**End of Database Implementation Guide**

This document provides complete instructions for implementing all databases for the PostBook social media platform. Any agent following these instructions should be able to create a fully functional, performant, and secure database layer.
