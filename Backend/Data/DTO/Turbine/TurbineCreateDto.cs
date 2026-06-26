namespace Backend.Data.DTO.Turbine;

public record TurbineCreateDto(
	string Name,
	float Longitude,
	float Latitude
);