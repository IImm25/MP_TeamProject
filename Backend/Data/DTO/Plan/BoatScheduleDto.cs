namespace Backend.Data.DTO.Plan;
public record BoatScheduleDto(
	TimeOnly Departure,
	TimeOnly Arrival
);