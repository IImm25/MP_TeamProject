using AutoMapper;
using Backend.Data.DTO;
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
