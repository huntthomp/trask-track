namespace TaskTrack.CalendarService.Jobs;

using System.Security.Claims;
using TaskTrack.CalendarService.Utils;
using TaskTrack.Shared.Repositories;

public class CalendarJobDispatcher
{
    private readonly IUserCalendarRepository _userCalendarRepository;
    private readonly UserCalendarCache _userCalendarCache;
    private readonly IJobScheduler _jobScheduler;
    private readonly ICalendarSyncHandler _calendarSyncHandler;

    public CalendarJobDispatcher(
        IUserCalendarRepository userCalendarRepository,
        UserCalendarCache userCalendarCache,
        IJobScheduler jobScheduler,
        ICalendarSyncHandler calendarSyncHandler)
    {
        _userCalendarRepository = userCalendarRepository;
        _userCalendarCache = userCalendarCache;
        _calendarSyncHandler = calendarSyncHandler;
        _jobScheduler = jobScheduler;
    }

    public async Task ScheduleAsync()
    {
        var calendars = await _userCalendarRepository.AllAsync();
        _userCalendarCache.AddRange(calendars);

        foreach (var calendar in calendars)
        {
            _jobScheduler.Enqueue<ICalendarSyncHandler>(h => h.UpdateCalendarAsync(calendar.PublicId));
        }
    }
}