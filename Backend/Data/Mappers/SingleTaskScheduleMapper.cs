using AutoMapper;
using Backend.Data.DTO.Plan;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers;
public class SingleTaskScheduleMapper : Profile
{
    public SingleTaskScheduleMapper()
    {
        CreateMap<TaskSchedule, SingleTaskScheduleDto>()
            .ForCtorParam(nameof(SingleTaskScheduleDto.Date),
                opt => opt.MapFrom(src => src.Boat.Plan.Date))
            .ForCtorParam(nameof(SingleTaskScheduleDto.CreatedAt),
                opt => opt.MapFrom(src => src.Boat.Plan.CreatedAt))
            .ForCtorParam(nameof(SingleTaskScheduleDto.StartTime),
                opt => opt.MapFrom(src => src.StartTime))
            .ForCtorParam(nameof(SingleTaskScheduleDto.BoatNumber),
                opt => opt.MapFrom(src => src.BoatNumber));
    }
}
