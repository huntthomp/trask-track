using System.Buffers.Text;
using System.Diagnostics;
using System.Net.Http.Json;
using NodaTime;
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
    private readonly IMiamiCourseInfoRepository _miamiCourseInfoRepository;
    private const string BaseUrl = "https://ws.apps.miamioh.edu/api/courseSection/v3/courseSection?compose=enrollmentCount,schedules,instructors";

    private const int PageCount = 100;
    public MiamiCourseDataService(
        IHttpClientFactory httpClientFactory,
        IInternalTaskRepository internalTaskRepository,
        IMiamiCourseInfoRepository miamiCourseInfoRepository)
    {
        _httpClientFactory = httpClientFactory;
        _internalTaskRepository = internalTaskRepository;
        _miamiCourseInfoRepository = miamiCourseInfoRepository;
    }

    public async Task UpdateCoursesAsync(string termCode)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var tag = $"course_update|{termCode}";

        var checkpoint = await _internalTaskRepository.GetCheckpointAsync(tag);
        var offset = checkpoint?.Next ?? "0";

        while (true)
        {
            string url;
            // 500 Response code if "offset=0" is a query parameter
            if (offset == null || offset == "0")
            {
                url = $"{BaseUrl}&limit={PageCount}&termCode={termCode}";
            }
            else
            {
                url = $"{BaseUrl}&limit={PageCount}&termCode={termCode}&offset={offset}";
            }
            Console.WriteLine(url);

            MiamiCourseResponseDto? response;

            try
            {
                response = await GetCourses(url, httpClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch data for offset {offset}: {ex.Message}");
                await _internalTaskRepository.SaveCheckpointAsync(new TaskCheckpoint
                {
                    Tag = tag,
                    Next = offset ?? "0",
                    Current = offset ?? "0"
                });
                throw;
            }

            if (response?.Sections is null || response.Sections.Count == 0)
            {
                Console.WriteLine("No more sections found, finishing.");
                break;
            }

            await InsertCourses(response.Sections);

            if (response.NextUrl == null)
            {
                Console.WriteLine("Reached end of data for term.");
                break;
            }

            await Task.Delay(TimeSpan.FromSeconds(10));

            offset = (int.Parse(offset!) + PageCount).ToString();

            await _internalTaskRepository.SaveCheckpointAsync(new TaskCheckpoint
            {
                Tag = tag,
                Next = offset,
                Current = offset
            });
        }

        await _internalTaskRepository.DeleteCheckpointAsync(tag);
    }
    private async Task<MiamiCourseResponseDto> GetCourses(string url, HttpClient client)
    {
        MiamiCourseResponseDto? courseData = null;
        try
        {
            courseData = await client.GetFromJsonAsync<MiamiCourseResponseDto>(url);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
        return courseData ?? throw new Exception("Error parsing course response");
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
            .Where(x => x.BuildingCode != null)
            .ToList();

        var terms = sections
            .Select(x => x.PartOfTerm)
            .GroupBy(x => (x.Code, x.StartDate, x.EndDate))
            .Select(x => x.First())
            .ToList();

        var courses = sections
            .Select(x => x.Course)
            .GroupBy(x => (x.SubjectCode, x.Number))
            .Select(x => x.First())
            .ToList();

        var schedules = sections
        .Where(s => s.Schedules != null && s.Schedules.Any())
        .SelectMany(section => section.Schedules.Select(schedule => new MiamiScheduleInsertDto
        {
            TermCode = section.PartOfTerm.Code,
            TermStartDate = section.PartOfTerm.StartDate,
            TermEndDate = section.PartOfTerm.EndDate,
            Crn = section.Crn,
            BuildingCode = schedule.BuildingCode,
            BuildingName = schedule.BuildingName,
            StartDate = schedule.StartDate,
            EndDate = schedule.EndDate,
            StartTime = TimeOnly.TryParse(schedule.StartTime, out var startTime) ? startTime : null,
            EndTime = TimeOnly.TryParse(schedule.EndTime, out var endTime) ? endTime : null,
            RoomNumber = schedule.RoomNumber,
            Days = schedule.Days,
            ScheduleTypeDescription = schedule.ScheduleTypeDescription
        }))
        .ToList();

        var instructorSections = sections
        .Where(s => s.Instructors != null && s.Instructors.Any())
        .SelectMany(section => section.Instructors.Select(instructor => new MiamiInstructorSectionDto
        {
            TermCode = section.PartOfTerm.Code,
            TermStartDate = section.PartOfTerm.StartDate,
            TermEndDate = section.PartOfTerm.EndDate,
            Crn = section.Crn,
            InstructorUniqueId = instructor.Person.UniqueId!,
            IsPrimary = instructor.IsPrimary
        }))
        .ToList();

        await _miamiCourseInfoRepository.InsertInstructorsAsync(instructors);
        await _miamiCourseInfoRepository.InsertBuildingsAsync(buildings);
        await _miamiCourseInfoRepository.InsertTermsAsync(terms);
        await _miamiCourseInfoRepository.InsertCoursesAsync(courses);
        await _miamiCourseInfoRepository.InsertSectionsAsync(sections);
        await _miamiCourseInfoRepository.InsertSchedulesAsync(schedules);
        await _miamiCourseInfoRepository.InsertInstructorSectionsAsync(instructorSections);

    }
}