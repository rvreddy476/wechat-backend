-- =============================================
-- Updated User Management Procedures with Profile Fields
-- Date: 2025-11-27
-- =============================================

SET search_path TO auth, public;

-- =============================================
-- Procedure: Register New User (Updated with Profile Fields)
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_RegisterUser(
    p_FirstName VARCHAR(100),
    p_LastName VARCHAR(100),
    p_Username VARCHAR(50),
    p_Email VARCHAR(255),
    p_PasswordHash VARCHAR(255),
    p_PhoneNumber VARCHAR(20),
    p_Gender VARCHAR(20),         -- MANDATORY
    p_DateOfBirth DATE,           -- MANDATORY
    p_Handler VARCHAR(50) DEFAULT NULL,
    p_IpAddress VARCHAR(45) DEFAULT NULL,
    p_UserAgent TEXT DEFAULT NULL
)
RETURNS TABLE (
    user_id UUID,
    username VARCHAR(50),
    email VARCHAR(255),
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    phone_number VARCHAR(20),
    handler VARCHAR(50),
    is_email_verified BOOLEAN,
    is_phone_verified BOOLEAN,
    is_active BOOLEAN,
    is_deleted BOOLEAN,
    bio TEXT,
    avatar_url TEXT,
    created_at TIMESTAMP WITH TIME ZONE,
    updated_at TIMESTAMP WITH TIME ZONE
) AS $$
DECLARE
    v_UserId UUID;
    v_RoleId UUID;
BEGIN
    -- Validate required fields
    IF p_FirstName IS NULL OR TRIM(p_FirstName) = '' THEN
        RAISE EXCEPTION 'First name is required';
    END IF;

    IF p_LastName IS NULL OR TRIM(p_LastName) = '' THEN
        RAISE EXCEPTION 'Last name is required';
    END IF;

    IF p_Email IS NULL OR TRIM(p_Email) = '' THEN
        RAISE EXCEPTION 'Email is required';
    END IF;

    IF p_PhoneNumber IS NULL OR TRIM(p_PhoneNumber) = '' THEN
        RAISE EXCEPTION 'Phone number is required';
    END IF;

    -- Validate gender (MANDATORY)
    IF p_Gender IS NULL OR TRIM(p_Gender) = '' THEN
        RAISE EXCEPTION 'Gender is required';
    END IF;

    IF p_Gender NOT IN ('Male', 'Female', 'Other', 'PreferNotToSay') THEN
        RAISE EXCEPTION 'Invalid gender value. Must be: Male, Female, Other, or PreferNotToSay';
    END IF;

    -- Validate date of birth (MANDATORY)
    IF p_DateOfBirth IS NULL THEN
        RAISE EXCEPTION 'Date of birth is required';
    END IF;

    IF p_DateOfBirth > CURRENT_DATE THEN
        RAISE EXCEPTION 'Date of birth cannot be in the future';
    END IF;

    -- Check if email already exists
    IF EXISTS (SELECT 1 FROM auth.Users WHERE Email = p_Email AND IsDeleted = FALSE) THEN
        RAISE EXCEPTION 'Email already registered';
    END IF;

    -- Check if username already exists
    IF EXISTS (SELECT 1 FROM auth.Users WHERE Username = p_Username AND IsDeleted = FALSE) THEN
        RAISE EXCEPTION 'Username already taken';
    END IF;

    -- Check if phone number already exists
    IF EXISTS (SELECT 1 FROM auth.Users WHERE PhoneNumber = p_PhoneNumber AND IsDeleted = FALSE) THEN
        RAISE EXCEPTION 'Phone number already registered';
    END IF;

    -- Check if handler already exists (if provided)
    IF p_Handler IS NOT NULL AND EXISTS (SELECT 1 FROM auth.Users WHERE Handler = p_Handler AND IsDeleted = FALSE) THEN
        RAISE EXCEPTION 'Handler already taken';
    END IF;

    -- Insert new user
    INSERT INTO auth.Users (
        FirstName,
        LastName,
        Username,
        Email,
        PasswordHash,
        PhoneNumber,
        Handler,
        Gender,
        DateOfBirth
    )
    VALUES (
        p_FirstName,
        p_LastName,
        p_Username,
        p_Email,
        p_PasswordHash,
        p_PhoneNumber,
        p_Handler,
        p_Gender,
        p_DateOfBirth
    )
    RETURNING Users.UserId INTO v_UserId;

    -- Assign default "User" role
    SELECT r.RoleId INTO v_RoleId
    FROM auth.Roles r
    WHERE r.RoleName = 'User';

    IF v_RoleId IS NOT NULL THEN
        INSERT INTO auth.UserRoles (UserId, RoleId)
        VALUES (v_UserId, v_RoleId);
    END IF;

    -- Log registration
    INSERT INTO auth.AuditLogs (UserId, Action, EntityType, EntityId, IpAddress, UserAgent, Success)
    VALUES (v_UserId, 'USER_REGISTERED', 'USER', v_UserId, p_IpAddress, p_UserAgent, TRUE);

    -- Return user data
    RETURN QUERY
    SELECT
        u.UserId,
        u.Username,
        u.Email,
        u.FirstName,
        u.LastName,
        u.PhoneNumber,
        u.Handler,
        u.EmailVerified,
        u.PhoneNumberVerified,
        u.IsActive,
        u.IsDeleted,
        NULL::TEXT as bio,
        NULL::TEXT as avatar_url,
        u.CreatedAt,
        u.UpdatedAt
    FROM auth.Users u
    WHERE u.UserId = v_UserId;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Procedure: Get User By Id (Updated)
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_GetUserById(
    p_UserId UUID
)
RETURNS TABLE (
    user_id UUID,
    username VARCHAR(50),
    email VARCHAR(255),
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    phone_number VARCHAR(20),
    handler VARCHAR(50),
    gender VARCHAR(20),
    date_of_birth DATE,
    email_verified BOOLEAN,
    phone_number_verified BOOLEAN,
    two_factor_enabled BOOLEAN,
    is_active BOOLEAN,
    created_at TIMESTAMP WITH TIME ZONE,
    updated_at TIMESTAMP WITH TIME ZONE,
    last_login_at TIMESTAMP WITH TIME ZONE,
    roles TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        u.UserId,
        u.Username,
        u.Email,
        u.FirstName,
        u.LastName,
        u.PhoneNumber,
        u.Handler,
        u.Gender,
        u.DateOfBirth,
        u.EmailVerified,
        u.PhoneNumberVerified,
        u.TwoFactorEnabled,
        u.IsActive,
        u.CreatedAt,
        u.UpdatedAt,
        u.LastLoginAt,
        STRING_AGG(r.RoleName, ',') AS Roles
    FROM auth.Users u
    LEFT JOIN auth.UserRoles ur ON u.UserId = ur.UserId
    LEFT JOIN auth.Roles r ON ur.RoleId = r.RoleId
    WHERE u.UserId = p_UserId AND u.IsDeleted = FALSE
    GROUP BY u.UserId, u.Username, u.Email, u.FirstName, u.LastName, u.PhoneNumber,
             u.Handler, u.Gender, u.DateOfBirth, u.EmailVerified, u.PhoneNumberVerified,
             u.TwoFactorEnabled, u.IsActive, u.CreatedAt, u.UpdatedAt, u.LastLoginAt;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Procedure: Update User Profile (Updated)
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_UpdateUserProfile(
    p_UserId UUID,
    p_FirstName VARCHAR(100) DEFAULT NULL,
    p_LastName VARCHAR(100) DEFAULT NULL,
    p_Username VARCHAR(50) DEFAULT NULL,
    p_Email VARCHAR(255) DEFAULT NULL,
    p_PhoneNumber VARCHAR(20) DEFAULT NULL,
    p_Handler VARCHAR(50) DEFAULT NULL,
    p_Gender VARCHAR(20) DEFAULT NULL,
    p_DateOfBirth DATE DEFAULT NULL
)
RETURNS TABLE (
    Success BOOLEAN,
    Message TEXT
) AS $$
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM auth.Users WHERE UserId = p_UserId AND IsDeleted = FALSE) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT;
        RETURN;
    END IF;

    -- Check username uniqueness
    IF p_Username IS NOT NULL AND
       EXISTS (SELECT 1 FROM auth.Users WHERE Username = p_Username AND UserId != p_UserId AND IsDeleted = FALSE) THEN
        RETURN QUERY SELECT FALSE, 'Username already taken'::TEXT;
        RETURN;
    END IF;

    -- Check email uniqueness
    IF p_Email IS NOT NULL AND
       EXISTS (SELECT 1 FROM auth.Users WHERE Email = p_Email AND UserId != p_UserId AND IsDeleted = FALSE) THEN
        RETURN QUERY SELECT FALSE, 'Email already registered'::TEXT;
        RETURN;
    END IF;

    -- Check phone number uniqueness
    IF p_PhoneNumber IS NOT NULL AND
       EXISTS (SELECT 1 FROM auth.Users WHERE PhoneNumber = p_PhoneNumber AND UserId != p_UserId AND IsDeleted = FALSE) THEN
        RETURN QUERY SELECT FALSE, 'Phone number already registered'::TEXT;
        RETURN;
    END IF;

    -- Check handler uniqueness
    IF p_Handler IS NOT NULL AND
       EXISTS (SELECT 1 FROM auth.Users WHERE Handler = p_Handler AND UserId != p_UserId AND IsDeleted = FALSE) THEN
        RETURN QUERY SELECT FALSE, 'Handler already taken'::TEXT;
        RETURN;
    END IF;

    -- Update user
    UPDATE auth.Users
    SET
        FirstName = COALESCE(p_FirstName, FirstName),
        LastName = COALESCE(p_LastName, LastName),
        Username = COALESCE(p_Username, Username),
        Email = COALESCE(p_Email, Email),
        PhoneNumber = COALESCE(p_PhoneNumber, PhoneNumber),
        Handler = COALESCE(p_Handler, Handler),
        Gender = COALESCE(p_Gender, Gender),
        DateOfBirth = COALESCE(p_DateOfBirth, DateOfBirth),
        EmailVerified = CASE WHEN p_Email IS NOT NULL AND p_Email != Email THEN FALSE ELSE EmailVerified END,
        PhoneNumberVerified = CASE WHEN p_PhoneNumber IS NOT NULL AND p_PhoneNumber != PhoneNumber THEN FALSE ELSE PhoneNumberVerified END,
        UpdatedAt = NOW()
    WHERE UserId = p_UserId;

    RETURN QUERY SELECT TRUE, 'Profile updated successfully'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- COMMENTS
-- =============================================

COMMENT ON FUNCTION auth.sp_RegisterUser IS 'Registers a new user with profile fields (FirstName, LastName, Handler, Gender, DOB)';
COMMENT ON FUNCTION auth.sp_GetUserById IS 'Gets user details including profile fields';
COMMENT ON FUNCTION auth.sp_UpdateUserProfile IS 'Updates user profile with new fields';
