using System.Threading.Tasks;
using Backend.Data.DTO;
using Backend.Data.DTO.Create;
using Backend.Data.Mappers;
using Backend.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TaskItemController : ControllerBase
    {

        private readonly TaskItemService service;

        public TaskItemController(TaskItemService service)
        {
            this.service = service;
        }

        [HttpGet("")]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetTaskItems()
        {
            return Ok(await service.GetAll());
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDetailDto>> GetTaskItem(int id)
        {
            var task = await service.GetTaskItem(id);
            return task != null ? Ok(task) : NotFound();
        }

        [HttpPost("")]
        public async Task<ActionResult<TaskItemDetailDto>> PostTaskItem([FromBody] TaskItemCreateDto dto)
        {
            return Ok(await service.CreateTaskItem(dto));
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<TaskItemDetailDto>> PatchTaskItem(int id, [FromBody] TaskItemUpdateDto dto)
        {
            var task = await service.UpdateTaskItem(id, dto);
            return task != null ? Ok(task) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskItem(int id)
        {
            var success = await service.DeleteTaskItem(id);
            return success ? Ok() : NotFound();
        }

    }
}
