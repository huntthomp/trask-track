CREATE ROLE auth_user NOLOGIN;

GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA user_data TO auth_user;
GRANT USAGE ON SCHEMA user_data TO auth_user;

CREATE ROLE app_backend LOGIN PASSWORD 'password'; --Dev password

GRANT auth_user TO app_backend;

GRANT USAGE, SELECT, UPDATE ON SEQUENCE user_data.users_id_seq TO app_backend;
GRANT USAGE, SELECT, UPDATE ON SEQUENCE user_data.calendars_id_seq TO app_backend;
