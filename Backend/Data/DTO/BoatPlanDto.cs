namespace Backend.Data.DTO;

public record BoatPlanDto(
	List<TaskScheduleDto> TaskSchedules,
	List<BoatScheduleDto> BoatSchedules,
	List<PersonSummaryDto> Persons,
	List<TaskToolDto> Tools
);
