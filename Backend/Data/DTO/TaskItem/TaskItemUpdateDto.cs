namespace Backend.Data.DTO.TaskItem;

public record TaskItemUpdateDto(
	string? Name,
	float? DurationHours,
	bool? IsCompleted,
	DateOnly? ExecutionIntervalStart,
	DateOnly? ExecutionIntervalEnd,
	int? LocationId,
	List<TaskQualificationDto>? RequiredQualifications,
	List<TaskToolDto>? RequiredTools
);