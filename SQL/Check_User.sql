-- Check if user exists
SELECT usename, usesuper FROM pg_user WHERE usename = 'homeradar_app_user';

-- If user doesn't exist, create it manually:
-- CREATE USER homeradar_app_user WITH PASSWORD 'HomeRadar2024!SecurePass';

