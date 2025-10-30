using System.Buffers.Text;
using System.Diagnostics;
using System.Net.Http.Json;
using TaskTrack.CalendarService.Models;
using TaskTrack.Shared.Models;
using TaskTrack.Shared.Repositories;

namespace TaskTrack.CalendarService.Jobs;

public interface IMiamiCourseDataService
{
    Task UpdateCoursesAsync(string termCode);
}

public class MiamiCourseDataService : IMiamiCourseDataService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IInternalTaskRepository _internalTaskRepository;
    private const string BaseUrl = "https://ws.apps.miamioh.edu/api/courseSection/v3/courseSection?compose=enrollmentCount,schedules,instructors&limit=100";

    public MiamiCourseDataService(
        IHttpClientFactory httpClientFactory,
        IInternalTaskRepository internalTaskRepository)
    {
        _httpClientFactory = httpClientFactory;
        _internalTaskRepository = internalTaskRepository;
    }

    public async Task UpdateCoursesAsync(string termCode)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var tag = $"course_update|{termCode}";

        var checkpoint = await _internalTaskRepository.GetCheckpointAsync(tag);

        string? nextUrl = checkpoint?.Next ?? BaseUrl + $"$term={termCode}";

        while (!string.IsNullOrEmpty(nextUrl))
        {
            MiamiCourseResponseDto response = await GetCourses(nextUrl, httpClient);

            await InsertCourses(response.Sections);

            if (response.NextUrl != null)
            {
                await _internalTaskRepository.SaveCheckpointAsync(new TaskCheckpoint() { Tag = tag, Current = nextUrl, Next = response.NextUrl });
            }

            nextUrl = response.NextUrl;
        }

        await _internalTaskRepository.DeleteCheckpointAsync(tag);
    }
    private async Task<MiamiCourseResponseDto> GetCourses(string url, HttpClient client)
    {
        var response = await client.GetFromJsonAsync<MiamiCourseResponseDto>(url);
        if (response == null || response.Status != 200) throw new Exception($"Failed getting course data {url}");
        return response;
    }
    private async Task InsertCourses(List<MiamiSectionDto> sections)
    {
        var instructors = sections
            .SelectMany(x => x.Instructors)
            .DistinctBy(x => x.Person.UniqueId)
            .ToList();

        var buildings = sections
            .SelectMany(x => x.Schedules)
            .Select(sch => new MiamiBuildingDto { BuildingCode = sch.BuildingCode, BuildingName = sch.BuildingName })
            .DistinctBy(x => x.BuildingCode)
            .ToList();

        var terms = sections
            .Select(x => x.PartOfTerm)
            .DistinctBy(x => x.Code)
            .ToList();

        var courses = sections
            .Select(x => x.Course)
            .GroupBy(x => (x.SubjectCode, x.Number))
            .Select(x => x.First())
            .ToList();


    }
}