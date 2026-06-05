using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.Entitites;
using Backend.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers
{

    [ApiController]
    [Route("api/turbines")]
    public class TurbineController : ControllerBase
    {
        private readonly TurbineService service;

        public TurbineController(TurbineService service)
        {
            this.service = service;
        }

        [HttpGet("")]
        public async Task<ActionResult<List<TurbineResponseDto>>> GetTurbines()
        {
            return Ok(await service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TurbineResponseDto>> GetTurbineById(int id)
        {
            return Ok(await service.GetTurbine(id));
        }


        [HttpPost("")]
        public async Task<ActionResult<TurbineResponseDto>> CreateTurbine([FromBody] TurbineCreateDto createInfo)
        {
            return Ok(await service.CreateTurbine(createInfo));
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<TurbineResponseDto>> UpdateTurbine(int id,[FromBody] TurbineUpdateDto updateInfo)
        {
            return Ok(await service.UpdateTurbine(id,updateInfo));
        }

        [HttpDelete("")]
        public async Task<ActionResult<TurbineResponseDto>> DeleteTurbine(int id)
        {
            return Ok(await service.DeleteTurbine(id));
        }

    }
}
