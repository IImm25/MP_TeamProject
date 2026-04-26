using AutoMapper;
using Backend.Data.DTO;

namespace Backend.Data.Mappers
{
    public class TaskQualificationMapper : Profile
    {
        public TaskQualificationMapper() { 
            CreateMap<TaskQualification, TaskQualificationDto>();
        }
    }
}
