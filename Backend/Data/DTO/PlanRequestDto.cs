namespace Backend.Data.DTO;

public record PlanRequestDto(
    float MaxTime,
    int BoatNumber,
    List<int> TaskItemIds,
    List<int> PersonIds,
    List<int> ToolIds
);