using Backend.Data.DTO.Create;
using Backend.Data.DTO.Read;
using Backend.Data.DTO.Update;
using Backend.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers
{
    [ApiController]
    [Route("api/tools")]
    public class ToolController : ControllerBase
    {
        private readonly ToolService service;

        public ToolController(ToolService service)
        {
            this.service = service;
        }

        [HttpGet("")]
        public async Task<ActionResult<List<ToolResponseDto>>> GetTools()
        {
            return Ok(await service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ToolResponseDto>> GetTool(int id)
        {
            var tool = await service.GetTool(id);
            return tool != null ? Ok(tool) : NotFound();
        }

        [HttpPost("")]
        public async Task<ActionResult<ToolResponseDto>> PostTool([FromBody] ToolCreateDto dto)
        {
            return Ok(await service.CreateTool(dto));
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ToolResponseDto>> PatchTool(int id, [FromBody] ToolUpdateDto dto)
        {
            var tools = await service.UpdateTool(id, dto);
            return tools != null ? Ok(tools) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTool(int id)
        {
            var success = await service.DeleteTool(id);
            return success ? Ok() : NotFound();
        }
    }
}
