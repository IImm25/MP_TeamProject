using Backend.Data.DTO;
using Backend.GMPL;
using Backend.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
namespace Backend.Web.Controllers;

[ApiController]
[Route("api/plan")]
public class GmplController : ControllerBase
{
    GmplService _gmplService;
    public GmplController(GmplService gmplService)
    {
        _gmplService = gmplService;
    }

    [HttpPost("")]
    public async Task<ActionResult<List<PlanResponseDto>>> Plan([FromBody] PlanRequestDto request)
    {

        if (request == null) return BadRequest("Wrong Inpout");

        List<PlanResponseDto> response = await _gmplService.CaculateGmplModel(request);

        if (response != null)
        {
            return Ok(response);
        }
        else return Ok("Planning failed.");
    }


    [HttpGet("/test")]
    public async Task<ActionResult<GmplResults>> Test()
    {

        var res = await _gmplService.TestGLPK();

        string path = await DataFileGenerator.SaveDataFile("sometext");
        return Ok(res);

    }


}
