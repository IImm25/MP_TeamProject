using AutoMapper;
using Backend.Data.DTO.Qualification;
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
