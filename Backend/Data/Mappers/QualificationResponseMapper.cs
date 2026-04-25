using AutoMapper;
using Backend.Data.DTO.Read;

namespace Backend.Data.Mappers
{
    public class QualificationResponseMapper : Profile
    {
        public QualificationResponseMapper() {
            CreateMap<Qualification, QualificationResponseDto>();
        }
    }
}
