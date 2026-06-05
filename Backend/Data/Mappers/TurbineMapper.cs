using Backend.Data.DTO;
using AutoMapper;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class TurbineMapper : Profile
    {
        public TurbineMapper()
        {
            CreateMap<Turbine,TurbineResponseDto>();
        }
    }
}
