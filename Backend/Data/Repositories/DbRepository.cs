namespace Backend.Data.Repositories;

using Backend.Data.DBContext;
using Microsoft.EntityFrameworkCore;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext dbContext;
    private readonly DbSet<T> dbSet;

    public Repository(AppDbContext db)
    {
        dbContext = db;
        dbSet = db.Set<T>();
    }

    public async Task<List<T>> GetAllAsync()
        => await dbSet.ToListAsync();

    public async Task<T?> GetByIdAsync(int id)
        => await dbSet.FindAsync(id);

    public async Task AddAsync(T entity)
        => await dbSet.AddAsync(entity);

    public void Update(T entity)
        => dbSet.Update(entity);

    public void Remove(T entity)
        => dbSet.Remove(entity);
}