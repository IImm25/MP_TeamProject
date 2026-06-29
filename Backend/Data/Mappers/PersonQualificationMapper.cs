using AutoMapper;
using Backend.Data.DTO.Qualification;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class PersonQualificationMapper : Profile
    {
        public PersonQualificationMapper()
        {
            CreateMap<PersonQualification, QualificationResponseDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Qualification.Id))
                .ForCtorParam("Name", opt => opt.MapFrom(src => src.Qualification.Name))
                .ForCtorParam("Description", opt => opt.MapFrom(src => src.Qualification.Description));
        }
    }
}
