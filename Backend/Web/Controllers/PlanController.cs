using Backend.Data.DTO.Plan;
using Backend.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Web.Controllers;

[ApiController]
[Route("api/plan")]
public class PlanController : ControllerBase
{
    private readonly PlanService planService;

    public PlanController(PlanService planService)
    {
        this.planService = planService;
    }

    [HttpGet("{date}")]
    public async Task<ActionResult<PlanResponseDto>> GetPlan(DateOnly date)
    {
        var plan = await planService.GetPlan(date);
        return plan != null ? Ok(plan) : NotFound();
    }

    [HttpPost("{date}")]
    public async Task<ActionResult<PlanResponseDto>> Plan(DateOnly date, [FromBody] PlanRequestDto request)
    {
        if (request.MaxWorkHours == 0 || request.BoatNumber == 0 || request.BoatSpeed <= 0.0f) {
            return BadRequest();
        }
        
        var plan = await planService.GeneratePlan(date,request);
        return plan != null ? Ok(plan) : NotFound();
    }

} 