namespace TaskTrack.CalendarService.Utils;

public static class ColorUtils
{
    readonly static Random random = new Random();
    readonly static List<string> Colors = [
        "#A8DADC",
        "#F4C2C2",
        "#C5D5C5",
        "#D8BFD8",
        "#FFF5BA",
        "#B3E5FC",
        "#E0B0FF",
        "#B9FBC0",
        "#FFD6A5",
        "#E8AEB7",
        "#AEC6CF",
        "#E6E6FA",
        "#C3CDE6",
        "#C7EFCF",
        "#F9C6B0",
    ];
    public static string GetRandomColorMetadata()
    {
        int randomIndex = random.Next(0, Colors.Count);
        return $"{{\"Color\":\"{Colors[randomIndex]}\"}}";
    }
}