namespace TaskTrack.AppServer.Controllers;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaskTrack.Shared.Models;
using TaskTrack.Shared.Repositories;

[Route("calendar")]
public class CalendarController : Controller
{
    [HttpPost("new")]
    public async Task<IActionResult> AddCalendar([FromServices] IUserCalendarRepository usercalendarRepository, [FromBody] NewUserCalendar request)
    {
        Guid newCalendarId;
        try
        {
            newCalendarId = await usercalendarRepository.InsertAsync(User, request);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
        var createdCalendar = new
        {
            id = newCalendarId,
            name = request.CalendarName,
            icsUrl = request.CalendarIcsUrl
        };

        return CreatedAtAction(nameof(GetCalendarByPublicId), new { publicId = newCalendarId }, createdCalendar);
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetCalendarByPublicId([FromServices] IUserCalendarRepository usercalendarRepository, Guid publicId)
    {
        var calendar = await usercalendarRepository.GetByPublicIdAsync(User, publicId);
        if (calendar == null)
            return NotFound();
        return Ok(calendar);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetCalendars([FromServices] IUserCalendarRepository usercalendarRepository)
    {
        var calendars = await usercalendarRepository.AllAsync(User);
        Console.WriteLine(JsonSerializer.Serialize(calendars));
        return Ok(JsonSerializer.Serialize(calendars));
    }
}
