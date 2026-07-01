using AutoMapper;
using Backend.Data.DTO.Person;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class PersonSummaryMapper : Profile
    {
        public PersonSummaryMapper() {
            CreateMap<Person, PersonSummaryDto>();
        }
    }
}
