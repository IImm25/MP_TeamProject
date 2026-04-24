using Backend.Data.DBContext;
using Backend.Data.DTO;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Repositories;

public class TaskItemRepository : ITaskItemRepository
{
    private readonly AppDbContext _db;

    public TaskItemRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Alle Tasks inkl. benötigter Qualifikationen und Tools.</summary>
    public async Task<List<TaskItem>> GetAllAsync()
        => await _db.Tasks
            .Include(t => t.RequiredQualifications)
                .ThenInclude(tq => tq.Qualification)
            .Include(t => t.RequiredTools)
                .ThenInclude(tt => tt.Tool)
            .AsNoTracking()
            .ToListAsync();

    /// <summary>Einzelnen Task per ID; null wenn nicht gefunden.</summary>
    public async Task<TaskItem?> GetByIdAsync(int id)
        => await _db.Tasks
            .Include(t => t.RequiredQualifications)
                .ThenInclude(tq => tq.Qualification)
            .Include(t => t.RequiredTools)
                .ThenInclude(tt => tt.Tool)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TaskItem> CreateAsync(TaskItemCreateDto dto)
    {
        var item = new TaskItem(dto.Name, dto.DurationHours);

        foreach (var qualId in dto.RequiredQualificationIds)
            item.RequiredQualifications.Add(new TaskQualification { QualificationId = qualId });

        foreach (var t in dto.RequiredTools)
            item.RequiredTools.Add(new TaskTool { ToolId = t.ToolId, RequiredAmount = t.RequiredAmount });

        _db.Tasks.Add(item);
        await _db.SaveChangesAsync();

        // Navigation-Properties nachladen, damit MapTask nicht auf null läuft
        return (await GetByIdAsync(item.Id))!;
    }

    public async Task<TaskItem?> UpdateAsync(int id, TaskItemUpdateDto dto)
    {
        var item = await _db.Tasks.FindAsync(id);
        if (item is null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name)) item.Name = dto.Name;
        if (dto.DurationHours.HasValue) item.DurationHours = dto.DurationHours.Value;

        await _db.SaveChangesAsync();

        // Navigation-Properties nachladen
        return await GetByIdAsync(id);
    }

    public async Task<TaskItem?> DeleteAsync(int id)
    {
        // Erst mit Includes laden, damit der Caller noch mappen kann
        var item = await GetByIdAsync(id);
        if (item is null) return null;

        _db.Tasks.Remove(_db.Tasks.Find(id)!);
        await _db.SaveChangesAsync();
        return item;
    }
}