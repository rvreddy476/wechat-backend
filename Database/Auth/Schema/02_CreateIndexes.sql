-- ========================================
-- Auth Service Indexes
-- Performance optimization
-- ========================================

-- Users table indexes
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email) WHERE NOT is_deleted;
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username) WHERE NOT is_deleted;
CREATE INDEX IF NOT EXISTS idx_users_phone_number ON users(phone_number) WHERE phone_number IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_users_refresh_token ON users(refresh_token) WHERE refresh_token IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_users_created_at ON users(created_at);
CREATE INDEX IF NOT EXISTS idx_users_is_deleted ON users(is_deleted);

-- GIN index for JSONB roles
CREATE INDEX IF NOT EXISTS idx_users_roles_gin ON users USING GIN (roles);

-- User sessions indexes
CREATE INDEX IF NOT EXISTS idx_user_sessions_user_id ON user_sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_user_sessions_refresh_token ON user_sessions(refresh_token);
CREATE INDEX IF NOT EXISTS idx_user_sessions_is_active ON user_sessions(is_active);
CREATE INDEX IF NOT EXISTS idx_user_sessions_expires_at ON user_sessions(expires_at);

-- Verification codes indexes
CREATE INDEX IF NOT EXISTS idx_verification_codes_user_id ON verification_codes(user_id);
CREATE INDEX IF NOT EXISTS idx_verification_codes_code ON verification_codes(code);
CREATE INDEX IF NOT EXISTS idx_verification_codes_is_used ON verification_codes(is_used);
CREATE INDEX IF NOT EXISTS idx_verification_codes_expires_at ON verification_codes(expires_at);

-- Login history indexes
CREATE INDEX IF NOT EXISTS idx_user_login_history_user_id ON user_login_history(user_id);
CREATE INDEX IF NOT EXISTS idx_user_login_history_login_at ON user_login_history(login_at DESC);
CREATE INDEX IF NOT EXISTS idx_user_login_history_ip_address ON user_login_history(ip_address);

-- Password reset tokens indexes
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_user_id ON password_reset_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_token ON password_reset_tokens(token);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_is_used ON password_reset_tokens(is_used);

-- Composite indexes for common queries
CREATE INDEX IF NOT EXISTS idx_users_email_password ON users(email, password_hash) WHERE NOT is_deleted;
CREATE INDEX IF NOT EXISTS idx_verification_codes_lookup ON verification_codes(user_id, verification_type, is_used);
