# Auth Service - Database Operations Guide

## Table of Contents
1. [Database Schema Overview](#database-schema-overview)
2. [Tables Reference](#tables-reference)
3. [Functions Reference](#functions-reference)
4. [Procedures Reference](#procedures-reference)
5. [Common Operations](#common-operations)

---

## Database Schema Overview

### Tables
- **users** - Main user authentication table
- **user_sessions** - Active user sessions
- **verification_codes** - Email/phone verification codes
- **user_login_history** - Login audit trail
- **password_reset_tokens** - Password reset tokens

### Functions
- `email_exists(p_email)` - Check if email is already registered
- `username_exists(p_username)` - Check if username is taken
- `get_user_by_login(p_login)` - Get user by email or username
- `generate_verification_code()` - Generate 6-digit verification code
- `cleanup_old_data()` - Clean expired sessions and codes
- `get_user_stats()` - Get platform statistics

### Procedures
- `create_user()` - Create new user account
- `update_user_profile()` - Update user information
- `soft_delete_user()` - Soft delete user account
- `verify_email()` - Verify user email
- `store_refresh_token()` - Store JWT refresh token
- `create_user_session()` - Create new user session
- `revoke_user_sessions()` - Revoke user sessions

---

## Tables Reference

### 1. users

**Purpose**: Store user account information

**Columns**:
```sql
id                UUID PRIMARY KEY
username          VARCHAR(50) UNIQUE NOT NULL
email             VARCHAR(255) UNIQUE NOT NULL
password_hash     VARCHAR(255) NOT NULL
phone_number      VARCHAR(20) UNIQUE
is_email_verified BOOLEAN DEFAULT FALSE
is_phone_verified BOOLEAN DEFAULT FALSE
roles             JSONB DEFAULT '["User"]'
refresh_token     TEXT
refresh_token_expires_at TIMESTAMP
created_at        TIMESTAMP DEFAULT CURRENT_TIMESTAMP
updated_at        TIMESTAMP DEFAULT CURRENT_TIMESTAMP
is_deleted        BOOLEAN DEFAULT FALSE
deleted_at        TIMESTAMP
```

**Indexes**:
- `idx_users_email` - Fast email lookup
- `idx_users_username` - Fast username lookup
- `idx_users_refresh_token` - Token validation
- `idx_users_roles_gin` - Role-based queries

---

### 2. user_sessions

**Purpose**: Track active user sessions across devices

**Columns**:
```sql
id                UUID PRIMARY KEY
user_id           UUID REFERENCES users(id)
refresh_token     TEXT UNIQUE NOT NULL
device_info       VARCHAR(500)
ip_address        VARCHAR(45)
user_agent        TEXT
is_active         BOOLEAN DEFAULT TRUE
created_at        TIMESTAMP DEFAULT CURRENT_TIMESTAMP
expires_at        TIMESTAMP NOT NULL
last_activity_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
```

**Indexes**:
- `idx_user_sessions_user_id` - Get user's sessions
- `idx_user_sessions_refresh_token` - Token lookup
- `idx_user_sessions_expires_at` - Cleanup expired sessions

---

### 3. verification_codes

**Purpose**: Store email/phone verification codes

**Columns**:
```sql
id                UUID PRIMARY KEY
user_id           UUID REFERENCES users(id)
code              VARCHAR(6) NOT NULL
code_type         VARCHAR(20) NOT NULL (Email/Phone/PasswordReset)
is_used           BOOLEAN DEFAULT FALSE
created_at        TIMESTAMP DEFAULT CURRENT_TIMESTAMP
expires_at        TIMESTAMP NOT NULL
used_at           TIMESTAMP
```

**Indexes**:
- `idx_verification_codes_user_id` - User's codes
- `idx_verification_codes_code` - Code verification
- `idx_verification_codes_expires_at` - Cleanup

---

### 4. user_login_history

**Purpose**: Audit log of login attempts

**Columns**:
```sql
id                UUID PRIMARY KEY
user_id           UUID REFERENCES users(id)
login_type        VARCHAR(20) (Success/Failed)
ip_address        VARCHAR(45)
user_agent        TEXT
login_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP
```

**Indexes**:
- `idx_user_login_history_user_id` - User's login history
- `idx_user_login_history_login_at` - Time-based queries

---

### 5. password_reset_tokens

**Purpose**: Secure password reset flow

**Columns**:
```sql
id                UUID PRIMARY KEY
user_id           UUID REFERENCES users(id)
token             TEXT UNIQUE NOT NULL
is_used           BOOLEAN DEFAULT FALSE
created_at        TIMESTAMP DEFAULT CURRENT_TIMESTAMP
expires_at        TIMESTAMP NOT NULL
used_at           TIMESTAMP
```

**Indexes**:
- `idx_password_reset_tokens_token` - Token lookup
- `idx_password_reset_tokens_user_id` - User's tokens

---

## Functions Reference

### 1. email_exists(p_email VARCHAR)

**Purpose**: Check if email is already registered

**Parameters**:
- `p_email` - Email address to check

**Returns**: BOOLEAN

**Usage**:
```sql
-- Check if email exists before registration
SELECT email_exists('user@example.com');
-- Returns: true or false
```

**Use Cases**:
- User registration validation
- Duplicate email prevention
- Email availability check

---

### 2. username_exists(p_username VARCHAR)

**Purpose**: Check if username is already taken

**Parameters**:
- `p_username` - Username to check

**Returns**: BOOLEAN

**Usage**:
```sql
-- Check username availability
SELECT username_exists('john_doe');
-- Returns: true or false
```

**Use Cases**:
- Username validation during registration
- Real-time username availability check
- Prevent duplicate usernames

---

### 3. get_user_by_login(p_login VARCHAR)

**Purpose**: Get user by email or username

**Parameters**:
- `p_login` - Email or username

**Returns**: TABLE (user record)

**Usage**:
```sql
-- Find user for login
SELECT * FROM get_user_by_login('user@example.com');
-- OR
SELECT * FROM get_user_by_login('john_doe');
```

**Use Cases**:
- User login
- Flexible login with email or username
- User lookup

---

### 4. generate_verification_code()

**Purpose**: Generate random 6-digit verification code

**Parameters**: None

**Returns**: VARCHAR(6)

**Usage**:
```sql
-- Generate verification code
SELECT generate_verification_code();
-- Returns: '123456' (random)
```

**Use Cases**:
- Email verification
- Phone verification
- 2FA codes

---

### 5. cleanup_old_data()

**Purpose**: Clean expired sessions and verification codes

**Parameters**: None

**Returns**: VOID

**Usage**:
```sql
-- Run cleanup (typically in scheduled job)
SELECT cleanup_old_data();
```

**Use Cases**:
- Daily maintenance task
- Free up database space
- Remove expired data

---

### 6. get_user_stats()

**Purpose**: Get platform statistics

**Parameters**: None

**Returns**: TABLE (statistics)

**Usage**:
```sql
-- Get platform stats
SELECT * FROM get_user_stats();
-- Returns: total_users, verified_users, active_today, etc.
```

**Use Cases**:
- Admin dashboard
- Analytics
- Platform monitoring

---

## Procedures Reference

### 1. create_user()

**Purpose**: Create new user account with validation

**Parameters**:
```sql
IN  p_username        VARCHAR(50)
IN  p_email           VARCHAR(255)
IN  p_password_hash   VARCHAR(255)
IN  p_phone_number    VARCHAR(20) DEFAULT NULL
OUT p_user_id         UUID
OUT p_error_message   TEXT
```

**Usage**:
```sql
-- Create new user
CALL create_user(
    'john_doe',                              -- username
    'john@example.com',                      -- email
    '$2a$10$hashed_password_here',           -- password hash (BCrypt)
    '+1234567890',                           -- phone (optional)
    NULL,                                     -- user_id (OUT)
    NULL                                      -- error_message (OUT)
);

-- Check result
-- If p_user_id IS NOT NULL: Success
-- If p_error_message IS NOT NULL: Error
```

**Validation**:
- Email format validation
- Username format validation (alphanumeric + underscore)
- Username length (3-50 characters)
- Duplicate email/username check

**Use Cases**:
- User registration
- Admin user creation
- Bulk user import

---

### 2. update_user_profile()

**Purpose**: Update user profile information

**Parameters**:
```sql
IN  p_user_id         UUID
IN  p_phone_number    VARCHAR(20) DEFAULT NULL
IN  p_is_email_verified BOOLEAN DEFAULT NULL
IN  p_is_phone_verified BOOLEAN DEFAULT NULL
OUT p_success         BOOLEAN
OUT p_error_message   TEXT
```

**Usage**:
```sql
-- Update user profile
CALL update_user_profile(
    '550e8400-e29b-41d4-a716-446655440000'::UUID,  -- user_id
    '+1234567890',                                   -- new phone number
    true,                                            -- email verified
    true,                                            -- phone verified
    NULL,                                            -- success (OUT)
    NULL                                             -- error_message (OUT)
);
```

**Use Cases**:
- Update phone number
- Mark email as verified
- Mark phone as verified
- Profile updates

---

### 3. soft_delete_user()

**Purpose**: Soft delete user account (mark as deleted without removing data)

**Parameters**:
```sql
IN  p_user_id         UUID
OUT p_success         BOOLEAN
OUT p_error_message   TEXT
```

**Usage**:
```sql
-- Soft delete user
CALL soft_delete_user(
    '550e8400-e29b-41d4-a716-446655440000'::UUID,  -- user_id
    NULL,                                            -- success (OUT)
    NULL                                             -- error_message (OUT)
);
```

**What It Does**:
- Sets `is_deleted = true`
- Sets `deleted_at = CURRENT_TIMESTAMP`
- Revokes all active sessions
- Preserves all user data for audit

**Use Cases**:
- User account deletion
- GDPR compliance preparation
- Account deactivation

---

### 4. verify_email()

**Purpose**: Verify user email with verification code

**Parameters**:
```sql
IN  p_user_id         UUID
IN  p_code            VARCHAR(6)
OUT p_success         BOOLEAN
OUT p_error_message   TEXT
```

**Usage**:
```sql
-- Verify email with code
CALL verify_email(
    '550e8400-e29b-41d4-a716-446655440000'::UUID,  -- user_id
    '123456',                                        -- verification code
    NULL,                                            -- success (OUT)
    NULL                                             -- error_message (OUT)
);
```

**Validation**:
- Code must exist
- Code must not be expired (15 minutes)
- Code must not be used
- Code type must be 'Email'

**What It Does**:
- Marks code as used
- Sets user `is_email_verified = true`
- Records `used_at` timestamp

**Use Cases**:
- Email verification flow
- Account activation
- Email ownership confirmation

---

### 5. store_refresh_token()

**Purpose**: Store JWT refresh token for user

**Parameters**:
```sql
IN  p_user_id                   UUID
IN  p_refresh_token             TEXT
IN  p_refresh_token_expires_at  TIMESTAMP
OUT p_success                   BOOLEAN
OUT p_error_message             TEXT
```

**Usage**:
```sql
-- Store refresh token
CALL store_refresh_token(
    '550e8400-e29b-41d4-a716-446655440000'::UUID,  -- user_id
    'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...',    -- JWT refresh token
    CURRENT_TIMESTAMP + INTERVAL '7 days',          -- expiration (7 days)
    NULL,                                            -- success (OUT)
    NULL                                             -- error_message (OUT)
);
```

**What It Does**:
- Updates user's refresh_token
- Sets expiration timestamp
- Updates updated_at

**Use Cases**:
- Login flow
- Token refresh
- Session management

---

### 6. create_user_session()

**Purpose**: Create new user session with device tracking

**Parameters**:
```sql
IN  p_user_id         UUID
IN  p_refresh_token   TEXT
IN  p_device_info     VARCHAR(500) DEFAULT NULL
IN  p_ip_address      VARCHAR(45) DEFAULT NULL
IN  p_user_agent      TEXT DEFAULT NULL
IN  p_expires_at      TIMESTAMP DEFAULT NULL
OUT p_session_id      UUID
OUT p_error_message   TEXT
```

**Usage**:
```sql
-- Create session on login
CALL create_user_session(
    '550e8400-e29b-41d4-a716-446655440000'::UUID,  -- user_id
    'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...',    -- refresh token
    'iPhone 14 Pro',                                 -- device info
    '192.168.1.100',                                 -- IP address
    'Mozilla/5.0...',                                -- user agent
    CURRENT_TIMESTAMP + INTERVAL '7 days',          -- expiration
    NULL,                                            -- session_id (OUT)
    NULL                                             -- error_message (OUT)
);
```

**Use Cases**:
- User login
- Multi-device support
- Session tracking

---

### 7. revoke_user_sessions()

**Purpose**: Revoke all or specific user sessions

**Parameters**:
```sql
IN  p_user_id         UUID
IN  p_session_id      UUID DEFAULT NULL  -- NULL revokes all sessions
OUT p_sessions_revoked INT
OUT p_error_message   TEXT
```

**Usage**:
```sql
-- Revoke all user sessions (logout from all devices)
CALL revoke_user_sessions(
    '550e8400-e29b-41d4-a716-446655440000'::UUID,  -- user_id
    NULL,                                            -- session_id (NULL = all)
    NULL,                                            -- sessions_revoked (OUT)
    NULL                                             -- error_message (OUT)
);

-- Revoke specific session (logout from one device)
CALL revoke_user_sessions(
    '550e8400-e29b-41d4-a716-446655440000'::UUID,  -- user_id
    'session-uuid-here'::UUID,                       -- specific session_id
    NULL,                                            -- sessions_revoked (OUT)
    NULL                                             -- error_message (OUT)
);
```

**Use Cases**:
- User logout
- Logout from all devices
- Security: revoke compromised sessions
- Account deletion

---

## Common Operations

### 🔐 User Registration Flow

**Step 1: Validate Input**
```sql
-- Check if email exists
SELECT email_exists('john@example.com');
-- Returns: false (available)

-- Check if username exists
SELECT username_exists('john_doe');
-- Returns: false (available)
```

**Step 2: Hash Password** (In Application Code)
```javascript
// Node.js example with bcrypt
const bcrypt = require('bcrypt');
const passwordHash = await bcrypt.hash(password, 10);
```

**Step 3: Create User**
```sql
DO $$
DECLARE
    v_user_id UUID;
    v_error TEXT;
BEGIN
    CALL create_user(
        'john_doe',                              -- username
        'john@example.com',                      -- email
        '$2a$10$hashed_password_here',           -- password hash
        '+1234567890',                           -- phone
        v_user_id,                               -- OUT: user_id
        v_error                                  -- OUT: error
    );

    IF v_error IS NOT NULL THEN
        RAISE EXCEPTION '%', v_error;
    END IF;

    RAISE NOTICE 'User created with ID: %', v_user_id;
END $$;
```

**Step 4: Create Verification Code**
```sql
INSERT INTO verification_codes (user_id, code, code_type, expires_at)
VALUES (
    '550e8400-e29b-41d4-a716-446655440000',
    generate_verification_code(),
    'Email',
    CURRENT_TIMESTAMP + INTERVAL '15 minutes'
)
RETURNING code;
-- Returns: '123456'
```

**Step 5: Send Verification Email** (In Application Code)
```javascript
// Send email with code '123456'
await sendEmail(email, 'Verify your email', `Your code: ${code}`);
```

---

### 📧 Email Verification Flow

**Step 1: User Submits Code**
```sql
DO $$
DECLARE
    v_success BOOLEAN;
    v_error TEXT;
BEGIN
    CALL verify_email(
        '550e8400-e29b-41d4-a716-446655440000',  -- user_id
        '123456',                                  -- code from user
        v_success,
        v_error
    );

    IF v_error IS NOT NULL THEN
        RAISE EXCEPTION '%', v_error;
    END IF;

    RAISE NOTICE 'Email verified successfully';
END $$;
```

---

### 🔑 Login Flow

**Step 1: Find User**
```sql
-- Get user by email or username
SELECT * FROM get_user_by_login('john@example.com');
-- OR
SELECT * FROM get_user_by_login('john_doe');
```

**Step 2: Verify Password** (In Application Code)
```javascript
const isValid = await bcrypt.compare(password, user.password_hash);
if (!isValid) throw new Error('Invalid credentials');
```

**Step 3: Generate Tokens** (In Application Code)
```javascript
const accessToken = jwt.sign({ userId: user.id }, SECRET, { expiresIn: '15m' });
const refreshToken = jwt.sign({ userId: user.id }, SECRET, { expiresIn: '7d' });
```

**Step 4: Store Refresh Token**
```sql
DO $$
DECLARE
    v_success BOOLEAN;
    v_error TEXT;
BEGIN
    CALL store_refresh_token(
        '550e8400-e29b-41d4-a716-446655440000',     -- user_id
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...', -- refresh token
        CURRENT_TIMESTAMP + INTERVAL '7 days',       -- expiration
        v_success,
        v_error
    );
END $$;
```

**Step 5: Create Session**
```sql
DO $$
DECLARE
    v_session_id UUID;
    v_error TEXT;
BEGIN
    CALL create_user_session(
        '550e8400-e29b-41d4-a716-446655440000',     -- user_id
        'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...', -- refresh token
        'iPhone 14 Pro',                             -- device
        '192.168.1.100',                             -- IP
        'Mozilla/5.0...',                            -- user agent
        CURRENT_TIMESTAMP + INTERVAL '7 days',       -- expiration
        v_session_id,
        v_error
    );

    RAISE NOTICE 'Session created: %', v_session_id;
END $$;
```

**Step 6: Log Login Attempt**
```sql
INSERT INTO user_login_history (user_id, login_type, ip_address, user_agent)
VALUES (
    '550e8400-e29b-41d4-a716-446655440000',
    'Success',
    '192.168.1.100',
    'Mozilla/5.0...'
);
```

---

### 🔄 Token Refresh Flow

**Step 1: Verify Refresh Token** (In Application Code)
```javascript
const decoded = jwt.verify(refreshToken, SECRET);
```

**Step 2: Get User by Refresh Token**
```sql
SELECT * FROM users
WHERE refresh_token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'
  AND refresh_token_expires_at > CURRENT_TIMESTAMP
  AND NOT is_deleted;
```

**Step 3: Generate New Tokens** (In Application Code)
```javascript
const newAccessToken = jwt.sign({ userId: user.id }, SECRET, { expiresIn: '15m' });
const newRefreshToken = jwt.sign({ userId: user.id }, SECRET, { expiresIn: '7d' });
```

**Step 4: Store New Refresh Token**
```sql
CALL store_refresh_token(
    user_id,
    newRefreshToken,
    CURRENT_TIMESTAMP + INTERVAL '7 days',
    NULL,
    NULL
);
```

**Step 5: Update Session**
```sql
UPDATE user_sessions
SET refresh_token = 'new_refresh_token_here',
    last_activity_at = CURRENT_TIMESTAMP
WHERE user_id = '550e8400-e29b-41d4-a716-446655440000'
  AND refresh_token = 'old_refresh_token_here';
```

---

### 🚪 Logout Flow

**Logout from Current Device**:
```sql
DO $$
DECLARE
    v_revoked INT;
    v_error TEXT;
BEGIN
    CALL revoke_user_sessions(
        '550e8400-e29b-41d4-a716-446655440000',  -- user_id
        'current_session_id'::UUID,               -- specific session
        v_revoked,
        v_error
    );

    RAISE NOTICE 'Sessions revoked: %', v_revoked;
END $$;
```

**Logout from All Devices**:
```sql
DO $$
DECLARE
    v_revoked INT;
    v_error TEXT;
BEGIN
    CALL revoke_user_sessions(
        '550e8400-e29b-41d4-a716-446655440000',  -- user_id
        NULL,                                     -- NULL = all sessions
        v_revoked,
        v_error
    );

    RAISE NOTICE 'All sessions revoked: %', v_revoked;
END $$;
```

---

### 🔒 Password Reset Flow

**Step 1: Generate Reset Token**
```sql
INSERT INTO password_reset_tokens (user_id, token, expires_at)
VALUES (
    '550e8400-e29b-41d4-a716-446655440000',
    encode(gen_random_bytes(32), 'hex'),  -- Secure random token
    CURRENT_TIMESTAMP + INTERVAL '1 hour'
)
RETURNING token;
-- Returns: 'a1b2c3d4e5f6...'
```

**Step 2: Send Reset Email** (In Application Code)
```javascript
const resetLink = `https://example.com/reset-password?token=${token}`;
await sendEmail(email, 'Password Reset', `Reset link: ${resetLink}`);
```

**Step 3: Verify Reset Token**
```sql
SELECT user_id FROM password_reset_tokens
WHERE token = 'token_from_url'
  AND NOT is_used
  AND expires_at > CURRENT_TIMESTAMP;
```

**Step 4: Update Password**
```sql
-- Start transaction
BEGIN;

-- Update password
UPDATE users
SET password_hash = '$2a$10$new_hashed_password_here',
    updated_at = CURRENT_TIMESTAMP
WHERE id = '550e8400-e29b-41d4-a716-446655440000';

-- Mark token as used
UPDATE password_reset_tokens
SET is_used = true,
    used_at = CURRENT_TIMESTAMP
WHERE token = 'token_from_url';

-- Revoke all sessions (force re-login)
CALL revoke_user_sessions(
    '550e8400-e29b-41d4-a716-446655440000',
    NULL,
    NULL,
    NULL
);

COMMIT;
```

---

### 🗑️ Account Deletion Flow

```sql
DO $$
DECLARE
    v_success BOOLEAN;
    v_error TEXT;
BEGIN
    CALL soft_delete_user(
        '550e8400-e29b-41d4-a716-446655440000',
        v_success,
        v_error
    );

    IF v_error IS NOT NULL THEN
        RAISE EXCEPTION '%', v_error;
    END IF;

    RAISE NOTICE 'User account deleted successfully';
END $$;
```

**What Happens**:
1. User marked as deleted (`is_deleted = true`)
2. Deletion timestamp recorded
3. All sessions revoked
4. User data preserved for audit/GDPR compliance

---

### 📊 Admin Operations

**Get Platform Statistics**:
```sql
SELECT * FROM get_user_stats();
```

**Find Active Users**:
```sql
SELECT u.id, u.username, u.email, s.last_activity_at
FROM users u
JOIN user_sessions s ON u.id = s.user_id
WHERE s.is_active = true
  AND s.expires_at > CURRENT_TIMESTAMP
ORDER BY s.last_activity_at DESC;
```

**View User Login History**:
```sql
SELECT * FROM user_login_history
WHERE user_id = '550e8400-e29b-41d4-a716-446655440000'
ORDER BY login_at DESC
LIMIT 20;
```

**Clean Old Data (Run Daily)**:
```sql
SELECT cleanup_old_data();
```

---

### 🔍 Security Checks

**Check for Multiple Failed Logins**:
```sql
SELECT user_id, COUNT(*) as failed_attempts
FROM user_login_history
WHERE login_type = 'Failed'
  AND login_at > CURRENT_TIMESTAMP - INTERVAL '1 hour'
GROUP BY user_id
HAVING COUNT(*) > 5;
```

**Find Suspicious Login Patterns**:
```sql
-- Different IPs in short time
SELECT user_id, COUNT(DISTINCT ip_address) as ip_count
FROM user_sessions
WHERE created_at > CURRENT_TIMESTAMP - INTERVAL '1 hour'
GROUP BY user_id
HAVING COUNT(DISTINCT ip_address) > 3;
```

---

## Best Practices

1. **Always Use Procedures**: Use stored procedures for complex operations to ensure consistency
2. **Hash Passwords**: Never store plain text passwords, always use BCrypt with salt rounds ≥ 10
3. **Validate Input**: Check email/username availability before creating user
4. **Use Transactions**: Wrap related operations in transactions for atomicity
5. **Handle Errors**: Always check OUT parameters for error messages
6. **Log Activities**: Track logins, password resets, and security events
7. **Clean Regularly**: Run `cleanup_old_data()` daily via cron job
8. **Secure Tokens**: Use cryptographically secure random tokens for password reset
9. **Set Expiration**: Always set expiration for sessions, tokens, and codes
10. **Soft Delete**: Use soft delete for user accounts to preserve audit trail

---

## Scheduled Maintenance

**Daily Tasks** (Run via cron):
```sql
-- Clean expired sessions and codes
SELECT cleanup_old_data();

-- Archive old login history (older than 90 days)
DELETE FROM user_login_history
WHERE login_at < CURRENT_TIMESTAMP - INTERVAL '90 days';
```

**Weekly Tasks**:
```sql
-- Vacuum tables
VACUUM ANALYZE users;
VACUUM ANALYZE user_sessions;
VACUUM ANALYZE verification_codes;
```

**Monthly Tasks**:
```sql
-- Reindex for performance
REINDEX TABLE users;
REINDEX TABLE user_sessions;
```

---

## Error Handling Examples

**Check for Errors in Application Code**:
```javascript
// Node.js example with pg library
const result = await client.query(`
    CALL create_user($1, $2, $3, $4, NULL, NULL)
`, [username, email, passwordHash, phoneNumber]);

const errorMessage = result.rows[0].p_error_message;
if (errorMessage) {
    throw new Error(errorMessage);
}

const userId = result.rows[0].p_user_id;
return userId;
```

---

This guide covers all database operations for the Auth service. For integration examples and API usage, refer to the main application documentation.
