namespace TaskTrack.Shared.Repositories;

using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;
using Npgsql;
using TaskTrack.Shared.Models;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;

public interface IUserCalendarRepository
{
    Task<Guid> InsertAsync(ClaimsPrincipal user, NewUserCalendar userCalendar);
    Task<UserCalendar?> GetByPublicIdAsync(ClaimsPrincipal user, Guid id);
    Task<IEnumerable<UserCalendar>> AllAsync(ClaimsPrincipal user);
    Task UpdateAsync(ClaimsPrincipal user, UserCalendar userCalendar);
    Task DeleteAsync(ClaimsPrincipal user, Guid id);
}

public class UserCalendarRepository : IUserCalendarRepository
{
    private readonly NpgsqlDataSource _dataSource;
    const string ColorPattern = @"^#?([0-9a-fA-F]{6}|[0-9a-fA-F]{3})$";
    const string IcsUrlPattern = "^https?:\\/\\/[^\\/\\s?#]+(?:\\/[^\\s?#]*)*\\.ics(?:\\?[^\\s#]*)?(?:#.*)?$";

    public UserCalendarRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Guid> InsertAsync(ClaimsPrincipal user, NewUserCalendar userCalendar)
    {
        if (string.IsNullOrWhiteSpace(userCalendar.Metadata.Color) || !Regex.IsMatch(userCalendar.Metadata.Color, ColorPattern))
        {
            throw new Exception("Invalid color provided");
        }
        if (string.IsNullOrWhiteSpace(userCalendar.CalendarIcsUrl) || !Regex.IsMatch(userCalendar.CalendarIcsUrl, IcsUrlPattern))
        {
            throw new Exception("Invalid calendar url provided");
        }

        await using var connection = await _dataSource.OpenConnectionAsync();

        await Properties.SetTenant(connection, user);

        const string sql = @"
        INSERT INTO user_data.calendars (user_id, calendar_name, calendar_ics_url, metadata)
        VALUES (
            (SELECT id FROM user_data.users WHERE auth0_user_id = current_setting('app.tenant')),
            @CalendarName,
            @CalendarIcsUrl,
            @Metadata::json
        )
        RETURNING public_id;
        ";

        var publicId = await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            CalendarName = userCalendar.CalendarName,
            CalendarIcsUrl = userCalendar.CalendarIcsUrl,
            Metadata = System.Text.Json.JsonSerializer.Serialize(userCalendar.Metadata),
        });

        return publicId;
    }

    public async Task<UserCalendar?> GetByPublicIdAsync(ClaimsPrincipal user, Guid id)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        await Properties.SetTenant(connection, user);

        const string sql = @"
        SELECT
            public_id AS CalendarId,
            calendar_name AS CalendarName,
            calendar_ics_url AS CalendarIcsUrl,
            synced_at AS SyncedAt,
            metadata AS Metadata
        FROM user_data.calendars
        WHERE public_id = @PublicId
        ";

        var rawCalendar = (await connection.QueryAsync<UserCalendarRaw>(sql, new
        {
            PublicId = id,
        })).FirstOrDefault();

        if (rawCalendar == null) return null;

        return new UserCalendar
        {
            CalendarId = rawCalendar.CalendarId,
            CalendarName = rawCalendar.CalendarName,
            CalendarIcsUrl = rawCalendar.CalendarIcsUrl,
            SyncedAt = rawCalendar.SyncedAt,
            Metadata = JsonConvert.DeserializeObject<CalendarMetadata>(rawCalendar.Metadata)!
        };
    }

    public async Task<IEnumerable<UserCalendar>> AllAsync(ClaimsPrincipal user)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        await Properties.SetTenant(connection, user);

        const string sql = @"
        SELECT
            public_id AS CalendarId,
            calendar_name AS CalendarName,
            calendar_ics_url AS CalendarIcsUrl,
            synced_at AS SyncedAt,
            metadata AS Metadata
        FROM user_data.calendars
        ";

        var raw = await connection.QueryAsync<UserCalendarRaw>(sql);
        var calendars = raw.Select(r => new UserCalendar
        {
            CalendarId = r.CalendarId,
            CalendarName = r.CalendarName,
            CalendarIcsUrl = r.CalendarIcsUrl,
            SyncedAt = r.SyncedAt,
            Metadata = JsonConvert.DeserializeObject<CalendarMetadata>(r.Metadata)!
        });

        return calendars;
    }

    public async Task UpdateAsync(ClaimsPrincipal user, UserCalendar userCalendar)
    {
        if (string.IsNullOrWhiteSpace(userCalendar.Metadata.Color) || !Regex.IsMatch(userCalendar.Metadata.Color, ColorPattern))
        {
            throw new Exception("Invalid color provided");
        }
        if (string.IsNullOrWhiteSpace(userCalendar.CalendarIcsUrl) || !Regex.IsMatch(userCalendar.CalendarIcsUrl, IcsUrlPattern))
        {
            throw new Exception("Invalid calendar url provided");
        }

        await using var connection = await _dataSource.OpenConnectionAsync();

        await Properties.SetTenant(connection, user);

        const string sql = @"
        UPDATE user_data.calendars
        SET calendar_name = @CalendarName,
            calendar_ics_url = @CalendarIcsUrl,
            metadata = @Metadata::json
        WHERE public_id = @PublicId;
        ";

        await connection.ExecuteAsync(sql, new
        {
            CalendarName = userCalendar.CalendarName,
            CalendarIcsUrl = userCalendar.CalendarIcsUrl,
            Metadata = System.Text.Json.JsonSerializer.Serialize(userCalendar.Metadata),
            PublicId = userCalendar.CalendarId,
        });
    }

    public async Task DeleteAsync(ClaimsPrincipal user, Guid id)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        await Properties.SetTenant(connection, user);

        const string sql = @"
        DELETE FROM user_data.calendars
        WHERE public_id = @PublicId
        ";

        await connection.ExecuteAsync(sql, new
        {
            PublicId = id,
        });
    }


}