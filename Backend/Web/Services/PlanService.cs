using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.Entitites;
using Backend.Data.Repositories;

namespace Backend.Web.Services;

public class PlanService
{
    private readonly IPlanRepository repository;
    private readonly IRepository<PlanQuery> planQueryRepository;
    private readonly IMapper mapper;
    public PlanService(IPlanRepository repository, IRepository<PlanQuery> planQueryRepository, IMapper mapper)
    {
        this.repository = repository;
        this.planQueryRepository = planQueryRepository;
        this.mapper = mapper;
    }
    public async Task<PlanResponseDto> GetPlan(DateOnly date)
    {
        try
        {
            var plans = await repository.GetAllFullAsync();

            Plan? p = plans.Where(x => x.Date == date).OrderByDescending(x => x.CreatedAt).FirstOrDefault();

            if (p == null) return null;
            else
            {
                var response = MapPlanToResponseDto(p);
                return response;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error: " + ex.Message);
        }

    }

    public async Task<PlanQuery?> GetPlanQueryAsync(DateOnly date)
    {
        try
        {
            List<PlanQuery> queries = await planQueryRepository.GetAllAsync();
            PlanQuery? fitlered = queries.Where(x => x.PlanDate == date).FirstOrDefault();
            return fitlered;
        }
        catch (Exception ex)
        {
            throw new Exception("Error: " + ex.Message);
        }
    }
    private PlanResponseDto MapPlanToResponseDto(Plan plan)
    {
        List<BoatPlanDto> boatDtos = new List<BoatPlanDto>();

        foreach (PlanBoat planBoat in plan.PlanBoats)
        {
            List<PersonSummaryDto> persons = planBoat.Persons
                .Select(bp => mapper.Map<PersonSummaryDto>(bp.Person))
                .ToList();

            List<TaskToolDto> tools = planBoat.Tools
                .Select(bt => new TaskToolDto
                {
                    ToolId = bt.ToolId,
                    RequiredAmount = bt.RequiredAmount
                }).ToList();

            List<TaskScheduleDto> taskSchedules = planBoat.TaskSchedules
                .Select(ts => new TaskScheduleDto(
                    ts.StartTime,
                    mapper.Map<TaskItemSummaryDto>(ts.TaskItem)
                )).ToList();

            List<BoatScheduleDto> boatSchedules = planBoat.BoatSchedules
                .Select(bs => new BoatScheduleDto(bs.Departure, bs.Arrival))
                .ToList();

            boatDtos.Add(new BoatPlanDto(taskSchedules, boatSchedules, persons, tools));
        }

        return new PlanResponseDto(plan.Date, plan.CreatedAt, boatDtos);
    }
}
