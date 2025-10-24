CREATE ROLE authenticated_user NOLOGIN;

GRANT USAGE ON SCHEMA user_data TO authenticated_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA user_data TO authenticated_user;

-- Backend application user
CREATE ROLE default_user LOGIN PASSWORD 'defaultpass';
GRANT authenticated_user TO default_user;

-- Grant sequence permissions to backend user
GRANT USAGE, SELECT, UPDATE ON SEQUENCE user_data.users_id_seq TO default_user;
GRANT USAGE, SELECT, UPDATE ON SEQUENCE user_data.calendars_id_seq TO default_user;