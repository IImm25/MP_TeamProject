using Backend.Data.Entitites;

namespace Backend.Data.Repositories
{
    public interface ITaskItemRepository : IRepository<TaskItem>
    {
        Task<TaskItem?> GetFullByIdAsync(int id);
        Task<List<TaskItem>> GetAllFullAsync();

        Task<TaskSchedule?> GetTaskScheduleByTaskIdAsync(int taskId);

    }
}
