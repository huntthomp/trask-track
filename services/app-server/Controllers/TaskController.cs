namespace TaskTrack.AppServer.Controllers;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TaskTrack.Shared.Exceptions;
using TaskTrack.Shared.Models;
using TaskTrack.Shared.Repositories;

[Route("tasks")]
public class TaskController : Controller
{
    [HttpGet("/all")]
    public async Task<IActionResult> GetTasks([FromServices] IUserCalendarRepository usercalendarRepository, Guid publicId)
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
        catch
        {
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
