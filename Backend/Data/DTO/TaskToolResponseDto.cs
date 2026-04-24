namespace Backend.Data.DTO;

public class TaskToolResponseDto
{
    public ToolResponseDto Tool { get; set; } = null!;
    public int RequiredAmount { get; set; }
}
