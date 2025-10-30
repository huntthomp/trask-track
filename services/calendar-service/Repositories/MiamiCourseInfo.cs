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
        FROM staging_buildings 
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
                await writer.WriteAsync(term.StartDate, NpgsqlDbType.TimestampTz);
                await writer.WriteAsync(term.EndDate, NpgsqlDbType.TimestampTz);
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
            credit_hours_low INT NOT NULL,
            credit_hours_high INT NOT NULL,
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
                await writer.WriteAsync(course.CreditHoursHigh, NpgsqlDbType.Integer);
                await writer.WriteAsync(course.CreditHoursLow, NpgsqlDbType.Integer);
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
}