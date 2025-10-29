namespace TaskTrack.CalendarService.Models;

public class Instructor
{
    public required string UniqueId { get; set; }
    public required string LastName { get; set; }
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public string? PreferredFirstName { get; set; }
}