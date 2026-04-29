namespace Backend.Data.DTO;

public record BoatPlanDto(
    List<TaskItemSummaryDto> TaskItems,
    List<PersonSummaryDto> Persons,
    List<TaskToolDto> Tools
    );
