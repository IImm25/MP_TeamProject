using Backend.Data.Entitites;

namespace Backend.Data.Repositories
{
    public interface ITaskItemRepository : IRepository<TaskItem>
    {
        Task<TaskItem?> GetFullByIdAsync(int id);
        Task<List<TaskItem>> GetAllFullAsync();

        Task<TaskSchedule?> GetTaskScheduleByTaskIdAsync(int taskId);
        Task<List<TaskItem>> GetAllByLocationAsync(int locationId);
        Task<List<TaskItem>> GetAllUncompletedTasksByDateAsync(DateOnly date);
        Task<List<TaskItem>> GetAllScheduledTasksByDateAsync(DateOnly date);
        Task<List<TaskItem>> GetAllOpenUnscheduledTasksByDateAsync(DateOnly date);
        Task<List<TaskSchedule>> GetAllOngoingTasksByDateAndTimeAsync(DateOnly date, TimeOnly referenceTime);
        Task<List<TaskItem>> GetAllScheduledNotOngoingTasksByDateAndTimeAsync(DateOnly date, TimeOnly referenceTime);
    }
}
