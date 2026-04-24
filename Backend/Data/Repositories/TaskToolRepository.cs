using Backend.Data;
using Backend.Data.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Repositories;

#region implementation

// ─────────────────────────────────────────────

public class TaskToolRepository : ITaskToolRepository
{
    private readonly AppDbContext _db;

    public TaskToolRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Alle benötigten Tools eines Tasks inkl. RequiredAmount.</summary>
    public async Task<List<TaskTool>> GetToolsByTaskAsync(int taskId)
        => await _db.TaskTools
            .Where(tt => tt.TaskItemId == taskId)
            .Include(tt => tt.Tool)
            .AsNoTracking()
            .ToListAsync();

    /// <summary>
    /// Verknüpft ein Tool mit einem Task und setzt die benötigte Menge.
    /// Gibt null zurück wenn Task oder Tool nicht existieren,
    /// oder die Verknüpfung bereits vorhanden ist.
    /// </summary>
    public async Task<TaskTool?> AddAsync(int taskId, int toolId, int requiredAmount)
    {
        var taskExists = await _db.Tasks.AnyAsync(t => t.Id == taskId);
        var toolExists = await _db.Tools.AnyAsync(t => t.Id == toolId);

        if (!taskExists || !toolExists) return null;

        var alreadyLinked = await _db.TaskTools
            .AnyAsync(tt => tt.TaskItemId == taskId && tt.ToolId == toolId);

        if (alreadyLinked) return null;

        var link = new TaskTool
        {
            TaskItemId = taskId,
            ToolId = toolId,
            RequiredAmount = requiredAmount
        };

        _db.TaskTools.Add(link);
        await _db.SaveChangesAsync();
        return link;
    }

    /// <summary>Aktualisiert die benötigte Menge eines bereits verknüpften Tools.</summary>
    public async Task<TaskTool?> UpdateAmountAsync(int taskId, int toolId, int requiredAmount)
    {
        var link = await _db.TaskTools
            .FirstOrDefaultAsync(tt => tt.TaskItemId == taskId && tt.ToolId == toolId);

        if (link is null) return null;

        link.RequiredAmount = requiredAmount;
        await _db.SaveChangesAsync();
        return link;
    }

    /// <summary>Entfernt die Verknüpfung. Gibt null zurück wenn sie nicht existiert.</summary>
    public async Task<TaskTool?> RemoveAsync(int taskId, int toolId)
    {
        var link = await _db.TaskTools
            .FirstOrDefaultAsync(tt => tt.TaskItemId == taskId && tt.ToolId == toolId);

        if (link is null) return null;

        _db.TaskTools.Remove(link);
        await _db.SaveChangesAsync();
        return link;
    }

    #endregion
}