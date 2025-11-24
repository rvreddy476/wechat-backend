-- =============================================
-- WeChat.com - AuthService Token Management Procedures
-- Purpose: Refresh Token, Email Verification, Password Reset Operations
-- =============================================

SET search_path TO auth, public;

-- =============================================
-- Procedure: Create Refresh Token
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_CreateRefreshToken(
    p_UserId UUID,
    p_Token VARCHAR(500),
    p_TokenHash VARCHAR(255),
    p_ExpiresAt TIMESTAMP WITH TIME ZONE,
    p_IpAddress VARCHAR(45) DEFAULT NULL,
    p_UserAgent TEXT DEFAULT NULL,
    p_DeviceInfo JSONB DEFAULT NULL
)
RETURNS TABLE (
    RefreshTokenId UUID,
    Success BOOLEAN,
    Message TEXT
) AS $$
DECLARE
    v_TokenId UUID;
BEGIN
    -- Check if user exists and is active
    IF NOT EXISTS (SELECT 1 FROM auth.Users WHERE UserId = p_UserId AND IsActive = TRUE AND IsDeleted = FALSE) THEN
        RETURN QUERY SELECT NULL::UUID, FALSE, 'User not found or inactive'::TEXT;
        RETURN;
    END IF;

    -- Insert refresh token
    INSERT INTO auth.RefreshTokens (UserId, Token, TokenHash, ExpiresAt, IpAddress, UserAgent, DeviceInfo)
    VALUES (p_UserId, p_Token, p_TokenHash, p_ExpiresAt, p_IpAddress, p_UserAgent, p_DeviceInfo)
    RETURNING RefreshTokens.RefreshTokenId INTO v_TokenId;

    -- Create session
    INSERT INTO auth.UserSessions (UserId, RefreshTokenId, SessionToken, IpAddress, UserAgent, DeviceInfo, ExpiresAt)
    VALUES (p_UserId, v_TokenId, p_Token, p_IpAddress, p_UserAgent, p_DeviceInfo, p_ExpiresAt);

    -- Log token creation
    INSERT INTO auth.AuditLogs (UserId, Action, EntityType, EntityId, IpAddress, Success)
    VALUES (p_UserId, 'REFRESH_TOKEN_CREATED', 'TOKEN', v_TokenId, p_IpAddress, TRUE);

    RETURN QUERY SELECT v_TokenId, TRUE, 'Refresh token created successfully'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Procedure: Validate Refresh Token
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_ValidateRefreshToken(
    p_Token VARCHAR(500)
)
RETURNS TABLE (
    RefreshTokenId UUID,
    UserId UUID,
    IsValid BOOLEAN,
    Message TEXT,
    SecurityStamp UUID
) AS $$
DECLARE
    v_Token RECORD;
    v_SecurityStamp UUID;
BEGIN
    -- Get token details
    SELECT rt.* INTO v_Token
    FROM auth.RefreshTokens rt
    WHERE rt.Token = p_Token;

    -- Token not found
    IF v_Token.RefreshTokenId IS NULL THEN
        RETURN QUERY SELECT NULL::UUID, NULL::UUID, FALSE, 'Invalid token'::TEXT, NULL::UUID;
        RETURN;
    END IF;

    -- Token revoked
    IF v_Token.IsRevoked = TRUE THEN
        RETURN QUERY SELECT v_Token.RefreshTokenId, v_Token.UserId, FALSE, 'Token has been revoked'::TEXT, NULL::UUID;
        RETURN;
    END IF;

    -- Token expired
    IF v_Token.ExpiresAt < NOW() THEN
        -- Mark as revoked
        UPDATE auth.RefreshTokens
        SET IsRevoked = TRUE, RevokedAt = NOW()
        WHERE RefreshTokenId = v_Token.RefreshTokenId;

        RETURN QUERY SELECT v_Token.RefreshTokenId, v_Token.UserId, FALSE, 'Token expired'::TEXT, NULL::UUID;
        RETURN;
    END IF;

    -- Token already used
    IF v_Token.IsUsed = TRUE THEN
        -- Possible token replay attack - revoke all tokens for this user
        UPDATE auth.RefreshTokens
        SET IsRevoked = TRUE, RevokedAt = NOW()
        WHERE UserId = v_Token.UserId AND IsRevoked = FALSE;

        -- Log security incident
        INSERT INTO auth.AuditLogs (UserId, Action, EntityType, EntityId, Success, FailureReason)
        VALUES (v_Token.UserId, 'TOKEN_REPLAY_DETECTED', 'TOKEN', v_Token.RefreshTokenId, FALSE,
                'Possible token replay attack - all tokens revoked');

        RETURN QUERY SELECT v_Token.RefreshTokenId, v_Token.UserId, FALSE,
                     'Token already used - security incident logged'::TEXT, NULL::UUID;
        RETURN;
    END IF;

    -- Get user's current security stamp
    SELECT u.SecurityStamp INTO v_SecurityStamp
    FROM auth.Users u
    WHERE u.UserId = v_Token.UserId AND u.IsActive = TRUE AND u.IsDeleted = FALSE;

    -- User not found or inactive
    IF v_SecurityStamp IS NULL THEN
        RETURN QUERY SELECT v_Token.RefreshTokenId, v_Token.UserId, FALSE, 'User not found or inactive'::TEXT, NULL::UUID;
        RETURN;
    END IF;

    -- Mark token as used
    UPDATE auth.RefreshTokens
    SET IsUsed = TRUE
    WHERE RefreshTokenId = v_Token.RefreshTokenId;

    -- Update session activity
    UPDATE auth.UserSessions
    SET LastActivityAt = NOW()
    WHERE RefreshTokenId = v_Token.RefreshTokenId AND IsActive = TRUE;

    RETURN QUERY SELECT v_Token.RefreshTokenId, v_Token.UserId, TRUE,
                 'Token is valid'::TEXT, v_SecurityStamp;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Procedure: Revoke Refresh Token
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_RevokeRefreshToken(
    p_Token VARCHAR(500),
    p_ReasonUserId UUID DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT
) AS $$
DECLARE
    v_TokenId UUID;
    v_UserId UUID;
BEGIN
    -- Get token details
    SELECT RefreshTokenId, UserId INTO v_TokenId, v_UserId
    FROM auth.RefreshTokens
    WHERE Token = p_Token;

    IF v_TokenId IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Token not found'::TEXT;
        RETURN;
    END IF;

    -- Revoke token
    UPDATE auth.RefreshTokens
    SET IsRevoked = TRUE, RevokedAt = NOW()
    WHERE RefreshTokenId = v_TokenId;

    -- End associated session
    UPDATE auth.UserSessions
    SET IsActive = FALSE, EndedAt = NOW()
    WHERE RefreshTokenId = v_TokenId AND IsActive = TRUE;

    -- Log revocation
    INSERT INTO auth.AuditLogs (UserId, Action, EntityType, EntityId, Success, AdditionalData)
    VALUES (COALESCE(p_ReasonUserId, v_UserId), 'TOKEN_REVOKED', 'TOKEN', v_TokenId, TRUE,
            jsonb_build_object('tokenId', v_TokenId, 'revokedBy', p_ReasonUserId));

    RETURN QUERY SELECT TRUE, 'Token revoked successfully'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Procedure: Revoke All User Tokens
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_RevokeAllUserTokens(
    p_UserId UUID
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT,
    TokensRevoked INTEGER
) AS $$
DECLARE
    v_Count INTEGER;
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM auth.Users WHERE UserId = p_UserId AND IsDeleted = FALSE) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT, 0;
        RETURN;
    END IF;

    -- Count tokens to revoke
    SELECT COUNT(*) INTO v_Count
    FROM auth.RefreshTokens
    WHERE UserId = p_UserId AND IsRevoked = FALSE;

    -- Revoke all tokens
    UPDATE auth.RefreshTokens
    SET IsRevoked = TRUE, RevokedAt = NOW()
    WHERE UserId = p_UserId AND IsRevoked = FALSE;

    -- End all sessions
    UPDATE auth.UserSessions
    SET IsActive = FALSE, EndedAt = NOW()
    WHERE UserId = p_UserId AND IsActive = TRUE;

    -- Log action
    INSERT INTO auth.AuditLogs (UserId, Action, EntityType, Success, AdditionalData)
    VALUES (p_UserId, 'ALL_TOKENS_REVOKED', 'USER', TRUE,
            jsonb_build_object('tokensRevoked', v_Count));

    RETURN QUERY SELECT TRUE, 'All tokens revoked'::TEXT, v_Count;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Procedure: Create Email Verification Token
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_CreateEmailVerificationToken(
    p_UserId UUID,
    p_Email VARCHAR(255),
    p_Token VARCHAR(255),
    p_TokenHash VARCHAR(255),
    p_ExpiresAt TIMESTAMP WITH TIME ZONE
)
RETURNS TABLE (
    TokenId UUID,
    Success BOOLEAN,
    Message TEXT
) AS $$
DECLARE
    v_TokenId UUID;
BEGIN
    -- Invalidate any existing tokens for this email
    UPDATE auth.EmailVerificationTokens
    SET IsUsed = TRUE
    WHERE UserId = p_UserId AND Email = p_Email AND IsUsed = FALSE;

    -- Create new token
    INSERT INTO auth.EmailVerificationTokens (UserId, Email, Token, TokenHash, ExpiresAt)
    VALUES (p_UserId, p_Email, p_Token, p_TokenHash, p_ExpiresAt)
    RETURNING EmailVerificationTokens.TokenId INTO v_TokenId;

    RETURN QUERY SELECT v_TokenId, TRUE, 'Email verification token created'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Procedure: Verify Email
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_VerifyEmail(
    p_Token VARCHAR(255)
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT
) AS $$
DECLARE
    v_Token RECORD;
BEGIN
    -- Get token details
    SELECT * INTO v_Token
    FROM auth.EmailVerificationTokens
    WHERE Token = p_Token AND IsUsed = FALSE;

    IF v_Token.TokenId IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Invalid or expired token'::TEXT;
        RETURN;
    END IF;

    IF v_Token.ExpiresAt < NOW() THEN
        RETURN QUERY SELECT FALSE, 'Token has expired'::TEXT;
        RETURN;
    END IF;

    -- Mark email as verified
    UPDATE auth.Users
    SET EmailVerified = TRUE, UpdatedAt = NOW()
    WHERE UserId = v_Token.UserId;

    -- Mark token as used
    UPDATE auth.EmailVerificationTokens
    SET IsUsed = TRUE, VerifiedAt = NOW()
    WHERE TokenId = v_Token.TokenId;

    -- Log verification
    INSERT INTO auth.AuditLogs (UserId, Action, EntityType, EntityId, Success)
    VALUES (v_Token.UserId, 'EMAIL_VERIFIED', 'USER', v_Token.UserId, TRUE);

    RETURN QUERY SELECT TRUE, 'Email verified successfully'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Procedure: Create Password Reset Token
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_CreatePasswordResetToken(
    p_Email VARCHAR(255),
    p_Token VARCHAR(255),
    p_TokenHash VARCHAR(255),
    p_ExpiresAt TIMESTAMP WITH TIME ZONE,
    p_IpAddress VARCHAR(45) DEFAULT NULL
)
RETURNS TABLE (
    TokenId UUID,
    Success BOOLEAN,
    Message TEXT
) AS $$
DECLARE
    v_UserId UUID;
    v_TokenId UUID;
BEGIN
    -- Get user ID
    SELECT UserId INTO v_UserId
    FROM auth.Users
    WHERE Email = p_Email AND IsDeleted = FALSE;

    IF v_UserId IS NULL THEN
        -- Don't reveal if email exists or not (security)
        RETURN QUERY SELECT NULL::UUID, TRUE, 'If email exists, reset link has been sent'::TEXT;
        RETURN;
    END IF;

    -- Invalidate existing tokens
    UPDATE auth.PasswordResetTokens
    SET IsUsed = TRUE
    WHERE UserId = v_UserId AND IsUsed = FALSE;

    -- Create new token
    INSERT INTO auth.PasswordResetTokens (UserId, Email, Token, TokenHash, ExpiresAt, IpAddress)
    VALUES (v_UserId, p_Email, p_Token, p_TokenHash, p_ExpiresAt, p_IpAddress)
    RETURNING PasswordResetTokens.TokenId INTO v_TokenId;

    -- Log request
    INSERT INTO auth.AuditLogs (UserId, Action, EntityType, EntityId, IpAddress, Success)
    VALUES (v_UserId, 'PASSWORD_RESET_REQUESTED', 'TOKEN', v_TokenId, p_IpAddress, TRUE);

    RETURN QUERY SELECT v_TokenId, TRUE, 'Password reset token created'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Procedure: Reset Password with Token
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_ResetPassword(
    p_Token VARCHAR(255),
    p_NewPasswordHash VARCHAR(255),
    p_IpAddress VARCHAR(45) DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT
) AS $$
DECLARE
    v_Token RECORD;
BEGIN
    -- Get token details
    SELECT * INTO v_Token
    FROM auth.PasswordResetTokens
    WHERE Token = p_Token AND IsUsed = FALSE;

    IF v_Token.TokenId IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Invalid or expired token'::TEXT;
        RETURN;
    END IF;

    IF v_Token.ExpiresAt < NOW() THEN
        RETURN QUERY SELECT FALSE, 'Token has expired'::TEXT;
        RETURN;
    END IF;

    -- Update password
    UPDATE auth.Users
    SET PasswordHash = p_NewPasswordHash, UpdatedAt = NOW()
    WHERE UserId = v_Token.UserId;

    -- Mark token as used
    UPDATE auth.PasswordResetTokens
    SET IsUsed = TRUE, UsedAt = NOW()
    WHERE TokenId = v_Token.TokenId;

    -- Log password reset
    INSERT INTO auth.AuditLogs (UserId, Action, EntityType, EntityId, IpAddress, Success)
    VALUES (v_Token.UserId, 'PASSWORD_RESET_COMPLETED', 'USER', v_Token.UserId, p_IpAddress, TRUE);

    RETURN QUERY SELECT TRUE, 'Password reset successfully'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Procedure: Cleanup Expired Tokens (Manual)
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_CleanupExpiredTokens()
RETURNS TABLE (
    RefreshTokensDeleted INTEGER,
    EmailTokensDeleted INTEGER,
    PasswordTokensDeleted INTEGER,
    OldSessionsDeleted INTEGER
) AS $$
DECLARE
    v_RefreshCount INTEGER;
    v_EmailCount INTEGER;
    v_PasswordCount INTEGER;
    v_SessionCount INTEGER;
BEGIN
    -- Delete expired refresh tokens older than 30 days
    WITH deleted AS (
        DELETE FROM auth.RefreshTokens
        WHERE ExpiresAt < NOW() - INTERVAL '30 days'
        RETURNING 1
    )
    SELECT COUNT(*) INTO v_RefreshCount FROM deleted;

    -- Delete used email tokens older than 7 days
    WITH deleted AS (
        DELETE FROM auth.EmailVerificationTokens
        WHERE IsUsed = TRUE AND CreatedAt < NOW() - INTERVAL '7 days'
        RETURNING 1
    )
    SELECT COUNT(*) INTO v_EmailCount FROM deleted;

    -- Delete used password tokens older than 7 days
    WITH deleted AS (
        DELETE FROM auth.PasswordResetTokens
        WHERE IsUsed = TRUE AND CreatedAt < NOW() - INTERVAL '7 days'
        RETURNING 1
    )
    SELECT COUNT(*) INTO v_PasswordCount FROM deleted;

    -- Delete inactive sessions older than 30 days
    WITH deleted AS (
        DELETE FROM auth.UserSessions
        WHERE IsActive = FALSE AND EndedAt < NOW() - INTERVAL '30 days'
        RETURNING 1
    )
    SELECT COUNT(*) INTO v_SessionCount FROM deleted;

    RETURN QUERY SELECT v_RefreshCount, v_EmailCount, v_PasswordCount, v_SessionCount;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- COMMENTS
-- =============================================

COMMENT ON FUNCTION auth.sp_CreateRefreshToken IS 'Creates a new refresh token for user session';
COMMENT ON FUNCTION auth.sp_ValidateRefreshToken IS 'Validates refresh token and detects replay attacks';
COMMENT ON FUNCTION auth.sp_RevokeRefreshToken IS 'Revokes a specific refresh token';
COMMENT ON FUNCTION auth.sp_RevokeAllUserTokens IS 'Revokes all tokens for a user (logout all devices)';
COMMENT ON FUNCTION auth.sp_CreateEmailVerificationToken IS 'Creates email verification token';
COMMENT ON FUNCTION auth.sp_VerifyEmail IS 'Verifies user email with token';
COMMENT ON FUNCTION auth.sp_CreatePasswordResetToken IS 'Creates password reset token';
COMMENT ON FUNCTION auth.sp_ResetPassword IS 'Resets password using reset token';
COMMENT ON FUNCTION auth.sp_CleanupExpiredTokens IS 'Manually cleanup expired tokens and old sessions';
