using AutoMapper;
using Backend.Data.DTO.TaskItem;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class TaskToolMapper : Profile
    {
        public TaskToolMapper() {
            CreateMap<TaskTool, TaskToolDto>();
        }
    }
}
