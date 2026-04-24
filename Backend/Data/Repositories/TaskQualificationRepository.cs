using Backend.Data.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Repositories;

public class TaskQualificationRepository : ITaskQualificationRepository
{
    private readonly AppDbContext _db;

    public TaskQualificationRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Alle benötigten Qualifikationen eines Tasks.</summary>
    public async Task<List<Qualification>> GetQualificationsByTaskAsync(int taskId)
        => await _db.TaskQualifications
            .Where(tq => tq.TaskItemId == taskId)
            .Include(tq => tq.Qualification)
            .AsNoTracking()
            .Select(tq => tq.Qualification)
            .ToListAsync();

    /// <summary>
    /// Fügt einem Task eine benötigte Qualifikation hinzu.
    /// Gibt null zurück wenn Task oder Qualifikation nicht existieren,
    /// oder die Verknüpfung bereits vorhanden ist.
    /// </summary>
    public async Task<TaskQualification?> AddAsync(int taskId, int qualificationId)
    {
        var taskExists = await _db.Tasks.AnyAsync(t => t.Id == taskId);
        var qualExists = await _db.Qualifications.AnyAsync(q => q.Id == qualificationId);

        if (!taskExists || !qualExists) return null;

        var alreadyLinked = await _db.TaskQualifications
            .AnyAsync(tq => tq.TaskItemId == taskId && tq.QualificationId == qualificationId);

        if (alreadyLinked) return null;

        var link = new TaskQualification
        {
            TaskItemId = taskId,
            QualificationId = qualificationId
        };

        _db.TaskQualifications.Add(link);
        await _db.SaveChangesAsync();
        return link;
    }

    /// <summary>Entfernt die Verknüpfung. Gibt null zurück wenn sie nicht existiert.</summary>
    public async Task<TaskQualification?> RemoveAsync(int taskId, int qualificationId)
    {
        var link = await _db.TaskQualifications
            .FirstOrDefaultAsync(tq => tq.TaskItemId == taskId && tq.QualificationId == qualificationId);

        if (link is null) return null;

        _db.TaskQualifications.Remove(link);
        await _db.SaveChangesAsync();
        return link;
    }
}
