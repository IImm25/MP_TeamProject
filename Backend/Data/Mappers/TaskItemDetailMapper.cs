using AutoMapper;
using Backend.Data.DTO;

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
