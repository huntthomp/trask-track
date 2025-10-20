using DotNetEnv;
using TaskTrack.CalenderService;

var dotenvPath = Environment.GetEnvironmentVariable("DOTENV_PATH") ?? ".env";
Env.Load(dotenvPath);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
