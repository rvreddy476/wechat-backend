-- =============================================
-- WeChat.com - AuthService Database Triggers
-- Purpose: Automatic timestamp updates and data validation
-- =============================================

SET search_path TO auth, public;

-- =============================================
-- Function: Update timestamp on record modification
-- =============================================
CREATE OR REPLACE FUNCTION auth.update_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.UpdatedAt = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Function: Update SecurityStamp when password changes
-- =============================================
CREATE OR REPLACE FUNCTION auth.update_security_stamp()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.PasswordHash IS DISTINCT FROM OLD.PasswordHash THEN
        NEW.SecurityStamp = gen_random_uuid();
        NEW.UpdatedAt = NOW();
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Function: Revoke all tokens when password changes
-- =============================================
CREATE OR REPLACE FUNCTION auth.revoke_tokens_on_password_change()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.PasswordHash IS DISTINCT FROM OLD.PasswordHash THEN
        -- Revoke all refresh tokens
        UPDATE auth.RefreshTokens
        SET IsRevoked = TRUE, RevokedAt = NOW()
        WHERE UserId = NEW.UserId AND IsRevoked = FALSE;

        -- End all active sessions
        UPDATE auth.UserSessions
        SET IsActive = FALSE, EndedAt = NOW()
        WHERE UserId = NEW.UserId AND IsActive = TRUE;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Function: Auto-lock account after failed attempts
-- =============================================
CREATE OR REPLACE FUNCTION auth.check_lockout()
RETURNS TRIGGER AS $$
BEGIN
    -- If access failed count reaches 5, lock account for 15 minutes
    IF NEW.AccessFailedCount >= 5 AND OLD.AccessFailedCount < 5 THEN
        NEW.LockoutEnd = NOW() + INTERVAL '15 minutes';
    END IF;

    -- Reset failed count if successfully logged in
    IF NEW.LastLoginAt IS DISTINCT FROM OLD.LastLoginAt THEN
        NEW.AccessFailedCount = 0;
        NEW.LockoutEnd = NULL;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Function: Log user changes to audit log
-- =============================================
CREATE OR REPLACE FUNCTION auth.log_user_changes()
RETURNS TRIGGER AS $$
DECLARE
    action_type VARCHAR(100);
    old_vals JSONB;
    new_vals JSONB;
BEGIN
    IF TG_OP = 'INSERT' THEN
        action_type := 'USER_CREATED';
        new_vals := to_jsonb(NEW);
    ELSIF TG_OP = 'UPDATE' THEN
        action_type := 'USER_UPDATED';
        old_vals := to_jsonb(OLD);
        new_vals := to_jsonb(NEW);
    ELSIF TG_OP = 'DELETE' THEN
        action_type := 'USER_DELETED';
        old_vals := to_jsonb(OLD);
    END IF;

    INSERT INTO auth.AuditLogs (UserId, Action, EntityType, EntityId, OldValues, NewValues, Success)
    VALUES (
        COALESCE(NEW.UserId, OLD.UserId),
        action_type,
        'USER',
        COALESCE(NEW.UserId, OLD.UserId),
        old_vals,
        new_vals,
        TRUE
    );

    IF TG_OP = 'DELETE' THEN
        RETURN OLD;
    ELSE
        RETURN NEW;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Function: Cleanup expired tokens
-- =============================================
CREATE OR REPLACE FUNCTION auth.cleanup_expired_tokens()
RETURNS TRIGGER AS $$
BEGIN
    -- Delete expired refresh tokens older than 30 days
    DELETE FROM auth.RefreshTokens
    WHERE ExpiresAt < NOW() - INTERVAL '30 days';

    -- Delete used email verification tokens older than 7 days
    DELETE FROM auth.EmailVerificationTokens
    WHERE IsUsed = TRUE AND CreatedAt < NOW() - INTERVAL '7 days';

    -- Delete used password reset tokens older than 7 days
    DELETE FROM auth.PasswordResetTokens
    WHERE IsUsed = TRUE AND CreatedAt < NOW() - INTERVAL '7 days';

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- APPLY TRIGGERS
-- =============================================

-- UpdatedAt triggers
DROP TRIGGER IF EXISTS trg_users_update_timestamp ON auth.Users;
CREATE TRIGGER trg_users_update_timestamp
    BEFORE UPDATE ON auth.Users
    FOR EACH ROW
    EXECUTE FUNCTION auth.update_timestamp();

DROP TRIGGER IF EXISTS trg_roles_update_timestamp ON auth.Roles;
CREATE TRIGGER trg_roles_update_timestamp
    BEFORE UPDATE ON auth.Roles
    FOR EACH ROW
    EXECUTE FUNCTION auth.update_timestamp();

DROP TRIGGER IF EXISTS trg_externalproviders_update_timestamp ON auth.ExternalLoginProviders;
CREATE TRIGGER trg_externalproviders_update_timestamp
    BEFORE UPDATE ON auth.ExternalLoginProviders
    FOR EACH ROW
    EXECUTE FUNCTION auth.update_timestamp();

-- Security triggers
DROP TRIGGER IF EXISTS trg_users_security_stamp ON auth.Users;
CREATE TRIGGER trg_users_security_stamp
    BEFORE UPDATE ON auth.Users
    FOR EACH ROW
    EXECUTE FUNCTION auth.update_security_stamp();

DROP TRIGGER IF EXISTS trg_users_revoke_tokens ON auth.Users;
CREATE TRIGGER trg_users_revoke_tokens
    AFTER UPDATE ON auth.Users
    FOR EACH ROW
    EXECUTE FUNCTION auth.revoke_tokens_on_password_change();

DROP TRIGGER IF EXISTS trg_users_check_lockout ON auth.Users;
CREATE TRIGGER trg_users_check_lockout
    BEFORE UPDATE ON auth.Users
    FOR EACH ROW
    EXECUTE FUNCTION auth.check_lockout();

-- Audit triggers
DROP TRIGGER IF EXISTS trg_users_audit_log ON auth.Users;
CREATE TRIGGER trg_users_audit_log
    AFTER INSERT OR UPDATE OR DELETE ON auth.Users
    FOR EACH ROW
    EXECUTE FUNCTION auth.log_user_changes();

-- Cleanup trigger (runs on refresh token insert)
DROP TRIGGER IF EXISTS trg_cleanup_expired_tokens ON auth.RefreshTokens;
CREATE TRIGGER trg_cleanup_expired_tokens
    AFTER INSERT ON auth.RefreshTokens
    FOR EACH STATEMENT
    EXECUTE FUNCTION auth.cleanup_expired_tokens();

-- =============================================
-- COMMENTS
-- =============================================

COMMENT ON FUNCTION auth.update_timestamp() IS 'Automatically updates UpdatedAt timestamp on record modification';
COMMENT ON FUNCTION auth.update_security_stamp() IS 'Updates SecurityStamp when password changes';
COMMENT ON FUNCTION auth.revoke_tokens_on_password_change() IS 'Revokes all tokens and sessions when password changes';
COMMENT ON FUNCTION auth.check_lockout() IS 'Automatically locks account after failed login attempts';
COMMENT ON FUNCTION auth.log_user_changes() IS 'Logs all user changes to audit log';
COMMENT ON FUNCTION auth.cleanup_expired_tokens() IS 'Periodically cleans up expired tokens';
