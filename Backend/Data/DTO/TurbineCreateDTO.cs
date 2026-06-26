namespace Backend.Data.DTO;

public record TurbineCreateDto(
	string Name,
	float Longitude,
	float Latitude
);