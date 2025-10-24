namespace TaskTrack.CalendarService.Jobs;

using System.Linq.Expressions;
using Hangfire;

public class HangfireJobScheduler : IJobScheduler
{
    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        return BackgroundJob.Enqueue<T>(methodCall);
    }
}