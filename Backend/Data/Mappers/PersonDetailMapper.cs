using AutoMapper;
using Backend.Data.DTO;

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
