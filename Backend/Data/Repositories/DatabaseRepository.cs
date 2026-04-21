using Backend.Data;
using Backend.Data.DBContext;
using Backend.Web.Dto;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Repositories;

// ─────────────────────────────────────────────
//  Interfaces
// ─────────────────────────────────────────────

public interface ITaskItemRepository
{
    Task<List<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem> CreateAsync(TaskItemCreateDto dto);
    Task<TaskItem?> UpdateAsync(int id, TaskItemUpdateDto dto);
    Task<TaskItem?> DeleteAsync(int id);
}

public interface IPersonRepository
{
    Task<List<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(int id);
    Task<Person> CreateAsync(PersonCreateDto dto);
    Task<Person?> UpdateAsync(int id, PersonUpdateDto dto);
    Task<Person?> DeleteAsync(int id);
}

public interface IQualificationRepository
{
    Task<List<Qualification>> GetAllAsync();
    Task<Qualification?> GetByIdAsync(int id);
    Task<Qualification> CreateAsync(QualificationCreateDto dto);
    Task<Qualification?> UpdateAsync(int id, QualificationUpdateDto dto);
    Task<Qualification?> DeleteAsync(int id);
}

public interface IToolRepository
{
    Task<List<Tool>> GetAllAsync();
    Task<Tool?> GetByIdAsync(int id);
    Task<Tool> CreateAsync(ToolCreateDto dto);
    Task<Tool?> UpdateAsync(int id, ToolUpdateDto dto);
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

    public async Task<Person> CreateAsync(PersonCreateDto dto)
    {
        var person = new Person(dto.Firstname, dto.Lastname);

        foreach (var qualId in dto.QualificationIds)
            person.Qualifications.Add(new PersonQualification { QualificationId = qualId });

        _db.People.Add(person);
        await _db.SaveChangesAsync();

        // Navigation-Properties nachladen
        return (await GetByIdAsync(person.Id))!;
    }

    public async Task<Person?> UpdateAsync(int id, PersonUpdateDto dto)
    {
        var person = await _db.People.FindAsync(id);
        if (person is null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Firstname)) person.Firstname = dto.Firstname;
        if (!string.IsNullOrWhiteSpace(dto.Lastname)) person.Lastname = dto.Lastname;

        await _db.SaveChangesAsync();

        // Navigation-Properties nachladen
        return await GetByIdAsync(id);
    }

    public async Task<Person?> DeleteAsync(int id)
    {
        var person = await GetByIdAsync(id);
        if (person is null) return null;

        _db.People.Remove(_db.People.Find(id)!);
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

    public async Task<Qualification> CreateAsync(QualificationCreateDto dto)
    {
        var qualification = new Qualification(dto.Name, dto.Description);
        _db.Qualifications.Add(qualification);
        await _db.SaveChangesAsync();
        return qualification;
    }

    public async Task<Qualification?> UpdateAsync(int id, QualificationUpdateDto dto)
    {
        var qualification = await _db.Qualifications.FindAsync(id);
        if (qualification is null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name)) qualification.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Description)) qualification.Description = dto.Description;

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