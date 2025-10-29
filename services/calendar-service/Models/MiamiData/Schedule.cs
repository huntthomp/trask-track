namespace TaskTrack.CalendarService.Models;

public class Schedule
{
    // For joining with section on merge
    public required string Crn { get; set; }
    public required string TermCode { get; set; }
    public required DateTime TermStartDate { get; set; }
    public required DateTime TermEndDate { get; set; }
    // For joining with building on merge
    public required string BuildingCode { get; set; }
    // Data
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public required string RoomNumber { get; set; }
    public required string Days { get; set; }
    public required string ScheduleTypeDescription { get; set; }
}