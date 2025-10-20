CREATE SCHEMA user_data;

CREATE TABLE user_data.users (
    id SERIAL PRIMARY KEY,
    auth0_user_id TEXT UNIQUE NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE user_data.calendars (
    id BIGSERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES user_data.users(id),
    public_id UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    calendar_name TEXT NOT NULL,
    calendar_ics_url TEXT NOT NULL,
    synced_at TIMESTAMPTZ DEFAULT NULL,
    metadata JSON NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    UNIQUE (user_id, calendar_ics_url)
);