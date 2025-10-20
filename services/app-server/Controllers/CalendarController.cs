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

    [HttpPut("update")]
    public async Task<IActionResult> UpdateCalendar([FromServices] IUserCalendarRepository usercalendarRepository, [FromBody] UserCalendar request)
    {
        try
        {
            await usercalendarRepository.UpdateAsync(User, request);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        return Ok();
    }



    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetCalendarByPublicId([FromServices] IUserCalendarRepository usercalendarRepository, Guid publicId)
    {
        UserCalendar? calendar;
        try
        {
            calendar = await usercalendarRepository.GetByPublicIdAsync(User, publicId);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
            "An unexpected error occurred while retrieving the calendar.");
        }
        if (calendar == null)
            return NotFound();
        return Ok(calendar);
    }

    [HttpDelete("{publicId:guid}")]
    public async Task<IActionResult> DeleteCalendarByPublicId([FromServices] IUserCalendarRepository usercalendarRepository, Guid publicId)
    {
        try
        {
            await usercalendarRepository.DeleteAsync(User, publicId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(StatusCodes.Status500InternalServerError,
            "An unexpected error occurred while deleting the calendar.");
        }
        return Ok();
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetCalendars([FromServices] IUserCalendarRepository usercalendarRepository)
    {
        IEnumerable<UserCalendar> calendars = [];
        try
        {
            calendars = await usercalendarRepository.AllAsync(User);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
            "An unexpected error occurred while retrieving calendars.");
        }
        return Ok(JsonSerializer.Serialize(calendars));
    }
}
