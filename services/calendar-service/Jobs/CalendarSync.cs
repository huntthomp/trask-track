namespace TaskTrack.CalendarService.Jobs;

using TaskTrack.CalendarService.Models;
using TaskTrack.CalendarService.Repositories;
using TaskTrack.CalendarService.Utils;
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
        var tasks = ICSUtils.ParseCalendar(calendar.CalendarId, calendarData);

        await InsertAsync(tasks);
    }
    private async Task<string> GetCalendar(string url)
    {
        var httpClient = _httpClientFactory.CreateClient("CalendarRequest");
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
    private async Task InsertAsync(List<NewTask> tasks)
    {
        await _taskRepository.InsertAsync(tasks);
    }
}