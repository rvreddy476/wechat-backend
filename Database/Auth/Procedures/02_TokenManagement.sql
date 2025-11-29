-- ========================================
-- Auth Service Token Management Procedures
-- ========================================

-- ========================================
-- Procedure: Store refresh token
-- ========================================
CREATE OR REPLACE PROCEDURE store_refresh_token(
    p_user_id UUID,
    p_refresh_token TEXT,
    p_expiry_time TIMESTAMP,
    OUT p_success BOOLEAN
)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE users
    SET
        refresh_token = p_refresh_token,
        refresh_token_expiry_time = p_expiry_time,
        updated_at = NOW()
    WHERE id = p_user_id;
    
    p_success := FOUND;
END;
$$;

-- ========================================
-- Procedure: Create user session
-- ========================================
CREATE OR REPLACE PROCEDURE create_user_session(
    p_user_id UUID,
    p_access_token TEXT,
    p_refresh_token TEXT,
    p_ip_address INET,
    p_user_agent TEXT,
    p_expires_at TIMESTAMP,
    OUT p_session_id UUID
)
LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO user_sessions (
        user_id,
        access_token,
        refresh_token,
        ip_address,
        user_agent,
        expires_at
    )
    VALUES (
        p_user_id,
        p_access_token,
        p_refresh_token,
        p_ip_address,
        p_user_agent,
        p_expires_at
    )
    RETURNING id INTO p_session_id;
END;
$$;

-- ========================================
-- Procedure: Revoke all user sessions
-- ========================================
CREATE OR REPLACE PROCEDURE revoke_user_sessions(
    p_user_id UUID,
    OUT p_revoked_count INTEGER
)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE user_sessions
    SET is_active = FALSE
    WHERE user_id = p_user_id
    AND is_active = TRUE;
    
    GET DIAGNOSTICS p_revoked_count = ROW_COUNT;
END;
$$;
