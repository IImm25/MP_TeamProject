namespace Backend.Data.DTO;

public class PersonUpdateDto
{
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public List<int> QualificationIds { get; set; } = [];
}
