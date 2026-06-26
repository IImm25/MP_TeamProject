namespace Backend.Data.DTO.TaskItem;

public record TaskItemCreateDto(
	string Name,
	float DurationHours,
	DateOnly ExecutionIntervalStart,
	DateOnly ExecutionIntervalEnd,
	int LocationId,
	List<TaskQualificationDto> RequiredQualifications,
	List<TaskToolDto> RequiredTools
);
