using AutoMapper;
using Backend.Data.DTO;

namespace Backend.Data.Mappers
{
    public class PersonSummaryMapper : Profile
    {
        public PersonSummaryMapper() {
            CreateMap<Person, PersonSummaryDto>();
        }
    }
}
