-- Simple User Creation Script (No Turkish Characters)
-- Run this instead of 05_UserPermissions.sql if you get encoding errors

-- 1. Create or Update Application User
DO $$
BEGIN
    IF EXISTS (SELECT FROM pg_user WHERE usename = 'homeradar_app_user') THEN
        ALTER USER homeradar_app_user WITH PASSWORD '250400';
    ELSE
        CREATE USER homeradar_app_user WITH PASSWORD '250400';
    END IF;
END $$;

-- 2. Database Permissions
GRANT CONNECT ON DATABASE "HomeRadar_db" TO homeradar_app_user;
GRANT USAGE ON SCHEMA public TO homeradar_app_user;

-- 3. Table Permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO homeradar_app_user;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO homeradar_app_user;

-- 4. Function Permissions
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO homeradar_app_user;

-- 5. Default Privileges for Future Objects
ALTER DEFAULT PRIVILEGES IN SCHEMA public 
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO homeradar_app_user;

ALTER DEFAULT PRIVILEGES IN SCHEMA public 
    GRANT USAGE, SELECT ON SEQUENCES TO homeradar_app_user;

ALTER DEFAULT PRIVILEGES IN SCHEMA public 
    GRANT EXECUTE ON FUNCTIONS TO homeradar_app_user;

