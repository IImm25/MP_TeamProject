namespace Backend.Web.Repositories;

public interface ITaskQualificationRepository
{
    Task<List<Qualification>> GetQualificationsByTaskAsync(int taskId);
    Task<TaskQualification?> AddAsync(int taskId, int qualificationId);
    Task<TaskQualification?> RemoveAsync(int taskId, int qualificationId);
}
