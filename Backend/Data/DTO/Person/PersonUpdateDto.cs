namespace Backend.Data.DTO.Person;

public record PersonUpdateDto(
	string? Firstname,
	string? Lastname,
	List<int> QualificationIds
);