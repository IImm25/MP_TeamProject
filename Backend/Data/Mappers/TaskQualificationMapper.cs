using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class TaskQualificationMapper : Profile
    {
        public TaskQualificationMapper() { 
            CreateMap<TaskQualification, TaskQualificationDto>();
        }
    }
}
