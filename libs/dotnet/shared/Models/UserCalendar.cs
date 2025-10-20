namespace TaskTrack.Shared.Models;

public class UserCalendar
{
    public required Guid CalendarId { get; set; }
    public required string CalendarName { get; set; }
    public required string CalendarIcsUrl { get; set; }
    public required DateTime SyncedAt { get; set; }
    public required string Metadata { get; set; }
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