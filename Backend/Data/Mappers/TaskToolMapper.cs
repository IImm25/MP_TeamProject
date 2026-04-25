using AutoMapper;
using Backend.Data.DTO;

namespace Backend.Data.Mappers
{
    public class TaskToolMapper : Profile
    {
        public TaskToolMapper() {
            CreateMap<TaskTool, TaskToolDto>();
        }
    }
}
