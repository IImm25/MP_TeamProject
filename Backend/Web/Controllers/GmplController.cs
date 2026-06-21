using Backend.Data.DTO;
using Backend.Web.Services;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Web.Controllers;

[ApiController]
[Route("api/plan")]
public class GMPLController : ControllerBase
{
    private readonly GmplService gmplService;

    public GMPLController(GmplService gmplService)
    {
        this.gmplService = gmplService; // ← injiziertes Objekt direkt verwenden
    }

    [HttpGet("{date}")]
    public async Task<ActionResult<PlanResponseDto>> GetPlan(DateOnly date)
    {
        return Ok(new PlanResponseDto(DateOnly.FromDateTime(DateTime.Now), DateTimeOffset.UtcNow, []));
    }

    [HttpPost("")]
    public async Task<ActionResult<PlanResponseDto>> Plan([FromBody] PlanRequestDto request)
    {
        try
        {
            PlanResponseDto response = await gmplService.Solve(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{date}")]
    public async Task<ActionResult<PlanResponseDto>> DeletePlan(DateOnly date)
    {
        return Ok();
    }
}