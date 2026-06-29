namespace Backend.Data.DTO.Location;

public record LocationResponseDto(
	int Id,
	string Name,
	float Latitude,
	float Longitude
);