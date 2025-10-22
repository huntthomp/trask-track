using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Npgsql;
using TaskTrack.AppServer.Repositories;
using TaskTrack.Shared.Repositories;

string defaultConnectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING") ?? "";
string adminConnectionString = Environment.GetEnvironmentVariable("ADMIN_CONNECTION_STRING") ?? "";
string auth0Domain = Environment.GetEnvironmentVariable("AUTH0_DOMAIN") ?? "";
string auth0ClientId = Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID") ?? "";
string auth0ClientSecret = Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET") ?? "";
string frontendOrigin = Environment.GetEnvironmentVariable("FRONTEND_ORIGIN") ?? "";

var builder = WebApplication.CreateBuilder(args);

// Determine environment
var env = builder.Environment;

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigin = frontendOrigin;
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// --- Authentication ---
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
    options.Authority = auth0Domain;
    options.ClientId = auth0ClientId;
    options.ClientSecret = auth0ClientSecret;
    options.ResponseType = "code";
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.CallbackPath = "/callback";

    options.RequireHttpsMetadata = !env.IsDevelopment();

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

// --- Database ---
builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
{
    var dsBuilder = new NpgsqlDataSourceBuilder(defaultConnectionString);
    return dsBuilder.Build();
});

builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IUserCalendarRepository, UserCalendarRepository>();
builder.Services.AddControllers();

// --- Kestrel Configuration ---
/* builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 8081, listenOptions =>
    {
        listenOptions.UseHttps();
    });
}); */

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
