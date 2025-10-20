namespace TaskTrack.Shared.Models;


public class UserCalendarRaw
{
    public required Guid CalendarId { get; set; }
    public required string CalendarName { get; set; }
    public required string CalendarIcsUrl { get; set; }
    public DateTime? SyncedAt { get; set; } = null;
    public required string Metadata { get; set; }
}
public class UserCalendar
{
    public required Guid CalendarId { get; set; }
    public required string CalendarName { get; set; }
    public required string CalendarIcsUrl { get; set; }
    public DateTime? SyncedAt { get; set; } = null;
    public required CalendarMetadata Metadata { get; set; }
}

public class CalendarMetadata
{
    public required string Color { get; set; }
}

public class NewUserCalendar
{
    public required string CalendarName { get; set; }
    public required string CalendarIcsUrl { get; set; }
    public required CalendarMetadata Metadata { get; set; }
}