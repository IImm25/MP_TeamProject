namespace Backend.Data.DTO;

public record TaskItemSummaryDto(
	int Id,
	string Name,
	float DurationHours,
	bool isCompleted,
	DateOnly ExecutionIntervalStart,
	DateOnly ExecutionIntervalEnd
);