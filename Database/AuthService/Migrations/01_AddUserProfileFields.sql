-- =============================================
-- Migration: Add User Profile Fields
-- Date: 2025-11-27
-- Description: Add FirstName, LastName, Handler, Gender, and DOB fields
-- =============================================

-- Set search path
SET search_path TO auth, public;

-- Add new columns to Users table
ALTER TABLE auth.Users
ADD COLUMN IF NOT EXISTS FirstName VARCHAR(100),
ADD COLUMN IF NOT EXISTS LastName VARCHAR(100),
ADD COLUMN IF NOT EXISTS Handler VARCHAR(50) UNIQUE,
ADD COLUMN IF NOT EXISTS Gender VARCHAR(20),
ADD COLUMN IF NOT EXISTS DateOfBirth DATE;

-- Add constraints
ALTER TABLE auth.Users
ADD CONSTRAINT chk_handler_length CHECK (Handler IS NULL OR LENGTH(Handler) >= 3),
ADD CONSTRAINT chk_gender_values CHECK (Gender IS NULL OR Gender IN ('Male', 'Female', 'Other', 'PreferNotToSay')),
ADD CONSTRAINT chk_dob_valid CHECK (DateOfBirth IS NULL OR DateOfBirth <= CURRENT_DATE);

-- Create index for Handler
CREATE INDEX IF NOT EXISTS idx_users_handler ON auth.Users(Handler) WHERE Handler IS NOT NULL AND IsDeleted = FALSE;

-- Add comments
COMMENT ON COLUMN auth.Users.FirstName IS 'User first name';
COMMENT ON COLUMN auth.Users.LastName IS 'User last name';
COMMENT ON COLUMN auth.Users.Handler IS 'Unique handler for user channel (optional at registration, mandatory for channel creation)';
COMMENT ON COLUMN auth.Users.Gender IS 'User gender (Male, Female, Other, PreferNotToSay)';
COMMENT ON COLUMN auth.Users.DateOfBirth IS 'User date of birth';
