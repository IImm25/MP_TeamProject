namespace Backend.Data.DTO;

public class TaskItemDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public float DurationHours { get; set; }

    public bool IsCompleted { get; set; }
    public DateOnly ExecutionIntervalStart { get; set; }
    public DateOnly ExecutionIntervalEnd { get; set; }

    public TurbineResponseDto Location { get; set; } = null!;
    public List<TaskQualificationDto> RequiredQualifications { get; set; } = [];
    public List<TaskToolDto> RequiredTools { get; set; } = [];
}
