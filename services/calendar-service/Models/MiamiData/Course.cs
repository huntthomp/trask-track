namespace TaskTrack.CalendarService.Models;

public class Course
{
    public required string SchoolName { get; set; }
    public required string DepartmentName { get; set; }
    public required string Title { get; set; }
    public required string SubjectCode { get; set; }
    public required string SubjectDescription { get; set; }
    public required string CourseNumber { get; set; }
    public required int CreditHoursHigh { get; set; }
    public required int CreditHoursLow { get; set; }
    public required string? Description { get; set; }
}