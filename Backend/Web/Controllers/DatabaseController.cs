using Backend.Data.DTO;
using Backend.Web.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers;

[ApiController]
[Route("api")]
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

    // ─── Mapping ──────────────────────────────────────────────────────────────

    private static QualificationResponseDto MapQual(Qualification q) => new()
    {
        Id = q.Id,
        Name = q.Name,
        Description = q.Description
    };

    private static ToolResponseDto MapTool(Tool t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        AvailableStock = t.AvailableStock
    };

    private static TaskItemResponseDto MapTask(TaskItem t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        DurationHours = t.DurationHours,
        RequiredQualifications = t.RequiredQualifications
            .Select(tq => MapQual(tq.Qualification))
            .ToList(),
        RequiredTools = t.RequiredTools
            .Select(tt => new TaskToolResponseDto
            {
                Tool = MapTool(tt.Tool),
                RequiredAmount = tt.RequiredAmount
            })
            .ToList()
    };

    private static PersonResponseDto MapPerson(Person p) => new()
    {
        Id = p.Id,
        Firstname = p.Firstname,
        Lastname = p.Lastname,
        Qualifications = p.Qualifications
            .Select(pq => MapQual(pq.Qualification))
            .ToList()
    };

    // ─── TaskItems ────────────────────────────────────────────────────────────

    [HttpGet("tasks")]
    public async Task<ActionResult<List<TaskItemResponseDto>>> GetTaskItems()
        => Ok((await _tasks.GetAllAsync()).Select(MapTask).ToList());

    [HttpGet("tasks/{id}")]
    public async Task<ActionResult<TaskItemResponseDto>> GetTaskItem(int id)
    {
        var item = await _tasks.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(MapTask(item));
    }

    [HttpPost("tasks")]
    public async Task<ActionResult<TaskItemResponseDto>> PostTaskItem([FromBody] TaskItemCreateDto dto)
    {
        var created = await _tasks.CreateAsync(dto);
        return CreatedAtAction(nameof(GetTaskItem), new { id = created.Id }, MapTask(created));
    }

    [HttpPatch("tasks/{id}")]
    public async Task<ActionResult<TaskItemResponseDto>> PatchTaskItem(int id, [FromBody] TaskItemUpdateDto dto)
    {
        var updated = await _tasks.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(MapTask(updated));
    }

    [HttpDelete("tasks/{id}")]
    public async Task<ActionResult<TaskItemResponseDto>> DeleteTaskItem(int id)
    {
        var deleted = await _tasks.DeleteAsync(id);
        return deleted is null ? NotFound() : Ok(MapTask(deleted));
    }

    // ─── People ───────────────────────────────────────────────────────────────

    [HttpGet("people")]
    public async Task<ActionResult<List<PersonResponseDto>>> GetPeople()
        => Ok((await _people.GetAllAsync()).Select(MapPerson).ToList());

    [HttpGet("people/{id}")]
    public async Task<ActionResult<PersonResponseDto>> GetPerson(int id)
    {
        var person = await _people.GetByIdAsync(id);
        return person is null ? NotFound() : Ok(MapPerson(person));
    }

    [HttpPost("people")]
    public async Task<ActionResult<PersonResponseDto>> PostPerson([FromBody] PersonCreateDto dto)
    {
        var created = await _people.CreateAsync(dto);
        return CreatedAtAction(nameof(GetPerson), new { id = created.Id }, MapPerson(created));
    }

    [HttpPatch("people/{id}")]
    public async Task<ActionResult<PersonResponseDto>> PatchPerson(int id, [FromBody] PersonUpdateDto dto)
    {
        var updated = await _people.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(MapPerson(updated));
    }

    [HttpDelete("people/{id}")]
    public async Task<ActionResult<PersonResponseDto>> DeletePerson(int id)
    {
        var deleted = await _people.DeleteAsync(id);
        return deleted is null ? NotFound() : Ok(MapPerson(deleted));
    }

    // ─── Qualifications ───────────────────────────────────────────────────────

    [HttpGet("qualifications")]
    public async Task<ActionResult<List<QualificationResponseDto>>> GetQualifications()
        => Ok((await _qualifications.GetAllAsync()).Select(MapQual).ToList());

    [HttpGet("qualifications/{id}")]
    public async Task<ActionResult<QualificationResponseDto>> GetQualification(int id)
    {
        var q = await _qualifications.GetByIdAsync(id);
        return q is null ? NotFound() : Ok(MapQual(q));
    }

    [HttpPost("qualifications")]
    public async Task<ActionResult<QualificationResponseDto>> PostQualification([FromBody] QualificationCreateDto dto)
    {
        var created = await _qualifications.CreateAsync(dto);
        return CreatedAtAction(nameof(GetQualification), new { id = created.Id }, MapQual(created));
    }

    [HttpPatch("qualifications/{id}")]
    public async Task<ActionResult<QualificationResponseDto>> PatchQualification(int id, [FromBody] QualificationUpdateDto dto)
    {
        var updated = await _qualifications.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(MapQual(updated));
    }

    [HttpDelete("qualifications/{id}")]
    public async Task<ActionResult<QualificationResponseDto>> DeleteQualification(int id)
    {
        var deleted = await _qualifications.DeleteAsync(id);
        return deleted is null ? NotFound() : Ok(MapQual(deleted));
    }

    // ─── Tools ────────────────────────────────────────────────────────────────

    [HttpGet("tools")]
    public async Task<ActionResult<List<ToolResponseDto>>> GetTools()
        => Ok((await _tools.GetAllAsync()).Select(MapTool).ToList());

    [HttpGet("tools/{id}")]
    public async Task<ActionResult<ToolResponseDto>> GetTool(int id)
    {
        var tool = await _tools.GetByIdAsync(id);
        return tool is null ? NotFound() : Ok(MapTool(tool));
    }

    [HttpPost("tools")]
    public async Task<ActionResult<ToolResponseDto>> PostTool([FromBody] ToolCreateDto dto)
    {
        var created = await _tools.CreateAsync(dto);
        return CreatedAtAction(nameof(GetTool), new { id = created.Id }, MapTool(created));
    }

    [HttpPatch("tools/{id}")]
    public async Task<ActionResult<ToolResponseDto>> PatchTool(int id, [FromBody] ToolUpdateDto dto)
    {
        var updated = await _tools.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(MapTool(updated));
    }

    [HttpDelete("tools/{id}")]
    public async Task<ActionResult<ToolResponseDto>> DeleteTool(int id)
    {
        var deleted = await _tools.DeleteAsync(id);
        return deleted is null ? NotFound() : Ok(MapTool(deleted));
    }

    // ─── PersonQualifications ─────────────────────────────────────────────────

    [HttpGet("people/{personId}/qualifications")]
    public async Task<ActionResult<List<QualificationResponseDto>>> GetPersonQualifications(int personId)
        => Ok((await _personQualifications.GetQualificationsByPersonAsync(personId)).Select(MapQual).ToList());

    [HttpPost("people/{personId}/qualifications/{qualificationId}")]
    public async Task<ActionResult> AddPersonQualification(int personId, int qualificationId)
    {
        var link = await _personQualifications.AddAsync(personId, qualificationId);
        return link is null
            ? Conflict("Verknüpfung existiert bereits oder Person/Qualifikation nicht gefunden.")
            : Ok();
    }

    [HttpDelete("people/{personId}/qualifications/{qualificationId}")]
    public async Task<ActionResult> RemovePersonQualification(int personId, int qualificationId)
    {
        var link = await _personQualifications.RemoveAsync(personId, qualificationId);
        return link is null ? NotFound() : Ok();
    }

    // ─── TaskQualifications ───────────────────────────────────────────────────

    [HttpGet("tasks/{taskId}/qualifications")]
    public async Task<ActionResult<List<QualificationResponseDto>>> GetTaskQualifications(int taskId)
        => Ok((await _taskQualifications.GetQualificationsByTaskAsync(taskId)).Select(MapQual).ToList());

    [HttpPost("tasks/{taskId}/qualifications/{qualificationId}")]
    public async Task<ActionResult> AddTaskQualification(int taskId, int qualificationId)
    {
        var link = await _taskQualifications.AddAsync(taskId, qualificationId);
        return link is null
            ? Conflict("Verknüpfung existiert bereits oder Task/Qualifikation nicht gefunden.")
            : Ok();
    }

    [HttpDelete("tasks/{taskId}/qualifications/{qualificationId}")]
    public async Task<ActionResult> RemoveTaskQualification(int taskId, int qualificationId)
    {
        var link = await _taskQualifications.RemoveAsync(taskId, qualificationId);
        return link is null ? NotFound() : Ok();
    }

    // ─── TaskTools ────────────────────────────────────────────────────────────

    [HttpGet("tasks/{taskId}/tools")]
    public async Task<ActionResult<List<TaskToolResponseDto>>> GetTaskTools(int taskId)
        => Ok((await _taskTools.GetToolsByTaskAsync(taskId))
            .Select(tt => new TaskToolResponseDto
            {
                Tool = MapTool(tt.Tool),
                RequiredAmount = tt.RequiredAmount
            }).ToList());

    [HttpPost("tasks/{taskId}/tools/{toolId}")]
    public async Task<ActionResult> AddTaskTool(int taskId, int toolId, [FromQuery] int requiredAmount = 1)
    {
        var link = await _taskTools.AddAsync(taskId, toolId, requiredAmount);
        return link is null
            ? Conflict("Verknüpfung existiert bereits oder Task/Tool nicht gefunden.")
            : Ok();
    }

    [HttpPatch("tasks/{taskId}/tools/{toolId}")]
    public async Task<ActionResult> UpdateTaskToolAmount(int taskId, int toolId, [FromQuery] int requiredAmount)
    {
        var link = await _taskTools.UpdateAmountAsync(taskId, toolId, requiredAmount);
        return link is null ? NotFound() : Ok();
    }

    [HttpDelete("tasks/{taskId}/tools/{toolId}")]
    public async Task<ActionResult> RemoveTaskTool(int taskId, int toolId)
    {
        var link = await _taskTools.RemoveAsync(taskId, toolId);
        return link is null ? NotFound() : Ok();
    }
}