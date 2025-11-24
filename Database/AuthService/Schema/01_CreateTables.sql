-- =============================================
-- WeChat.com - AuthService Database Schema
-- Database: PostgreSQL 16+
-- Purpose: Authentication, Authorization, and User Management
-- =============================================

-- Create schema if not exists
CREATE SCHEMA IF NOT EXISTS auth;

-- Set search path
SET search_path TO auth, public;

-- =============================================
-- Table: Users
-- Purpose: Core user authentication data
-- =============================================
CREATE TABLE IF NOT EXISTS auth.Users (
    UserId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    EmailVerified BOOLEAN NOT NULL DEFAULT FALSE,
    PhoneNumber VARCHAR(20),
    PhoneNumberVerified BOOLEAN NOT NULL DEFAULT FALSE,
    PasswordHash VARCHAR(255) NOT NULL,
    SecurityStamp UUID NOT NULL DEFAULT gen_random_uuid(), -- Changes when password/security settings change
    TwoFactorEnabled BOOLEAN NOT NULL DEFAULT FALSE,
    TwoFactorSecret VARCHAR(255), -- For TOTP
    LockoutEnabled BOOLEAN NOT NULL DEFAULT TRUE,
    LockoutEnd TIMESTAMP WITH TIME ZONE,
    AccessFailedCount INTEGER NOT NULL DEFAULT 0,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    LastLoginAt TIMESTAMP WITH TIME ZONE,
    DeletedAt TIMESTAMP WITH TIME ZONE,

    -- Constraints
    CONSTRAINT chk_username_length CHECK (LENGTH(Username) >= 3),
    CONSTRAINT chk_email_format CHECK (Email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    CONSTRAINT chk_access_failed_count CHECK (AccessFailedCount >= 0)
);

-- =============================================
-- Table: Roles
-- Purpose: Define user roles (Admin, User, Moderator, etc.)
-- =============================================
CREATE TABLE IF NOT EXISTS auth.Roles (
    RoleId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    RoleName VARCHAR(50) NOT NULL UNIQUE,
    Description TEXT,
    IsSystemRole BOOLEAN NOT NULL DEFAULT FALSE, -- System roles cannot be deleted
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_rolename_length CHECK (LENGTH(RoleName) >= 2)
);

-- =============================================
-- Table: UserRoles
-- Purpose: Many-to-many relationship between Users and Roles
-- =============================================
CREATE TABLE IF NOT EXISTS auth.UserRoles (
    UserRoleId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    RoleId UUID NOT NULL,
    AssignedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    AssignedBy UUID, -- UserId of admin who assigned this role

    CONSTRAINT fk_userroles_user FOREIGN KEY (UserId) REFERENCES auth.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT fk_userroles_role FOREIGN KEY (RoleId) REFERENCES auth.Roles(RoleId) ON DELETE CASCADE,
    CONSTRAINT fk_userroles_assignedby FOREIGN KEY (AssignedBy) REFERENCES auth.Users(UserId) ON DELETE SET NULL,
    CONSTRAINT uq_user_role UNIQUE (UserId, RoleId)
);

-- =============================================
-- Table: RefreshTokens
-- Purpose: Store JWT refresh tokens
-- =============================================
CREATE TABLE IF NOT EXISTS auth.RefreshTokens (
    RefreshTokenId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    Token VARCHAR(500) NOT NULL UNIQUE,
    TokenHash VARCHAR(255) NOT NULL, -- SHA256 hash of token for security
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    RevokedAt TIMESTAMP WITH TIME ZONE,
    ReplacedByToken VARCHAR(500),
    IsRevoked BOOLEAN NOT NULL DEFAULT FALSE,
    IsUsed BOOLEAN NOT NULL DEFAULT FALSE,
    IpAddress VARCHAR(45), -- Supports IPv6
    UserAgent TEXT,
    DeviceInfo JSONB, -- Store device information as JSON

    CONSTRAINT fk_refreshtokens_user FOREIGN KEY (UserId) REFERENCES auth.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT chk_expires_future CHECK (ExpiresAt > CreatedAt)
);

-- =============================================
-- Table: EmailVerificationTokens
-- Purpose: Email verification tokens
-- =============================================
CREATE TABLE IF NOT EXISTS auth.EmailVerificationTokens (
    TokenId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    Email VARCHAR(255) NOT NULL,
    Token VARCHAR(255) NOT NULL UNIQUE,
    TokenHash VARCHAR(255) NOT NULL,
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    VerifiedAt TIMESTAMP WITH TIME ZONE,
    IsUsed BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT fk_emailtokens_user FOREIGN KEY (UserId) REFERENCES auth.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT chk_email_token_expires CHECK (ExpiresAt > CreatedAt)
);

-- =============================================
-- Table: PasswordResetTokens
-- Purpose: Password reset tokens
-- =============================================
CREATE TABLE IF NOT EXISTS auth.PasswordResetTokens (
    TokenId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    Email VARCHAR(255) NOT NULL,
    Token VARCHAR(255) NOT NULL UNIQUE,
    TokenHash VARCHAR(255) NOT NULL,
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UsedAt TIMESTAMP WITH TIME ZONE,
    IsUsed BOOLEAN NOT NULL DEFAULT FALSE,
    IpAddress VARCHAR(45),

    CONSTRAINT fk_passwordtokens_user FOREIGN KEY (UserId) REFERENCES auth.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT chk_password_token_expires CHECK (ExpiresAt > CreatedAt)
);

-- =============================================
-- Table: UserSessions
-- Purpose: Track active user sessions
-- =============================================
CREATE TABLE IF NOT EXISTS auth.UserSessions (
    SessionId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    RefreshTokenId UUID,
    SessionToken VARCHAR(500) NOT NULL UNIQUE,
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    DeviceInfo JSONB,
    Location JSONB, -- Store geo-location data
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    LastActivityAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    EndedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,

    CONSTRAINT fk_sessions_user FOREIGN KEY (UserId) REFERENCES auth.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT fk_sessions_refreshtoken FOREIGN KEY (RefreshTokenId) REFERENCES auth.RefreshTokens(RefreshTokenId) ON DELETE SET NULL,
    CONSTRAINT chk_session_expires CHECK (ExpiresAt > CreatedAt)
);

-- =============================================
-- Table: AuditLogs
-- Purpose: Security audit trail
-- =============================================
CREATE TABLE IF NOT EXISTS auth.AuditLogs (
    LogId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID,
    Action VARCHAR(100) NOT NULL, -- LOGIN, LOGOUT, PASSWORD_CHANGE, etc.
    EntityType VARCHAR(50), -- USER, ROLE, TOKEN, etc.
    EntityId UUID,
    OldValues JSONB,
    NewValues JSONB,
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    Success BOOLEAN NOT NULL,
    FailureReason TEXT,
    Timestamp TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    AdditionalData JSONB,

    CONSTRAINT fk_auditlogs_user FOREIGN KEY (UserId) REFERENCES auth.Users(UserId) ON DELETE SET NULL
);

-- =============================================
-- Table: LoginAttempts
-- Purpose: Track login attempts for security
-- =============================================
CREATE TABLE IF NOT EXISTS auth.LoginAttempts (
    AttemptId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID,
    Email VARCHAR(255),
    Username VARCHAR(50),
    IpAddress VARCHAR(45) NOT NULL,
    UserAgent TEXT,
    Success BOOLEAN NOT NULL,
    FailureReason VARCHAR(255),
    AttemptedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_loginattempts_user FOREIGN KEY (UserId) REFERENCES auth.Users(UserId) ON DELETE SET NULL
);

-- =============================================
-- Table: ExternalLoginProviders
-- Purpose: OAuth/External authentication providers (Google, Facebook, etc.)
-- =============================================
CREATE TABLE IF NOT EXISTS auth.ExternalLoginProviders (
    ProviderId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    ProviderName VARCHAR(50) NOT NULL, -- Google, Facebook, Apple, etc.
    ProviderKey VARCHAR(255) NOT NULL, -- External user ID
    ProviderDisplayName VARCHAR(100),
    AccessToken TEXT,
    RefreshToken TEXT,
    TokenExpiresAt TIMESTAMP WITH TIME ZONE,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_externalproviders_user FOREIGN KEY (UserId) REFERENCES auth.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT uq_provider_key UNIQUE (ProviderName, ProviderKey)
);

-- =============================================
-- INDEXES for Performance Optimization
-- =============================================

-- Users table indexes
CREATE INDEX IF NOT EXISTS idx_users_email ON auth.Users(Email) WHERE IsDeleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_users_username ON auth.Users(Username) WHERE IsDeleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_users_isactive ON auth.Users(IsActive) WHERE IsDeleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_users_createdat ON auth.Users(CreatedAt DESC);
CREATE INDEX IF NOT EXISTS idx_users_lastloginat ON auth.Users(LastLoginAt DESC);

-- RefreshTokens table indexes
CREATE INDEX IF NOT EXISTS idx_refreshtokens_userid ON auth.RefreshTokens(UserId);
CREATE INDEX IF NOT EXISTS idx_refreshtokens_token ON auth.RefreshTokens(Token) WHERE IsRevoked = FALSE;
CREATE INDEX IF NOT EXISTS idx_refreshtokens_expiresat ON auth.RefreshTokens(ExpiresAt) WHERE IsRevoked = FALSE;
CREATE INDEX IF NOT EXISTS idx_refreshtokens_tokenhash ON auth.RefreshTokens(TokenHash);

-- UserRoles table indexes
CREATE INDEX IF NOT EXISTS idx_userroles_userid ON auth.UserRoles(UserId);
CREATE INDEX IF NOT EXISTS idx_userroles_roleid ON auth.UserRoles(RoleId);

-- UserSessions table indexes
CREATE INDEX IF NOT EXISTS idx_sessions_userid ON auth.UserSessions(UserId);
CREATE INDEX IF NOT EXISTS idx_sessions_isactive ON auth.UserSessions(IsActive) WHERE IsActive = TRUE;
CREATE INDEX IF NOT EXISTS idx_sessions_expiresat ON auth.UserSessions(ExpiresAt);
CREATE INDEX IF NOT EXISTS idx_sessions_lastactivityat ON auth.UserSessions(LastActivityAt DESC);

-- AuditLogs table indexes
CREATE INDEX IF NOT EXISTS idx_auditlogs_userid ON auth.AuditLogs(UserId);
CREATE INDEX IF NOT EXISTS idx_auditlogs_action ON auth.AuditLogs(Action);
CREATE INDEX IF NOT EXISTS idx_auditlogs_timestamp ON auth.AuditLogs(Timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_auditlogs_entitytype_entityid ON auth.AuditLogs(EntityType, EntityId);

-- LoginAttempts table indexes
CREATE INDEX IF NOT EXISTS idx_loginattempts_userid ON auth.LoginAttempts(UserId);
CREATE INDEX IF NOT EXISTS idx_loginattempts_ipaddress ON auth.LoginAttempts(IpAddress);
CREATE INDEX IF NOT EXISTS idx_loginattempts_attemptedat ON auth.LoginAttempts(AttemptedAt DESC);
CREATE INDEX IF NOT EXISTS idx_loginattempts_email ON auth.LoginAttempts(Email);

-- EmailVerificationTokens table indexes
CREATE INDEX IF NOT EXISTS idx_emailtokens_userid ON auth.EmailVerificationTokens(UserId);
CREATE INDEX IF NOT EXISTS idx_emailtokens_token ON auth.EmailVerificationTokens(Token) WHERE IsUsed = FALSE;
CREATE INDEX IF NOT EXISTS idx_emailtokens_expiresat ON auth.EmailVerificationTokens(ExpiresAt);

-- PasswordResetTokens table indexes
CREATE INDEX IF NOT EXISTS idx_passwordtokens_userid ON auth.PasswordResetTokens(UserId);
CREATE INDEX IF NOT EXISTS idx_passwordtokens_token ON auth.PasswordResetTokens(Token) WHERE IsUsed = FALSE;
CREATE INDEX IF NOT EXISTS idx_passwordtokens_expiresat ON auth.PasswordResetTokens(ExpiresAt);

-- ExternalLoginProviders table indexes
CREATE INDEX IF NOT EXISTS idx_externalproviders_userid ON auth.ExternalLoginProviders(UserId);
CREATE INDEX IF NOT EXISTS idx_externalproviders_provider ON auth.ExternalLoginProviders(ProviderName, ProviderKey);

-- =============================================
-- COMMENTS for documentation
-- =============================================

COMMENT ON TABLE auth.Users IS 'Core user authentication and account data';
COMMENT ON TABLE auth.Roles IS 'User roles for role-based access control (RBAC)';
COMMENT ON TABLE auth.UserRoles IS 'Many-to-many relationship between users and roles';
COMMENT ON TABLE auth.RefreshTokens IS 'JWT refresh tokens for maintaining user sessions';
COMMENT ON TABLE auth.EmailVerificationTokens IS 'Tokens for email verification process';
COMMENT ON TABLE auth.PasswordResetTokens IS 'Tokens for password reset functionality';
COMMENT ON TABLE auth.UserSessions IS 'Active user sessions tracking';
COMMENT ON TABLE auth.AuditLogs IS 'Security audit trail for all authentication events';
COMMENT ON TABLE auth.LoginAttempts IS 'Login attempt tracking for security monitoring';
COMMENT ON TABLE auth.ExternalLoginProviders IS 'OAuth and external authentication provider mappings';
