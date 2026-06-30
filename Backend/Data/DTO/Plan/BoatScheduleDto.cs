using Backend.Data.DTO.Location;

namespace Backend.Data.DTO.Plan;
public record BoatScheduleDto(
	TimeOnly Departure,
	TimeOnly Arrival,
	LocationResponseDto Origin,
	LocationResponseDto Destination
);