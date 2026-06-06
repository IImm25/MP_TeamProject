using AutoMapper;
using Backend.Data.DTO;
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
