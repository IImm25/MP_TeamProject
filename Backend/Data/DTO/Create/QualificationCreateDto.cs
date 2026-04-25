namespace Backend.Data.DTO.Create;

public class QualificationCreateDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = "";
}
