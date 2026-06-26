namespace Backend.Data.DTO.Plan;

public record PlanResponseDto(
	DateOnly Date,
	DateTimeOffset CreatedAt,
	List<BoatPlanDto> Boats
);