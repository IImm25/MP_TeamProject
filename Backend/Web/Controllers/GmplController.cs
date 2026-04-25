using Backend.Data.DTO;
using Backend.GMPL;
using Backend.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
namespace Backend.Web.Controllers;

[ApiController]
[Route("api/gmpl")]
public class GmplController : ControllerBase
{

    public GmplController()
    {
        
    }


    [HttpGet("")]
    public async Task<ActionResult<string>> Test()
    {
        await DataFileGenerator.SaveDataFile("sometext");
        return Ok("sometext");

    }


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
