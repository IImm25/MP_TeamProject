namespace Backend.Data.DTO;

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public class TaskItemCreateDto
{
    public required string Name { get; set; }
    public float DurationHours { get; set; }
    public List<int> RequiredQualificationIds { get; set; } = [];
    public List<TaskToolDto> RequiredTools { get; set; } = [];
}
