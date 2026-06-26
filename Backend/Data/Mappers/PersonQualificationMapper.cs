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
                .IncludeMembers(s => s.Qualification);
        }
    }
}
