using Backend.Data.DTO;
using Backend.Data.Mappers;
using Backend.Data.Repositories;
using Backend.GMPL;
using System.Xml;
namespace Backend.Web.Services;

public class GmplService
{
    private readonly IRepository<Tool> repository;
    private readonly IPersonRepository personRepository;
    private readonly ITaskItemRepository taskItemRepository;
    private readonly IRepository<Qualification> qualificationRepository;
    private readonly IRepository<PersonQualification> qualificationPersonRepository;

    private List<TaskItem> TaskItems = new List<TaskItem>();
    private List<Person> People = new List<Person>();
    private List<Tool> Tools = new List<Tool>();
    private List<Qualification> Qualifications = new List<Qualification>();
    public GmplService(
        IRepository<Tool> repo,
        IPersonRepository personRepo,
        ITaskItemRepository taskItemRepo,
        IRepository<Qualification> qualiRepo,
        IRepository<PersonQualification> qualiPersonRepo
        )
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

        return new List<PlanResponseDto>();
    }

    public async Task<List<PlanResponseDto>> CaculateGmplModel(PlanRequestDto request)
    {
        try
        {
            await ReadInRequestDto(request);
            
            DataFileGenerator datFileGenerator = new DataFileGenerator(TaskItems, People, Tools, Qualifications, request.maxTime , request.BoatNumber);

            string datFileText = await datFileGenerator.CreateDataFile();
            string resp = await DataFileGenerator.SaveDataFile(datFileText);
              
            GmplValidator.Test(resp);            

            if (!resp.Contains("Exception"))
            {
                GmplResults result = GmplSolver.Solve(Path.Combine(Directory.GetCurrentDirectory(), "..", "GMPL", "modell.mod"), resp);
                //GmplResults result = GmplSolver.Solve(Path.Combine(Directory.GetCurrentDirectory(), "..", "GMPL", "modell.mod"), DAT);
                GmplOutput2Console.GetGmplResults(result);
                return ResponseMapper.MapToResponse(result, TaskItems, People, Tools, People.SelectMany(x => x.Qualifications).ToList());
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
        this.Qualifications = await GetQualificationsFromPeople();
    }
    
    private async Task<List<Qualification>> GetQualificationsFromPeople()
    {
        List<Qualification> qualifications = People.SelectMany(x => x.Qualifications.Select(y => y.Qualification)).ToList();
        
        return qualifications;
    }
    private async Task<List<Tool>> GetToolsFromIds(List<int> toolIds)
    {
        try
        {
            var tools = new List<Tool>();
            foreach (var id in toolIds)
            {
                var tool = await repository.GetByIdAsync(id);
                if (tool is not null)
                    tools.Add(tool);
            }
            return tools.Any() ? tools : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
    private async Task<List<TaskItem>> GetTasksFromIds(List<int> taskIds)
    {
        try
        {
            var tasks = new List<TaskItem>();
            foreach (var id in taskIds)
            {
                var task = await taskItemRepository.GetFullByIdAsync(id);
                if (task is not null)
                    tasks.Add(task);
            }
            return tasks.Any() ? tasks : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
    private async Task<List<Person>> GetPeopleFromIds(List<int> personIds)
    {
        try
        {
            var people = new List<Person>();
            foreach (var id in personIds)
            {
                var person = await personRepository.GetFullByIdAsync(id);
                if (person is not null)
                    people.Add(person);
            }
            if (people.Any())
            {
                return people.ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

        }
        return null;
    }

    private const string MOD = @"C:\Users\ALEX\source\repos\MP_TeamProject\GMPL\modell.mod";
    private const string DAT = @"C:\Users\ALEX\source\repos\MP_TeamProject\GMPL\data.dat";
}
