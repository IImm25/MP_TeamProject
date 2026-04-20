using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Controllers;

[ApiController]
[Route("database/[controller]")]
public class DatabaseController : ControllerBase
{
    public DatabaseController()
    {
        
    }

    #region Taskitem
    [HttpGet("tasks")]
    public async Task<List<TaskItem>> GetTaskItems()
    {

    }

    [HttpGet("tasks/{id}")]
    public async Task<TaskItem> GetTaskItem(int id)
    {

    }

    [HttpPost("tasks")]
    public async Task<TaskItem> PostTaskItem()
    {

    }

    //vielleicht auch put mal sehen
    [HttpPatch("tasks")]
    public async Task<TaskItem> PatchTaskItem()
    {

    }

    [HttpDelete("tasks/{id}")]
    public async Task<TaskItem> DeleteTaskItem(int id)
    {

    }
    #endregion

    #region People
    [HttpGet("people")]
    public async Task<List<Person>> GetPeople()
    {

    }

    [HttpGet("people/{id}")]
    public async Task<Person> GetPerson(int id)
    {

    }

    [HttpPost("people")]
    public async Task<Person> PostPerson()
    {

    }

    //vielleicht auch put mal sehen
    [HttpPatch("people")]
    public async Task<Person> PatchPerson()
    {

    }

    [HttpDelete("people/{id}")]
    public async Task<Person> DeletePerson(int id)
    {

    }
    #endregion

    #region qualifications
    [HttpGet("qualifications")]
    public async Task<List<Qualification>> GetQualifications()
    {

    }

    [HttpGet("qualifications/{id}")]
    public async Task<Qualification> GetQualification(int id)
    {

    }

    [HttpPost("qualifications")]
    public async Task<Qualification> PostQualification()
    {

    }

    //vielleicht auch put mal sehen
    [HttpPatch("qualifications")]
    public async Task<Qualification> PatchQualification()
    {

    }

    [HttpDelete("qualifications/{id}")]
    public async Task<Qualification> DeleteQualification(int id)
    {

    }
    #endregion

    #region tools
    [HttpGet("tools")]
    public async Task<List<Tool>> GetTools()
    {

    }

    [HttpGet("tools/{id}")]
    public async Task<Tool> GetTool(int id)
    {

    }

    [HttpPost("tools")]
    public async Task<Tool> PostTool()
    {

    }

    //vielleicht auch put mal sehen
    [HttpPatch("tools")]
    public async Task<Tool> PatchTool()
    {

    }

    [HttpDelete("tools/{id}")]
    public async Task<Tool> DeleteTool(int id)
    {

    }
    #endregion
}
