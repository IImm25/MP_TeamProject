namespace Backend.Data.DTO;

public record TurbineUpdateDto(
	string? Name,
	float? Latitude,
	float? Longitude
);