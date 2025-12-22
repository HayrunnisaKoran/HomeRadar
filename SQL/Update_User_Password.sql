-- Update homeradar_app_user password
ALTER USER homeradar_app_user WITH PASSWORD '250400';

-- Grant permissions again (to be sure)
GRANT CONNECT ON DATABASE "HomeRadar_db" TO homeradar_app_user;
GRANT USAGE ON SCHEMA public TO homeradar_app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO homeradar_app_user;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO homeradar_app_user;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO homeradar_app_user;

