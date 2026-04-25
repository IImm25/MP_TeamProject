namespace Backend.Data.DTO.Update;

public class PersonUpdateDto
{
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public List<int> QualificationIds { get; set; } = [];
}
