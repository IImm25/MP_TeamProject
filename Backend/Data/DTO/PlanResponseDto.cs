namespace Backend.Data.DTO;

public record PlanResponseDto(
    int BoatID,
    List<TaskItemSummaryDto> TaskItems,
    List<PersonDetailDto> People,
    List<TaskToolDto> Tools
    );
