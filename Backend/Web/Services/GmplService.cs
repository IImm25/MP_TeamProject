namespace Backend.Web.Services;

using Backend.GMPL;

public class GmplService
{
    private List<TaskItem> TaskItems = new List<TaskItem>();
    private List<Person> People = new List<Person>();
    private List<Tool> Tools = new List<Tool>(); 
    public GmplService(List<int> taskIds, List<int> personIds, List<int> toolIds)
    {
        TaskItems = GetTasksFromIds(taskIds);
        People = GetPeopleFromIds(personIds);
        Tools = GetToolsFromIds(toolIds);
    }

    private List<Tool> GetToolsFromIds(List<int> toolIds)
    {
        List<Tool> tools = new List<Tool>();

        //from repo
        throw new NotImplementedException();
        return tools;
    }
    private List<TaskItem> GetTasksFromIds(List<int> taskIds)
    {
        List<TaskItem> tasks = new List<TaskItem>();


        //from repo
        throw new NotImplementedException();
        return tasks;
    }
    private List<Person> GetPeopleFromIds(List<int> personIds)
    {
        List<Person> people = new List<Person>();

        //fom repo
        throw new NotImplementedException();

        return people;
    }

    public async Task<GmplResults> CaculateGmplModel()
    {
        try
        {
            DataFileGenerator datFileGenerator = new DataFileGenerator(TaskItems, People);

            string datFileText = await datFileGenerator.CreateDataFile();
            string resp = await DataFileGenerator.SaveDataFile(datFileText);

            // ── Validate  ────────────────────────────────────
            GmplValidator.Test(DAT);

            // ── Solve ──────────────────────────────────────────

            if (!resp.Contains("Exception"))
            {
                GmplResults result = GmplSolver.Solve(Path.Combine(Directory.GetCurrentDirectory(), "..", "GMPL", "modell.mod"), resp);
                
                GmplOutput2Console.GetGmplResults(result);

                return result;
            }

        }
        catch (ValidationError ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [Validation-Error] {ex.Message}");
            Console.ResetColor();
        }
        catch (NotSolvableError ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [Invalid] {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [unexpected Error] {ex.Message}");
            Console.ResetColor();
        }
        return null;
    }

    //public async void call()
    //{
    //    PersonRepository _personRepo = new PersonRepository(new AppDbContext());
    //    TaskItemRepository _taskRepo = new TaskItemRepository(new AppDbContext());
    //        var tasks = await _taskRepo.GetAllAsync();   // inkl. Includes
    //        var people = await _personRepo.GetAllAsync(); // inkl. Qualifikationen
    //        var service = new GmplService(tasks.ToList());
    //        string datFile = service.WriteDatafile(people, maxWorkingHours: 8);
    //        // z.B. als Datei speichern:
    //        await File.WriteAllTextAsync("output.dat", datFile);
    //}
    private const string MOD = @"C:\Users\ALEX\source\repos\MP_TeamProject\GMPL\modell.mod";
    private const string DAT = @"C:\Users\ALEX\source\repos\MP_TeamProject\GMPL\data.dat";
}
