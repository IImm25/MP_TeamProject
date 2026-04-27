namespace Backend.Data.DTO;

public record PlanRequestDto(
    float MaxWorkingHours,
    int BoatNumber,
    List<int> TaskItemIds,
    List<int> PersonIds,
    List<int> ToolIds
);