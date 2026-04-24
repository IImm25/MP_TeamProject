namespace Backend.Data.DTO;

public class TaskItemResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public float DurationHours { get; set; }
    public List<QualificationResponseDto> RequiredQualifications { get; set; } = [];
    public List<TaskToolResponseDto> RequiredTools { get; set; } = [];
}
