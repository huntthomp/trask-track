namespace TaskTrack.Shared.Repositories;

using System.Security.Claims;
using Npgsql;

public static class Properties
{
    public static async Task SetTenant(NpgsqlConnection connection, ClaimsPrincipal user)
    {
        var tenantId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? throw new InvalidOperationException("User has no NameIdentifier claim");

        if (!System.Text.RegularExpressions.Regex.IsMatch(tenantId, @"^[a-zA-Z0-9|_\-]+$"))
            throw new InvalidOperationException("Invalid tenant ID format");

        var escapedTenant = tenantId.Replace("'", "''");

        var sql = $"SET app.tenant = '{escapedTenant}';";

        await using var cmd = new NpgsqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync();
    }
}
