using Backend.Data.DTO.Location;
using AutoMapper;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class LocationMapper : Profile
    {
        public LocationMapper()
        {
            CreateMap<Location,LocationResponseDto>();
        }
    }
}
