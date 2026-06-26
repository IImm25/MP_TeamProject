namespace Backend.Data.DTO.Turbine;

public record TurbineResponseDto(
	int Id,
	string Name,
	float Latitude,
	float Longitude
);