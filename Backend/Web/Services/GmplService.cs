namespace Backend.Web.Services;

using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Backend.Data.DTO;
using Backend.GMPL;

public class GmplService
{
    private IntPtr prob = nint.Zero;
    private IntPtr tran = nint.Zero;

    private static string columnRegex = @"^([A-Za-z_]\w*)(?:\[(.*?)\])?$";
    private static string modFile = @"..\GMPL\modell.mod";

    //private readonly DataFileGeneratorService dataFileGeneratorService;
    private readonly PersonService personService;
    private readonly TaskItemService taskItemService;
    private readonly QualificationService qualificationService;
    private readonly ToolService toolService;
    private readonly IMapper mapper;

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

    public GmplService(PersonService personService, TaskItemService taskItemService, QualificationService qualificationService, ToolService toolService, IMapper mapper)
    {
        this.personService = personService;
        this.taskItemService = taskItemService;
        this.qualificationService = qualificationService;
        this.toolService = toolService;
        this.mapper = mapper;

        prob = GLPKDllWrapper.glp_create_prob();
        tran = GLPKDllWrapper.glp_mpl_alloc_wksp();

        if (GLPKDllWrapper.glp_mpl_read_model(tran, modFile, 0) != 0)
        {
            throw new Exception($"Error while reading GMPL model file: '{Path.GetFullPath(modFile)}'");
        }
    }

    ~GmplService()
    {
        GLPKDllWrapper.glp_mpl_free_wksp(tran);
        GLPKDllWrapper.glp_delete_prob(prob);
        //Glpk.glp_free_env();
    }

    public async Task<PlanResponseDto> Solve(PlanRequestDto request)
    {
        List<int> qualIds = await GetUsedQualificationIds(request.TaskItemIds, request.PersonIds);
        string datFileText = await CreateDataFileAsync(request, qualIds);
        string datFile = Path.GetTempFileName();
        File.WriteAllText(datFile, datFileText);

        if (GLPKDllWrapper.glp_mpl_read_data(tran, datFile) != 0)
        {
            throw new Exception($"Error while reading GMPL data File: '{Path.GetFullPath(datFile)}'");
        }
        File.Delete(datFile);   // cleanup temp dat file on sucess

        if (GLPKDllWrapper.glp_mpl_generate(tran, nint.Zero) != 0)
        {
            throw new Exception($"Error generating GMPL Modell.");
        }

        GLPKDllWrapper.glp_mpl_build_prob(tran, prob);

        // Simplex
        var smcp = new GLPKDllWrapper.glp_smcp();
        GLPKDllWrapper.glp_init_smcp(ref smcp);
        smcp.msg_lev = GLPKDllWrapper.GLP_MSG_OFF;

        int simplexErr = GLPKDllWrapper.glp_simplex(prob, ref smcp);

        // MIP
        var iocp = new GLPKDllWrapper.glp_iocp();
        GLPKDllWrapper.glp_init_iocp(ref iocp);
        iocp.msg_lev = GLPKDllWrapper.GLP_MSG_OFF;

        int mipError = GLPKDllWrapper.glp_intopt(prob, ref iocp);
        if (simplexErr != 0 || mipError != 0)
        {
            var (toolDiff, qualDiff) = await Validate(request.TaskItemIds,request.PersonIds,request.ToolIds,qualIds);
            return new PlanResponseDto(0.0, [], toolDiff, qualDiff);
        }
        GLPKDllWrapper.glp_mpl_postsolve(tran, prob, GLPKDllWrapper.GLP_MIP);


        var taskOnBoat = new HashSet<int>[request.BoatNumber];
        var personOnBoat = new HashSet<int>[request.BoatNumber];
        var toolOnBoat = new Dictionary<int, int>[request.BoatNumber];
        var boatUsage = new bool[request.BoatNumber];

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
            int val = (int)Math.Round(GLPKDllWrapper.glp_mip_col_val(prob, j));

            var (varName, indices) = parseColumnName(colName);

            switch (varName)
            {
                case "taskOnBoat":
                    {
                        int boatId = indices[0];
                        int taskId = indices[1];
                        if (val != 0)
                            taskOnBoat[boatId].Add(taskId);
                        break;
                    }
                case "personOnBoat":
                    {
                        int boatId = indices[0];
                        int personId = indices[1];
                        if (val != 0)
                            personOnBoat[boatId].Add(personId);
                        break;
                    }
                case "toolOnBoat":
                    {
                        int boatId = indices[0];
                        int toolId = indices[1];
                        if (val > 0)
                            toolOnBoat[boatId][toolId] = val;
                        break;
                    }
                case "boatUsage":
                    {
                        int boatId = indices[0];
                        boatUsage[boatId] = val != 0;
                        break;
                    }
                default:
                    throw new Exception($"Unknown column name id GMPL Solver results: {varName}[{indices}]");
            }
        }

        List<BoatPlanDto> boats = new List<BoatPlanDto>();
      
        for (int i = 0; i < request.BoatNumber; i++)
        {
            if (!boatUsage[i]) continue;

            List<PersonSummaryDto> persons = new List<PersonSummaryDto>();
            foreach (var id in personOnBoat[i])
            {
                persons.Add(
                    mapper.Map<PersonSummaryDto>(
                        await personService.GetPerson(id + 1) // very ugly fix
                    )
                );
            }

            List<TaskToolDto> tools = toolOnBoat[i]
                .Select(x => new TaskToolDto
                {
                    ToolId = x.Key + 1,
                    RequiredAmount = x.Value
                }).ToList();


            List<TaskItemSummaryDto> tasks = new List<TaskItemSummaryDto>();
            foreach (var id in taskOnBoat[i])
            {
                tasks.Add(
                    mapper.Map<TaskItemSummaryDto>(
                        await taskItemService.GetTaskItem(id + 1) // very ugly fix
                    )
                );
            }

            boats.Add(new BoatPlanDto(tasks, persons, tools));
        }

        double workingHours = GLPKDllWrapper.glp_mip_obj_val(prob);

        var (partialToolDiff, partialQualDiff) = await Validate(request.TaskItemIds, request.PersonIds, request.ToolIds, qualIds);

        return new PlanResponseDto(workingHours, boats, partialToolDiff, partialQualDiff);
    }


    private async Task<string> CreateDataFileAsync(PlanRequestDto info, List<int> qualIds)
    {
        var taskIds = info.TaskItemIds;
        var personIds = info.PersonIds;
        var toolIds = info.ToolIds;
     
        var sb = new StringBuilder();

        sb.AppendLine("data;");

        sb.AppendLine($"set TASKS := {string.Join(" ", taskIds.Select(id => $"ta_{id}"))};");
        sb.AppendLine($"set QUALIS := {string.Join(" ", qualIds.Select(id => $"q_{id}"))};");
        sb.AppendLine($"set PEOPLE := {string.Join(" ", personIds.Select(id => $"p_{id}"))};");
        sb.AppendLine($"set TOOLS := {string.Join(" ", toolIds.Select(id => $"to_{id}"))};");

        sb.AppendLine();

        // Duration

        sb.AppendLine("param duration :=");
        for (int i = 0; i < taskIds.Count; i++)
        {
            var taskItem = await taskItemService.GetTaskItem(taskIds[i]);
            sb.AppendLine($"\tta_{taskIds[i]} {taskItem!.DurationHours.ToString(CultureInfo.InvariantCulture)}");
        }
        sb.AppendLine(";\n");

        // Person <-> Qualification

        sb.AppendLine($"param hasQuali : {string.Join(" ", qualIds.Select(id => $"q_{id}"))} := ");

        for (int i = 0; i < personIds.Count; i++)
        {
            var qualMask = await qualificationService.GetPersonQualificationMask(personIds[i], qualIds);
            var maskStr = qualMask.Select(b => b ? "1" : "0");
            sb.AppendLine($"\tp_{personIds[i]} {string.Join(" ", maskStr)}");
        }
        sb.AppendLine(";");

        // Task <-> Qualification

        sb.AppendLine($"param requiredQualis : {string.Join(" ", qualIds.Select(id => $"q_{id}"))} := ");

        for (int i = 0; i < taskIds.Count; i++)
        {
            var qualReq = await qualificationService.GetTaskQualificationRequirements(taskIds[i],qualIds);
            sb.AppendLine($"\tta_{taskIds[i]} {string.Join(" ", qualReq)}");
        }
        sb.Append(";");

        // Task <-> Tools

        sb.AppendLine();

        sb.AppendLine($"param requiredTools: {string.Join(" ", toolIds.Select(id => $"to_{id}"))} := ");

        for (int i = 0; i < taskIds.Count; i++)
        {
            var qualTools = await toolService.GetTaskToolRequirements(taskIds[i],toolIds);
            sb.AppendLine($"\tta_{taskIds[i]} {string.Join(" ", qualTools)}");
        }
        sb.AppendLine(";");

        // Tools

        sb.AppendLine("param stockTools := ");
        for (int i = 0; i < toolIds.Count; i++)
        {
            var tool = await toolService.GetTool(toolIds[i]);
            sb.AppendLine($"\tto_{toolIds[i]} {tool!.AvailableStock}");
        }
        sb.AppendLine(";");

        sb.AppendLine($"param maxWorkingHours := {info.MaxTime};");
        sb.AppendLine($"param amountBoats := {info.BoatNumber};");
        sb.AppendLine("end;");

        return sb.ToString();
    }

    private async Task<List<int>> GetUsedQualificationIds(List<int> taskIds,List<int> personIds)
    {
        var allQualIds = await qualificationService.GetAllIds();
        var used = new HashSet<int>();

        foreach (var taskId in taskIds)
        {
            var req = await qualificationService.GetTaskQualificationRequirements(taskId,allQualIds);

            for (int i = 0; i < allQualIds.Count; i++)
            {
                if (req[i] > 0) used.Add(allQualIds[i]);
            }
        }

        foreach (var personId in personIds)
        {
            var mask = await qualificationService.GetPersonQualificationMask(personId,allQualIds);

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

        // -------------------------
        // REQUIRED QUALIFICATIONS
        // -------------------------
        var requiredQuals = qualIds.ToDictionary(id => id, _ => 0);

        foreach (var taskId in taskIds)
        {
            var req = await qualificationService
                .GetTaskQualificationRequirements(taskId, qualIds);

            for (int i = 0; i < qualIds.Count; i++)
            {
                requiredQuals[qualIds[i]] += req[i];
            }
        }

        // -------------------------
        // AVAILABLE QUALIFICATIONS
        // -------------------------
        var availableQuals = qualIds.ToDictionary(id => id, _ => 0);

        foreach (var personId in personIds)
        {
            var mask = await qualificationService
                .GetPersonQualificationMask(personId, qualIds);

            for (int i = 0; i < qualIds.Count; i++)
            {
                if (mask[i])
                    availableQuals[qualIds[i]] += 1;
            }
        }

        // -------------------------
        // REQUIRED TOOLS
        // -------------------------
        var requiredTools = toolIds.ToDictionary(id => id, _ => 0);

        foreach (var taskId in taskIds)
        {
            var req = await toolService
                .GetTaskToolRequirements(taskId, toolIds);

            for (int i = 0; i < toolIds.Count; i++)
            {
                requiredTools[toolIds[i]] += req[i];
            }
        }

        // -------------------------
        // AVAILABLE TOOLS
        // -------------------------
        var availableTools = toolIds.ToDictionary(id => id, _ => 0);

        foreach (var toolId in toolIds)
        {
            var tool = await toolService.GetTool(toolId);
            availableTools[toolId] = tool!.AvailableStock;
        }

        // -------------------------
        // DIFF OUTPUT (FILTERED)
        // -------------------------
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
