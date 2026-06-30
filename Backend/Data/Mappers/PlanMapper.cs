using Backend.Data.DTO.Location;
using AutoMapper;
using Backend.Data.Entitites;
using Backend.Data.DTO.Plan;
using Backend.Data.DTO.Person;
using Backend.Data.DTO.TaskItem;

namespace Backend.Data.Mappers
{
    public class PlanMapper : Profile
    {
        public PlanMapper()
        {
            CreateMap<Plan, PlanResponseDto>();
            CreateMap<PlanBoat, BoatPlanDto>();

            CreateMap<BoatSchedule, BoatScheduleDto>();
            CreateMap<BoatTool, TaskToolDto>();

            CreateMap<BoatPerson, PersonSummaryDto>()
                .ForCtorParam(nameof(PersonSummaryDto.Id), opt => opt.MapFrom(src => src.Person.Id))
                .ForCtorParam(nameof(PersonSummaryDto.Firstname), opt => opt.MapFrom(src => src.Person.Firstname))
                .ForCtorParam(nameof(PersonSummaryDto.Lastname), opt => opt.MapFrom(src => src.Person.Lastname));
            
            CreateMap<TaskSchedule, TaskScheduleDto>()
                .ForCtorParam(nameof(TaskScheduleDto.StartTime), opt => opt.MapFrom(src => src.StartTime))
                .ForCtorParam(nameof(TaskScheduleDto.Task), opt => opt.MapFrom(src => src.TaskItem));
        }
    }
}
