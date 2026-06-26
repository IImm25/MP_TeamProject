using AutoMapper;
using Backend.Data.DTO.TaskItem;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class TaskItemSummaryMapper : Profile
    {
        public TaskItemSummaryMapper() {
            CreateMap<TaskItem, TaskItemSummaryDto>();
        }
    }
}
