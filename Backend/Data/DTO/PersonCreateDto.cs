namespace Backend.Data.DTO.Create;

public record PersonCreateDto(
	string Firstname,
	string Lastname,
	List<int> QualificationIds
);