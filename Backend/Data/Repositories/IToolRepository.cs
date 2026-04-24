using Backend.Data.DTO;

namespace Backend.Web.Repositories;

public interface IToolRepository
{
    Task<List<Tool>> GetAllAsync();
    Task<Tool?> GetByIdAsync(int id);
    Task<Tool> CreateAsync(ToolCreateDto dto);
    Task<Tool?> UpdateAsync(int id, ToolUpdateDto dto);
    Task<Tool?> DeleteAsync(int id);
}
