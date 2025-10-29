namespace TaskTrack.CalendarService.Utils;

using System.Collections.Concurrent;
using TaskTrack.Shared.Models;

public class UserCalendarCache
{
    private readonly ConcurrentDictionary<Guid, UserCalendar> _cache = new();

    public void Add(UserCalendar job)
    {
        _cache[job.PublicId] = job;
    }

    public void AddRange(IEnumerable<UserCalendar> jobs)
    {
        foreach (var job in jobs) _cache[job.PublicId] = job;
    }

    public bool TryGet(Guid id, out UserCalendar? job)
    {
        return _cache.TryGetValue(id, out job);
    }

    public void Remove(Guid id)
    {
        _cache.TryRemove(id, out _);
    }
}