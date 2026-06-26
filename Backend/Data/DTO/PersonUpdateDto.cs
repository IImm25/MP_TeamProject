namespace Backend.Data.DTO;

public record PersonUpdateDto(
	string? Firstname,
	string? Lastname,
	List<int> QualificationIds
);