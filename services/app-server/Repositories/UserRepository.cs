namespace TaskTrack.AppServer.Repositories;

using System.Security.Claims;
using Dapper;
using Npgsql;
using TaskTrack.Shared.Repositories;

public interface IUserRepository
{
    Task InsertAsync(ClaimsPrincipal user);
}

public class UserRepository : IUserRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UserRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task InsertAsync(ClaimsPrincipal user)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        await Properties.SetTenant(connection, user);

        const string sql = @"INSERT INTO user_data.users (auth0_user_id)
                            VALUES (current_setting('app.tenant'))
                            ON CONFLICT DO NOTHING;";


        await connection.ExecuteAsync(sql);
    }
}