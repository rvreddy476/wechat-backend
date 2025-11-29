-- ========================================
-- Auth Service Database Schema
-- Database: wechat_auth
-- ========================================

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ========================================
-- Users Table
-- ========================================
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    phone_number VARCHAR(20),
    
    -- Verification status
    is_email_verified BOOLEAN DEFAULT FALSE,
    is_phone_verified BOOLEAN DEFAULT FALSE,
    
    -- Roles (stored as JSONB for flexibility)
    roles JSONB DEFAULT '["User"]'::jsonb,
    
    -- Token management
    refresh_token TEXT,
    refresh_token_expiry_time TIMESTAMP,
    
    -- Timestamps
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    -- Soft delete
    is_deleted BOOLEAN DEFAULT FALSE,
    deleted_at TIMESTAMP,
    
    -- Constraints
    CONSTRAINT email_format CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    CONSTRAINT username_format CHECK (username ~* '^[A-Za-z0-9_]+$'),
    CONSTRAINT username_length CHECK (LENGTH(username) >= 3 AND LENGTH(username) <= 50)
);

-- ========================================
-- User Sessions Table
-- ========================================
CREATE TABLE IF NOT EXISTS user_sessions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    access_token TEXT NOT NULL,
    refresh_token TEXT NOT NULL,
    ip_address INET,
    user_agent TEXT,
    device_info JSONB,
    
    -- Session tracking
    is_active BOOLEAN DEFAULT TRUE,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    last_activity_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ========================================
-- Verification Codes Table
-- ========================================
CREATE TABLE IF NOT EXISTS verification_codes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    code VARCHAR(10) NOT NULL,
    verification_type VARCHAR(20) NOT NULL, -- 'email', 'phone', 'password_reset'
    
    -- Status
    is_used BOOLEAN DEFAULT FALSE,
    used_at TIMESTAMP,
    
    -- Expiry
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT valid_verification_type CHECK (verification_type IN ('email', 'phone', 'password_reset'))
);

-- ========================================
-- User Login History Table
-- ========================================
CREATE TABLE IF NOT EXISTS user_login_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    login_at TIMESTAMP NOT NULL DEFAULT NOW(),
    ip_address INET,
    user_agent TEXT,
    device_info JSONB,
    login_status VARCHAR(20) NOT NULL, -- 'success', 'failed', 'blocked'
    failure_reason TEXT,
    
    CONSTRAINT valid_login_status CHECK (login_status IN ('success', 'failed', 'blocked'))
);

-- ========================================
-- Password Reset Tokens Table
-- ========================================
CREATE TABLE IF NOT EXISTS password_reset_tokens (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token TEXT NOT NULL UNIQUE,
    is_used BOOLEAN DEFAULT FALSE,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    used_at TIMESTAMP
);

-- Comments
COMMENT ON TABLE users IS 'Main users table storing authentication credentials and profile information';
COMMENT ON TABLE user_sessions IS 'Active user sessions with JWT tokens';
COMMENT ON TABLE verification_codes IS 'Email/phone verification codes and password reset codes';
COMMENT ON TABLE user_login_history IS 'Audit log of user login attempts';
COMMENT ON TABLE password_reset_tokens IS 'Password reset token management';
