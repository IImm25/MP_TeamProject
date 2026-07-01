namespace Backend.Data.DTO.Plan;

public record SingleTaskScheduleDto(
    DateOnly Date,
    DateTimeOffset CreatedAt,
    TimeOnly StartTime,
    int BoatNumber
);
