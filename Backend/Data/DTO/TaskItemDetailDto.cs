namespace Backend.Data.DTO;

public record TaskItemDetailDto(
	int Id,
	string Name,
	float DurationHours,
	bool IsCompleted,
	DateOnly ExecutionIntervalStart,
	DateOnly ExecutionIntervalEnd,
	TurbineResponseDto Location,
	List<TaskQualificationDto> RequiredQualifications,
	List<TaskToolDto> RequiredTools
);