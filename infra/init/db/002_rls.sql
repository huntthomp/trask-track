ALTER TABLE user_data.users ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_data.calendars ENABLE ROW LEVEL SECURITY;

CREATE POLICY allow_inserts ON user_data.users
    FOR INSERT
    WITH CHECK (auth0_user_id = current_setting('app.tenant', true));

CREATE POLICY allow_select ON user_data.users
    FOR SELECT
    USING (auth0_user_id = current_setting('app.tenant', true));


CREATE POLICY allow_inserts ON user_data.calendars
    FOR INSERT
    WITH CHECK (user_id = (SELECT id FROM user_data.users WHERE auth0_user_id = current_setting('app.tenant')));

CREATE POLICY allow_select ON user_data.calendars
    FOR SELECT
    USING (user_id = (SELECT id FROM user_data.users WHERE auth0_user_id = current_setting('app.tenant')));