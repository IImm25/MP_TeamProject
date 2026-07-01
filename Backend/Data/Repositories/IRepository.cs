namespace Backend.Data.Repositories;

public interface IRepository<T>
where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<int> AddAsync(T entity);
    Task<int> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    int Rows();
}
