using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Minio;
using Npgsql;
using TaskTrack.CalendarService.Jobs;
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
builder.Services.AddSingleton<UserCalendarCache>();

// Application Setup
var app = builder.Build();

// Hanfgire steup
app.UseHangfireDashboard("/jobs");
RecurringJob.AddOrUpdate<CalendarJobDispatcher>(
    "calendar-sync-scheduler",
    job => job.ScheduleAsync(),
    Cron.Never
);

app.MapGet("/", () => "Application Healthy");

await app.RunAsync();
