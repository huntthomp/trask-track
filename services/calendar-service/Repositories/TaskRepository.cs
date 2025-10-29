namespace TaskTrack.Shared.Repositories;

using Npgsql;
using TaskTrack.CalendarService.Models;
using NpgsqlTypes;

public interface ITaskRepository
{
    Task InsertAsync(List<NewTask> tasks);
}

public class TaskRepository : ITaskRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public TaskRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task InsertAsync(List<NewTask> tasks)
    {
        const string staging = @"
        CREATE TEMP TABLE staging_tasks(
            calendar_id BIGINT NOT NULL,
            ics_event_id TEXT NOT NULL,
            course_id TEXT,
            assignment_id TEXT,
            description TEXT,
            summary TEXT,
            url TEXT,
            due_date TIMESTAMPTZ
        ) ON COMMIT DROP;
        ";

        const string copy = @"
        COPY staging_tasks (calendar_id, ics_event_id, course_id, assignment_id, description, summary, url, due_date)
        FROM STDIN (FORMAT BINARY)
        ";

        const string merge = @"
        INSERT INTO user_data.tasks(
            calendar_id,
            ics_event_id,
            course_id,
            assignment_id,
            description,
            summary,
            url,
            due_date
        )
        SELECT calendar_id,
            ics_event_id,
            course_id,
            assignment_id,
            description,
            summary,
            url,
            due_date
        FROM staging_tasks ON CONFLICT (calendar_id, ics_event_id, course_id, assignment_id) DO NOTHING;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var createStage = new NpgsqlCommand(staging, connection, transaction))
        {
            await createStage.ExecuteNonQueryAsync();
        }

        await using (var writer = connection.BeginBinaryImport(copy))
        {
            foreach (var task in tasks)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(task.CalendarId, NpgsqlDbType.Bigint);
                await writer.WriteAsync(task.IcsEventId, NpgsqlDbType.Text);
                await writer.WriteAsync(task.CourseId, NpgsqlDbType.Text);
                await writer.WriteAsync(task.AssignmentId, NpgsqlDbType.Text);
                await writer.WriteAsync(task.Description, NpgsqlDbType.Text);
                await writer.WriteAsync(task.Summary, NpgsqlDbType.Text);
                await writer.WriteAsync(task.Url, NpgsqlDbType.Text);
                await writer.WriteAsync(task.DueDate, NpgsqlDbType.TimestampTz);
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