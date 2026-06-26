namespace Backend.Data.DTO.Turbine;

public record TurbineUpdateDto(
	string? Name,
	float? Latitude,
	float? Longitude
);