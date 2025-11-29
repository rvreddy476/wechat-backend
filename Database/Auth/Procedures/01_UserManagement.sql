-- ========================================
-- Auth Service User Management Procedures
-- ========================================

-- ========================================
-- Procedure: Create new user
-- ========================================
CREATE OR REPLACE PROCEDURE create_user(
    p_username VARCHAR,
    p_email VARCHAR,
    p_password_hash VARCHAR,
    p_phone_number VARCHAR DEFAULT NULL,
    OUT p_user_id UUID,
    OUT p_error_message TEXT
)
LANGUAGE plpgsql AS $$
BEGIN
    p_error_message := NULL;
    
    -- Check if email exists
    IF email_exists(p_email) THEN
        p_error_message := 'Email already registered';
        RETURN;
    END IF;
    
    -- Check if username exists
    IF username_exists(p_username) THEN
        p_error_message := 'Username already taken';
        RETURN;
    END IF;
    
    -- Create user
    INSERT INTO users (username, email, password_hash, phone_number)
    VALUES (p_username, LOWER(p_email), p_password_hash, p_phone_number)
    RETURNING id INTO p_user_id;
    
    EXCEPTION WHEN OTHERS THEN
        p_error_message := SQLERRM;
        p_user_id := NULL;
END;
$$;

-- ========================================
-- Procedure: Update user profile
-- ========================================
CREATE OR REPLACE PROCEDURE update_user_profile(
    p_user_id UUID,
    p_username VARCHAR DEFAULT NULL,
    p_phone_number VARCHAR DEFAULT NULL,
    OUT p_success BOOLEAN,
    OUT p_error_message TEXT
)
LANGUAGE plpgsql AS $$
BEGIN
    p_success := FALSE;
    p_error_message := NULL;
    
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM users WHERE id = p_user_id) THEN
        p_error_message := 'User not found';
        RETURN;
    END IF;
    
    -- Update fields that are provided
    UPDATE users
    SET
        username = COALESCE(p_username, username),
        phone_number = COALESCE(p_phone_number, phone_number),
        updated_at = NOW()
    WHERE id = p_user_id;
    
    p_success := TRUE;
    
    EXCEPTION WHEN OTHERS THEN
        p_error_message := SQLERRM;
END;
$$;

-- ========================================
-- Procedure: Soft delete user
-- ========================================
CREATE OR REPLACE PROCEDURE soft_delete_user(
    p_user_id UUID,
    OUT p_success BOOLEAN
)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE users
    SET
        is_deleted = TRUE,
        deleted_at = NOW(),
        updated_at = NOW()
    WHERE id = p_user_id;
    
    p_success := FOUND;
END;
$$;

-- ========================================
-- Procedure: Verify email
-- ========================================
CREATE OR REPLACE PROCEDURE verify_email(
    p_user_id UUID,
    OUT p_success BOOLEAN
)
LANGUAGE plpgsql AS $$
BEGIN
    UPDATE users
    SET
        is_email_verified = TRUE,
        updated_at = NOW()
    WHERE id = p_user_id;
    
    p_success := FOUND;
END;
$$;
