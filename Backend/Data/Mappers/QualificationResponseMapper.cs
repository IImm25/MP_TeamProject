using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.Entitites;

namespace Backend.Data.Mappers
{
    public class QualificationResponseMapper : Profile
    {
        public QualificationResponseMapper() {
            CreateMap<Qualification, QualificationResponseDto>();
        }
    }
}
