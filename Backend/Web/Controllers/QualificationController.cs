using Backend.Data.DTO.Create;
using Backend.Data.DTO.Read;
using Backend.Data.DTO.Update;
using Backend.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers
{
    [ApiController]
    [Route("api/qualifications")]
    public class QualificationController : ControllerBase
    {
        private readonly QualificationService service;

        public QualificationController(QualificationService service)
        {
            this.service = service;
        }

        [HttpGet("")]
        public async Task<ActionResult<List<QualificationResponseDto>>> GetQualifications()
        {
            return Ok(await service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QualificationResponseDto>> GetQualification(int id)
        {
            var q = await service.GetQualification(id);
            return q != null ? Ok(q) : NotFound();
        }

        [HttpPost("")]
        public async Task<ActionResult<QualificationResponseDto>> PostQualification([FromBody] QualificationCreateDto dto)
        {
            var q = await service.CreateQualification(dto);
            return q != null ? Ok(q) : NotFound();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<QualificationResponseDto>> PatchQualification(int id, [FromBody] QualificationUpdateDto dto)
        {
            var q = await service.UpdateQualification(id, dto);
            return q != null ? Ok(q) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQualification(int id)
        {
            var success = await service.DeleteQualification(id);
            return success ? Ok() : NotFound();
        }

    }
}
