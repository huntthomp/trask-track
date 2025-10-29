namespace TaskTrack.CalendarService.Models;

public class InstructorSection
{
    // Instructor join on merge
    public required string InstructorUniqueId { get; set; }
    // Term join on merge
    public required string Crn { get; set; }
    public required string TermCode { get; set; }
    public required DateTime TermStartDate { get; set; }
    public required DateTime TermEndDate { get; set; }
}