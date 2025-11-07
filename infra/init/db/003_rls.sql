ALTER TABLE user_data.users ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_data.calendars ENABLE ROW LEVEL SECURITY;

-- ============================================
-- RLS POLICIES: users table
-- ============================================

CREATE POLICY allow_select ON user_data.users
    FOR SELECT
    USING (auth0_user_id = current_setting('app.tenant', true));

CREATE POLICY allow_inserts ON user_data.users
    FOR INSERT
    WITH CHECK (auth0_user_id = current_setting('app.tenant', true));

-- ============================================
-- RLS POLICIES: calendars table
-- ============================================

CREATE POLICY allow_select ON user_data.calendars
    FOR SELECT
    USING (user_id = (SELECT id FROM user_data.users WHERE auth0_user_id = current_setting('app.tenant')));

CREATE POLICY allow_inserts ON user_data.calendars
    FOR INSERT
    WITH CHECK (user_id = (SELECT id FROM user_data.users WHERE auth0_user_id = current_setting('app.tenant')));

CREATE POLICY allow_update ON user_data.calendars
    FOR UPDATE
    USING (user_id = (SELECT id FROM user_data.users WHERE auth0_user_id = current_setting('app.tenant')))
    WITH CHECK (user_id = (SELECT id FROM user_data.users WHERE auth0_user_id = current_setting('app.tenant')));

-- Task related RLS
CREATE POLICY allow_inserts ON user_data.task_groups
    FOR INSERT
    WITH CHECK (calendar_id = (SELECT id FROM user_data.calendars WHERE user_id::text = current_setting('app.tenant')));

CREATE POLICY allow_inserts ON user_data.tasks
    FOR INSERT
    WITH CHECK (task_group_id = (SELECT id FROM user_data.task_groups WHERE calendar_id = (SELECT id FROM user_data.calendars WHERE user_id::text = current_setting('app.tenant'))));