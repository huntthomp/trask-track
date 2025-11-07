namespace TaskTrack.CalendarService.Jobs;

using System.Text.RegularExpressions;
using TaskTrack.CalendarService.Utils;
using TaskTrack.Shared.Models;
using TaskTrack.Shared.Repositories;

public interface ICalendarSyncHandler
{
    Task UpdateCalendarAsync(Guid calendarId);
}

public class CalendarSyncHandler : ICalendarSyncHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UserCalendarCache _calendarCahe;

    private readonly ITaskRepository _taskRepository;
    public CalendarSyncHandler(
        IHttpClientFactory httpClientFactory,
        UserCalendarCache calendarCache,
        ITaskRepository taskRepository)
    {
        _httpClientFactory = httpClientFactory;
        _calendarCahe = calendarCache;
        _taskRepository = taskRepository;
    }

    public async Task UpdateCalendarAsync(Guid publicId)
    {
        _calendarCahe.TryGet(publicId, out var calendar);
        if (calendar == null) throw new Exception("No calendar found in cache");

        string calendarData = await GetCalendar(calendar.CalendarIcsUrl);
        var (taskGroups, tasks) = ICSUtils.ParseCalendar(calendar.CalendarId, calendarData);

        await InsertAsync(calendar.UserId, taskGroups, tasks);
    }
    private async Task<string> GetCalendar(string url)
    {
        var httpClient = _httpClientFactory.CreateClient("CalendarRequest");
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
    private async Task InsertAsync(int userId, List<NewTaskGroup> taskGroups, List<NewTask> tasks)
    {
        await _taskRepository.InsertTaskGroupAsync(userId, taskGroups);
        await _taskRepository.InsertAsync(userId, tasks);
    }
}