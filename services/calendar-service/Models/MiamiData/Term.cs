namespace TaskTrack.CalendarService.Models;

public class Term
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
}