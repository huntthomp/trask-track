namespace TaskTrack.Shared.Repositories;

using System.Formats.Asn1;
using Dapper;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql;
using TaskTrack.Shared.Models;

public interface IInternalTaskRepository
{
    Task<TaskCheckpoint?> GetCheckpointAsync(string tag);
    Task SaveCheckpointAsync(TaskCheckpoint checkpoint);
    Task DeleteCheckpointAsync(string tag);
}

public class InternalTaskRepository : IInternalTaskRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public InternalTaskRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<TaskCheckpoint?> GetCheckpointAsync(string tag)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        const string sql = @"
        SELECT tag AS Tag,
        next AS Next,
        current AS Current
        FROM internal.task_checkpoints
        WHERE tag = @QueryTag
        LIMIT 1;
        ";

        return await connection.QueryFirstOrDefaultAsync<TaskCheckpoint>(sql, new { QueryTag = tag });
    }
    public async Task SaveCheckpointAsync(TaskCheckpoint checkpoint)
    {
        Console.WriteLine("saving");
        await using var connection = await _dataSource.OpenConnectionAsync();

        const string sql = @"
        INSERT INTO internal.task_checkpoints (tag, next, current, last_updated)
        VALUES (@Tag, @Next, @Current, now())
        ON CONFLICT (tag) DO UPDATE
        SET 
            next = EXCLUDED.next,
            current = EXCLUDED.current,
            last_updated = now();
        ";

        await connection.ExecuteAsync(sql, new { Tag = checkpoint.Tag, Next = checkpoint.Next, Current = checkpoint.Current });
    }
    public async Task DeleteCheckpointAsync(string tag)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        const string sql = @"
        DELETE FROM internal.task_checkpoints
        WHERE tag = @Tag
        ";

        await connection.ExecuteAsync(sql, new { Tag = tag });
    }
}