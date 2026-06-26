using AutoMapper;
using Backend.Data.DTO.Person;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class PersonDetailMapper : Profile
    {
        public PersonDetailMapper() {
            CreateMap<Person, PersonDetailDto>();
            CreateMap<PersonDetailDto, PersonSummaryDto>();
        }
    }
}
