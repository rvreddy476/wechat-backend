-- ========================================
-- Initial Database Setup Migration
-- Run this script to set up the entire Auth database
-- ========================================

-- Execute in order:
\i Schema/01_CreateTables.sql
\i Schema/02_CreateIndexes.sql
\i Schema/03_CreateConstraints.sql
\i Functions/01_HelperFunctions.sql
\i Procedures/01_UserManagement.sql
\i Procedures/02_TokenManagement.sql

-- Create scheduled job for cleanup (requires pg_cron extension)
-- Uncomment if pg_cron is available
-- CREATE EXTENSION IF NOT EXISTS pg_cron;
-- SELECT cron.schedule('cleanup-old-data', '0 2 * * *', 'SELECT cleanup_old_data()');
