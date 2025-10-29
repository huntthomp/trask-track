namespace TaskTrack.CalendarService.Models;

public class NewSection
{
    // For joining with course on merge
    public required string SubjectCode { get; set; }
    public required string CourseNumber { get; set; }
    // For joining with term on merge
    public required string TermCode { get; set; }
    public required DateTime TermStartDate { get; set; }
    public required DateTime TermEndDate { get; set; }
    // Data
    public required string Crn { get; set; }
    public required Guid SectionUniqueId { get; set; }
    public required string SectionName { get; set; }
    public required string InstructionTypeDescription { get; set; }
    public required string CampusName { get; set; }
    public required bool FinalGradeRequired { get; set; }
    public required int MaxSeats { get; set; }
    public required int AvailableSeats { get; set; }
}