using Backend.Data;
using Backend.Data.DTO;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Repositories;

public class ToolRepository : IToolRepository
{
    private readonly AppDbContext _db;

    public ToolRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Tool>> GetAllAsync()
        => await _db.Tools
            .AsNoTracking()
            .ToListAsync();

    public async Task<Tool?> GetByIdAsync(int id)
        => await _db.Tools
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Tool> CreateAsync(ToolCreateDto dto)
    {
        var tool = new Tool(dto.Name, dto.AvailableStock);
        _db.Tools.Add(tool);
        await _db.SaveChangesAsync();
        return tool;
    }

    public async Task<Tool?> UpdateAsync(int id, ToolUpdateDto dto)
    {
        var tool = await _db.Tools.FindAsync(id);
        if (tool is null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name)) tool.Name = dto.Name;
        if (dto.AvailableStock.HasValue) tool.AvailableStock = dto.AvailableStock.Value;

        await _db.SaveChangesAsync();
        return tool;
    }

    public async Task<Tool?> DeleteAsync(int id)
    {
        var tool = await _db.Tools.FindAsync(id);
        if (tool is null) return null;

        _db.Tools.Remove(tool);
        await _db.SaveChangesAsync();
        return tool;
    }
}
