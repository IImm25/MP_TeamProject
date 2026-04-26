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
        else return BadRequest();
    }




    //[HttpGet("")]
    //public async Task<ActionResult<string>> Test()
    //{
    //    string path = await DataFileGenerator.SaveDataFile("sometext");
    //    return Ok(path);

    //}


    //[HttpPost("calculate")]
    //public async Task<ActionResult<List<TaskItem>>> PostCalculationRequest([FromBody]List<TaskItemCreateDto> tasks)
    //{
    //    if (tasks.Count != 0 && people.Count != 0)
    //    {
    //        GmplService gmpl = new GmplService(tasks);
    //        var res = await gmpl.WriteDatafile(people);



    //        return Ok();
    //    }
    //    else return UnprocessableEntity();
    //}
}
