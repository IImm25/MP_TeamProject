using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.Entitites;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers
{

    [ApiController]
    [Route("api/turbines")]
    public class TurbineController : ControllerBase
    {
        private readonly IMapper mapper;

        public TurbineController(IMapper mapper)
        {
            this.mapper = mapper;
        }

        [HttpGet("")]
        public async Task<ActionResult<List<TurbineResponseDTO>>> GetTurbines()
        {
            var list = new List<TurbineResponseDTO>([new TurbineResponseDTO(1, "rolf", 51.15f, 15.01f), new TurbineResponseDTO(2, "gertrude", 51.17f, 14.91f), new TurbineResponseDTO(3, "klaus", 51.02f, 15.07f)]);

            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TurbineResponseDTO>> GetTurbineById(int id)
        {
            var turbine = new TurbineResponseDTO(1, "rolf", 51.15f, 15.01f);

            return Ok(mapper.Map<TurbineResponseDTO>(turbine));
        }


        [HttpPost("")]
        public async Task<ActionResult<TurbineResponseDTO>> CreateTurbine([FromBody] TurbineCreateDTO createInfo)
        {
            var turbine = new Turbine(1, createInfo.Name, createInfo.Latitude, createInfo.Longitude);

            return Ok(mapper.Map<TurbineResponseDTO>(turbine));
        }

        [HttpPatch("")]
        public async Task<ActionResult<TurbineResponseDTO>> UpdateTurbine([FromBody] TurbineUpdateDTO updateInfo)
        {
            var turbine = new Turbine(1, "rolf", 51.15f, 15.01f);
            if (updateInfo.Name != null) turbine.Name = updateInfo.Name;
            if (updateInfo.Latitude != null) turbine.Latitude = (float)updateInfo.Latitude;
            if (updateInfo.Longitude != null) turbine.Longitude = (float)updateInfo.Longitude;

            return Ok(mapper.Map<TurbineResponseDTO>(turbine));
        }

        [HttpDelete("")]
        public async Task<ActionResult<TurbineResponseDTO>> DeleteTurbine(int id)
        {
            return Ok();
        }

    }
}
