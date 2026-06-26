using Backend.Data.DTO.TaskItem;

namespace Backend.Data.DTO.Plan;

public record TaskScheduleDto (
	TimeOnly StartTime,
	TaskItemSummaryDto Task
);