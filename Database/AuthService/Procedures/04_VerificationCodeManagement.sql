-- =============================================
-- Verification Code Management Procedures
-- =============================================

-- =============================================
-- 1. Generate and Store Verification Code
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_CreateVerificationCode(
    p_UserId UUID,
    p_VerificationType VARCHAR(20),
    p_Target VARCHAR(255),
    p_Code VARCHAR(6),
    p_ExpiryMinutes INTEGER DEFAULT 10
)
RETURNS TABLE (
    verification_code_id UUID,
    code VARCHAR(6),
    verification_type VARCHAR(20),
    target VARCHAR(255),
    expires_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE
) AS $$
DECLARE
    v_VerificationCodeId UUID;
    v_ExpiresAt TIMESTAMP WITH TIME ZONE;
BEGIN
    -- Calculate expiry time
    v_ExpiresAt := NOW() + (p_ExpiryMinutes || ' minutes')::INTERVAL;

    -- Invalidate any existing unused codes for this user and type
    UPDATE auth.VerificationCodes
    SET IsExpired = TRUE
    WHERE UserId = p_UserId
      AND VerificationType = p_VerificationType
      AND IsUsed = FALSE
      AND IsExpired = FALSE;

    -- Insert new verification code
    INSERT INTO auth.VerificationCodes (
        UserId,
        Code,
        VerificationType,
        Target,
        ExpiresAt
    )
    VALUES (
        p_UserId,
        p_Code,
        p_VerificationType,
        p_Target,
        v_ExpiresAt
    )
    RETURNING VerificationCodeId INTO v_VerificationCodeId;

    -- Return the verification code details
    RETURN QUERY
    SELECT
        vc.VerificationCodeId,
        vc.Code,
        vc.VerificationType,
        vc.Target,
        vc.ExpiresAt,
        vc.CreatedAt
    FROM auth.VerificationCodes vc
    WHERE vc.VerificationCodeId = v_VerificationCodeId;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- 2. Verify Code
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_VerifyCode(
    p_UserId UUID,
    p_Code VARCHAR(6),
    p_VerificationType VARCHAR(20)
)
RETURNS TABLE (
    is_valid BOOLEAN,
    message VARCHAR(255),
    verification_code_id UUID
) AS $$
DECLARE
    v_VerificationCodeId UUID;
    v_IsUsed BOOLEAN;
    v_IsExpired BOOLEAN;
    v_ExpiresAt TIMESTAMP WITH TIME ZONE;
BEGIN
    -- Find the verification code
    SELECT
        VerificationCodeId,
        IsUsed,
        IsExpired,
        ExpiresAt
    INTO
        v_VerificationCodeId,
        v_IsUsed,
        v_IsExpired,
        v_ExpiresAt
    FROM auth.VerificationCodes
    WHERE UserId = p_UserId
      AND Code = p_Code
      AND VerificationType = p_VerificationType
    ORDER BY CreatedAt DESC
    LIMIT 1;

    -- Code not found
    IF v_VerificationCodeId IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Invalid verification code'::VARCHAR(255), NULL::UUID;
        RETURN;
    END IF;

    -- Code already used
    IF v_IsUsed THEN
        RETURN QUERY SELECT FALSE, 'Verification code has already been used'::VARCHAR(255), v_VerificationCodeId;
        RETURN;
    END IF;

    -- Code expired
    IF v_IsExpired OR v_ExpiresAt < NOW() THEN
        -- Mark as expired if not already
        UPDATE auth.VerificationCodes
        SET IsExpired = TRUE
        WHERE VerificationCodeId = v_VerificationCodeId;

        RETURN QUERY SELECT FALSE, 'Verification code has expired'::VARCHAR(255), v_VerificationCodeId;
        RETURN;
    END IF;

    -- Valid code - mark as used
    UPDATE auth.VerificationCodes
    SET IsUsed = TRUE,
        UsedAt = NOW()
    WHERE VerificationCodeId = v_VerificationCodeId;

    -- Update user verification status
    IF p_VerificationType = 'Email' THEN
        UPDATE auth.Users
        SET EmailVerified = TRUE,
            UpdatedAt = NOW()
        WHERE UserId = p_UserId;
    ELSIF p_VerificationType = 'Phone' THEN
        UPDATE auth.Users
        SET PhoneNumberVerified = TRUE,
            UpdatedAt = NOW()
        WHERE UserId = p_UserId;
    END IF;

    -- Return success
    RETURN QUERY SELECT TRUE, 'Verification successful'::VARCHAR(255), v_VerificationCodeId;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- 3. Check if User Can Request New Code (Rate Limiting)
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_CanRequestVerificationCode(
    p_UserId UUID,
    p_VerificationType VARCHAR(20),
    p_MinutesBetweenRequests INTEGER DEFAULT 1
)
RETURNS TABLE (
    can_request BOOLEAN,
    message VARCHAR(255),
    seconds_until_next_request INTEGER
) AS $$
DECLARE
    v_LastCodeTime TIMESTAMP WITH TIME ZONE;
    v_NextAllowedTime TIMESTAMP WITH TIME ZONE;
    v_SecondsRemaining INTEGER;
BEGIN
    -- Get the most recent verification code creation time
    SELECT CreatedAt
    INTO v_LastCodeTime
    FROM auth.VerificationCodes
    WHERE UserId = p_UserId
      AND VerificationType = p_VerificationType
    ORDER BY CreatedAt DESC
    LIMIT 1;

    -- If no previous code, user can request
    IF v_LastCodeTime IS NULL THEN
        RETURN QUERY SELECT TRUE, 'Can request verification code'::VARCHAR(255), 0;
        RETURN;
    END IF;

    -- Calculate next allowed time
    v_NextAllowedTime := v_LastCodeTime + (p_MinutesBetweenRequests || ' minutes')::INTERVAL;

    -- Check if enough time has passed
    IF NOW() >= v_NextAllowedTime THEN
        RETURN QUERY SELECT TRUE, 'Can request verification code'::VARCHAR(255), 0;
        RETURN;
    ELSE
        v_SecondsRemaining := EXTRACT(EPOCH FROM (v_NextAllowedTime - NOW()))::INTEGER;
        RETURN QUERY SELECT
            FALSE,
            ('Please wait ' || v_SecondsRemaining || ' seconds before requesting a new code')::VARCHAR(255),
            v_SecondsRemaining;
        RETURN;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- 4. Get Verification Code Details
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_GetVerificationCode(
    p_UserId UUID,
    p_VerificationType VARCHAR(20)
)
RETURNS TABLE (
    verification_code_id UUID,
    code VARCHAR(6),
    verification_type VARCHAR(20),
    target VARCHAR(255),
    is_used BOOLEAN,
    is_expired BOOLEAN,
    created_at TIMESTAMP WITH TIME ZONE,
    expires_at TIMESTAMP WITH TIME ZONE,
    used_at TIMESTAMP WITH TIME ZONE
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        vc.VerificationCodeId,
        vc.Code,
        vc.VerificationType,
        vc.Target,
        vc.IsUsed,
        vc.IsExpired OR (vc.ExpiresAt < NOW()) as is_expired,
        vc.CreatedAt,
        vc.ExpiresAt,
        vc.UsedAt
    FROM auth.VerificationCodes vc
    WHERE vc.UserId = p_UserId
      AND vc.VerificationType = p_VerificationType
    ORDER BY vc.CreatedAt DESC
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- 5. Clean Up Expired Verification Codes (Maintenance)
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_CleanupExpiredVerificationCodes(
    p_DaysToKeep INTEGER DEFAULT 7
)
RETURNS TABLE (
    deleted_count INTEGER
) AS $$
DECLARE
    v_DeletedCount INTEGER;
BEGIN
    -- Delete verification codes older than specified days
    DELETE FROM auth.VerificationCodes
    WHERE CreatedAt < NOW() - (p_DaysToKeep || ' days')::INTERVAL;

    GET DIAGNOSTICS v_DeletedCount = ROW_COUNT;

    RETURN QUERY SELECT v_DeletedCount;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- 6. Resend Verification Code (Invalidate old and create new)
-- =============================================
CREATE OR REPLACE FUNCTION auth.sp_ResendVerificationCode(
    p_UserId UUID,
    p_VerificationType VARCHAR(20),
    p_Target VARCHAR(255),
    p_NewCode VARCHAR(6),
    p_ExpiryMinutes INTEGER DEFAULT 10
)
RETURNS TABLE (
    can_resend BOOLEAN,
    message VARCHAR(255),
    verification_code_id UUID,
    code VARCHAR(6),
    expires_at TIMESTAMP WITH TIME ZONE
) AS $$
DECLARE
    v_CanRequest BOOLEAN;
    v_RateLimitMessage VARCHAR(255);
    v_SecondsRemaining INTEGER;
    v_NewCodeId UUID;
    v_NewCode VARCHAR(6);
    v_ExpiresAt TIMESTAMP WITH TIME ZONE;
BEGIN
    -- Check rate limiting
    SELECT
        cr.can_request,
        cr.message,
        cr.seconds_until_next_request
    INTO
        v_CanRequest,
        v_RateLimitMessage,
        v_SecondsRemaining
    FROM auth.sp_CanRequestVerificationCode(p_UserId, p_VerificationType, 1) cr;

    -- If rate limited, return error
    IF NOT v_CanRequest THEN
        RETURN QUERY SELECT
            FALSE,
            v_RateLimitMessage,
            NULL::UUID,
            NULL::VARCHAR(6),
            NULL::TIMESTAMP WITH TIME ZONE;
        RETURN;
    END IF;

    -- Create new verification code (this will invalidate old ones)
    SELECT
        cc.verification_code_id,
        cc.code,
        cc.expires_at
    INTO
        v_NewCodeId,
        v_NewCode,
        v_ExpiresAt
    FROM auth.sp_CreateVerificationCode(
        p_UserId,
        p_VerificationType,
        p_Target,
        p_NewCode,
        p_ExpiryMinutes
    ) cc;

    -- Return success
    RETURN QUERY SELECT
        TRUE,
        'Verification code sent successfully'::VARCHAR(255),
        v_NewCodeId,
        v_NewCode,
        v_ExpiresAt;
END;
$$ LANGUAGE plpgsql;
