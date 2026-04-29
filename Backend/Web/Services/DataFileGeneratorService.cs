using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Backend.Data.DTO;

namespace Backend.Web.Services;

public class DataFileGeneratorService
{
    private readonly QualificationService qualificationService;
    private readonly ToolService toolService;
    private readonly TaskItemService taskItemService;
    public DataFileGeneratorService(QualificationService qualificationService, ToolService toolService, TaskItemService taskItemService)
    {
        this.qualificationService = qualificationService;
        this.toolService = toolService;
        this.taskItemService = taskItemService;
    }

    public async Task<string> CreateDataFileAsync(PlanRequestDto info)
    {
        var taskIds = info.TaskItemIds;
        var personIds = info.PersonIds;
        var toolIds = info.ToolIds;
        var qualIds = await qualificationService.GetAllIds();

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
            var qualMask = await qualificationService.GetPersonQualificationMask(personIds[i]);
            var maskStr = qualMask.Select(b => b ? "1" : "0");
            sb.AppendLine($"\tp_{personIds[i]} {string.Join(" ", maskStr)}");
        }
        sb.AppendLine(";");

        // Task <-> Qualification

        sb.AppendLine($"param requiredQualis : {string.Join(" ", qualIds.Select(id => $"q_{id}"))} := ");

        for (int i = 0; i < taskIds.Count; i++)
        {
            var qualReq = await qualificationService.GetTaskQualificationRequirements(taskIds[i]);
            sb.AppendLine($"\tta_{taskIds[i]} {string.Join(" ", qualReq)}");
        }
        sb.Append(";");

        // Task <-> Tools

        sb.AppendLine();

        sb.AppendLine($"param requiredTools: {string.Join(" ", toolIds.Select(id => $"to_{id}"))} := ");

        for (int i = 0; i < taskIds.Count; i++)
        {
            var qualTools = await toolService.GetTaskToolRequirements(taskIds[i]);
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
}
