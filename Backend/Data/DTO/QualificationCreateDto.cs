namespace Backend.Data.DTO;

public class QualificationCreateDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = "";
}
