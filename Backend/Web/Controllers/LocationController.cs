using AutoMapper;
using Backend.Data.DTO.Location;
using Backend.Data.Entitites;
using Backend.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers
{

    [ApiController]
    [Route("api/turbines")]
    public class LocationController : ControllerBase
    {
        private readonly LocationService service;

        public LocationController(LocationService service)
        {
            this.service = service;
        }

        [HttpGet("")]
        public async Task<ActionResult<List<LocationResponseDto>>> GetTurbines()
        {
            return Ok(await service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LocationResponseDto>> GetTurbineById(int id)
        {
            return Ok(await service.GetLocation(id));
        }


        [HttpPost("")]
        public async Task<ActionResult<LocationResponseDto>> CreateTurbine([FromBody] LocationCreateDto createInfo)
        {
            return Ok(await service.CreateLocation(createInfo));
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<LocationResponseDto>> UpdateTurbine(int id,[FromBody] LocationUpdateDto updateInfo)
        {
            return Ok(await service.UpdateLocation(id,updateInfo));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<LocationResponseDto>> DeleteTurbine(int id)
        {
            return Ok(await service.DeleteLocation(id));
        }

    }
}
