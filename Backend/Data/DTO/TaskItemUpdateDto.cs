namespace Backend.Data.DTO;

public class TaskItemUpdateDto
{
    public string? Name { get; set; }
    public float? DurationHours { get; set; }

    public bool? IsCompleted { get; set; } 
    public DateOnly? ExecutionIntervalStart { get; set; }
    public DateOnly? ExecutionIntervalEnd { get; set; }

    public int? LocationId { get; set; }

    public List<TaskQualificationDto>? RequiredQualifications { get; set; } = [];
    public List<TaskToolDto>? RequiredTools { get; set; } = [];
}
