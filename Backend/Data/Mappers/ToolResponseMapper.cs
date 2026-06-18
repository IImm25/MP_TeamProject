using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class ToolResponseMapper : Profile
    {
        public ToolResponseMapper() {
            CreateMap<Tool, ToolResponseDto>();
        }
    }
}
