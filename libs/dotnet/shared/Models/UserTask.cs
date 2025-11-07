namespace TaskTrack.Shared.Models;

public class NewTask
{
    public long CalendarId { get; set; }
    public string? GroupName { get; set; }
    public string? IcsEventId { get; set; }
    public string? CourseId { get; set; }
    public string? AssignmentId { get; set; }
    public string? Description { get; set; }
    public string? Summary { get; set; }
    public string? Url { get; set; }
    public DateTime? DueDate { get; set; }
}

public class NewTaskGroup
{
    public required long CalendarId { get; set; }
    public string? GroupName { get; set; }
    public required string Metadata { get; set; }
}

public class UserTask
{

}