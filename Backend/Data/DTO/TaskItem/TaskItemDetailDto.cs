using Backend.Data.DTO.Location;

namespace Backend.Data.DTO.TaskItem;

public record TaskItemDetailDto(
	int Id,
	string Name,
	float DurationHours,
	bool IsCompleted,
	DateOnly ExecutionIntervalStart,
	DateOnly ExecutionIntervalEnd,
	LocationResponseDto Location,
	List<TaskQualificationDto> RequiredQualifications,
	List<TaskToolDto> RequiredTools
);