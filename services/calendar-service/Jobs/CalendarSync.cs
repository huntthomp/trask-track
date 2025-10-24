namespace TaskTrack.CalendarService.Jobs;

using TaskTrack.CalendarService.Utils;

public interface ICalendarSyncHandler
{
    Task UpdateCalendarAsync(Guid calendarId);
}

public class CalendarSyncHandler : ICalendarSyncHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly UserCalendarCache _calendarCahe;

    public CalendarSyncHandler(
        IHttpClientFactory httpClientFactory,
        UserCalendarCache calendarCache)
    {
        _httpClientFactory = httpClientFactory;
        _calendarCahe = calendarCache;
    }

    public async Task UpdateCalendarAsync(Guid calendarId)
    {
        _calendarCahe.TryGet(calendarId, out var calendar);
        if (calendar == null) throw new Exception("No calendar found in cache");
        await GetCalendar(calendar.CalendarIcsUrl);

    }
    private async Task GetCalendar(string url)
    {
        var httpClient = _httpClientFactory.CreateClient("CalendarRequest");
        var response = await httpClient.GetAsync(url);
        var calendar = await response.Content.ReadAsStringAsync();

        Console.WriteLine(calendar);
    }
}