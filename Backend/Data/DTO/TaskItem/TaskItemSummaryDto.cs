namespace Backend.Data.DTO.TaskItem;

public record TaskItemSummaryDto(
	int Id,
	string Name,
	float DurationHours,
	bool isCompleted,
	DateOnly ExecutionIntervalStart,
	DateOnly ExecutionIntervalEnd
);