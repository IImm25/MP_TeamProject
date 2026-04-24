using Backend.Data.DTO;

namespace Backend.Web.Repositories;

public interface IQualificationRepository
{
    Task<List<Qualification>> GetAllAsync();
    Task<Qualification?> GetByIdAsync(int id);
    Task<Qualification> CreateAsync(QualificationCreateDto dto);
    Task<Qualification?> UpdateAsync(int id, QualificationUpdateDto dto);
    Task<Qualification?> DeleteAsync(int id);
}
