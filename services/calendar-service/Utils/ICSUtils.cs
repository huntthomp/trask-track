using System.Text.RegularExpressions;
using Ical.Net;
using TaskTrack.Shared.Models;

namespace TaskTrack.CalendarService.Utils;

public static class ICSUtils
{
    const string GroupNameMatch = "\\[(?!.*\\[)([^]]+)\\]$";
    // Only single tasks are kept
    // Recurring events & interval date events
    // to be implemented at a later date
    public static (List<NewTaskGroup>, List<NewTask>) ParseCalendar(long calendarId, string calendarData)
    {
        var calendar = Calendar.Load(calendarData);
        if (calendar == null) return ([], []);

        var tasks = new List<NewTask>();

        foreach (var component in calendar.Events)
        {
            if (component.DtEnd == null || component.DtEnd == component.DtStart)
            {
                var url = component.Url?.ToString();
                var (courseId, assignmentId) = ExtractCanvasIds(url);

                var regexMatch = Regex.Match(component.Summary ?? "", GroupNameMatch, RegexOptions.IgnoreCase);

                tasks.Add(new NewTask
                {
                    CalendarId = calendarId,
                    GroupName = regexMatch.Groups[1].Value,
                    IcsEventId = component.Uid,
                    CourseId = courseId,
                    AssignmentId = assignmentId,
                    Summary = string.IsNullOrEmpty(regexMatch.Groups[1].Value)
                            ? component.Summary
                            : component.Summary?.Substring(0, regexMatch.Index).TrimEnd(),
                    Description = component.Description,
                    Url = url,
                    DueDate = NormalizeToUtc(component.DtStart?.Value)
                });
            }
        }

        var taskGroups = tasks.Select(task => new NewTaskGroup()
        {
            CalendarId = calendarId,
            GroupName = task.GroupName,
            Metadata = ColorUtils.GetRandomColorMetadata(),

        })
        .Where(task => !string.IsNullOrEmpty(task.GroupName))
        .ToList();

        return (taskGroups, tasks);
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