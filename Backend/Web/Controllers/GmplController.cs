using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers;

[ApiController]
[Route("/[controller]")]
public class GmplController : ControllerBase
{
    public GmplController()
    {
        

    }

    [HttpPost("gmpl")]
    public async void PostCalculationReqest()
    {

    }
}
