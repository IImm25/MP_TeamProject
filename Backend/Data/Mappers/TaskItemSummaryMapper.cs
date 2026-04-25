using AutoMapper;
using Backend.Data.DTO.Read;

namespace Backend.Data.Mappers
{
    public class TaskItemSummaryMapper : Profile
    {
        public TaskItemSummaryMapper() {
            CreateMap<TaskItem, TaskItemSummaryDto>();
        }
    }
}
