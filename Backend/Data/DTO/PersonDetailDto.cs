namespace Backend.Data.DTO;

public record PersonDetailDto(
	int Id,
	string Firstname,
	string Lastname,
	List<QualificationResponseDto> Qualifications
);