using System.Collections.Concurrent;
using AutoMapper;
using Backend.Data.DTO.Plan;
using Backend.Data.Entitites;
using Backend.Data.Repositories;
using Backend.GMPL;

namespace Backend.Web.Services;

public class PlanService
{

    private readonly IMapper mapper;
    private readonly IGmplInputBuilder inputBuilder;
    private readonly IGlpkSolver solver;
    private static readonly ConcurrentDictionary<DateOnly, Task<Plan>> _activeJobs = new();

    private readonly IPersonRepository personRepo;
    private readonly IRepository<Qualification> qualiRepo;
    private readonly IRepository<Tool> toolRepo;
    private readonly IRepository<Location> locationRepo;
    private readonly IPlanRepository planRepo;
    private readonly ITaskItemRepository taskRepo;

    private static readonly TimeOnly startOfWorkDay = TimeOnly.FromTimeSpan(TimeSpan.FromHours(8));
    private static readonly string modFilePath = @"../GMPL/modell_new.mod";


    public PlanService(IMapper mapper, IGmplInputBuilder inputBuilder, IGlpkSolver solver, IPersonRepository personRepo, IRepository<Qualification> qualiRepo, IRepository<Tool> toolRepo, IRepository<Location> locationRepo, IPlanRepository planRepo, ITaskItemRepository taskRepo)
    {
        this.mapper = mapper;
        this.inputBuilder = inputBuilder;
        this.solver = solver;
        this.personRepo = personRepo;
        this.qualiRepo = qualiRepo;
        this.toolRepo = toolRepo;
        this.locationRepo = locationRepo;
        this.planRepo = planRepo;
        this.taskRepo = taskRepo;
    }

    public async Task<PlanResponseDto?> GetPlan(DateOnly date)
    {
        await taskRepo.SetAllScheduledTasksBeforeDateTimeAsCompleted(DateTime.UtcNow);
        return mapper.Map<PlanResponseDto?>(await planRepo.GetLatestPlanForDayAsync(date));
    }

    public async Task<PlanResponseDto?> GeneratePlan(DateOnly date, PlanRequestDto request)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        int totalDays = date.DayNumber - DateOnly.FromDateTime(now).DayNumber;

        if (totalDays < 0) // return historical plans may be null if there was never a plan for that day
        {
            return await GetPlan(date);
        }

        await taskRepo.SetAllScheduledTasksBeforeDateTimeAsCompleted(now);

        for (int i = 0; i <= totalDays; i++)
        {
            DateOnly day = today.AddDays(i);
            await _activeJobs.GetOrAdd(day, async (targetDay) =>
            {
                try
                {
                    var openTasks = await taskRepo.GetAllOpenUnscheduledTasksBeforeDateAsync(targetDay);
                    var scheduledTasks = await taskRepo.GetAllScheduledTasksByDateAsync(targetDay);
                    List<TaskSchedule> ongoingTasks = [];

                    if (targetDay == DateOnly.FromDateTime(now))
                    {
                        ongoingTasks = await taskRepo.GetAllOngoingTasksByDateAndTimeAsync(targetDay, TimeOnly.FromDateTime(now));

                        foreach (var task in scheduledTasks)
                        {
                            if (!ongoingTasks.Select(ts => ts.TaskItemId).Contains(task.Id))
                            {
                                openTasks.Add(task);
                            }
                        }
                    }
                    else
                    {
                        openTasks.AddRange(scheduledTasks);
                    }

                    
                    if (openTasks.Count == 0)
                    {
                        var plan = new Plan
                        {
                            CreatedAt = now,
                            Date = day,
                        };
                        await planRepo.AddAsync(plan);
                    }

                    
                    var persons = await personRepo.GetAllFullAsync();
                    var tools = await toolRepo.GetAllAsync();
                    var locations = await locationRepo.GetAllAsync();
                    var qualifications = await qualiRepo.GetAllAsync();

                    
                    var input = inputBuilder.CreateDataFileContent(request, openTasks, ongoingTasks, persons, qualifications, tools, locations);

                    string datFilePath = @"../GMPL/generated.txt";
                    
                    await File.WriteAllTextAsync(datFilePath, input);

                    var path = Path.GetFullPath(modFilePath);

                    
                    var res = await solver.SolveAsync(datFilePath, modFilePath, request.BoatNumber, targetDay);

                    if (res != null)
                    {
                        var plan = MapSolverResultToPlan(res, openTasks, targetDay);
                        await planRepo.AddAsync(plan);

                        return plan;
                    }

                    return null;
                }
                finally
                {
                    _activeJobs.TryRemove(targetDay, out _);
                }
            });

        }
        return await GetPlan(date);
    }

    public Plan MapSolverResultToPlan(SolverResult result, List<TaskItem> tasks, DateOnly date)
    {
        List<PlanBoat> boats = new List<PlanBoat>();
        foreach (var boat in result.Boats)
        {
            if (!boat.IsUsed) continue;

            List<BoatPerson> persons = new List<BoatPerson>();
            foreach (var personId in boat.PersonIds)
            {
                persons.Add(new BoatPerson
                {
                    BoatNumber = boat.BoatNumber,
                    PersonId = personId,
                });
            }

            List<BoatTool> tools = new List<BoatTool>();
            foreach (var (toolId, RequiredAmount) in boat.ToolQuantities)
            {
                tools.Add(new BoatTool
                {
                    BoatNumber = boat.BoatNumber,
                    ToolId = toolId,
                    RequiredAmount = RequiredAmount
                });
            }

            // Ensure all tasks are in correct order
            boat.Tasks.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            if (!boat.Tasks.First().IsFirst || !boat.Tasks.Last().IsLast)
            {
                throw new Exception($"GMPL Result Parsing Error: After sorting TaskSchedules for Boat ${boat.BoatNumber} isFirst or isLast do not match with solver results.");
            }

            List<TaskSchedule> taskSchedules = new List<TaskSchedule>();
            foreach (var schedule in boat.Tasks)
            {
                taskSchedules.Add(new TaskSchedule
                {
                    BoatNumber = boat.BoatNumber,
                    TaskItemId = schedule.TaskId,
                    StartTime = schedule.StartTime,
                });
            }

            List<BoatSchedule> boatSchedules = generateBoatSchedules(boat, tasks);

            boats.Add(new PlanBoat
            {
                BoatNumber = boat.BoatNumber,
                Persons = persons,
                Tools = tools,
                TaskSchedules = taskSchedules,
                BoatSchedules = boatSchedules
            });
        }

        var plan = new Plan
        {
            Boats = boats,
            Date = date,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        return plan;
    }

    private static List<BoatSchedule> generateBoatSchedules(SolverBoat boat, List<TaskItem> tasks)
    {


        List<BoatSchedule> boatSchedules = new List<BoatSchedule>();
        List<TaskItem> boatTasks = new List<TaskItem>();

        var prev = boat.Tasks.First();

        int tripNumber = 1;

        var firstTask = tasks.Where(t => t.Id == prev.TaskId).FirstOrDefault();
        if (firstTask == null) throw new Exception($"GMPL Result Parsing Error: TaskItem with id {prev.TaskId} does not exist");
        boatTasks.Add(firstTask);

        boatSchedules.Add(new BoatSchedule
        {
            BoatNumber = boat.BoatNumber,
            TripNumber = tripNumber++,
            OriginId = 1, // for multiple harbour get this from the results once the model is modified
            DestinationId = firstTask.LocationId,
            Departure = startOfWorkDay,
            Arrival = prev.StartTime
        });

        for (int i = 1; i < boat.Tasks.Count; i++)
        {
            var curr = boat.Tasks[i];

            var pTask = boatTasks[i - 1];
            var cTask = tasks.Where(t => t.Id == curr.TaskId).FirstOrDefault();
            if (cTask == null) throw new Exception($"GMPL Result Parsing Error: TaskItem with id {curr.TaskId} does not exist");
            boatTasks.Add(cTask);

            if (pTask.LocationId == cTask.LocationId) continue; // if consecutive Tasks are at the same location no Trip is needed

            boatSchedules.Add(new BoatSchedule
            {
                BoatNumber = boat.BoatNumber,
                TripNumber = tripNumber++,
                OriginId = pTask.LocationId,
                DestinationId = cTask.LocationId,
                Departure = prev.StartTime.AddHours(pTask.DurationHours),
                Arrival = curr.StartTime
            });

            prev = curr;
        }

        var lastTask = boatTasks.Last();

        boatSchedules.Add(new BoatSchedule
        {
            BoatNumber = boat.BoatNumber,
            TripNumber = tripNumber++,
            OriginId = lastTask.LocationId,
            DestinationId = 1, // for multiple harbour get this from the results once the model is modified
            Departure = prev.StartTime.AddHours(lastTask.DurationHours),
            Arrival = prev.StartTime.AddHours(lastTask.DurationHours + prev.TravelToHarbor.TotalHours)
        });
        return boatSchedules;
    }




}
