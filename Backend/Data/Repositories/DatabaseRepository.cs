using Backend.Data;
using Backend.Data.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Repositories;

// ─────────────────────────────────────────────
//  Interfaces
// ─────────────────────────────────────────────

public interface ITaskItemRepository
{
    Task<List<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem> CreateAsync(TaskItem item);
    Task<TaskItem?> UpdateAsync(int id, Action<TaskItem> patch);
    Task<TaskItem?> DeleteAsync(int id);
}

public interface IPersonRepository
{
    Task<List<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(int id);
    Task<Person> CreateAsync(Person person);
    Task<Person?> UpdateAsync(int id, Action<Person> patch);
    Task<Person?> DeleteAsync(int id);
}

public interface IQualificationRepository
{
    Task<List<Qualification>> GetAllAsync();
    Task<Qualification?> GetByIdAsync(int id);
    Task<Qualification> CreateAsync(Qualification qualification);
    Task<Qualification?> UpdateAsync(int id, Action<Qualification> patch);
    Task<Qualification?> DeleteAsync(int id);
}

public interface IToolRepository
{
    Task<List<Tool>> GetAllAsync();
    Task<Tool?> GetByIdAsync(int id);
    Task<Tool> CreateAsync(Tool tool);
    Task<Tool?> UpdateAsync(int id, Action<Tool> patch);
    Task<Tool?> DeleteAsync(int id);
}

public interface IPersonQualificationRepository
{
    Task<List<Qualification>> GetQualificationsByPersonAsync(int personId);
    Task<PersonQualification?> AddAsync(int personId, int qualificationId);
    Task<PersonQualification?> RemoveAsync(int personId, int qualificationId);
}

public interface ITaskQualificationRepository
{
    Task<List<Qualification>> GetQualificationsByTaskAsync(int taskId);
    Task<TaskQualification?> AddAsync(int taskId, int qualificationId);
    Task<TaskQualification?> RemoveAsync(int taskId, int qualificationId);
}

public interface ITaskToolRepository
{
    Task<List<TaskTool>> GetToolsByTaskAsync(int taskId);
    Task<TaskTool?> AddAsync(int taskId, int toolId, int requiredAmount);
    Task<TaskTool?> UpdateAmountAsync(int taskId, int toolId, int requiredAmount);
    Task<TaskTool?> RemoveAsync(int taskId, int toolId);
}

// ─────────────────────────────────────────────
//  Implementations
// ─────────────────────────────────────────────

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

    public async Task<TaskItem> CreateAsync(TaskItem item)
    {
        _db.Tasks.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    /// <summary>
    /// Wendet den <paramref name="patch"/>-Delegate auf das geladene Entity an
    /// und speichert danach. Gibt null zurück wenn ID nicht existiert.
    /// </summary>
    public async Task<TaskItem?> UpdateAsync(int id, Action<TaskItem> patch)
    {
        var item = await _db.Tasks.FindAsync(id);
        if (item is null) return null;

        patch(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<TaskItem?> DeleteAsync(int id)
    {
        var item = await _db.Tasks.FindAsync(id);
        if (item is null) return null;

        _db.Tasks.Remove(item);
        await _db.SaveChangesAsync();
        return item;
    }
}

// ─────────────────────────────────────────────

public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _db;

    public PersonRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Person>> GetAllAsync()
        => await _db.People
            .Include(p => p.Qualifications)
                .ThenInclude(pq => pq.Qualification)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Person?> GetByIdAsync(int id)
        => await _db.People
            .Include(p => p.Qualifications)
                .ThenInclude(pq => pq.Qualification)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Person> CreateAsync(Person person)
    {
        _db.People.Add(person);
        await _db.SaveChangesAsync();
        return person;
    }

    public async Task<Person?> UpdateAsync(int id, Action<Person> patch)
    {
        var person = await _db.People.FindAsync(id);
        if (person is null) return null;

        patch(person);
        await _db.SaveChangesAsync();
        return person;
    }

    public async Task<Person?> DeleteAsync(int id)
    {
        var person = await _db.People.FindAsync(id);
        if (person is null) return null;

        _db.People.Remove(person);
        await _db.SaveChangesAsync();
        return person;
    }
}

// ─────────────────────────────────────────────

public class QualificationRepository : IQualificationRepository
{
    private readonly AppDbContext _db;

    public QualificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Qualification>> GetAllAsync()
        => await _db.Qualifications
            .AsNoTracking()
            .ToListAsync();

    public async Task<Qualification?> GetByIdAsync(int id)
        => await _db.Qualifications
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id);

    public async Task<Qualification> CreateAsync(Qualification qualification)
    {
        _db.Qualifications.Add(qualification);
        await _db.SaveChangesAsync();
        return qualification;
    }

    public async Task<Qualification?> UpdateAsync(int id, Action<Qualification> patch)
    {
        var qualification = await _db.Qualifications.FindAsync(id);
        if (qualification is null) return null;

        patch(qualification);
        await _db.SaveChangesAsync();
        return qualification;
    }

    public async Task<Qualification?> DeleteAsync(int id)
    {
        var qualification = await _db.Qualifications.FindAsync(id);
        if (qualification is null) return null;

        _db.Qualifications.Remove(qualification);
        await _db.SaveChangesAsync();
        return qualification;
    }
}

// ─────────────────────────────────────────────

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

    public async Task<Tool> CreateAsync(Tool tool)
    {
        _db.Tools.Add(tool);
        await _db.SaveChangesAsync();
        return tool;
    }

    public async Task<Tool?> UpdateAsync(int id, Action<Tool> patch)
    {
        var tool = await _db.Tools.FindAsync(id);
        if (tool is null) return null;

        patch(tool);
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

// ─────────────────────────────────────────────

public class PersonQualificationRepository : IPersonQualificationRepository
{
    private readonly AppDbContext _db;

    public PersonQualificationRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Alle Qualifikationen einer Person.</summary>
    public async Task<List<Qualification>> GetQualificationsByPersonAsync(int personId)
        => await _db.PersonQualifications
            .Where(pq => pq.PersonId == personId)
            .Include(pq => pq.Qualification)
            .AsNoTracking()
            .Select(pq => pq.Qualification)
            .ToListAsync();

    /// <summary>
    /// Verknüpft eine Qualifikation mit einer Person.
    /// Gibt null zurück wenn Person oder Qualifikation nicht existieren,
    /// oder die Verknüpfung bereits vorhanden ist.
    /// </summary>
    public async Task<PersonQualification?> AddAsync(int personId, int qualificationId)
    {
        var personExists = await _db.People.AnyAsync(p => p.Id == personId);
        var qualExists = await _db.Qualifications.AnyAsync(q => q.Id == qualificationId);

        if (!personExists || !qualExists) return null;

        var alreadyLinked = await _db.PersonQualifications
            .AnyAsync(pq => pq.PersonId == personId && pq.QualificationId == qualificationId);

        if (alreadyLinked) return null;

        var link = new PersonQualification
        {
            PersonId = personId,
            QualificationId = qualificationId
        };

        _db.PersonQualifications.Add(link);
        await _db.SaveChangesAsync();
        return link;
    }

    /// <summary>Entfernt die Verknüpfung. Gibt null zurück wenn sie nicht existiert.</summary>
    public async Task<PersonQualification?> RemoveAsync(int personId, int qualificationId)
    {
        var link = await _db.PersonQualifications
            .FirstOrDefaultAsync(pq => pq.PersonId == personId && pq.QualificationId == qualificationId);

        if (link is null) return null;

        _db.PersonQualifications.Remove(link);
        await _db.SaveChangesAsync();
        return link;
    }
}

// ─────────────────────────────────────────────

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
}