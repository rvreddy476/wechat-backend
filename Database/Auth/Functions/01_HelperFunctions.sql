-- ========================================
-- Auth Service Helper Functions
-- ========================================

-- ========================================
-- Function: Check if email exists
-- ========================================
CREATE OR REPLACE FUNCTION email_exists(p_email VARCHAR)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1 FROM users 
        WHERE email = LOWER(p_email) 
        AND NOT is_deleted
    );
END;
$$ LANGUAGE plpgsql;

-- ========================================
-- Function: Check if username exists
-- ========================================
CREATE OR REPLACE FUNCTION username_exists(p_username VARCHAR)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1 FROM users 
        WHERE username = p_username 
        AND NOT is_deleted
    );
END;
$$ LANGUAGE plpgsql;

-- ========================================
-- Function: Get user by email or username
-- ========================================
CREATE OR REPLACE FUNCTION get_user_by_login(p_login VARCHAR)
RETURNS TABLE (
    id UUID,
    username VARCHAR,
    email VARCHAR,
    password_hash VARCHAR,
    roles JSONB,
    is_deleted BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT u.id, u.username, u.email, u.password_hash, u.roles, u.is_deleted
    FROM users u
    WHERE (u.email = LOWER(p_login) OR u.username = p_login)
    AND NOT u.is_deleted
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

-- ========================================
-- Function: Generate verification code
-- ========================================
CREATE OR REPLACE FUNCTION generate_verification_code()
RETURNS VARCHAR AS $$
DECLARE
    code VARCHAR(6);
BEGIN
    code := LPAD(FLOOR(RANDOM() * 1000000)::TEXT, 6, '0');
    RETURN code;
END;
$$ LANGUAGE plpgsql;

-- ========================================
-- Function: Clean old data (maintenance)
-- ========================================
CREATE OR REPLACE FUNCTION cleanup_old_data()
RETURNS VOID AS $$
BEGIN
    -- Delete expired sessions older than 7 days
    DELETE FROM user_sessions
    WHERE expires_at < NOW() - INTERVAL '7 days';
    
    -- Delete used verification codes older than 24 hours
    DELETE FROM verification_codes
    WHERE is_used = TRUE AND created_at < NOW() - INTERVAL '24 hours';
    
    -- Delete expired verification codes
    DELETE FROM verification_codes
    WHERE expires_at < NOW();
    
    -- Delete old login history (keep last 90 days)
    DELETE FROM user_login_history
    WHERE login_at < NOW() - INTERVAL '90 days';
    
    RAISE NOTICE 'Cleanup completed successfully';
END;
$$ LANGUAGE plpgsql;

-- ========================================
-- Function: Get user statistics
-- ========================================
CREATE OR REPLACE FUNCTION get_user_stats()
RETURNS TABLE (
    total_users BIGINT,
    verified_users BIGINT,
    active_today BIGINT,
    new_today BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*)::BIGINT AS total_users,
        COUNT(*) FILTER (WHERE is_email_verified)::BIGINT AS verified_users,
        COUNT(DISTINCT us.user_id)::BIGINT AS active_today,
        COUNT(*) FILTER (WHERE u.created_at >= CURRENT_DATE)::BIGINT AS new_today
    FROM users u
    LEFT JOIN user_sessions us ON u.id = us.user_id 
        AND us.last_activity_at >= CURRENT_DATE
    WHERE NOT u.is_deleted;
END;
$$ LANGUAGE plpgsql;
