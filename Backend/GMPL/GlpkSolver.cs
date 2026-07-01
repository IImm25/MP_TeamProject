using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Backend.GMPL.GLPKDllWrapper;

namespace Backend.GMPL;

public class SolverResult
{
    public List<SolverBoat> Boats { get; set; } = [];
}

public class SolverBoat
{
    public int BoatNumber { get; set; }
    public bool IsUsed { get; set; }
    public HashSet<int> PersonIds { get; set; } = [];
    public Dictionary<int, int> ToolQuantities { get; set; } = []; // ToolId -> RequiredAmount
    public List<SolverTaskSchedule> Tasks { get; set; } = [];
    public Dictionary<int, Dictionary<int, double>> TravelBetweenTasks { get; set; } = [];
}

public class SolverTaskSchedule
{
    public int TaskId { get; set; }
    public TimeOnly StartTime { get; set; }
    public bool IsFirst { get; set; }
    public bool IsLast { get; set; }
    public TimeSpan TravelToHarbor { get; set; }
    public static SolverTaskSchedule GetOrCreateTask(SolverBoat boat, int taskId)
    {
        var task = boat.Tasks.FirstOrDefault(t => t.TaskId == taskId);
        if (task == null)
        {
            task = new SolverTaskSchedule { TaskId = taskId };
            boat.Tasks.Add(task);
        }
        return task;
    }
}

public interface IGlpkSolver
{
    Task<SolverResult?> SolveAsync(string datFilePath, string modFilePath, int boatNumber, DateOnly planedDate);
}

public class GlpkSolver : IGlpkSolver
{
    private readonly SemaphoreSlim _solverLock = new(1, 1);

    private static readonly GlpTermHook Hook = TermHook;

    private static int TermHook(IntPtr info, IntPtr str)
    {
        string msg = Marshal.PtrToStringAnsi(str)!;
        Console.Write(msg);      // or log it
        return 1;                // continue
    }

    private static (string VarName, List<int> Indices) parseColumnName(string colName)
    {
        const string columnRegex = @"^([A-Za-z_]\w*)(?:\[(.*?)\])?$";
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
                    indices.Add(id); // From GMPL 1 indexed to 0 indexed
            }
        }
        return (name, indices);
    }
    public async Task<SolverResult?> SolveAsync(string datFilePath, string modFilePath, int boatNumber, DateOnly planedDate)
    {
        await _solverLock.WaitAsync();
        nint tran = nint.Zero;
        nint prob = nint.Zero;

        string logFilePath = Path.GetTempFileName();
        GLPKDllWrapper.glp_term_hook(Hook, IntPtr.Zero);

        try
        {
            prob = GLPKDllWrapper.glp_create_prob();
            tran = GLPKDllWrapper.glp_mpl_alloc_wksp();

            if (GLPKDllWrapper.glp_mpl_read_model(tran, modFilePath, 0) != 0)
                throw new Exception($"Error loading model file: {modFilePath}");

            if (GLPKDllWrapper.glp_mpl_read_data(tran, datFilePath) != 0)
                throw new Exception($"Error loading data file: '{datFilePath}'");

            if (GLPKDllWrapper.glp_mpl_generate(tran, nint.Zero) != 0)
                throw new Exception("Error generating mpl model.");

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
                return null;
            }

            GLPKDllWrapper.glp_mpl_postsolve(tran, prob, GLPKDllWrapper.GLP_MIP);

            int numCols = GLPKDllWrapper.glp_get_num_cols(prob);

            var solverResult = new SolverResult();
            for (int i = 0; i < boatNumber; i++)
            {
                solverResult.Boats.Add(new SolverBoat { BoatNumber = i+1 });
            }

            const double Epsilon = 1e-6;

            for (int j = 1; j <= numCols; j++)
            {
                string colName = GLPKDllWrapper.GetColName(prob, j);
                double rawVal = GLPKDllWrapper.glp_mip_col_val(prob, j);
                int val = (int)Math.Round(rawVal);
                var (varName, indices) = parseColumnName(colName);


                var targetBoat = solverResult.Boats[indices[0]-1];

                switch (varName)
                {
                    case "boatUsage":
                        targetBoat.IsUsed = val != 0;
                        break;

                    case "personOnBoat":
                        if (val != 0) targetBoat.PersonIds.Add(indices[1]);
                        break;

                    case "toolOnBoat":
                        if (val > 0) targetBoat.ToolQuantities[indices[1]] = val;
                        break;

                    case "taskOnBoat":
                        if (val != 0)
                        {
                            targetBoat.Tasks.Add(new SolverTaskSchedule { TaskId = indices[1] });
                        }
                        break;

                    case "startTime":
                        var stTask = targetBoat.Tasks.FirstOrDefault(t => t.TaskId == indices[1]);
                        if (stTask != null)
                        {
                            stTask.StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(rawVal + 8));
                        }
                        break;

                    case "isFirst":
                        var ifTask = targetBoat.Tasks.FirstOrDefault(t => t.TaskId == indices[1]);
                        if (ifTask != null && val != 0)
                        {
                            ifTask.IsFirst = true;
                        }
                        break;

                    case "isLast":
                        var ilTask = targetBoat.Tasks.FirstOrDefault(t => t.TaskId == indices[1]);
                        if (ilTask != null && val != 0)
                        {
                            ilTask.IsLast = true;
                        }
                        break;

                    case "travelToHarbor":
                        var ttHtask = targetBoat.Tasks.FirstOrDefault(t => t.TaskId == indices[1]);
                        if (ttHtask != null)
                        {
                            ttHtask.TravelToHarbor = TimeSpan.FromHours(rawVal);
                        }
                        break;

                    case "travelBetween":
                        // indices[0] = Boat, indices[1] = Task1, indices[2] = Task2
                        if (rawVal > 0 && indices.Count >= 3 && targetBoat.IsUsed)
                        {
                            int t1 = indices[1];
                            int t2 = indices[2];

                            if (!targetBoat.TravelBetweenTasks.ContainsKey(t1))
                                targetBoat.TravelBetweenTasks[t1] = new Dictionary<int, double>();

                            targetBoat.TravelBetweenTasks[t1][t2] = rawVal;
                        }
                        break;
                    // These variables are needed in the solver internally but do not contribute
                    case "after":
                        break;
                    case "lastTaskStart":
                        break;

                    default:
                        if (indices.Count > 0)
                            throw new Exception($"Unknown Column in GMPL-Result: {varName}[{string.Join(",", indices)}]");
                        break;
                }
            }
            return solverResult;
        }
        finally
        {
            if (tran != nint.Zero) GLPKDllWrapper.glp_mpl_free_wksp(tran);
            if (prob != nint.Zero) GLPKDllWrapper.glp_delete_prob(prob);
            _solverLock.Release();
        }
    }
}
