namespace Backend.Data.DTO.Read;

public class TaskItemDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public float DurationHours { get; set; }
    public List<TaskQualificationDto> RequiredQualifications { get; set; } = [];
    public List<TaskToolDto> RequiredTools { get; set; } = [];
}
