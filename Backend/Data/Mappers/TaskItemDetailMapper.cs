using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.DTO.Read;

namespace Backend.Data.Mappers
{
    public class TaskItemDetailMapper : Profile
    {
        public TaskItemDetailMapper() { 
            CreateMap<TaskItem,TaskItemDetailDto>();
        }
    }
}
