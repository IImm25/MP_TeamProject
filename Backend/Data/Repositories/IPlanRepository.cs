using Backend.Data.Entitites;
namespace Backend.Data.Repositories;

public interface IPlanRepository : IRepository<Plan>
{
    Task<Plan?> GetFullPlanByIdAsync(int id);
    Task<List<Plan>> GetAllFullAsync();
    Task DeleteFullAsync(int id);
}
