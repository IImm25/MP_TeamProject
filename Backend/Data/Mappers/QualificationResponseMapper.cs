using AutoMapper;
using Backend.Data.DTO;

namespace Backend.Data.Mappers
{
    public class QualificationResponseMapper : Profile
    {
        public QualificationResponseMapper() {
            CreateMap<Qualification, QualificationResponseDto>();
        }
    }
}
