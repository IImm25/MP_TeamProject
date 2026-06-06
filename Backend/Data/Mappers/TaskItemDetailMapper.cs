using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class TaskItemDetailMapper : Profile
    {
        public TaskItemDetailMapper() { 
            CreateMap<TaskItem,TaskItemDetailDto>();
            CreateMap<TaskItemDetailDto, TaskItemSummaryDto>();
        }
    }
}
