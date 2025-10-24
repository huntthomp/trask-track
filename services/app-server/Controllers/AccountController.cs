namespace TaskTrack.AppServer.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrack.AppServer.Repositories;

[Route("account")]
public class AccountController : Controller
{
    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        var redirectUrl = Url.Action("Callback", "Account", new { returnUrl });
        return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromServices] IUserRepository userRepository, string returnUrl = "/")
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
            return BadRequest("Unable to complete login. Error code 1");

        try
        {
            await userRepository.InsertAsync(authenticateResult.Principal);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return BadRequest("Unable to complete login. Error code 2");
        }

        return Redirect(returnUrl);
    }

    [Authorize]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout(string returnUrl = "/")
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties
            {
                RedirectUri = returnUrl
            });
        return Redirect(returnUrl);
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }
        return Json(null);
    }
}
