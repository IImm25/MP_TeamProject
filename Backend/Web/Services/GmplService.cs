using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.Entitites;
using Backend.Data.Repositories;
using Backend.GMPL;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;

namespace Backend.Web.Services;



public class GmplService
{
    private IntPtr prob = nint.Zero;
    private IntPtr tran = nint.Zero;
    private static string columnRegex = @"^([A-Za-z_]\w*)(?:\[(.*?)\])?$";
    private static string modFile = Path.Combine(AppContext.BaseDirectory, "GMPL", "modell.mod");

    private readonly PersonService personService;
    private readonly TaskItemService taskItemService;
    private readonly QualificationService qualificationService;
    private readonly ToolService toolService;
    private readonly TurbineService turbineService;
    private readonly IMapper mapper;
    private readonly IRepository<Plan> _planRepository;
    private readonly IPlanRepository repository;

    private static (string VarName, List<int> Indices) parseColumnName(string colName)
    {
        var match = Regex.Match(colName, columnRegex);

        if (!match.Success)
            return ("", []);

        string name = match.Groups[1].Value;
        string indexRaw = match.Groups[2].Value;

        var indices = new List<int>();

        if (!string.IsNullOrWhiteSpace(indexRaw))
        {
            foreach (var part in indexRaw.Split(','))
            {
                var cleaned = Regex.Replace(part.Trim(), @"^[A-Za-z]+_", "");

                if (int.TryParse(cleaned, out int id))
                    indices.Add(id - 1); // From GMPL 1 indexed to 0 indexed
            }
        }
        return (name, indices);
    }

    // Konstruktor mit DI-Parametern
    public GmplService(
        PersonService personService,
        TaskItemService taskItemService,
        QualificationService qualificationService,
        ToolService toolService,
        TurbineService turbineService,
        IMapper mapper,
        IRepository<Plan> planRepository,
        IPlanRepository repository)
    {
        this.personService = personService;
        this.taskItemService = taskItemService;
        this.qualificationService = qualificationService;
        this.toolService = toolService;
        this.turbineService = turbineService;
        this.mapper = mapper;
        this.repository = repository;

        prob = GLPKDllWrapper.glp_create_prob();
        tran = GLPKDllWrapper.glp_mpl_alloc_wksp();
        if (GLPKDllWrapper.glp_mpl_read_model(tran, modFile, 0) != 0)
        {
            throw new Exception($"Error while reading GMPL model file: '{Path.GetFullPath(modFile)}'");
        }

        _planRepository = planRepository;
    }

    ~GmplService()
    {
        GLPKDllWrapper.glp_mpl_free_wksp(tran);
        GLPKDllWrapper.glp_delete_prob(prob);
        //Glpk.glp_free_env();
    }

    public async Task<PlanResponseDto> GetPlan(DateOnly date)
    {
        var plans = await repository.GetAllAsync();

        Plan p = plans.Where(x => x.Date == date).FirstOrDefault();

        if (p == null) return null;
        else
        {
            var response = await MapPlanToResponseDto(p);
            return response;
        }
    }

    private async Task<PlanResponseDto> MapPlanToResponseDto(Plan plan)
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




    public async Task<PlanResponseDto> Solve(PlanRequestDto request)
    {

        List<int> taskIds = (await taskItemService.GetAll()).Select(t => t.Id).ToList();
        List<int> personIds = (await personService.GetAll()).Select(p => p.Id).ToList();
        List<int> toolIds = (await toolService.GetAll()).Select(t => t.Id).ToList();
        List<TurbineResponseDto> turbines = await turbineService.GetAll();
        List<int> qualIds = await GetUsedQualificationIds(taskIds, personIds);

        List<TaskItemDetailDto> taskItemDetails = new List<TaskItemDetailDto>();
        foreach (var taskId in taskIds)
        {
            var task = await taskItemService.GetTaskItem(taskId);
            if (task != null) taskItemDetails.Add(task);
        }


        List<PlanBoat> existingPlans = new List<PlanBoat>();
        List<BoatPerson> existingAssignments = new List<BoatPerson>();
        List<BoatTool> existingTools = new List<BoatTool>();

        List<Plan> plans = await _planRepository.GetAllAsync();
        var latestPlan = plans.LastOrDefault();
        if (latestPlan != null && latestPlan.PlanBoats != null)
        {
            existingPlans = latestPlan.PlanBoats.ToList();
            existingAssignments = latestPlan.PlanBoats.SelectMany(b => b.Persons).ToList();
            existingTools = latestPlan.PlanBoats.SelectMany(b => b.Tools).ToList();
        }

        // Danach die Data-Datei mit diesen (gefüllten) Listen erstellen
        string datFileText = await CreateDataFileAsync(
            request,
            qualIds,
            taskIds,
            personIds,
            toolIds,
            turbines,
            existingPlans,      // jetzt ggf. gefüllt
            existingAssignments,
            existingTools,
            taskItemDetails
        );

        string datFile = Path.GetTempFileName();
        File.WriteAllText(datFile, datFileText);

        try
        {
            if (GLPKDllWrapper.glp_mpl_read_data(tran, datFile) != 0)
                throw new Exception($"Fehler beim Lesen der GMPL-Data-Datei: '{datFile}'");
            File.Delete(datFile);


            if (GLPKDllWrapper.glp_mpl_generate(tran, nint.Zero) != 0)
                throw new Exception("Fehler beim Generieren des GMPL-Modells.");

            GLPKDllWrapper.glp_mpl_build_prob(tran, prob);

            var smcp = new GLPKDllWrapper.glp_smcp();
            GLPKDllWrapper.glp_init_smcp(ref smcp);
            smcp.msg_lev = GLPKDllWrapper.GLP_MSG_OFF;
            int simplexErr = GLPKDllWrapper.glp_simplex(prob, ref smcp);


            var iocp = new GLPKDllWrapper.glp_iocp();
            GLPKDllWrapper.glp_init_iocp(ref iocp);
            iocp.msg_lev = GLPKDllWrapper.GLP_MSG_OFF;
            int mipError = GLPKDllWrapper.glp_intopt(prob, ref iocp);

            if (simplexErr != 0 || mipError != 0)
            {
                //var (toolDiff, qualDiff) = await Validate(taskIds, personIds, toolIds, qualIds);
                return new PlanResponseDto(DateOnly.FromDateTime(request.Time), DateTimeOffset.UtcNow, new List<BoatPlanDto>());
            }

            GLPKDllWrapper.glp_mpl_postsolve(tran, prob, GLPKDllWrapper.GLP_MIP);


            var taskOnBoat = new HashSet<int>[request.BoatNumber];
            var personOnBoat = new HashSet<int>[request.BoatNumber];
            var toolOnBoat = new Dictionary<int, int>[request.BoatNumber];
            var boatUsage = new bool[request.BoatNumber];
            var startTimes = new Dictionary<(int boat, int task), double>(); // Stunden seit Tagesbeginn
            var travelToHarborValues = new Dictionary<(int boat, int task), double>(); // Stunden

            for (int i = 0; i < request.BoatNumber; i++)
            {
                taskOnBoat[i] = new HashSet<int>();
                personOnBoat[i] = new HashSet<int>();
                toolOnBoat[i] = new Dictionary<int, int>();
            }

            int numCols = GLPKDllWrapper.glp_get_num_cols(prob);

            for (int j = 1; j <= numCols; j++)
            {
                string colName = GLPKDllWrapper.GetColName(prob, j);
                double rawVal = GLPKDllWrapper.glp_mip_col_val(prob, j);
                int val = (int)Math.Round(rawVal);
                var (varName, indices) = parseColumnName(colName);

                switch (varName)
                {
                    case "taskOnBoat":
                        if (indices.Count >= 1 && val != 0)
                            taskOnBoat[indices[0]].Add(indices[1]);
                        break;
                    case "personOnBoat":
                        if (indices.Count >= 1 && val != 0)
                            personOnBoat[indices[0]].Add(indices[1]);
                        break;
                    case "toolOnBoat":
                        if (indices.Count >= 1 && val > 0)
                            toolOnBoat[indices[0]][indices[1]] = val;
                        break;
                    case "boatUsage":
                        if (indices.Count >= 1)
                            boatUsage[indices[0]] = val != 0;
                        break;
                    case "startTime":
                        if (indices.Count >= 1 && rawVal > 0)
                            startTimes[(indices[0], indices[1])] = rawVal + 8; // 8 Uhr arbeitsstart
                        break;
                    case "travelToHarbor":
                        if (indices.Count >= 1 && rawVal > 0)
                            travelToHarborValues[(indices[0], indices[1])] = rawVal;
                        break;
                    // Folgende Variablen werden nur für das Gantt-Display im GMPL gebraucht,
                    // in C# aber nicht weiter verarbeitet => einfach ignorieren
                    case "after":
                    case "isFirst":
                    case "isLast":
                    case "travelBetween":
                    case "lastTaskStart":
                        break;

                    default:
                        // Leer- oder unbekannte Spalten ohne Index ignorieren (z.B. GMPL-interne Hilfsvariablen)
                        if (indices.Count > 0)
                            throw new Exception($"Unbekannte Spalte im GMPL-Ergebnis: {varName}[{string.Join(",", indices)}]");
                        break;
                }
            }
            
            List<BoatPlanDto> boats = new List<BoatPlanDto>();
            for (int i = 0; i < request.BoatNumber; i++)
            {
                if (!boatUsage[i]) continue;

                List<PersonSummaryDto> persons = new List<PersonSummaryDto>();
                foreach (int pid in personOnBoat[i])
                {
                    var p = await personService.GetPerson(pid + 1);
                    if (p != null) persons.Add(mapper.Map<PersonSummaryDto>(p));
                }

                List<TaskToolDto> tools = toolOnBoat[i]
                    .Select(kv => new TaskToolDto { ToolId = kv.Key + 1, RequiredAmount = kv.Value })
                    .ToList();
               
                List<TaskScheduleDto> tasks = new List<TaskScheduleDto>();
                foreach (int tid in taskOnBoat[i])
                {
                    var t = await taskItemService.GetTaskItem(tid + 1);
                    if (t == null) continue;

                    TimeOnly startTime = TimeOnly.MinValue;
                    if (startTimes.TryGetValue((i, tid), out double hours))
                    {
                        int totalMinutes = (int)Math.Round(hours * 60);
                        startTime = new TimeOnly(totalMinutes / 60, totalMinutes % 60);
                    }

                    tasks.Add(new TaskScheduleDto(startTime, mapper.Map<TaskItemSummaryDto>(t)));
                }

                tasks.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

                // Departure: Der erste Task startet nach der Hafenausfahrt.
                // Da GMPL startTime[first] >= travelTime[harbor, location] gilt,
                // ist die Abfahrt = startTime des ersten Tasks (der früheste nach dem Sortieren).
                // Falls keine Tasks vorhanden (sollte nicht vorkommen), Fallback auf MinValue.
                TimeOnly departure = tasks.Count > 0 ? tasks.First().StartTime : TimeOnly.MinValue;

                // Arrival: letzter Task + dessen Dauer + Rückfahrt zum Hafen (travelToHarbor)
                TimeOnly arrival = TimeOnly.MinValue;
                if (tasks.Count > 0)
                {
                    var lastTask = tasks.Last();
                    int lastTaskId = taskOnBoat[i].First(tid =>
                    {
                        if (!startTimes.TryGetValue((i, tid), out double h)) return false;
                        int m = (int)Math.Round(h * 60);
                        return new TimeOnly(m / 60, m % 60) == lastTask.StartTime;
                    });

                    double lastDuration = lastTask.Task.DurationHours;
                    double returnTravel = travelToHarborValues.TryGetValue((i, lastTaskId), out double rv) ? rv : 0.0;

                    double arrivalHours = lastTask.StartTime.Hour
                                        + lastTask.StartTime.Minute / 60.0
                                        + lastDuration
                                        + returnTravel;

                    int arrivalMinutes = (int)Math.Round(arrivalHours * 60);
                    arrival = new TimeOnly(arrivalMinutes / 60 % 24, arrivalMinutes % 60);
                }

                List<BoatScheduleDto> boatSchedules = new List<BoatScheduleDto>
                {
                    new BoatScheduleDto(departure, arrival)
                };

                boats.Add(new BoatPlanDto(tasks, boatSchedules, persons, tools));
            }

            //var (toolDiff, qualDiff) = await Validate(taskIds, personIds, toolIds, qualIds);
            
            PlanResponseDto response = new PlanResponseDto(DateOnly.FromDateTime(request.Time), DateTimeOffset.UtcNow, boats);
            List<PlanBoat> planBoats = new List<PlanBoat>();
            for (int i = 0; i < boats.Count; i++)
            {
                int boatNumber = i + 1; // 1-basiert
                BoatPlanDto boatDto = boats[i];

                List<BoatPerson> boatPersons = boatDto.Persons
                    .Select(p => new BoatPerson
                    {
                        BoatNumber = boatNumber,
                        PersonId = p.Id
                    }).ToList();

                List<BoatTool> boatTools = boatDto.Tools
                    .Select(t => new BoatTool
                    {
                        BoatNumber = boatNumber,
                        ToolId = t.ToolId,
                        RequiredAmount = t.RequiredAmount
                    }).ToList();

                List<BoatSchedule> boatSchedules = boatDto.BoatSchedules
                    .Select((s, idx) => new BoatSchedule
                    {
                        BoatNumber = boatNumber,
                        TripNumber = idx + 1,
                        Departure = s.Departure,
                        Arrival = s.Arrival
                    }).ToList();

                List<TaskSchedule> taskSchedules = boatDto.TaskSchedules
                    .Select(ts => new TaskSchedule
                    {
                        BoatNumber = boatNumber,
                        TaskId = ts.Task.Id,
                        TaskItem = null!, // Navigation ignorieren, nur IDs setzen
                        StartTime = ts.StartTime
                    }).ToList();

                planBoats.Add(new PlanBoat
                {
                    BoatNumber = boatNumber,
                    Persons = boatPersons,
                    Tools = boatTools,
                    BoatSchedules = boatSchedules,
                    TaskSchedules = taskSchedules
                });
            }

            Plan entity = new Plan(
                DateOnly.FromDateTime(request.Time),
                DateTimeOffset.UtcNow,
                planBoats
            );


            await _planRepository.AddAsync( entity);
            return response;
        }
        catch (Exception ex)
        {
            if (File.Exists(datFile)) File.Delete(datFile);
            throw new Exception($"Fehler in Solve: {ex.Message}", ex);
        }
    }
    private async Task<string> CreateDataFileAsync(
        PlanRequestDto info,
        List<int> qualIds,
        List<int> taskIds,
        List<int> personIds,
        List<int> toolIds,
        List<TurbineResponseDto> turbines,
        List<PlanBoat> existingPlans,
        List<BoatPerson> existingAssignments,
        List<BoatTool> existingTools,
        List<TaskItemDetailDto> taskItemDetails)
    {
        var sb = new StringBuilder();

        sb.AppendLine("data;");
        sb.AppendLine();

        // Sets
        sb.AppendLine($"set TASKS := {string.Join(" ", taskIds.Select(id => $"ta_{id}"))};");
        sb.AppendLine($"set PEOPLE := {string.Join(" ", personIds.Select(id => $"p_{id}"))};");
        sb.AppendLine($"set QUALIS := {string.Join(" ", qualIds.Select(id => $"q_{id}"))};");
        sb.AppendLine($"set TOOLS := {string.Join(" ", toolIds.Select(id => $"to_{id}"))};");

        string places = "w_0 " + string.Join(" ", turbines.Select(t => $"w_{t.Id}"));
        sb.AppendLine($"set PLACES := {places};");
        sb.AppendLine();

        // Parameter
        sb.AppendLine($"param maxWorkingHours := {info.MaxWorkHours.ToString(CultureInfo.InvariantCulture)};");
        sb.AppendLine($"param amountBoats := {info.BoatNumber};");
        sb.AppendLine($"param drivingSpeed := {info.BoatSpeed.ToString(CultureInfo.InvariantCulture)};");
        sb.AppendLine();

        // Duration
        sb.AppendLine("param duration :=");
        foreach (var taskId in taskIds)
        {
            var taskItem = taskItemDetails.FirstOrDefault(t => t.Id == taskId);
            sb.AppendLine($"\tta_{taskId} {taskItem?.DurationHours.ToString(CultureInfo.InvariantCulture) ?? "0"}");
        }
        sb.AppendLine(";");
        sb.AppendLine();

        // Person -> Qualification
        sb.AppendLine($"param hasQuali : {string.Join(" ", qualIds.Select(id => $"q_{id}"))} :=");
        foreach (var personId in personIds)
        {
            var qualMask = await qualificationService.GetPersonQualificationMask(personId, qualIds);
            var maskStr = qualMask.Select(b => b ? "1" : "0");
            sb.AppendLine($"\tp_{personId} {string.Join(" ", maskStr)}");
        }
        sb.AppendLine(";");
        sb.AppendLine();

        // Task -> Qualification
        sb.AppendLine($"param requiredQualis : {string.Join(" ", qualIds.Select(id => $"q_{id}"))} :=");
        foreach (var taskId in taskIds)
        {
            var qualReq = await qualificationService.GetTaskQualificationRequirements(taskId, qualIds);
            sb.AppendLine($"\tta_{taskId} {string.Join(" ", qualReq)}");
        }
        sb.AppendLine(";");
        sb.AppendLine();

        // Task -> Tools
        sb.AppendLine($"param requiredTools : {string.Join(" ", toolIds.Select(id => $"to_{id}"))} :=");
        foreach (var taskId in taskIds)
        {
            var toolReqs = await toolService.GetTaskToolRequirements(taskId, toolIds);
            sb.AppendLine($"\tta_{taskId} {string.Join(" ", toolReqs)}");
        }
        sb.AppendLine(";");
        sb.AppendLine();

        // Stock Tools
        sb.AppendLine("param stockTools :=");
        foreach (var toolId in toolIds)
        {
            var tool = await toolService.GetTool(toolId);
            sb.AppendLine($"\tto_{toolId} {tool?.AvailableStock ?? 0}");
        }
        sb.AppendLine(";");
        sb.AppendLine();

        // Priority
        sb.AppendLine("param taskPrio :=");
        foreach (var taskId in taskIds)
        {
            var taskItem = taskItemDetails.FirstOrDefault(t => t.Id == taskId);
            float priority = CalculatePriority(info.Time, taskItem);
            sb.AppendLine($"\tta_{taskId} {priority.ToString(CultureInfo.InvariantCulture)}");
        }
        sb.AppendLine(";");
        sb.AppendLine();

        // Task Locations
        sb.AppendLine("param taskLocation :=");
        foreach (var task in taskItemDetails)
        {
            sb.AppendLine($"\tta_{task.Id} w_{task.Location.Id}");
        }
        sb.AppendLine(";");
        sb.AppendLine();

        // Distance Matrix
        var turbineEntities = turbines.Select(t => new Turbine(t.Name, t.Latitude, t.Longitude) { Id = t.Id }).ToList();
        float[,] distances = CalculateTurbineDistances(turbineEntities, new Harbor(54.433304330384395f, 13.031369793515506f));

        var locationNames = new List<string> { "w_0" };
        locationNames.AddRange(turbines.Select(t => $"w_{t.Id}"));

        sb.AppendLine($"param distance : {string.Join(" ", locationNames)} :=");
        for (int i = 0; i < locationNames.Count; i++)
        {
            sb.Append($"\t{locationNames[i]}");
            for (int j = 0; j < locationNames.Count; j++)
            {
                sb.Append($" {distances[i, j].ToString(CultureInfo.InvariantCulture)}");
            }
            sb.AppendLine();
        }
        sb.AppendLine(";");
        sb.AppendLine();

        // Fixed Boat
        sb.AppendLine("param fixedBoat :=");
        foreach (var taskId in taskIds)
        {
            var existingPlan = existingPlans.FirstOrDefault(p => p.TaskSchedules.Any(ts => ts.TaskId == taskId));
            int boatNumber = existingPlan?.BoatNumber ?? 0;
            sb.AppendLine($"\tta_{taskId} {boatNumber}");
        }
        sb.AppendLine(";");
        sb.AppendLine();

        // Fixed Order
        sb.AppendLine("param fixedOrder :=");
        foreach (var taskId in taskIds)
        {
            var existingSchedule = existingPlans
                .SelectMany(p => p.TaskSchedules)
                .FirstOrDefault(ts => ts.TaskId == taskId);

            if (existingSchedule != null)
            {
                // TODO: Order aus existingSchedule holen
                int order = 1;
                sb.AppendLine($"\tta_{taskId} {order}");
            }
        }
        sb.AppendLine(";");
        sb.AppendLine();

        // Fixed Person
        sb.AppendLine("param fixedPerson :=");
        foreach (var personId in personIds)
        {
            var existingAssignment = existingAssignments.FirstOrDefault(a => a.PersonId == personId);
            int boatNumber = existingAssignment?.BoatNumber ?? 0;
            sb.AppendLine($"\tp_{personId} {boatNumber}");
        }
        sb.AppendLine(";");
        sb.AppendLine();

        // Fixed Tool Amount
        // wird in fixed boat noch genauer definiert als zwei listen 
        sb.AppendLine($"param fixedToolAmount : {string.Join(" ", toolIds.Select(id => $"to_{id}"))} :=");
        for (int boatNum = 1; boatNum <= info.BoatNumber; boatNum++)
        {
            sb.Append($"\t{boatNum}");
            foreach (var toolId in toolIds)
            {
                var existingTool = existingTools.FirstOrDefault(bt =>
                    bt.BoatNumber == boatNum && bt.ToolId == toolId);
                int amount = existingTool?.RequiredAmount ?? 0;
                sb.Append($" {amount}");
            }
            sb.AppendLine();
        }
        sb.AppendLine(";");
        sb.AppendLine();

        sb.AppendLine("end;");

        return sb.ToString();
    }

    private float CalculatePriority(DateTime currentTime, TaskItemDetailDto task)
    {
        if (currentTime >= task.ExecutionIntervalStart.ToDateTime(TimeOnly.MinValue))
        {
            var start = task.ExecutionIntervalStart.ToDateTime(TimeOnly.MinValue);
            var end = task.ExecutionIntervalEnd.ToDateTime(TimeOnly.MinValue);

            if (end == start) return 1.0f;

            float priority = (float)((currentTime - start).TotalHours / (end - start).TotalHours);
            return Math.Clamp(priority, 0.0f, 1.0f);
        }

        return 0.0f;
    }

    private record Harbor(float Latitude, float Longitude);

    private float[,] CalculateTurbineDistances(List<Turbine> turbines, Harbor harbor)
    {
        var allLocations = new List<Turbine> { new Turbine("Harbor", harbor.Latitude, harbor.Longitude) { Id = 0 } };
        allLocations.AddRange(turbines);

        float[,] distances = new float[allLocations.Count, allLocations.Count];

        for (int i = 0; i < allLocations.Count; i++)
        {
            for (int j = i + 1; j < allLocations.Count; j++)
            {
                float dist = Haversine2Km(
                    allLocations[i].Latitude, allLocations[i].Longitude,
                    allLocations[j].Latitude, allLocations[j].Longitude);

                distances[i, j] = dist;
                distances[j, i] = dist;
            }
        }

        return distances;
    }

    private float Haversine2Km(float latitude1, float longitude1, float latitude2, float longitude2)
    {
        const float earthRadius = 6371f;

        float diffLatitude = ToRad(latitude2 - latitude1);
        float diffLongitude = ToRad(longitude2 - longitude1);

        float a = MathF.Sin(diffLatitude / 2) * MathF.Sin(diffLatitude / 2)
                + MathF.Cos(ToRad(latitude1)) * MathF.Cos(ToRad(latitude2))
                * MathF.Sin(diffLongitude / 2) * MathF.Sin(diffLongitude / 2);

        float c = 2 * MathF.Atan2(MathF.Sqrt(a), MathF.Sqrt(1 - a));

        return earthRadius * c;
    }

    private float ToRad(float degrees) => degrees * (MathF.PI / 180f);

    private async Task<List<int>> GetUsedQualificationIds(List<int> taskIds, List<int> personIds)
    {
        var allQualIds = await qualificationService.GetAllIds();
        var used = new HashSet<int>();

        foreach (var taskId in taskIds)
        {
            var req = await qualificationService.GetTaskQualificationRequirements(taskId, allQualIds);

            for (int i = 0; i < allQualIds.Count; i++)
            {
                if (req[i] > 0) used.Add(allQualIds[i]);
            }
        }

        foreach (var personId in personIds)
        {
            var mask = await qualificationService.GetPersonQualificationMask(personId, allQualIds);

            for (int i = 0; i < allQualIds.Count; i++)
            {
                if (mask[i]) used.Add(allQualIds[i]);
            }
        }

        return used.OrderBy(x => x).ToList();
    }

    private async Task<(List<RequirementDiffDto> Tools, List<RequirementDiffDto> Quals)> Validate(
        List<int> taskIds,
        List<int> personIds,
        List<int> toolIds,
        List<int> qualIds)
    {
        toolIds = toolIds.Distinct().ToList();

        // Required Qualifications
        var requiredQuals = qualIds.ToDictionary(id => id, _ => 0);

        foreach (var taskId in taskIds)
        {
            var req = await qualificationService.GetTaskQualificationRequirements(taskId, qualIds);

            for (int i = 0; i < qualIds.Count; i++)
            {
                requiredQuals[qualIds[i]] += req[i];
            }
        }

        // Available Qualifications
        var availableQuals = qualIds.ToDictionary(id => id, _ => 0);

        foreach (var personId in personIds)
        {
            var mask = await qualificationService.GetPersonQualificationMask(personId, qualIds);

            for (int i = 0; i < qualIds.Count; i++)
            {
                if (mask[i])
                    availableQuals[qualIds[i]] += 1;
            }
        }

        // Required Tools
        var requiredTools = toolIds.ToDictionary(id => id, _ => 0);

        foreach (var taskId in taskIds)
        {
            var req = await toolService.GetTaskToolRequirements(taskId, toolIds);

            for (int i = 0; i < toolIds.Count; i++)
            {
                requiredTools[toolIds[i]] += req[i];
            }
        }

        // Available Tools
        var availableTools = toolIds.ToDictionary(id => id, _ => 0);

        foreach (var toolId in toolIds)
        {
            var tool = await toolService.GetTool(toolId);
            availableTools[toolId] = tool?.AvailableStock ?? 0;
        }

        // Diff Output
        var toolDiff = toolIds
            .Select(id => new RequirementDiffDto(
                id,
                requiredTools[id],
                availableTools[id]))
            .Where(x => x.Required > 0 || x.Available > 0)
            .ToList();

        var qualDiff = qualIds
            .Select(id => new RequirementDiffDto(
                id,
                requiredQuals[id],
                availableQuals[id]))
            .Where(x => x.Required > 0 || x.Available > 0)
            .ToList();

        return (toolDiff, qualDiff);
    }

}
