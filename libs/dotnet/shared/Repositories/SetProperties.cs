namespace TaskTrack.Shared.Repositories;

using System.Security.Claims;
using Npgsql;

public static class Properties
{
    public static async Task SetTenant(NpgsqlConnection connection, ClaimsPrincipal user)
    {
        await using var cmd = new NpgsqlCommand($"SET app.tenant = '{user.FindFirst(ClaimTypes.NameIdentifier)?.Value}';", connection);
        await cmd.ExecuteNonQueryAsync();
    }
}
