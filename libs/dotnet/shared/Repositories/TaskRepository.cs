namespace TaskTrack.Shared.Repositories;

using Npgsql;
using TaskTrack.Shared.Models;
using NpgsqlTypes;

public interface ITaskRepository
{
    Task InsertTaskGroupAsync(int userId, List<NewTaskGroup> taskGroups);
    Task InsertAsync(int userId, List<NewTask> tasks);
}

public class TaskRepository : ITaskRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public TaskRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task InsertTaskGroupAsync(int userId, List<NewTaskGroup> taskGroups)
    {
        const string staging = @"
        CREATE TEMP TABLE staging_task_groups (
            calendar_id BIGINT NOT NULL,
            group_name TEXT NOT NULL,
            metadata TEXT NOT NULL
        ) ON COMMIT DROP;
        ";

        const string copy = @"
        COPY staging_task_groups (calendar_id, group_name, metadata)
        FROM STDIN (FORMAT BINARY)
        ";

        const string merge = @"
        INSERT INTO user_data.task_groups(
            calendar_id,
            group_name,
            metadata
        )
        SELECT calendar_id,
            group_name,
            metadata::json
        FROM staging_task_groups ON CONFLICT (calendar_id, group_name) DO NOTHING;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await Properties.SetTenantWithUserId(connection, userId);

        await using (var createStage = new NpgsqlCommand(staging, connection, transaction))
        {
            await createStage.ExecuteNonQueryAsync();
        }

        await using (var writer = connection.BeginBinaryImport(copy))
        {
            foreach (var taskGroup in taskGroups)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(taskGroup.CalendarId, NpgsqlDbType.Bigint);
                await writer.WriteAsync(taskGroup.GroupName, NpgsqlDbType.Text);
                await writer.WriteAsync(taskGroup.Metadata, NpgsqlDbType.Text);
            }

            await writer.CompleteAsync();
        }

        await using (var mergeStage = new NpgsqlCommand(merge, connection, transaction))
        {
            await mergeStage.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }


    public async Task InsertAsync(int userId, List<NewTask> tasks)
    {
        const string staging = @"
        CREATE TEMP TABLE staging_tasks(
            calendar_id BIGINT NOT NULL,
            group_name TEXT,
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
        COPY staging_tasks (calendar_id, group_name, ics_event_id, course_id, assignment_id, description, summary, url, due_date)
        FROM STDIN (FORMAT BINARY)
        ";

        const string merge = @"
        INSERT INTO user_data.tasks(
            task_group_id,
            ics_event_id,
            course_id,
            assignment_id,
            description,
            summary,
            url,
            due_date
        )
        SELECT tg.id,
            ics_event_id,
            course_id,
            assignment_id,
            description,
            summary,
            url,
            due_date
        FROM staging_tasks t
        LEFT JOIN user_data.task_groups tg ON tg.calendar_id = t.calendar_id AND tg.group_name = t.group_name
        ON CONFLICT (task_group_id, ics_event_id, course_id, assignment_id) DO NOTHING;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await Properties.SetTenantWithUserId(connection, userId);

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
                await writer.WriteAsync(task.GroupName, NpgsqlDbType.Text);
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