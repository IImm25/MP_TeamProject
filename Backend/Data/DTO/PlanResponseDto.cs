namespace Backend.Data.DTO;

public record PlanResponseDto(
	DateOnly Date,
	DateTimeOffset CreatedAt,
	List<BoatPlanDto> Boats
);