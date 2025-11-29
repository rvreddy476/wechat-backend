-- ========================================
-- Auth Service Constraints & Triggers
-- ========================================

-- ========================================
-- Trigger: Auto-update updated_at timestamp
-- ========================================
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply to users table
CREATE TRIGGER update_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Apply to user_sessions table
CREATE TRIGGER update_user_sessions_updated_at
    BEFORE UPDATE ON user_sessions
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- ========================================
-- Trigger: Log login attempts
-- ========================================
CREATE OR REPLACE FUNCTION log_user_login()
RETURNS TRIGGER AS $$
BEGIN
    -- This trigger would be called from application logic
    -- Placeholder for future enhancements
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ========================================
-- Trigger: Clean expired sessions
-- ========================================
CREATE OR REPLACE FUNCTION clean_expired_sessions()
RETURNS TRIGGER AS $$
BEGIN
    DELETE FROM user_sessions
    WHERE expires_at < NOW() - INTERVAL '7 days';
    
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Run after insert on user_sessions
CREATE TRIGGER trigger_clean_expired_sessions
    AFTER INSERT ON user_sessions
    EXECUTE FUNCTION clean_expired_sessions();

-- ========================================
-- Trigger: Clean expired verification codes
-- ========================================
CREATE OR REPLACE FUNCTION clean_expired_codes()
RETURNS TRIGGER AS $$
BEGIN
    DELETE FROM verification_codes
    WHERE expires_at < NOW() - INTERVAL '24 hours';
    
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Run after insert on verification_codes
CREATE TRIGGER trigger_clean_expired_codes
    AFTER INSERT ON verification_codes
    EXECUTE FUNCTION clean_expired_codes();
