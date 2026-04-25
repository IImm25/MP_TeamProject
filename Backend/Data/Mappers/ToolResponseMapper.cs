using AutoMapper;
using Backend.Data.DTO.Read;

namespace Backend.Data.Mappers
{
    public class ToolResponseMapper : Profile
    {
        public ToolResponseMapper() {
            CreateMap<Tool, ToolResponseDto>();
        }
    }
}
