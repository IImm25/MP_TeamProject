namespace Backend.Data.Repositories
using System;
using Microsoft.EntityFrameworkCore;

public class EfRepository<T> : IRepository<T>
    where T : class
{
    private readonly AppDbContext _db;
    private readonly DbSet<T> _set;

    public EfRepository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public Task<List<T>> GetAllAsync()
        => _set.ToListAsync();

    public Task<T?> GetByIdAsync(params object[] id)
        => _set.FindAsync(id).AsTask();

    public async Task AddAsync(T entity)
        => await _set.AddAsync(entity);

    public Task DeleteAsync(T entity)
    {
        _set.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveAsync()
        => _db.SaveChangesAsync();
}