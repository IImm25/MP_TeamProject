namespace Backend.Data.DTO.Create;

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public class TaskItemCreateDto
{
    public required string Name { get; set; }
    public float DurationHours { get; set; }

    public DateOnly ExecutionIntervalStart { get; set; }
    public DateOnly ExecutionIntervalEnd { get; set; }

    public int LocationId { get; set; }
    public List<TaskQualificationDto> RequiredQualifications { get; set; } = [];
    public List<TaskToolDto> RequiredTools { get; set; } = [];
}
