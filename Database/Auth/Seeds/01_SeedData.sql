-- ========================================
-- Seed Data for Development/Testing
-- WARNING: Do not run in production!
-- ========================================

-- Insert test users
-- Password: "Password123!" hashed with BCrypt
INSERT INTO users (id, username, email, password_hash, is_email_verified, roles) VALUES
(
    '550e8400-e29b-41d4-a716-446655440000',
    'admin',
    'admin@wechat.com',
    '$2a$11$YourHashedPasswordHere',
    TRUE,
    '["User", "Admin"]'::jsonb
),
(
    '550e8400-e29b-41d4-a716-446655440001',
    'testuser1',
    'test1@example.com',
    '$2a$11$YourHashedPasswordHere',
    TRUE,
    '["User"]'::jsonb
),
(
    '550e8400-e29b-41d4-a716-446655440002',
    'testuser2',
    'test2@example.com',
    '$2a$11$YourHashedPasswordHere',
    FALSE,
    '["User"]'::jsonb
)
ON CONFLICT (id) DO NOTHING;

-- Note: Replace $2a$11$YourHashedPasswordHere with actual BCrypt hash
-- Generate using: BCrypt.Net.BCrypt.HashPassword("Password123!")
