namespace Backend.Data.Repositories;

using Microsoft.EntityFrameworkCore;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _db;
    private readonly DbSet<T> _set;

    public Repository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task<List<T>> GetAllAsync()
        => await _set.ToListAsync();

    public async Task<T?> GetByIdAsync(int id)
        => await _set.FindAsync(id);

    public async Task AddAsync(T entity)
        => await _set.AddAsync(entity);

    public void Update(T entity)
        => _set.Update(entity);

    public void Remove(T entity)
        => _set.Remove(entity);
}