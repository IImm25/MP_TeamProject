using Backend.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
namespace Backend.Web.Controllers;

[ApiController]
[Route("/[controller]")]
public class GmplController : ControllerBase
{
    public GmplController()
    {
        
    }

    [HttpPost("boats")]
    public async Task<ActionResult<List<TaskItem>>> PostCalculationRequest([FromBody] List<TaskItem> tasks, [FromBody] List<Person> people)
    {
        if (tasks.Count != 0 && people.Count != 0)
        {
            GmplService gmpl = new GmplService(tasks);  
            var res = await gmpl.WriteDatafile(people);



            return Ok();
        }
        else return UnprocessableEntity();
    }
}
