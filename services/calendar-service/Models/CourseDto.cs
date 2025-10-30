namespace TaskTrack.CalendarService.Models;

using System.Text.Json.Serialization;

public class MiamiCourseResponseDto
{
    [JsonPropertyName("data")]
    public List<MiamiSectionDto> Sections { get; set; } = [];

    [JsonPropertyName("status")]
    public required int Status { get; set; }

    [JsonPropertyName("error")]
    public bool Error { get; set; }

    [JsonPropertyName("currentUrl")]
    public string? CurrentUrl { get; set; }

    [JsonPropertyName("firstUrl")]
    public string? FirstUrl { get; set; }

    [JsonPropertyName("lastUrl")]
    public string? LastUrl { get; set; }

    [JsonPropertyName("nextUrl")]
    public string? NextUrl { get; set; }

    [JsonPropertyName("prevUrl")]
    public string? PrevUrl { get; set; }

}

public class MiamiSectionDto
{
    [JsonPropertyName("termCode")]
    public required string TermCode { get; set; }

    [JsonPropertyName("termDescription")]
    public required string TermDescription { get; set; }

    [JsonPropertyName("crn")]
    public required string Crn { get; set; }

    [JsonPropertyName("courseSectionGuid")]
    public required Guid CourseSectionGuid { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("sectionName")]
    public required string SectionName { get; set; }

    [JsonPropertyName("instructionTypeCode")]
    public required string InstructionTypeCode { get; set; }

    [JsonPropertyName("instructionTypeDescription")]
    public required string InstructionTypeDescription { get; set; }

    [JsonPropertyName("courseSectionCode")]
    public required string CourseSectionCode { get; set; }

    [JsonPropertyName("courseSectionStatusCode")]
    public required string CourseSectionStatusCode { get; set; }

    [JsonPropertyName("courseSectionStatusDescription")]
    public required string CourseSectionStatusDescription { get; set; }

    [JsonPropertyName("campusCode")]
    public required string CampusCode { get; set; }

    [JsonPropertyName("campusName")]
    public required string CampusName { get; set; }

    [JsonPropertyName("creditHoursDescription")]
    public string? CreditHoursDescription { get; set; }

    [JsonPropertyName("creditHoursLow")]
    public int CreditHoursLow { get; set; }

    [JsonPropertyName("creditHoursHigh")]
    public int CreditHoursHigh { get; set; }

    [JsonPropertyName("isMidtermGradeSubmissionAvailable")]
    public bool IsMidtermGradeSubmissionAvailable { get; set; }

    [JsonPropertyName("isFinalGradeSubmissionAvailable")]
    public bool IsFinalGradeSubmissionAvailable { get; set; }

    [JsonPropertyName("isFinalGradeRequired")]
    public bool IsFinalGradeRequired { get; set; }

    [JsonPropertyName("standardizedDivisionCode")]
    public required string StandardizedDivisionCode { get; set; }

    [JsonPropertyName("standardizedDivisionName")]
    public required string StandardizedDivisionName { get; set; }

    [JsonPropertyName("standardizedDepartmentCode")]
    public required string StandardizedDepartmentCode { get; set; }

    [JsonPropertyName("standardizedDepartmentName")]
    public string? StandardizedDepartmentName { get; set; }

    [JsonPropertyName("legacyStandardizedDepartmentCode")]
    public string? LegacyStandardizedDepartmentCode { get; set; }

    [JsonPropertyName("legacyStandardizedDepartmentName")]
    public string? LegacyStandardizedDepartmentName { get; set; }

    [JsonPropertyName("creditHoursAvailable")]
    public List<int> CreditHoursAvailable { get; set; } = [];

    [JsonPropertyName("isDisplayed")]
    public bool IsDisplayed { get; set; }

    [JsonPropertyName("course")]
    public required MiamiCourseDto Course { get; set; }

    [JsonPropertyName("partOfTerm")]
    public required MiamiPartOfTermDto PartOfTerm { get; set; }

    [JsonPropertyName("enrollmentCount")]
    public required MiamiEnrollmentDto EnrollmentCount { get; set; }

    [JsonPropertyName("instructors")]
    public List<MiamiInstructorDto> Instructors { get; set; } = [];

    [JsonPropertyName("schedules")]
    public List<MiamiScheduleDto> Schedules { get; set; } = [];
}

public class MiamiCourseDto
{
    [JsonPropertyName("schoolCode")]
    public string? SchoolCode { get; set; }

    [JsonPropertyName("schoolName")]
    public string? SchoolName { get; set; }

    [JsonPropertyName("departmentCode")]
    public string? DepartmentCode { get; set; }

    [JsonPropertyName("departmentName")]
    public string? DepartmentName { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("subjectCode")]
    public string? SubjectCode { get; set; }

    [JsonPropertyName("subjectDescription")]
    public string? SubjectDescription { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("lectureHoursDescription")]
    public string? LectureHoursDescription { get; set; }

    [JsonPropertyName("labHoursDescription")]
    public string? LabHoursDescription { get; set; }

    [JsonPropertyName("creditHoursHigh")]
    public int? CreditHoursHigh { get; set; }

    [JsonPropertyName("creditHoursLow")]
    public int? CreditHoursLow { get; set; }

    [JsonPropertyName("lectureHoursHigh")]
    public int? LectureHoursHigh { get; set; }

    [JsonPropertyName("lectureHoursLow")]
    public int? LectureHoursLow { get; set; }

    [JsonPropertyName("labHoursHigh")]
    public int? LabHoursHigh { get; set; }

    [JsonPropertyName("labHoursLow")]
    public int? LabHoursLow { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class MiamiPartOfTermDto
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("startDate")]
    public required DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public required DateTime EndDate { get; set; }
}

public class MiamiEnrollmentDto
{
    [JsonPropertyName("numberOfMax")]
    public required int NumberOfMax { get; set; }

    [JsonPropertyName("numberOfCurrent")]
    public required int NumberOfCurrent { get; set; }

    [JsonPropertyName("numberOfActive")]
    public required int NumberOfActive { get; set; }

    [JsonPropertyName("numberOfAvailable")]
    public required int NumberOfAvailable { get; set; }
}

public class MiamiInstructorDto
{
    [JsonPropertyName("isPrimary")]
    public bool IsPrimary { get; set; }

    [JsonPropertyName("person")]
    public required MiamiPersonDto Person { get; set; }
}

public class MiamiPersonDto
{
    [JsonPropertyName("uniqueId")]
    public string? UniqueId { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("prefix")]
    public string? Prefix { get; set; }

    [JsonPropertyName("suffix")]
    public string? Suffix { get; set; }

    [JsonPropertyName("preferredFirstName")]
    public string? PreferredFirstName { get; set; }

    [JsonPropertyName("informalDisplayedName")]
    public string? InformalDisplayedName { get; set; }

    [JsonPropertyName("formalDisplayedName")]
    public string? FormalDisplayedName { get; set; }

    [JsonPropertyName("informalSortedName")]
    public string? InformalSortedName { get; set; }

    [JsonPropertyName("formalSortedName")]
    public string? FormalSortedName { get; set; }
}

public class MiamiScheduleDto
{
    [JsonPropertyName("startDate")]
    public required DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public required DateTime EndDate { get; set; }

    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }

    [JsonPropertyName("roomNumber")]
    public string? RoomNumber { get; set; }

    [JsonPropertyName("buildingCode")]
    public required string BuildingCode { get; set; }

    [JsonPropertyName("buildingName")]
    public required string BuildingName { get; set; }

    [JsonPropertyName("days")]
    public string? Days { get; set; }

    [JsonPropertyName("scheduleTypeCode")]
    public required string ScheduleTypeCode { get; set; }

    [JsonPropertyName("scheduleTypeDescription")]
    public required string ScheduleTypeDescription { get; set; }
}

public class MiamiBuildingDto
{
    public required string BuildingCode { get; set; }
    public required string BuildingName { get; set; }
}