using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Npgsql;
using TaskTrack.AppServer.Repositories;
using TaskTrack.Shared.Repositories;

string auth0Domain = Environment.GetEnvironmentVariable("AUTH0_DOMAIN") ?? "";
string auth0ClientId = Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID") ?? "";
string auth0ClientSecret = Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET") ?? "";
string postgresDefaultUser = Environment.GetEnvironmentVariable("POSTGRES_DEFAULT_USER") ?? "";
string postgresDefaultPassword = Environment.GetEnvironmentVariable("POSTGRES_DEFAULT_PASSWORD") ?? "";
string postgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "";
string postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "";
string corsAllow = Environment.GetEnvironmentVariable("CORS_ALLOW") ?? "";

string defaultConnectionString = $"Host=postgres;Port={postgresPort};Username={postgresDefaultUser};Password={postgresDefaultPassword};Database={postgresDb};";

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

if (env.IsDevelopment())
{
    defaultConnectionString += "Include Error Detail=true;";
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCors", policy =>
    {
        policy.WithOrigins(corsAllow)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Auth & OIDC Config
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "tasktrack.auth";
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);

    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.Domain = null;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    }
    else
    {
        options.Cookie.Domain = ".hlthompson.dev";
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }
})
.AddOpenIdConnect(options =>
{
    options.Authority = auth0Domain;
    options.ClientId = auth0ClientId;
    options.ClientSecret = auth0ClientSecret;
    options.ResponseType = "code";

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.CallbackPath = "/callback";
    options.RequireHttpsMetadata = false;
    options.SaveTokens = true;

    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            context.ProtocolMessage.Prompt = "login";
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters.NameClaimType = "name";
});

// Dependecy Injection Config
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
{
    var dsBuilder = new NpgsqlDataSourceBuilder(defaultConnectionString);
    return dsBuilder.Build();
});

builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IUserCalendarRepository, UserCalendarRepository>();
builder.Services.AddControllers();

// Application Setup
var app = builder.Build();

app.UseForwardedHeaders();
app.UseCors("AllowCors");
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Application Healthy");

app.Run();
