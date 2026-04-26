using AutoMapper;
using Backend.Data.DTO;

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
