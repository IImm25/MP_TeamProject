using Backend.Data.DTO;
using Backend.Data.DTO.Create;
using Backend.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers
{
    [ApiController]
    [Route("api/persons")]
    public class PersonController : ControllerBase
    {
        private readonly PersonService service;

        public PersonController(PersonService service)
        {
            this.service = service;
        }

        [HttpGet("")]
        public async Task<ActionResult<List<PersonDetailDto>>> GetPeople()
        {
            return Ok(await service.GetAll());
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<PersonDetailDto>> GetPerson(int id)
        {
            var person = await service.GetPerson(id);
            return person != null ? Ok(person) : NotFound();
        }

        [HttpPost("")]
        public async Task<ActionResult<PersonDetailDto>> PostPerson([FromBody] PersonCreateDto dto)
        {
            return Ok(await service.CreatePerson(dto));
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<PersonDetailDto>> PatchPerson(int id, [FromBody] PersonUpdateDto dto)
        {
            var person = await service.UpdatePerson(id, dto);
            return person != null ? Ok(person) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            var success = await service.DeletePerson(id);
            return success ? Ok() : NotFound();
        }
    }
}
