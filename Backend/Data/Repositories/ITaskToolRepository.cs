namespace Backend.Web.Repositories;

public interface ITaskToolRepository
{
    Task<List<TaskTool>> GetToolsByTaskAsync(int taskId);
    Task<TaskTool?> AddAsync(int taskId, int toolId, int requiredAmount);
    Task<TaskTool?> UpdateAmountAsync(int taskId, int toolId, int requiredAmount);
    Task<TaskTool?> RemoveAsync(int taskId, int toolId);
}
