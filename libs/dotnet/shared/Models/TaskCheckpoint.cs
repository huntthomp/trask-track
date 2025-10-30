namespace TaskTrack.Shared.Models;

public class TaskCheckpoint
{
    public required string Tag { get; set; }
    public required string Next { get; set; }
    public required string Current { get; set; }
}