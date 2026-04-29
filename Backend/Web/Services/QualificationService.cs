using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.DTO.Create;
using Backend.Data.Repositories;
using Microsoft.VisualBasic;

namespace Backend.Web.Services
{
    public class QualificationService
    {
        private readonly IRepository<Qualification> qualifications;
        private readonly IPersonRepository persons;
        private readonly ITaskItemRepository taskItems;
        private readonly IMapper mapper;

        public QualificationService(IRepository<Qualification> qualifications, IPersonRepository persons, ITaskItemRepository taskItems, IMapper mapper)
        {
            this.qualifications = qualifications;
            this.persons = persons;
            this.taskItems = taskItems;
            this.mapper = mapper;
        }

        public async Task<QualificationResponseDto> CreateQualification(QualificationCreateDto create)
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

        public async Task<List<bool>> GetPersonQualificationMask(int personId,List<int> qualIds)
        {
            var person = await persons.GetFullByIdAsync(personId);
            if (person == null)
                return qualIds.Select(_ => false).ToList();

            var personQualIds = person.Qualifications
                .Select(x => x.QualificationId)
                .ToHashSet();

            return qualIds
                .Select(id => personQualIds.Contains(id))
                .ToList();
        }

        public async Task<List<int>> GetTaskQualificationRequirements(int taskId,List<int> qualIds)
        {
            var task = await taskItems.GetFullByIdAsync(taskId);

            if (task == null)
                return qualIds.Select(_ => 0).ToList();

            var required = task.RequiredQualifications
                .ToDictionary(
                    x => x.QualificationId,
                    x => x.RequiredAmount);

            return qualIds
                .Select(id => required.GetValueOrDefault(id))
                .ToList();
        }

        public async Task<List<int>> GetAllIds()
        {
            var quals = await qualifications.GetAllAsync();
            return quals.Select(q => q.Id).ToList();
        }

    }
}
