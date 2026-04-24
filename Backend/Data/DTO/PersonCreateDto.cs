namespace Backend.Data.DTO;

public class PersonCreateDto
{
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public List<int> QualificationIds { get; set; } = [];
}
