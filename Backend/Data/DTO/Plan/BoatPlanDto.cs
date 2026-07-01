using Backend.Data.DTO.Person;
using Backend.Data.DTO.TaskItem;

namespace Backend.Data.DTO.Plan;

public record BoatPlanDto(
	List<TaskScheduleDto> TaskSchedules,
	List<BoatScheduleDto> BoatSchedules,
	List<PersonSummaryDto> Persons,
	List<TaskToolDto> Tools
);
