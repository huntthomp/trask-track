using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Minio;
using Npgsql;
using TaskTrack.CalendarService.Jobs;
using TaskTrack.CalendarService.Repositories;
using TaskTrack.CalendarService.Utils;
using TaskTrack.Shared.Repositories;

string postgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "";
string postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "";
string postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "";
string postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "";
string minioRootUser = Environment.GetEnvironmentVariable("MINIO_ROOT_USER") ?? "";
string minioRootPassword = Environment.GetEnvironmentVariable("MINIO_ROOT_PASSWORD") ?? "";

string adminConnectionString = $"Host=postgres;Port={postgresPort};Username={postgresUser};Password={postgresPassword};Database={postgresDb};";

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

if (env.IsDevelopment())
{
    adminConnectionString += "Include Error Detail=true;";
}

// Hangfire Config 
builder.Services.AddHangfire((sp, config) =>
{
    config.UsePostgreSqlStorage(options => { options.UseNpgsqlConnection(adminConnectionString); });

    config.UseFilter(new AutomaticRetryAttribute
    {
        Attempts = 0,
        OnAttemptsExceeded = AttemptsExceededAction.Fail
    });
});
builder.Services.AddHangfireServer();

// Minio Config
builder.Services.AddSingleton<IMinioClient>(_ =>
    (MinioClient)new MinioClient()
        .WithEndpoint("localhost:9000")
        .WithCredentials(minioRootUser, minioRootPassword)
        .WithSSL(false)
        .Build()
);

// Dependecy Injection Config
builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
{
    var dsBuilder = new NpgsqlDataSourceBuilder(adminConnectionString);
    return dsBuilder.Build();
});

builder.Services.AddHttpClient("CalendarRequest", client =>
{
    client.DefaultRequestHeaders.Add("Accept", "text/calendar");
});

builder.Services.AddSingleton<ICalendarSyncHandler, CalendarSyncHandler>();
builder.Services.AddSingleton<IJobScheduler, HangfireJobScheduler>();
builder.Services.AddSingleton<IUserCalendarRepository, UserCalendarRepository>();
builder.Services.AddSingleton<ITaskRepository, TaskRepository>();
builder.Services.AddSingleton<IInternalTaskRepository, InternalTaskRepository>();
builder.Services.AddSingleton<IMiamiCourseInfoRepository, MiamiCourseInfoRepository>();
builder.Services.AddSingleton<UserCalendarCache>();

// Application Setup
var app = builder.Build();

// Hanfgire steup
app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
    {
        RequireSsl = false,
        SslRedirect = false,
        LoginCaseSensitive = true,
        Users = new[]
        {
            new BasicAuthAuthorizationUser
            {
                Login = "hangfireuser",
                PasswordClear = "hangfirepass"
            }
        }
    })}
});
RecurringJob.AddOrUpdate<CalendarJobDispatcher>(
    "calendar-sync-scheduler",
    job => job.ScheduleAsync(),
    Cron.Never
);

RecurringJob.AddOrUpdate<MiamiCourseDataService>(
    "miami-course-sync-fall",
    job => job.UpdateCoursesAsync("202610"),
    Cron.Never
);
RecurringJob.AddOrUpdate<MiamiCourseDataService>(
    "miami-course-sync-winter",
    job => job.UpdateCoursesAsync("202615"),
    Cron.Never
);
RecurringJob.AddOrUpdate<MiamiCourseDataService>(
    "miami-course-sync-spring",
    job => job.UpdateCoursesAsync("202620"),
    Cron.Never
);
RecurringJob.AddOrUpdate<MiamiCourseDataService>(
    "miami-course-sync-summer",
    job => job.UpdateCoursesAsync("202630"),
    Cron.Never
);

app.MapGet("/", () => "Calendar Service Healthy");

await app.RunAsync();
