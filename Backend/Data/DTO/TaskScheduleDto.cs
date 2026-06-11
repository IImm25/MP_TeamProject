namespace Backend.Data.DTO;

public record TaskScheduleDto (
    TimeOnly StartTime,
    TaskItemSummaryDto Task
);
