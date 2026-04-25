using AutoMapper;
using Backend.Data.DTO.Read;

namespace Backend.Data.Mappers
{
    public class PersonSummaryMapper : Profile
    {
        public PersonSummaryMapper() {
            CreateMap<Person, PersonSummaryDto>();
        }
    }
}
