using System.Buffers.Text;
using System.Diagnostics;
using System.Net.Http.Json;
using TaskTrack.CalendarService.Models;

namespace TaskTrack.CalendarService.Jobs;

public interface IMiamiCourseDataService
{
    Task UpdateCoursesAsync(string termCode);
}

public class MiamiCourseDataService : IMiamiCourseDataService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string BaseUrl = "https://ws.apps.miamioh.edu/api/courseSection/v3/courseSection";
    //?limit=100&termCode=202630&compose=enrollmentCount,schedules,instructors
    private Dictionary<string, string> DefaultQueryParams = new Dictionary<string, string>
    {
        {"compose", "enrollmentCount,schedules,instructors"},
        {"limit", "100"},
    };
    public MiamiCourseDataService(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task UpdateCoursesAsync(string termCode)
    {
        /* var httpClient = _httpClientFactory.CreateClient();

        var checkpoint = await GetCheckpointAsync(termCode);

        string? nextUrl = checkpoint?.NextUrl ?? $"https://api.university.edu/courses?term={termCode}&page=1";
        int currentPage = checkpoint?.CurrentPage ?? 1;

        while (!string.IsNullOrEmpty(nextUrl))
        {
            MiamiCourseResponseDto response = await GetCourses(nextUrl, httpClient);

            nextUrl = response.NextUrl;

            await SaveCheckpointAsync(termCode, nextUrl, currentPage);
        }

        await DeleteCheckpointAsync(termCode); */
    }
    private async Task<MiamiCourseResponseDto> GetCourses(string url, HttpClient client)
    {
        var response = await client.GetFromJsonAsync<MiamiCourseResponseDto>(url);
        if (response == null || response.Status != 200) throw new Exception($"Failed getting course data {url}");
        return response;
    }
}