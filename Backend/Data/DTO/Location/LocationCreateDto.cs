namespace Backend.Data.DTO.Location;

public record LocationCreateDto(
	string Name,
	float Longitude,
	float Latitude
);