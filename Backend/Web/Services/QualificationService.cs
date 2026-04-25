using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.DTO.Create;
using Backend.Data.Repositories;

namespace Backend.Web.Services
{
    public class QualificationService
    {
        private readonly IRepository<Qualification> qualifications;
        private readonly IMapper mapper;

        public QualificationService(IRepository<Qualification> qualifications, IMapper mapper)
        {
            this.qualifications = qualifications;
            this.mapper = mapper;
        }

        public async Task<QualificationResponseDto> CreateQualification (QualificationCreateDto create)
        {
            Qualification qualification = new Qualification(create.Name, create.Description);
            return mapper.Map<QualificationResponseDto>(await qualifications.AddAsync(qualification));
        }

        public async Task<List<QualificationResponseDto>> GetAll()
        {
            return mapper.Map<List<QualificationResponseDto>>(await qualifications.GetAllAsync());
        }

        public async Task<QualificationResponseDto?> GetQualification(int id)
        {
            return mapper.Map<QualificationResponseDto?>(await qualifications.GetByIdAsync(id));
        }

        public async Task<QualificationResponseDto?> UpdateQualification(int id, QualificationUpdateDto update)
        {
            var tool = await qualifications.GetByIdAsync(id);
            if (tool == null)
            {
                return null;
            }
            if (update.Name != null) tool.Name = update.Name;
            if (update.Description != null) tool.Description = update.Description;

            return mapper.Map<QualificationResponseDto?>(await qualifications.UpdateAsync(tool));
        }

        public async Task<bool> DeleteQualification(int id)
        {
            return await qualifications.DeleteAsync(id);
        }



    }
}
