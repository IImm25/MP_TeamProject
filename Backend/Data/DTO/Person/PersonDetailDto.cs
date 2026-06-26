using Backend.Data.DTO.Qualification;

namespace Backend.Data.DTO.Person;

public record PersonDetailDto(
	int Id,
	string Firstname,
	string Lastname,
	List<QualificationResponseDto> Qualifications
);