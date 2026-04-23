using Microsoft.AspNetCore.Mvc;
namespace Backend.Web.Controllers;

[ApiController]
[Route("/[controller]")]
public class GmplController : ControllerBase
{
    public GmplController()
    {
        

    }

    [HttpPost("boats")]
    public async void PostCalculationRequest()
    {
        
    }
}
