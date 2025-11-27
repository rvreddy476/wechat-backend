-- =============================================
-- Verification Codes Table
-- Stores 6-digit verification codes for email and phone verification
-- =============================================

CREATE TABLE IF NOT EXISTS auth.VerificationCodes (
    VerificationCodeId UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- User Reference
    UserId UUID NOT NULL,

    -- Verification Details
    Code VARCHAR(6) NOT NULL,
    VerificationType VARCHAR(20) NOT NULL, -- 'Email' or 'Phone'
    Target VARCHAR(255) NOT NULL, -- Email address or phone number

    -- Status
    IsUsed BOOLEAN NOT NULL DEFAULT FALSE,
    IsExpired BOOLEAN NOT NULL DEFAULT FALSE,

    -- Timestamps
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    UsedAt TIMESTAMP WITH TIME ZONE,

    -- Constraints
    CONSTRAINT fk_verification_user FOREIGN KEY (UserId) REFERENCES auth.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT chk_verification_type CHECK (VerificationType IN ('Email', 'Phone')),
    CONSTRAINT chk_code_format CHECK (Code ~ '^[0-9]{6}$'), -- Exactly 6 digits
    CONSTRAINT chk_target_not_empty CHECK (LENGTH(TRIM(Target)) > 0)
);

-- Indexes for better performance
CREATE INDEX IF NOT EXISTS idx_verification_codes_userid ON auth.VerificationCodes(UserId);
CREATE INDEX IF NOT EXISTS idx_verification_codes_code ON auth.VerificationCodes(Code);
CREATE INDEX IF NOT EXISTS idx_verification_codes_type ON auth.VerificationCodes(VerificationType);
CREATE INDEX IF NOT EXISTS idx_verification_codes_target ON auth.VerificationCodes(Target);
CREATE INDEX IF NOT EXISTS idx_verification_codes_expires ON auth.VerificationCodes(ExpiresAt);

-- Composite index for common queries
CREATE INDEX IF NOT EXISTS idx_verification_lookup
ON auth.VerificationCodes(UserId, VerificationType, IsUsed, IsExpired);

-- Comments
COMMENT ON TABLE auth.VerificationCodes IS 'Stores verification codes for email and phone verification';
COMMENT ON COLUMN auth.VerificationCodes.Code IS '6-digit verification code';
COMMENT ON COLUMN auth.VerificationCodes.VerificationType IS 'Type of verification: Email or Phone';
COMMENT ON COLUMN auth.VerificationCodes.Target IS 'Email address or phone number being verified';
COMMENT ON COLUMN auth.VerificationCodes.IsUsed IS 'Whether the code has been used';
COMMENT ON COLUMN auth.VerificationCodes.IsExpired IS 'Whether the code has expired (set by cleanup job or manual check)';
COMMENT ON COLUMN auth.VerificationCodes.ExpiresAt IS 'When the code expires (typically 10-15 minutes from creation)';
