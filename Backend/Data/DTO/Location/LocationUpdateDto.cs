namespace Backend.Data.DTO.Location;

public record LocationUpdateDto(
	string? Name,
	float? Latitude,
	float? Longitude
);