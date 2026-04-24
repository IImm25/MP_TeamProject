using Backend.Data.DTO;

namespace Backend.Web.Repositories;

public interface ITaskItemRepository
{
    Task<List<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem> CreateAsync(TaskItemCreateDto dto);
    Task<TaskItem?> UpdateAsync(int id, TaskItemUpdateDto dto);
    Task<TaskItem?> DeleteAsync(int id);
}
