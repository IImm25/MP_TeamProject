namespace Backend.Data.DTO;

public record PlanRequestDto(
    float maxTime,
    int BoatNumber,
    List<int> TaskItemIds,
    List<int> PersonIds,
    List<int> ToolIds
);