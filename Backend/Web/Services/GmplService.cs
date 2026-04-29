namespace Backend.Web.Services;
using System.IO;
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

    private readonly DataFileGeneratorService dataFileGeneratorService;
    private readonly PersonService personService;
    private readonly TaskItemService taskItemService;
    private readonly IMapper mapper;

    private static (string VarName, List<int> Indices) parseColumnName(string colName)
    {
        var match = Regex.Match(colName,columnRegex);

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
                    indices.Add(id-1); // From GMPL 1 indexed to 0 indexed
            }
        }
        return (name, indices);
    }

    public GmplService(DataFileGeneratorService dataFileGeneratorService, PersonService personService, TaskItemService taskItemService, IMapper mapper)
    {
        this.dataFileGeneratorService = dataFileGeneratorService;
        this.personService = personService;
        this.taskItemService = taskItemService;
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
        string datFileText = await dataFileGeneratorService.CreateDataFileAsync(request);
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
        if (simplexErr != 0)
        {
            throw new Exception($"Simplex-Error : {simplexErr}");
        }

        // MIP
        var iocp = new GLPKDllWrapper.glp_iocp();
        GLPKDllWrapper.glp_init_iocp(ref iocp);
        iocp.msg_lev = GLPKDllWrapper.GLP_MSG_OFF;

        int mipError = GLPKDllWrapper.glp_intopt(prob, ref iocp);
        if (mipError != 0)
        {
            throw new Exception($"MIP-Error : {mipError}");
        }
        GLPKDllWrapper.glp_mpl_postsolve(tran, prob, GLPKDllWrapper.GLP_MIP);

        bool[,] taskOnBoat = new bool[request.BoatNumber, request.TaskItemIds.Count];
        bool[,] personOnBoat = new bool[request.BoatNumber, request.PersonIds.Count];
        int[,] toolsOnBoat = new int[request.BoatNumber, request.ToolIds.Count];
        bool[] boatUsage = new bool[request.BoatNumber];

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
                        taskOnBoat[boatId,taskId] = val != 0;
                        break;
                    }
                case "personOnBoat":
                    {
                        int boatId = indices[0];
                        int personId = indices[1];
                        personOnBoat[boatId, personId] = val != 0;
                        break;
                    }
                case "toolOnBoat":
                    {
                        int boatId = indices[0];
                        int toolId = indices[1];
                        toolsOnBoat[boatId, toolId] = val;
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
            for (int j = 0; j < request.PersonIds.Count; j++)
            {
                if (personOnBoat[i,j])
                {
                    int id = j + 1; // ids start at 1 (ugly fix)
                    persons.Add(mapper.Map<PersonSummaryDto>(await personService.GetPerson(id)));
                }
            }

            List<TaskToolDto> tools = new List<TaskToolDto>();
            for (int j = 0; j < request.ToolIds.Count; j++)
            {
                int amount = toolsOnBoat[i, j];
                int id = j + 1; // ids start at 1 (ugly fix)
                if (amount > 0) {
                    tools.Add(new TaskToolDto { ToolId = id, RequiredAmount = amount });
                }
            }

            List<TaskItemSummaryDto> tasks = new List<TaskItemSummaryDto>();
            for (int j = 0; j < request.TaskItemIds.Count; j++)
            {
                if (taskOnBoat[i,j])
                {
                    int id = j + 1; // ids start at 1 (ugly fix)
                    tasks.Add(mapper.Map<TaskItemSummaryDto>(await taskItemService.GetTaskItem(id)));
                }
            }

            boats.Add(new BoatPlanDto(tasks, persons, tools));
        }

        double workingHours = GLPKDllWrapper.glp_mip_obj_val(prob);
        return new PlanResponseDto(workingHours, boats);
    }
}
