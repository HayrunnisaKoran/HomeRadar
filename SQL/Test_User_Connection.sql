-- Test if we can connect with the user and password
-- This will help us verify the password is correct

-- First, let's check the user exists
SELECT usename FROM pg_user WHERE usename = 'homeradar_app_user';

-- If user exists, try to update password again (just to be sure)
ALTER USER homeradar_app_user WITH PASSWORD '250400';

-- Verify the change
SELECT usename, usesuper FROM pg_user WHERE usename = 'homeradar_app_user';

