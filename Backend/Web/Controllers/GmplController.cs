using Backend.Data.DTO;
using Backend.GMPL;
using Backend.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
namespace Backend.Web.Controllers;

[ApiController]
[Route("api/plan")]
public class GMPLController : ControllerBase
{
    private readonly GmplService gmplService;

    public GMPLController(GmplService gmplservice)
    {
        this.gmplService = gmplservice;
    }

    [HttpGet("${date}")]
    public async Task<ActionResult<PlanResponseDto>> GetPlan(DateOnly date)
    {
        return Ok(new PlanResponseDto(DateOnly.FromDateTime(DateTime.Now), DateTimeOffset.UtcNow, []));
    }

    [HttpPost("")]
    public async Task<ActionResult<PlanResponseDto>> Plan([FromBody] PlanRequestDto request)
    {
        try
        {
            //PlanResponseDto response = await gmplService.Solve(request);
            return Ok(new PlanResponseDto(DateOnly.FromDateTime(DateTime.Now), DateTimeOffset.UtcNow, []));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("${date}")]
    public async Task<ActionResult<PlanResponseDto>> DeletePlan(DateOnly date)
    {
        return Ok();
    }


}
