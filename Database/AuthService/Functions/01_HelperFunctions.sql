-- =============================================
-- WeChat.com - AuthService Helper Functions
-- Purpose: Query and utility functions
-- =============================================

SET search_path TO auth, public;

-- =============================================
-- Function: Check if user is in role
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_IsUserInRole(
    p_UserId UUID,
    p_RoleName VARCHAR(50)
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM auth.UserRoles ur
        JOIN auth.Roles r ON ur.RoleId = r.RoleId
        WHERE ur.UserId = p_UserId
          AND r.RoleName = p_RoleName
    );
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Get user roles as array
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_GetUserRoles(
    p_UserId UUID
)
RETURNS TEXT[] AS $$
BEGIN
    RETURN ARRAY(
        SELECT r.RoleName
        FROM auth.UserRoles ur
        JOIN auth.Roles r ON ur.RoleId = r.RoleId
        WHERE ur.UserId = p_UserId
        ORDER BY r.RoleName
    );
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Check if account is locked
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_IsAccountLocked(
    p_UserId UUID
)
RETURNS BOOLEAN AS $$
DECLARE
    v_LockoutEnd TIMESTAMP WITH TIME ZONE;
BEGIN
    SELECT LockoutEnd INTO v_LockoutEnd
    FROM auth.Users
    WHERE UserId = p_UserId;

    RETURN v_LockoutEnd IS NOT NULL AND v_LockoutEnd > NOW();
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Get active session count for user
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_GetActiveSessionCount(
    p_UserId UUID
)
RETURNS INTEGER AS $$
BEGIN
    RETURN (
        SELECT COUNT(*)
        FROM auth.UserSessions
        WHERE UserId = p_UserId
          AND IsActive = TRUE
          AND ExpiresAt > NOW()
    );
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Get user statistics
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_GetUserStats(
    p_UserId UUID
)
RETURNS TABLE (
    TotalLogins BIGINT,
    FailedLoginAttempts BIGINT,
    LastLoginAt TIMESTAMP WITH TIME ZONE,
    ActiveSessions INTEGER,
    AccountAge INTERVAL,
    IsEmailVerified BOOLEAN,
    IsTwoFactorEnabled BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        (SELECT COUNT(*) FROM auth.LoginAttempts WHERE UserId = p_UserId AND Success = TRUE),
        (SELECT COUNT(*) FROM auth.LoginAttempts WHERE UserId = p_UserId AND Success = FALSE),
        u.LastLoginAt,
        (SELECT COUNT(*)::INTEGER FROM auth.UserSessions WHERE UserId = p_UserId AND IsActive = TRUE),
        AGE(NOW(), u.CreatedAt),
        u.EmailVerified,
        u.TwoFactorEnabled
    FROM auth.Users u
    WHERE u.UserId = p_UserId;
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Search users (for admin)
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_SearchUsers(
    p_SearchTerm VARCHAR(255),
    p_IsActive BOOLEAN DEFAULT NULL,
    p_Limit INTEGER DEFAULT 50,
    p_Offset INTEGER DEFAULT 0
)
RETURNS TABLE (
    UserId UUID,
    Username VARCHAR(50),
    Email VARCHAR(255),
    EmailVerified BOOLEAN,
    IsActive BOOLEAN,
    CreatedAt TIMESTAMP WITH TIME ZONE,
    LastLoginAt TIMESTAMP WITH TIME ZONE,
    Roles TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        u.UserId,
        u.Username,
        u.Email,
        u.EmailVerified,
        u.IsActive,
        u.CreatedAt,
        u.LastLoginAt,
        STRING_AGG(r.RoleName, ',') AS Roles
    FROM auth.Users u
    LEFT JOIN auth.UserRoles ur ON u.UserId = ur.UserId
    LEFT JOIN auth.Roles r ON ur.RoleId = r.RoleId
    WHERE u.IsDeleted = FALSE
      AND (p_IsActive IS NULL OR u.IsActive = p_IsActive)
      AND (
          u.Username ILIKE '%' || p_SearchTerm || '%' OR
          u.Email ILIKE '%' || p_SearchTerm || '%'
      )
    GROUP BY u.UserId, u.Username, u.Email, u.EmailVerified, u.IsActive, u.CreatedAt, u.LastLoginAt
    ORDER BY u.CreatedAt DESC
    LIMIT p_Limit
    OFFSET p_Offset;
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Get login history for user
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_GetLoginHistory(
    p_UserId UUID,
    p_Limit INTEGER DEFAULT 20
)
RETURNS TABLE (
    AttemptedAt TIMESTAMP WITH TIME ZONE,
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    Success BOOLEAN,
    FailureReason VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        la.AttemptedAt,
        la.IpAddress,
        la.UserAgent,
        la.Success,
        la.FailureReason
    FROM auth.LoginAttempts la
    WHERE la.UserId = p_UserId
    ORDER BY la.AttemptedAt DESC
    LIMIT p_Limit;
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Get active sessions for user
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_GetActiveSessions(
    p_UserId UUID
)
RETURNS TABLE (
    SessionId UUID,
    CreatedAt TIMESTAMP WITH TIME ZONE,
    LastActivityAt TIMESTAMP WITH TIME ZONE,
    ExpiresAt TIMESTAMP WITH TIME ZONE,
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    DeviceInfo JSONB,
    Location JSONB
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        s.SessionId,
        s.CreatedAt,
        s.LastActivityAt,
        s.ExpiresAt,
        s.IpAddress,
        s.UserAgent,
        s.DeviceInfo,
        s.Location
    FROM auth.UserSessions s
    WHERE s.UserId = p_UserId
      AND s.IsActive = TRUE
      AND s.ExpiresAt > NOW()
    ORDER BY s.LastActivityAt DESC;
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Get security audit log for user
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_GetUserAuditLog(
    p_UserId UUID,
    p_Limit INTEGER DEFAULT 50,
    p_Offset INTEGER DEFAULT 0
)
RETURNS TABLE (
    LogId UUID,
    Action VARCHAR(100),
    Timestamp TIMESTAMP WITH TIME ZONE,
    IpAddress VARCHAR(45),
    Success BOOLEAN,
    FailureReason TEXT,
    AdditionalData JSONB
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        al.LogId,
        al.Action,
        al.Timestamp,
        al.IpAddress,
        al.Success,
        al.FailureReason,
        al.AdditionalData
    FROM auth.AuditLogs al
    WHERE al.UserId = p_UserId
    ORDER BY al.Timestamp DESC
    LIMIT p_Limit
    OFFSET p_Offset;
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Get system statistics (for admin dashboard)
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_GetSystemStats()
RETURNS TABLE (
    TotalUsers BIGINT,
    ActiveUsers BIGINT,
    VerifiedUsers BIGINT,
    NewUsersToday BIGINT,
    ActiveSessions BIGINT,
    FailedLoginsToday BIGINT,
    LockedAccounts BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        (SELECT COUNT(*) FROM auth.Users WHERE IsDeleted = FALSE),
        (SELECT COUNT(*) FROM auth.Users WHERE IsActive = TRUE AND IsDeleted = FALSE),
        (SELECT COUNT(*) FROM auth.Users WHERE EmailVerified = TRUE AND IsDeleted = FALSE),
        (SELECT COUNT(*) FROM auth.Users WHERE CreatedAt >= CURRENT_DATE AND IsDeleted = FALSE),
        (SELECT COUNT(*) FROM auth.UserSessions WHERE IsActive = TRUE AND ExpiresAt > NOW()),
        (SELECT COUNT(*) FROM auth.LoginAttempts WHERE Success = FALSE AND AttemptedAt >= CURRENT_DATE),
        (SELECT COUNT(*) FROM auth.Users WHERE LockoutEnd IS NOT NULL AND LockoutEnd > NOW() AND IsDeleted = FALSE);
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Check if email exists
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_EmailExists(
    p_Email VARCHAR(255)
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM auth.Users
        WHERE Email = p_Email AND IsDeleted = FALSE
    );
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Check if username exists
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_UsernameExists(
    p_Username VARCHAR(50)
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM auth.Users
        WHERE Username = p_Username AND IsDeleted = FALSE
    );
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Get users by role
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_GetUsersByRole(
    p_RoleName VARCHAR(50),
    p_Limit INTEGER DEFAULT 100,
    p_Offset INTEGER DEFAULT 0
)
RETURNS TABLE (
    UserId UUID,
    Username VARCHAR(50),
    Email VARCHAR(255),
    IsActive BOOLEAN,
    CreatedAt TIMESTAMP WITH TIME ZONE
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        u.UserId,
        u.Username,
        u.Email,
        u.IsActive,
        u.CreatedAt
    FROM auth.Users u
    JOIN auth.UserRoles ur ON u.UserId = ur.UserId
    JOIN auth.Roles r ON ur.RoleId = r.RoleId
    WHERE r.RoleName = p_RoleName
      AND u.IsDeleted = FALSE
    ORDER BY u.CreatedAt DESC
    LIMIT p_Limit
    OFFSET p_Offset;
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- Function: Get suspicious activity (security monitoring)
-- =============================================
CREATE OR REPLACE FUNCTION auth.fn_GetSuspiciousActivity(
    p_Hours INTEGER DEFAULT 24
)
RETURNS TABLE (
    UserId UUID,
    Username VARCHAR(50),
    Email VARCHAR(255),
    FailedAttempts BIGINT,
    DifferentIPs BIGINT,
    LastAttempt TIMESTAMP WITH TIME ZONE
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        u.UserId,
        u.Username,
        u.Email,
        COUNT(*) AS FailedAttempts,
        COUNT(DISTINCT la.IpAddress) AS DifferentIPs,
        MAX(la.AttemptedAt) AS LastAttempt
    FROM auth.Users u
    JOIN auth.LoginAttempts la ON u.UserId = la.UserId
    WHERE la.Success = FALSE
      AND la.AttemptedAt >= NOW() - (p_Hours || ' hours')::INTERVAL
    GROUP BY u.UserId, u.Username, u.Email
    HAVING COUNT(*) >= 5  -- 5 or more failed attempts
    ORDER BY FailedAttempts DESC;
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================
-- COMMENTS
-- =============================================

COMMENT ON FUNCTION auth.fn_IsUserInRole IS 'Checks if user has a specific role';
COMMENT ON FUNCTION auth.fn_GetUserRoles IS 'Returns array of role names for a user';
COMMENT ON FUNCTION auth.fn_IsAccountLocked IS 'Checks if account is currently locked';
COMMENT ON FUNCTION auth.fn_GetActiveSessionCount IS 'Returns number of active sessions for user';
COMMENT ON FUNCTION auth.fn_GetUserStats IS 'Returns comprehensive statistics for a user';
COMMENT ON FUNCTION auth.fn_SearchUsers IS 'Searches users by username or email (admin function)';
COMMENT ON FUNCTION auth.fn_GetLoginHistory IS 'Returns login history for a user';
COMMENT ON FUNCTION auth.fn_GetActiveSessions IS 'Returns all active sessions for a user';
COMMENT ON FUNCTION auth.fn_GetUserAuditLog IS 'Returns security audit log for a user';
COMMENT ON FUNCTION auth.fn_GetSystemStats IS 'Returns system-wide statistics (admin dashboard)';
COMMENT ON FUNCTION auth.fn_EmailExists IS 'Checks if email is already registered';
COMMENT ON FUNCTION auth.fn_UsernameExists IS 'Checks if username is already taken';
COMMENT ON FUNCTION auth.fn_GetUsersByRole IS 'Gets all users with a specific role';
COMMENT ON FUNCTION auth.fn_GetSuspiciousActivity IS 'Returns users with suspicious login activity (security monitoring)';
