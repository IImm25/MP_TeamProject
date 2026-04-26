using AutoMapper;
using Backend.Data.DTO;
using Backend.Data.DTO.Create;
using Backend.Data.Mappers;
using Backend.Data.Repositories;

using Backend.Data.DTO;
using Backend.Data.Repositories;
using Backend.GMPL;
namespace Backend.Web.Services;

public class GmplService
{
    private readonly IRepository<Tool> repository;
    private readonly IPersonRepository personRepository;
    private readonly ITaskItemRepository taskItemRepository;

    private List<TaskItem> TaskItems = new List<TaskItem>();
    private List<Person> People = new List<Person>();
    private List<Tool> Tools = new List<Tool>();
    public GmplService(IRepository<Tool> repo, IPersonRepository personRepo, ITaskItemRepository taskItemRepo)
    {
        repository = repo;
        personRepository = personRepo;
        taskItemRepository = taskItemRepo;
    }

    public async Task<List<PlanResponseDto>> TestGLPK()
    {
        string pathMod = @"..\..\GMPL\modell.mod";
        string pathDat = @"..\GMPL\data.dat";

        GmplResults result = GmplSolver.Solve(Path.Combine(Directory.GetCurrentDirectory(), "..", "GMPL", "modell.mod"), pathDat);

        return ResponseMapper.MapToResponse(result);
    }

    public async Task<List<PlanResponseDto>> CaculateGmplModel(PlanRequestDto request)
    {
        try
        {
            await ReadInRequestDto(request);
            DataFileGenerator datFileGenerator = new DataFileGenerator(TaskItems, People, Tools);

            string datFileText = await datFileGenerator.CreateDataFile();
            string resp = await DataFileGenerator.SaveDataFile(datFileText);
                        
            GmplValidator.Test(DAT);            

            if (!resp.Contains("Exception"))
            {
                GmplResults result = GmplSolver.Solve(Path.Combine(Directory.GetCurrentDirectory(), "..", "GMPL", "modell.mod"), resp);
                GmplOutput2Console.GetGmplResults(result);
                return ResponseMapper.MapToResponse(result);
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

    private async Task ReadInRequestDto(PlanRequestDto request)
    {
        this.TaskItems = await GetTasksFromIds(request.TaskItemIds);
        this.People = await GetPeopleFromIds(request.PersonIds);
        this.Tools = await GetToolsFromIds(request.ToolIds);
    }
    
    private async Task<List<Tool>> GetToolsFromIds(List<int> toolIds)
    {
        try
        {
            var tools = await Task.WhenAll(toolIds.Select(repository.GetByIdAsync).ToList());
            if (tools.Any())
            {
                return tools.ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            
        }
        return null;
    }
    private async Task<List<TaskItem>> GetTasksFromIds(List<int> taskIds)
    {
        try
        {
            var tasks = await Task.WhenAll(taskIds.Select(taskItemRepository.GetByIdAsync).ToList());
            if (tasks.Any())
            {
                return tasks.ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

        }
        return null;
    }
    private async Task<List<Person>> GetPeopleFromIds(List<int> personIds)
    {
        try
        {
            var tasks = await Task.WhenAll(personIds.Select(personRepository.GetByIdAsync).ToList());
            if (tasks.Any())
            {
                return tasks.ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

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
