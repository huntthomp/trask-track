namespace TaskTrack.CalendarService.Jobs;

using System.Security.Claims;
using TaskTrack.Shared.Repositories;

public class CalendarJobScheduler
{
    private readonly IUserCalendarRepository _userCalendarRepository;

    public CalendarJobScheduler(
        IUserCalendarRepository userCalendarRepository
    )
    {
        _userCalendarRepository = userCalendarRepository;
    }

    public async Task ScheduleAsync()
    {
        var calendars = await _userCalendarRepository.AllAsync(new ClaimsPrincipal());
    }
}