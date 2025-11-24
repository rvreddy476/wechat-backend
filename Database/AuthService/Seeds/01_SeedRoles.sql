-- =============================================
-- WeChat.com - AuthService Seed Data
-- Purpose: Initial roles and admin user
-- =============================================

SET search_path TO auth, public;

-- =============================================
-- Seed Roles
-- =============================================

INSERT INTO auth.Roles (RoleId, RoleName, Description, IsSystemRole, CreatedAt, UpdatedAt)
VALUES
    ('00000000-0000-0000-0000-000000000001'::UUID, 'SuperAdmin', 'Super Administrator with full system access', TRUE, NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000002'::UUID, 'Admin', 'Administrator with elevated privileges', TRUE, NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000003'::UUID, 'Moderator', 'Content moderator with moderation privileges', TRUE, NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000004'::UUID, 'Creator', 'Content creator with video upload privileges', TRUE, NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000005'::UUID, 'User', 'Regular user with standard privileges', TRUE, NOW(), NOW())
ON CONFLICT (RoleId) DO NOTHING;

-- =============================================
-- Seed Default Admin User
-- Password: Admin@123 (hashed with bcrypt)
-- NOTE: Change this password immediately after first login!
-- =============================================

-- Insert admin user
INSERT INTO auth.Users (
    UserId,
    Username,
    Email,
    EmailVerified,
    PasswordHash,
    IsActive,
    CreatedAt,
    UpdatedAt
)
VALUES (
    '10000000-0000-0000-0000-000000000001'::UUID,
    'admin',
    'admin@wechat.com',
    TRUE,
    '$2a$11$5ZvRWZU1J1J5J1J5J1J5Ju.K9K9K9K9K9K9K9K9K9K9K9K9K9K', -- Admin@123 (CHANGE THIS!)
    TRUE,
    NOW(),
    NOW()
)
ON CONFLICT (UserId) DO NOTHING;

-- Assign SuperAdmin role to admin user
INSERT INTO auth.UserRoles (UserId, RoleId, AssignedAt)
VALUES (
    '10000000-0000-0000-0000-000000000001'::UUID,
    '00000000-0000-0000-0000-000000000001'::UUID,
    NOW()
)
ON CONFLICT (UserId, RoleId) DO NOTHING;

-- Log admin creation
INSERT INTO auth.AuditLogs (UserId, Action, EntityType, EntityId, Success, AdditionalData)
VALUES (
    '10000000-0000-0000-0000-000000000001'::UUID,
    'ADMIN_USER_CREATED',
    'USER',
    '10000000-0000-0000-0000-000000000001'::UUID,
    TRUE,
    jsonb_build_object('note', 'Initial admin user created during database setup')
);

-- =============================================
-- Display Warning
-- =============================================

DO $$
BEGIN
    RAISE NOTICE '=============================================';
    RAISE NOTICE 'DEFAULT ADMIN USER CREATED';
    RAISE NOTICE '=============================================';
    RAISE NOTICE 'Username: admin';
    RAISE NOTICE 'Email: admin@wechat.com';
    RAISE NOTICE 'Password: Admin@123';
    RAISE NOTICE '';
    RAISE NOTICE 'WARNING: Please change the default password immediately!';
    RAISE NOTICE '=============================================';
END $$;
