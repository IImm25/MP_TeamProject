using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class TaskItemUpdateDtoMapper : Profile
    {
        public TaskItemUpdateDtoMapper()
        {
            CreateMap<TaskItem, TaskItemUpdateDto>();
            CreateMap<TaskItemUpdateDto, TaskItem>();
        }
    }
}
