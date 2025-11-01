namespace TaskTrack.Shared.Repositories;

using Npgsql;
using TaskTrack.CalendarService.Models;
using NpgsqlTypes;

public interface IMiamiCourseInfoRepository
{
    Task InsertInstructorsAsync(List<MiamiInstructorDto> instructors);
    Task InsertBuildingsAsync(List<MiamiBuildingDto> buildings);
    Task InsertTermsAsync(List<MiamiPartOfTermDto> terms);
    Task InsertCoursesAsync(List<MiamiCourseDto> courses);
    Task InsertSectionsAsync(List<MiamiSectionDto> sections);
    Task InsertSchedulesAsync(List<MiamiScheduleInsertDto> schedules);
    Task InsertInstructorSectionsAsync(List<MiamiInstructorSectionDto> instructorSections);
}

public class MiamiCourseInfoRepository : IMiamiCourseInfoRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public MiamiCourseInfoRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task InsertInstructorsAsync(List<MiamiInstructorDto> instructors)
    {
        const string staging = @"
        CREATE TEMP TABLE staging_instructors(
            unique_id TEXT NOT NULL,
            last_name TEXT NOT NULL,
            first_name TEXT NOT NULL,
            middle_name TEXT,
            prefix TEXT,
            suffix TEXT,
            preferred_first_name TEXT
        ) ON COMMIT DROP;
        ";

        const string copy = @"
        COPY staging_instructors (unique_id, last_name, first_name, middle_name, prefix, suffix, preferred_first_name)
        FROM STDIN (FORMAT BINARY)
        ";

        const string merge = @"
        INSERT INTO miami.instructors(
            unique_id,
            last_name,
            first_name,
            middle_name,
            prefix,
            suffix,
            preferred_first_name
        )
        SELECT unique_id,
            last_name,
            first_name,
            middle_name,
            prefix,
            suffix,
            preferred_first_name
        FROM staging_instructors 
        ON CONFLICT (unique_id) DO UPDATE
        SET 
            last_name = EXCLUDED.last_name,
            first_name = EXCLUDED.first_name,
            middle_name = EXCLUDED.middle_name,
            prefix = EXCLUDED.prefix,
            suffix = EXCLUDED.suffix,
            preferred_first_name = EXCLUDED.preferred_first_name;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var createStage = new NpgsqlCommand(staging, connection, transaction))
        {
            await createStage.ExecuteNonQueryAsync();
        }

        await using (var writer = connection.BeginBinaryImport(copy))
        {
            foreach (var instructor in instructors)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(instructor.Person.UniqueId, NpgsqlDbType.Text);
                await writer.WriteAsync(instructor.Person.LastName, NpgsqlDbType.Text);
                await writer.WriteAsync(instructor.Person.FirstName, NpgsqlDbType.Text);
                await writer.WriteAsync(instructor.Person.MiddleName, NpgsqlDbType.Text);
                await writer.WriteAsync(instructor.Person.Prefix, NpgsqlDbType.Text);
                await writer.WriteAsync(instructor.Person.Suffix, NpgsqlDbType.Text);
                await writer.WriteAsync(instructor.Person.PreferredFirstName, NpgsqlDbType.Text);
            }

            await writer.CompleteAsync();
        }

        await using (var mergeStage = new NpgsqlCommand(merge, connection, transaction))
        {
            await mergeStage.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task InsertBuildingsAsync(List<MiamiBuildingDto> buildings)
    {
        const string staging = @"
        CREATE TEMP TABLE staging_buildings(
            building_code TEXT NOT NULL,
            building_name TEXT NOT NULL
        ) ON COMMIT DROP;
        ";

        const string copy = @"
        COPY staging_buildings (building_code, building_name)
        FROM STDIN (FORMAT BINARY)
        ";

        const string merge = @"
        INSERT INTO miami.buildings (
            building_code,
            building_name
        )
        SELECT building_code,
            building_name
        FROM staging_buildings 
        ON CONFLICT (building_code) DO UPDATE
        SET 
            building_name = EXCLUDED.building_name;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var createStage = new NpgsqlCommand(staging, connection, transaction))
        {
            await createStage.ExecuteNonQueryAsync();
        }

        await using (var writer = connection.BeginBinaryImport(copy))
        {
            foreach (var building in buildings)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(building.BuildingCode, NpgsqlDbType.Text);
                await writer.WriteAsync(building.BuildingName, NpgsqlDbType.Text);
            }

            await writer.CompleteAsync();
        }

        await using (var mergeStage = new NpgsqlCommand(merge, connection, transaction))
        {
            await mergeStage.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task InsertTermsAsync(List<MiamiPartOfTermDto> terms)
    {
        const string staging = @"
        CREATE TEMP TABLE staging_terms(
            code TEXT NOT NULL,
            name TEXT NOT NULL,
            start_date TIMESTAMPTZ NOT NULL,
            end_date TIMESTAMPTZ NOT NULL
        ) ON COMMIT DROP;
        ";

        const string copy = @"
        COPY staging_terms (code, name, start_date, end_date)
        FROM STDIN (FORMAT BINARY)
        ";

        const string merge = @"
        INSERT INTO miami.terms (
            code,
            name,
            start_date,
            end_date
        )
        SELECT code,
            name,
            start_date,
            end_date
        FROM staging_terms
        ON CONFLICT (code, start_date, end_date) DO NOTHING;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var createStage = new NpgsqlCommand(staging, connection, transaction))
        {
            await createStage.ExecuteNonQueryAsync();
        }

        await using (var writer = connection.BeginBinaryImport(copy))
        {
            foreach (var term in terms)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(term.Code, NpgsqlDbType.Text);
                await writer.WriteAsync(term.Name, NpgsqlDbType.Text);
                await writer.WriteAsync(term.StartDate, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(term.EndDate, NpgsqlDbType.Timestamp);
            }

            await writer.CompleteAsync();
        }

        await using (var mergeStage = new NpgsqlCommand(merge, connection, transaction))
        {
            await mergeStage.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task InsertCoursesAsync(List<MiamiCourseDto> courses)
    {
        const string staging = @"
        CREATE TEMP TABLE staging_courses(
            school_name TEXT NOT NULL,
            department_name TEXT NOT NULL,
            title TEXT NOT NULL,
            subject_code TEXT NOT NULL,
            subject_description TEXT NOT NULL,
            course_number TEXT NOT NULL,
            credit_hours_low NUMERIC(3,1),
            credit_hours_high NUMERIC(3,1),
            description TEXT
        ) ON COMMIT DROP;
        ";

        const string copy = @"
        COPY staging_courses (school_name, 
            department_name,
            title,
            subject_code,
            subject_description,
            course_number,
            credit_hours_low,
            credit_hours_high,
            description
        )
        FROM STDIN (FORMAT BINARY)
        ";

        const string merge = @"
        INSERT INTO miami.courses (
            school_name, 
            department_name,
            title,
            subject_code,
            subject_description,
            course_number,
            credit_hours_low,
            credit_hours_high,
            description
        )
        SELECT school_name, 
            department_name,
            title,
            subject_code,
            subject_description,
            course_number,
            credit_hours_low,
            credit_hours_high,
            description
        FROM staging_courses 
        ON CONFLICT (subject_code, course_number) DO UPDATE
        SET 
            school_name =  EXCLUDED.school_name,
            department_name =  EXCLUDED.department_name,
            title =  EXCLUDED.title,
            subject_description =  EXCLUDED.subject_description,
            credit_hours_low =  EXCLUDED.credit_hours_low,
            credit_hours_high =  EXCLUDED.credit_hours_high,
            description =  EXCLUDED.description;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var createStage = new NpgsqlCommand(staging, connection, transaction))
        {
            await createStage.ExecuteNonQueryAsync();
        }

        await using (var writer = connection.BeginBinaryImport(copy))
        {
            foreach (var course in courses)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(course.SchoolName, NpgsqlDbType.Text);
                await writer.WriteAsync(course.DepartmentName, NpgsqlDbType.Text);
                await writer.WriteAsync(course.Title, NpgsqlDbType.Text);
                await writer.WriteAsync(course.SubjectCode, NpgsqlDbType.Text);
                await writer.WriteAsync(course.SubjectDescription, NpgsqlDbType.Text);
                await writer.WriteAsync(course.Number, NpgsqlDbType.Text);
                await writer.WriteAsync(course.CreditHoursHigh, NpgsqlDbType.Numeric);
                await writer.WriteAsync(course.CreditHoursLow, NpgsqlDbType.Numeric);
                await writer.WriteAsync(course.Description, NpgsqlDbType.Text);
            }

            await writer.CompleteAsync();
        }

        await using (var mergeStage = new NpgsqlCommand(merge, connection, transaction))
        {
            await mergeStage.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task InsertSectionsAsync(List<MiamiSectionDto> sections)
    {
        const string staging = @"
        CREATE TEMP TABLE staging_sections(
            subject_code TEXT NOT NULL,
            course_number TEXT NOT NULL,
            code TEXT NOT NULL,
            start_date TIMESTAMPTZ NOT NULL,
            end_date TIMESTAMPTZ NOT NULL,
            crn TEXT NOT NULL,
            section_unique_id UUID NOT NULL,
            section_name TEXT NOT NULL,
            instruction_type_description TEXT NOT NULL,
            campus_name TEXT NOT NULL,
            final_grade_required BOOLEAN NOT NULL,
            max_seats INT NOT NULL,
            available_seats INT NOT NULL
        ) ON COMMIT DROP;
        ";

        const string copy = @"
        COPY staging_sections (
            subject_code,
            course_number,
            code,
            start_date,
            end_date,
            crn,
            section_unique_id,
            section_name,
            instruction_type_description,
            campus_name,
            final_grade_required,
            max_seats,
            available_seats
        )
        FROM STDIN (FORMAT BINARY)
        ";

        const string merge = @"
        INSERT INTO miami.sections (
            course_id,
            term_id,
            crn,
            section_unique_id,
            section_name,
            instruction_type_description,
            campus_name,
            final_grade_required,
            max_seats,
            available_seats
        )
        SELECT c.id,
            t.id,
            s.crn,
            s.section_unique_id,
            s.section_name,
            s.instruction_type_description,
            s.campus_name,
            s.final_grade_required,
            s.max_seats,
            s.available_seats
        FROM staging_sections s
        LEFT JOIN miami.terms t ON s.code = t.code AND s.start_date = t.start_date AND s.end_date = t.end_date
        LEFT JOIN miami.courses c ON s.subject_code = c.subject_code AND s.course_number = c.course_number
        ON CONFLICT (crn, term_id) DO UPDATE
        SET 
            section_unique_id = EXCLUDED.section_unique_id,
            section_name = EXCLUDED.section_name,
            instruction_type_description = EXCLUDED.instruction_type_description,
            campus_name = EXCLUDED.campus_name,
            final_grade_required = EXCLUDED.final_grade_required,
            max_seats = EXCLUDED.max_seats,
            available_seats = EXCLUDED.available_seats;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var createStage = new NpgsqlCommand(staging, connection, transaction))
        {
            await createStage.ExecuteNonQueryAsync();
        }

        await using (var writer = connection.BeginBinaryImport(copy))
        {
            foreach (var section in sections)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(section.Course.SubjectCode, NpgsqlDbType.Text);
                await writer.WriteAsync(section.Course.Number, NpgsqlDbType.Text);
                await writer.WriteAsync(section.PartOfTerm.Code, NpgsqlDbType.Text);
                await writer.WriteAsync(section.PartOfTerm.StartDate, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(section.PartOfTerm.EndDate, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(section.Crn, NpgsqlDbType.Text);
                await writer.WriteAsync(section.CourseSectionGuid, NpgsqlDbType.Uuid);
                await writer.WriteAsync(section.SectionName, NpgsqlDbType.Text);
                await writer.WriteAsync(section.InstructionTypeDescription, NpgsqlDbType.Text);
                await writer.WriteAsync(section.CampusName, NpgsqlDbType.Text);
                await writer.WriteAsync(section.IsFinalGradeRequired, NpgsqlDbType.Boolean);
                await writer.WriteAsync(section.EnrollmentCount.NumberOfMax, NpgsqlDbType.Integer);
                await writer.WriteAsync(section.EnrollmentCount.NumberOfAvailable, NpgsqlDbType.Integer);
            }

            await writer.CompleteAsync();
        }

        await using (var mergeStage = new NpgsqlCommand(merge, connection, transaction))
        {
            await mergeStage.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task InsertSchedulesAsync(List<MiamiScheduleInsertDto> schedules)
    {
        const string staging = @"
        CREATE TEMP TABLE staging_schedules(
            code TEXT NOT NULL,
            term_start_date TIMESTAMPTZ NOT NULL,
            term_end_date TIMESTAMPTZ NOT NULL,
            crn TEXT NOT NULL,
            building_code TEXT,
            start_date TIMESTAMPTZ NOT NULL,
            end_date TIMESTAMPTZ NOT NULL,
            start_time TIME,
            end_time TIME,
            room_number TEXT,
            days TEXT,
            schedule_type_description TEXT NOT NULL
        ) ON COMMIT DROP;
        ";

        const string copy = @"
        COPY staging_schedules (
            code,
            term_start_date,
            term_end_date,
            crn,
            building_code,
            start_date,
            end_date,
            start_time,
            end_time,
            room_number,
            days,
            schedule_type_description
        )
        FROM STDIN (FORMAT BINARY)
        ";

        const string merge = @"
        INSERT INTO miami.schedules (
            section_id,
            building_id,
            start_date,
            end_date,
            start_time,
            end_time,
            room_number,
            days,
            schedule_type_description
        )
        SELECT s.id,
            b.id,
            sch.start_date,
            sch.end_date,
            sch.start_time,
            sch.end_time,
            sch.room_number,
            sch.days,
            sch.schedule_type_description
        FROM staging_schedules sch
        LEFT JOIN miami.buildings b ON sch.building_code = b.building_code
        LEFT JOIN miami.terms t ON sch.code = t.code AND sch.term_start_date = t.start_date AND sch.term_end_date = t.end_date
        LEFT JOIN miami.sections s ON sch.crn = s.crn AND t.id = s.term_id
        ON CONFLICT (section_id, start_date, end_date, start_time, end_time, days, schedule_type_description) DO NOTHING
        ";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var createStage = new NpgsqlCommand(staging, connection, transaction))
        {
            await createStage.ExecuteNonQueryAsync();
        }

        await using (var writer = connection.BeginBinaryImport(copy))
        {
            foreach (var schedule in schedules)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(schedule.TermCode, NpgsqlDbType.Text);
                await writer.WriteAsync(schedule.TermStartDate, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(schedule.TermEndDate, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(schedule.Crn, NpgsqlDbType.Text);
                await writer.WriteAsync(schedule.BuildingCode, NpgsqlDbType.Text);
                await writer.WriteAsync(schedule.StartDate, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(schedule.EndDate, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(schedule.StartTime, NpgsqlDbType.Time);
                await writer.WriteAsync(schedule.EndTime, NpgsqlDbType.Time);
                await writer.WriteAsync(schedule.RoomNumber, NpgsqlDbType.Text);
                await writer.WriteAsync(schedule.Days, NpgsqlDbType.Text);
                await writer.WriteAsync(schedule.ScheduleTypeDescription, NpgsqlDbType.Text);
            }

            await writer.CompleteAsync();
        }

        await using (var mergeStage = new NpgsqlCommand(merge, connection, transaction))
        {
            await mergeStage.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task InsertInstructorSectionsAsync(List<MiamiInstructorSectionDto> instructorSections)
    {
        const string staging = @"
        CREATE TEMP TABLE staging_instructor_section(
            code TEXT NOT NULL,
            term_start_date TIMESTAMPTZ NOT NULL,
            term_end_date TIMESTAMPTZ NOT NULL,
            crn TEXT NOT NULL,
            instructor_unique_id TEXT NOT NULL,
            is_primary BOOLEAN NOT NULL
        ) ON COMMIT DROP;
        ";

        const string copy = @"
        COPY staging_instructor_section (
            code,
            term_start_date,
            term_end_date,
            crn,
            instructor_unique_id,
            is_primary
        )
        FROM STDIN (FORMAT BINARY)
        ";

        const string merge = @"
        INSERT INTO miami.instructor_section (
            instructor_id,
            section_id,
            is_primary
        )
        SELECT i.id,
            s.id,
            isb.is_primary
        FROM staging_instructor_section isb
        LEFT JOIN miami.terms t ON isb.code = t.code AND isb.term_start_date = t.start_date AND isb.term_end_date = t.end_date
        LEFT JOIN miami.sections s ON isb.crn = s.crn AND t.id = s.term_id
        LEFT JOIN miami.instructors i ON isb.instructor_unique_id = i.unique_id
        ON CONFLICT (instructor_id, section_id) DO UPDATE
        SET 
            is_primary = EXCLUDED.is_primary;
        
        ";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var createStage = new NpgsqlCommand(staging, connection, transaction))
        {
            await createStage.ExecuteNonQueryAsync();
        }

        await using (var writer = connection.BeginBinaryImport(copy))
        {
            foreach (var instructorSection in instructorSections)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(instructorSection.TermCode, NpgsqlDbType.Text);
                await writer.WriteAsync(instructorSection.TermStartDate, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(instructorSection.TermEndDate, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(instructorSection.Crn, NpgsqlDbType.Text);
                await writer.WriteAsync(instructorSection.InstructorUniqueId, NpgsqlDbType.Text);
                await writer.WriteAsync(instructorSection.IsPrimary, NpgsqlDbType.Boolean);
            }

            await writer.CompleteAsync();
        }

        await using (var mergeStage = new NpgsqlCommand(merge, connection, transaction))
        {
            await mergeStage.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
}