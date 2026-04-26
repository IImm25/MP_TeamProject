using AutoMapper;
using Backend.Data.DTO;

namespace Backend.Data.Mappers
{
    public class TaskItemSummaryMapper : Profile
    {
        public TaskItemSummaryMapper() {
            CreateMap<TaskItem, TaskItemSummaryDto>();
        }
    }
}
