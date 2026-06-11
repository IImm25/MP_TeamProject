namespace Backend.Data.DTO;

public record PlanRequestDto(
    float MaxWorkHours,
    int BoatNumber,
    DateTime Time,
    float BoatSpeed
);