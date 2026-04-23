namespace Backend.Web.Services;

public class GmplService
{
    List<TaskItem> TaskItems = new List<TaskItem>();
    public GmplService(List<TaskItem> tasks)
    {
        this.TaskItems = tasks;
    }

    public void WriteDataFile()
    {
        string filePath = @"C:\Users\ALEX\source\repos\MP_TeamProject\GMPL\data1.dat";

        List<string> taskNames = TaskItems.Select(x => x.Name).ToList();
        string setSeclarations = CreateSetDeclarations(TaskItems);


    }

    private string CreateSetDeclarations(List<TaskItem> tasks)
    {
        string text = "data;";
        

        
        return text;
    }
}
