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
	//private readonly GmplService gmplService;
	//public GMPLController(GmplService gmplService)
	//{
	//    this.gmplService = gmplService;
	//}

	[HttpGet("${date}")]
	public async Task<ActionResult<PlanResponseDto>> GetPlan(DateOnly date)
	{
		PlanResponseDto response = GetMockResponse(date);

		//PlanResponseDto response = await gmplService.Solve(request);
		return Ok(response);
	}

	[HttpPost("")]
	public async Task<ActionResult<PlanResponseDto>> Plan()
	{
		// TODO : Prevalidate
		try
        {
            PlanResponseDto response = GetMockResponse(DateOnly.FromDateTime(DateTime.Now));

            //PlanResponseDto response = await gmplService.Solve(request);
            return Ok(response);
        }
        catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			return StatusCode(500, ex.Message);
		}
	}

    private static PlanResponseDto GetMockResponse(DateOnly date)
    {
        // 1. Erstellung der Daten für Boot 1
        var taskSchedulesBoat1 = new List<TaskScheduleDto>
                {
                    new TaskScheduleDto(
                        new TimeOnly(8, 15, 0),
                        new TaskItemSummaryDto
                        {
                            Id = 12,
                            Name = "Wartung Generator",
                            DurationHours = 3.5f,
                            IsCompleted = false,
                            ExecutionIntervalStart = new DateOnly(2026, 6, 8),
                            ExecutionIntervalEnd = new DateOnly(2026, 6, 12)
                        }
                    ),
                    new TaskScheduleDto(
                        new TimeOnly(13, 0, 0),
                        new TaskItemSummaryDto
                        {
                            Id = 14,
                            Name = "Sichtprüfung Rotorblätter",
                            DurationHours = 1.5f,
                            IsCompleted = false,
                            ExecutionIntervalStart = new DateOnly(2026, 6, 9),
                            ExecutionIntervalEnd = new DateOnly(2026, 6, 10)
                        }
                    )
                };

        var boatSchedulesBoat1 = new List<BoatScheduleDto>
                {
                    new BoatScheduleDto(new TimeOnly(7, 30, 0), new TimeOnly(16, 0, 0))
                };

        var personsBoat1 = new List<PersonSummaryDto>
                {
                    new PersonSummaryDto { Id = 3, Firstname = "Max", Lastname = "Mustermann" },
                    new PersonSummaryDto { Id = 5, Firstname = "Erika", Lastname = "Müller" }
                };

        var toolsBoat1 = new List<TaskToolDto>
                {
                    new TaskToolDto { ToolId = 1, RequiredAmount = 2 },
                    new TaskToolDto { ToolId = 4, RequiredAmount = 1 }
                };


        // 2. Erstellung der Daten für Boot 2
        var taskSchedulesBoat2 = new List<TaskScheduleDto>
                {
                    new TaskScheduleDto(
                        new TimeOnly(9, 30, 0),
                        new TaskItemSummaryDto
                        {
                            Id = 22,
                            Name = "Softwareupdate Steuerung",
                            DurationHours = 4.0f,
                            IsCompleted = false,
                            ExecutionIntervalStart = new DateOnly(2026, 6, 9),
                            ExecutionIntervalEnd = new DateOnly(2026, 6, 9)
                        }
                    )
                };

        var boatSchedulesBoat2 = new List<BoatScheduleDto>
                {
                    new BoatScheduleDto(new TimeOnly(8, 45, 0), new TimeOnly(14, 30, 0))
                };

        var personsBoat2 = new List<PersonSummaryDto>
                {
                    new PersonSummaryDto { Id = 8, Firstname = "Hans", Lastname = "Dieter" }
                };

        var toolsBoat2 = new List<TaskToolDto>
                {
                    new TaskToolDto { ToolId = 2, RequiredAmount = 1 }
                };


        // 3. Zusammenführung in die vollständige PlanResponseDto
        var response = new PlanResponseDto(
            date,
            DateTimeOffset.UtcNow,
            new List<BoatPlanDto>
            {
                    new BoatPlanDto(taskSchedulesBoat1, boatSchedulesBoat1, personsBoat1, toolsBoat1),
                    new BoatPlanDto(taskSchedulesBoat2, boatSchedulesBoat2, personsBoat2, toolsBoat2)
            }
        );
        return response;
    }

    [HttpDelete("${date}")]
	public async Task<ActionResult<PlanResponseDto>> DeletePlan(DateOnly date)
	{
		return Ok();
	}


}
