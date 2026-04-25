namespace Backend.Data.DTO.Create;

public class PersonCreateDto
{
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public List<int> QualificationIds { get; set; } = [];
}
