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
    sync_error BOOLEAN NOT NULL DEFAULT FALSE,
    metadata JSON NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    UNIQUE (user_id, calendar_ics_url)
);

CREATE TABLE user_data.task_groups (
    id BIGSERIAL PRIMARY KEY,
    calendar_id BIGINT NOT NULL REFERENCES user_data.calendars(id),
    public_id UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    group_name TEXT NOT NULL,
    metadata JSON NOT NULL,

    UNIQUE (calendar_id, group_name)
);

CREATE TABLE user_data.tasks(
    id BIGSERIAL PRIMARY KEY,
    task_group_id BIGINT REFERENCES user_data.task_groups(id),
    ics_event_id TEXT NOT NULL,
    course_id TEXT,
    assignment_id TEXT,
    description TEXT,
    summary TEXT,
    url TEXT,
    due_date TIMESTAMPTZ,
    status TEXT NOT NULL DEFAULT 'incomplete',
    UNIQUE(task_group_id, ics_event_id, course_id, assignment_id)
);

-- Normalize courses, instructors, etc in the future

-- Derived from Miami public API
CREATE SCHEMA miami;

CREATE TABLE miami.instructors(
    id SERIAL PRIMARY KEY,
    unique_id TEXT NOT NULL,
    last_name TEXT NOT NULL,
    first_name TEXT NOT NULL,
    middle_name TEXT,
    prefix TEXT,
    suffix TEXT,
    preferred_first_name TEXT,
    UNIQUE (unique_id)
);

CREATE TABLE miami.buildings(
    id SERIAL PRIMARY KEY,
    building_code TEXT NOT NULL,
    building_name TEXT NOT NULL,
    UNIQUE (building_code)
);

CREATE TABLE miami.terms(
    id SERIAL PRIMARY KEY,
    code TEXT NOT NULL,
    name TEXT NOT NULL,
    start_date TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    end_date TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    UNIQUE (code, start_date, end_date)
);

CREATE TABLE miami.courses(
    id SERIAL PRIMARY KEY,
    school_name TEXT NOT NULL,
    department_name TEXT NOT NULL,
    title TEXT NOT NULL,
    subject_code TEXT NOT NULL,
    subject_description TEXT NOT NULL,
    course_number TEXT NOT NULL,
    credit_hours_low NUMERIC(3,1),
    credit_hours_high NUMERIC(3,1),
    description TEXT,
    UNIQUE (subject_code, course_number)
);

CREATE TABLE miami.sections(
    id SERIAL PRIMARY KEY,
    course_id INT NOT NULL REFERENCES miami.courses(id),
    term_id INT NOT NULL REFERENCES miami.terms(id),
    crn TEXT NOT NULL,
    section_unique_id UUID NOT NULL,
    section_name TEXT NOT NULL,
    instruction_type_description TEXT NOT NULL,
    campus_name TEXT NOT NULL,
    final_grade_required BOOLEAN NOT NULL,
    max_seats INT NOT NULL,
    available_seats INT NOT NULL,
    UNIQUE (crn, term_id)
);

CREATE TABLE miami.schedules(
    id SERIAL PRIMARY KEY,
    section_id INT NOT NULL REFERENCES miami.sections(id),
    building_id INT REFERENCES miami.buildings(id),
    start_date TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    end_date TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    start_time TIME,
    end_time TIME,
    room_number TEXT,
    days TEXT,
    schedule_type_description TEXT NOT NULL,
    UNIQUE (section_id, start_date, end_date, start_time, end_time, days, schedule_type_description)
);

-- Bridge tables

CREATE TABLE miami.instructor_section (
    instructor_id INT NOT NULL REFERENCES miami.instructors(id),
    section_id INT NOT NULL REFERENCES miami.sections(id),
    is_primary BOOLEAN NOT NULL,
    UNIQUE (instructor_id, section_id)
);


-- Schema for admin and system functions
CREATE SCHEMA internal;

-- Checkpoints for long running tasks
-- E.G Course data syncing. Storing next and current paginated urls
CREATE TABLE internal.task_checkpoints(
    tag TEXT PRIMARY KEY,
    next TEXT NULL,
    current TEXT NULL,
    last_updated TIMESTAMPTZ NOT NULL DEFAULT NOW()
);