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
    public GMPLController(GmplService gmplService)
    {
        this.gmplService = gmplService;
    }

    [HttpPost("")]
    public async Task<ActionResult<PlanResponseDto>> Plan([FromBody] PlanRequestDto request)
    {
        // TODO : Prevalidate
        try
        {
            PlanResponseDto response = await gmplService.Solve(request);
            return Ok(response);
        }
        catch (Exception ex) { 
            Console.WriteLine(ex.Message);
            return StatusCode(500,ex.Message);
        }
    }

}
