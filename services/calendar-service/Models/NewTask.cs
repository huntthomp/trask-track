namespace TaskTrack.CalendarService.Models;

public class NewTask
{
    public long CalendarId { get; set; }
    public required string? IcsEventId { get; set; }
    public required string? CourseId { get; set; }
    public required string? AssignmentId { get; set; }
    public required string? Description { get; set; }
    public required string? Summary { get; set; }
    public required string? Url { get; set; }
    public required DateTime? DueDate { get; set; }
}