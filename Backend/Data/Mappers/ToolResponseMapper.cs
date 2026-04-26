using AutoMapper;
using Backend.Data.DTO;

namespace Backend.Data.Mappers
{
    public class ToolResponseMapper : Profile
    {
        public ToolResponseMapper() {
            CreateMap<Tool, ToolResponseDto>();
        }
    }
}
