using Backend.Web.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers;

[ApiController]
[Route("database/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly ITaskItemRepository _tasks;
    private readonly IPersonRepository _people;
    private readonly IQualificationRepository _qualifications;
    private readonly IToolRepository _tools;
    private readonly IPersonQualificationRepository _personQualifications;
    private readonly ITaskQualificationRepository _taskQualifications;
    private readonly ITaskToolRepository _taskTools;

    public DatabaseController(
        ITaskItemRepository tasks,
        IPersonRepository people,
        IQualificationRepository qualifications,
        IToolRepository tools,
        IPersonQualificationRepository personQualifications,
        ITaskQualificationRepository taskQualifications,
        ITaskToolRepository taskTools)
    {
        _tasks = tasks;
        _people = people;
        _qualifications = qualifications;
        _tools = tools;
        _personQualifications = personQualifications;
        _taskQualifications = taskQualifications;
        _taskTools = taskTools;
    }

    // ─── TaskItems ────────────────────────────────────────────────────────────

    [HttpGet("tasks")]
    public async Task<ActionResult<List<TaskItem>>> GetTaskItems()
        => Ok(await _tasks.GetAllAsync());

    [HttpGet("tasks/{id}")]
    public async Task<ActionResult<TaskItem>> GetTaskItem(int id)
    {
        var item = await _tasks.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("tasks")]
    public async Task<ActionResult<TaskItem>> PostTaskItem([FromBody] TaskItem item)
    {
        var created = await _tasks.CreateAsync(item);
        return CreatedAtAction(nameof(GetTaskItem), new { id = created.Id }, created);
    }

    [HttpPatch("tasks/{id}")]
    public async Task<ActionResult<TaskItem>> PatchTaskItem(int id, [FromBody] TaskItem patch)
    {
        var updated = await _tasks.UpdateAsync(id, t =>
        {
            if (!string.IsNullOrWhiteSpace(patch.Name)) t.Name = patch.Name;
            if (patch.DurationHours > 0) t.DurationHours = patch.DurationHours;
        });
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("tasks/{id}")]
    public async Task<ActionResult<TaskItem>> DeleteTaskItem(int id)
    {
        var deleted = await _tasks.DeleteAsync(id);
        return deleted is null ? NotFound() : Ok(deleted);
    }

    // ─── People ───────────────────────────────────────────────────────────────

    [HttpGet("people")]
    public async Task<ActionResult<List<Person>>> GetPeople()
        => Ok(await _people.GetAllAsync());

    [HttpGet("people/{id}")]
    public async Task<ActionResult<Person>> GetPerson(int id)
    {
        var person = await _people.GetByIdAsync(id);
        return person is null ? NotFound() : Ok(person);
    }

    [HttpPost("people")]
    public async Task<ActionResult<Person>> PostPerson([FromBody] Person person)
    {
        var created = await _people.CreateAsync(person);
        return CreatedAtAction(nameof(GetPerson), new { id = created.Id }, created);
    }

    [HttpPatch("people/{id}")]
    public async Task<ActionResult<Person>> PatchPerson(int id, [FromBody] Person patch)
    {
        var updated = await _people.UpdateAsync(id, p =>
        {
            if (!string.IsNullOrWhiteSpace(patch.Firstname)) p.Firstname = patch.Firstname;
            if (!string.IsNullOrWhiteSpace(patch.Lastname)) p.Lastname = patch.Lastname;
        });
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("people/{id}")]
    public async Task<ActionResult<Person>> DeletePerson(int id)
    {
        var deleted = await _people.DeleteAsync(id);
        return deleted is null ? NotFound() : Ok(deleted);
    }

    // ─── Qualifications ───────────────────────────────────────────────────────

    [HttpGet("qualifications")]
    public async Task<ActionResult<List<Qualification>>> GetQualifications()
        => Ok(await _qualifications.GetAllAsync());

    [HttpGet("qualifications/{id}")]
    public async Task<ActionResult<Qualification>> GetQualification(int id)
    {
        var q = await _qualifications.GetByIdAsync(id);
        return q is null ? NotFound() : Ok(q);
    }

    [HttpPost("qualifications")]
    public async Task<ActionResult<Qualification>> PostQualification([FromBody] Qualification qualification)
    {
        var created = await _qualifications.CreateAsync(qualification);
        return CreatedAtAction(nameof(GetQualification), new { id = created.Id }, created);
    }

    [HttpPatch("qualifications/{id}")]
    public async Task<ActionResult<Qualification>> PatchQualification(int id, [FromBody] Qualification patch)
    {
        var updated = await _qualifications.UpdateAsync(id, q =>
        {
            if (!string.IsNullOrWhiteSpace(patch.Name)) q.Name = patch.Name;
            if (!string.IsNullOrWhiteSpace(patch.Description)) q.Description = patch.Description;
        });
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("qualifications/{id}")]
    public async Task<ActionResult<Qualification>> DeleteQualification(int id)
    {
        var deleted = await _qualifications.DeleteAsync(id);
        return deleted is null ? NotFound() : Ok(deleted);
    }

    // ─── Tools ────────────────────────────────────────────────────────────────

    [HttpGet("tools")]
    public async Task<ActionResult<List<Tool>>> GetTools()
        => Ok(await _tools.GetAllAsync());

    [HttpGet("tools/{id}")]
    public async Task<ActionResult<Tool>> GetTool(int id)
    {
        var tool = await _tools.GetByIdAsync(id);
        return tool is null ? NotFound() : Ok(tool);
    }

    [HttpPost("tools")]
    public async Task<ActionResult<Tool>> PostTool([FromBody] Tool tool)
    {
        var created = await _tools.CreateAsync(tool);
        return CreatedAtAction(nameof(GetTool), new { id = created.Id }, created);
    }

    [HttpPatch("tools/{id}")]
    public async Task<ActionResult<Tool>> PatchTool(int id, [FromBody] Tool patch)
    {
        var updated = await _tools.UpdateAsync(id, t =>
        {
            if (!string.IsNullOrWhiteSpace(patch.Name)) t.Name = patch.Name;
            if (patch.AvailableStock >= 0) t.AvailableStock = patch.AvailableStock;
        });
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("tools/{id}")]
    public async Task<ActionResult<Tool>> DeleteTool(int id)
    {
        var deleted = await _tools.DeleteAsync(id);
        return deleted is null ? NotFound() : Ok(deleted);
    }

    // ─── PersonQualifications ─────────────────────────────────────────────────

    [HttpGet("people/{personId}/qualifications")]
    public async Task<ActionResult<List<Qualification>>> GetPersonQualifications(int personId)
        => Ok(await _personQualifications.GetQualificationsByPersonAsync(personId));

    [HttpPost("people/{personId}/qualifications/{qualificationId}")]
    public async Task<ActionResult<PersonQualification>> AddPersonQualification(
        int personId, int qualificationId)
    {
        var link = await _personQualifications.AddAsync(personId, qualificationId);
        return link is null ? Conflict("Verknüpfung existiert bereits oder Person/Qualifikation nicht gefunden.") : Ok(link);
    }

    [HttpDelete("people/{personId}/qualifications/{qualificationId}")]
    public async Task<ActionResult<PersonQualification>> RemovePersonQualification(
        int personId, int qualificationId)
    {
        var link = await _personQualifications.RemoveAsync(personId, qualificationId);
        return link is null ? NotFound() : Ok(link);
    }

    // ─── TaskQualifications ───────────────────────────────────────────────────

    [HttpGet("tasks/{taskId}/qualifications")]
    public async Task<ActionResult<List<Qualification>>> GetTaskQualifications(int taskId)
        => Ok(await _taskQualifications.GetQualificationsByTaskAsync(taskId));

    [HttpPost("tasks/{taskId}/qualifications/{qualificationId}")]
    public async Task<ActionResult<TaskQualification>> AddTaskQualification(
        int taskId, int qualificationId)
    {
        var link = await _taskQualifications.AddAsync(taskId, qualificationId);
        return link is null ? Conflict("Verknüpfung existiert bereits oder Task/Qualifikation nicht gefunden.") : Ok(link);
    }

    [HttpDelete("tasks/{taskId}/qualifications/{qualificationId}")]
    public async Task<ActionResult<TaskQualification>> RemoveTaskQualification(
        int taskId, int qualificationId)
    {
        var link = await _taskQualifications.RemoveAsync(taskId, qualificationId);
        return link is null ? NotFound() : Ok(link);
    }

    // ─── TaskTools ────────────────────────────────────────────────────────────

    [HttpGet("tasks/{taskId}/tools")]
    public async Task<ActionResult<List<TaskTool>>> GetTaskTools(int taskId)
        => Ok(await _taskTools.GetToolsByTaskAsync(taskId));

    [HttpPost("tasks/{taskId}/tools/{toolId}")]
    public async Task<ActionResult<TaskTool>> AddTaskTool(
        int taskId, int toolId, [FromQuery] int requiredAmount = 1)
    {
        var link = await _taskTools.AddAsync(taskId, toolId, requiredAmount);
        return link is null ? Conflict("Verknüpfung existiert bereits oder Task/Tool nicht gefunden.") : Ok(link);
    }

    [HttpPatch("tasks/{taskId}/tools/{toolId}")]
    public async Task<ActionResult<TaskTool>> UpdateTaskToolAmount(
        int taskId, int toolId, [FromQuery] int requiredAmount)
    {
        var link = await _taskTools.UpdateAmountAsync(taskId, toolId, requiredAmount);
        return link is null ? NotFound() : Ok(link);
    }

    [HttpDelete("tasks/{taskId}/tools/{toolId}")]
    public async Task<ActionResult<TaskTool>> RemoveTaskTool(int taskId, int toolId)
    {
        var link = await _taskTools.RemoveAsync(taskId, toolId);
        return link is null ? NotFound() : Ok(link);
    }
}