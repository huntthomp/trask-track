using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using DotNetEnv;
using Npgsql;
using System.Net;
using TaskTrack.AppServer.Repositories;
using TaskTrack.Shared.Repositories;

var dotenvPath = Environment.GetEnvironmentVariable("DOTENV_PATH") ?? ".env";
Env.Load(dotenvPath);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions());

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://172.20.10.6:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;

})
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
    options.ResponseType = "code";

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.CallbackPath = "/callback";

    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            // Forces a login prompt instead of silent login
            context.ProtocolMessage.Prompt = "login";
            return Task.CompletedTask;
        }
    };

    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters.NameClaimType = "name";
});

// --- Database ---
builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
{
    var dsBuilder = new NpgsqlDataSourceBuilder(builder.Configuration["DefaultConnectionString"]);
    return dsBuilder.Build();
});

builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IUserCalendarRepository, UserCalendarRepository>();

// --- Controllers ---
builder.Services.AddControllers();

// --- Kestrel (HTTP only for dev) ---
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Parse("172.20.10.6"), 5001, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseStaticFiles();

// --- Middleware ---
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// --- API routes ---
app.MapControllers();

app.Run();
