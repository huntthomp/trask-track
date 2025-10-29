using System.Text.RegularExpressions;
using Ical.Net;
using TaskTrack.CalendarService.Models;

namespace TaskTrack.CalendarService.Utils;

public static class ICSUtils
{
    // Only single tasks are kept
    // Recurring events & interval date events
    // to be implemented at a later date
    public static List<NewTask> ParseCalendar(long calendarId, string calendarData)
    {
        var calendar = Calendar.Load(calendarData);
        if (calendar == null) return [];

        var tasks = new List<NewTask>();

        foreach (var component in calendar.Events)
        {
            if (component.DtEnd == null || component.DtEnd == component.DtStart)
            {
                var url = component.Url?.ToString();
                var (courseId, assignmentId) = ExtractCanvasIds(url);

                tasks.Add(new NewTask
                {
                    CalendarId = calendarId,
                    IcsEventId = component.Uid,
                    CourseId = courseId,
                    AssignmentId = assignmentId,
                    Summary = component.Summary,
                    Description = component.Description,
                    Url = url,
                    DueDate = NormalizeToUtc(component.DtStart?.Value)
                });
            }
        }

        Console.WriteLine($"Found {tasks.Count} single-task events.");

        return tasks;
    }

    private static DateTime? NormalizeToUtc(DateTime? dateTime)
    {
        if (dateTime == null)
            return null;

        return dateTime.Value.Kind switch
        {
            DateTimeKind.Utc => dateTime.Value,
            DateTimeKind.Local => dateTime.Value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc),
            _ => dateTime.Value
        };
    }

    private static (string? CourseId, string? AssignmentId) ExtractCanvasIds(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return (null, null);

        string? courseId = null;
        string? assignmentId = null;

        var courseMatch = Regex.Match(url, @"course_(\d+)", RegexOptions.IgnoreCase);
        if (courseMatch.Success)
            courseId = courseMatch.Groups[1].Value;

        var assignmentMatch = Regex.Match(url, @"assignment_(\d+)", RegexOptions.IgnoreCase);
        if (assignmentMatch.Success)
            assignmentId = assignmentMatch.Groups[1].Value;

        return (courseId, assignmentId);
    }
}