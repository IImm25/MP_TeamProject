namespace Backend.Data.DTO.Person;

public record PersonCreateDto(
	string Firstname,
	string Lastname,
	List<int> QualificationIds
);