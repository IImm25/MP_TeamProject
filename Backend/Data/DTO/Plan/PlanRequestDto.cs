namespace Backend.Data.DTO.Plan;

public record PlanRequestDto(
    float MaxWorkHours,
    int BoatNumber,
    float BoatSpeed
);