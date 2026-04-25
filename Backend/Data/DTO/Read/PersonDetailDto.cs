namespace Backend.Data.DTO.Read;

public class PersonDetailDto
{
    public int Id { get; set; }
    public string Firstname { get; set; } = "";
    public string Lastname { get; set; } = "";
    public List<QualificationResponseDto> Qualifications { get; set; } = [];
}