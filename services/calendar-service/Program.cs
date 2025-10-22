using DotNetEnv;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Minio;
using Npgsql;

var dotenvPath = Environment.GetEnvironmentVariable("DOTENV_PATH") ?? ".env";
Env.Load(dotenvPath);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire((sp, config) =>
{
    config.UsePostgreSqlStorage(options => { options.UseNpgsqlConnection(builder.Configuration["AdminConnectionString"]); });

    config.UseFilter(new AutomaticRetryAttribute
    {
        Attempts = 3,
        OnAttemptsExceeded = AttemptsExceededAction.Fail
    });
});
builder.Services.AddHangfireServer();

builder.Services.AddSingleton<IMinioClient>(_ =>
    (MinioClient)new MinioClient()
        .WithEndpoint("localhost:9000")
        .WithCredentials(builder.Configuration["Minio:Username"], builder.Configuration["Minio:Password"])
        .WithSSL(false)
        .Build()
);

builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
{
    var dsBuilder = new NpgsqlDataSourceBuilder(builder.Configuration["DefaultConnectionString"]);
    return dsBuilder.Build();
});

builder.Services.AddHttpClient("CalendarRequest", client =>
{
    client.DefaultRequestHeaders.Add("Accept", "text/calendar");
});


var host = builder.Build();
host.Run();
