namespace TaskTrack.CalendarService.Jobs;

using System.Linq.Expressions;

public interface IJobScheduler
{
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);
}