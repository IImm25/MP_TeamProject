using Backend.Data.DTO.Turbine;
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
