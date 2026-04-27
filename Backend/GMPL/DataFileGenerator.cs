using Backend.Data.Repositories;
using Backend.Web.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;
using System.Text;

namespace Backend.GMPL;

public class DataFileGenerator
{
    const int maxWorkingHours = 8;
    const int amoutBoats = 8;
    private List<TaskItem> TaskItems {  get; set; }
    private List<Person> People {  get; set; }
    private List<Tool> Tools { get; set; }
    private List<Qualification> Qualifications { get; set; }
    
    public DataFileGenerator(List<TaskItem> taskItems, List<Person> people, List<Tool> tools, List<Qualification> qualifications)
    {
        TaskItems = taskItems;
        People = people;
        Tools = tools;
        Qualifications = qualifications;
    }

    public async static Task<string> SaveDataFile(string dataFileText)
    {
        try
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "..", "GMPL", "test.dat");
            File.WriteAllText(path, dataFileText);
            return path;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

    }
    public async Task<string> CreateDataFile()
    {
        try
        {
            //var qualifications = await ExtractQualifications();
            //var tools = await ExtractTools();
            var taskIds = BuildTaskIds();

            var sb = new StringBuilder();

            sb.AppendLine("data;");
            sb.AppendLine();

            WriteSet(sb, "TASKS", TaskItems.Select((_, i) => $"ta{i + 1}"));
            WriteSet(sb, "QUALIS", Qualifications.Select((_, i) => $"q{i + 1}"));
            WriteSet(sb, "PEOPLE", People.Select((_, i) => $"p{i + 1}"));
            WriteSet(sb, "TOOLS", Tools.Select((_, i) => $"to{i + 1}" ));
            sb.AppendLine();

            WriteParamDuration(sb, taskIds);
            sb.AppendLine();

            WriteParamHasQuali(sb);
            sb.AppendLine();

            WriteParamRequiredQualis(sb, taskIds);
            sb.AppendLine();

            WriteParamRequiredTools(sb, taskIds);
            sb.AppendLine();

            WriteParamStockTools(sb, Tools);
            sb.AppendLine();

            sb.AppendLine($"param maxWorkingHours := {maxWorkingHours};");
            sb.AppendLine($"param amountBoats := {amoutBoats};");
            sb.AppendLine("end;");

            return sb.ToString();
        }
        catch(Exception ex)
        {
            throw ex;
        }
        return "";
    }

    // ─── Extraction ───────────────────────────────────────────────────────────

    private List<string> BuildTaskIds() => TaskItems.Select(x => Convert.ToString(x.Id)).ToList();
    
    // ─── Set-Writer ────────────────────────────────────────────────────────

    private static void WriteSet(StringBuilder sb, string name, IEnumerable<string> values)
        => sb.AppendLine($"set {name} := {string.Join(" ", values)};");

    // ─── Param: duration ──────────────────────────────────────────────────────

    private void WriteParamDuration(StringBuilder sb, List<string> taskIds)
    {
        sb.AppendLine("param duration :=");
        for (int i = 0; i < TaskItems.Count; i++)
        {
            string line = $"      {taskIds[i],-10}{(int)Math.Ceiling(TaskItems[i].DurationHours)}";
            sb.AppendLine(i < TaskItems.Count - 1 ? line : line + ";");
        }
    }

    // ─── Param: hasQuali ──────────────────────────────────────────────────────

    private void WriteParamHasQuali(StringBuilder sb)
    {
        if (People.Count == 0)
        {
            sb.AppendLine("# param hasQuali: keine Personen übergeben");
            return;
        }

        // calc ColumnWidth 
        var colWidths = Qualifications.Select(q => Sanitize(q.Name).Length + 2).ToList();

        // Header
        sb.Append("param hasQuali :");
        for (int c = 0; c < Qualifications.Count; c++)
            sb.Append($" {Sanitize(Qualifications[c].Name).PadRight(colWidths[c])}");
        sb.AppendLine(":=");

        for (int pi = 0; pi < People.Count; pi++)
        {
            var qualIds = (People[pi].Qualifications ?? [])
                .Select(pq => pq.QualificationId)
                .ToHashSet();

            sb.Append($"         {$"p{pi + 1}",-10}");
            for (int c = 0; c < Qualifications.Count; c++)
                sb.Append($" {(qualIds.Contains(Qualifications[c].Id) ? "1" : "0").PadRight(colWidths[c])}");

            sb.AppendLine(pi < People.Count - 1 ? "" : ";");
        }
    }

    // ─── Param: requiredQualis ────────────────────────────────────────────────

    private void WriteParamRequiredQualis(
        StringBuilder sb,
        List<string> taskIds
        )
    {
        var colWidths = Qualifications.Select(q => Sanitize(q.Name).Length + 2).ToList();

        sb.Append("param requiredQualis:");
        for (int c = 0; c < Qualifications.Count; c++)
            sb.Append($" {Sanitize(Qualifications[c].Name).PadRight(colWidths[c])}");
        sb.AppendLine(":=");

        for (int i = 0; i < TaskItems.Count; i++)
        {
            var requiredIds = TaskItems[i].RequiredQualifications
                .Select(tq => tq.QualificationId)
                .ToHashSet();

            sb.Append($"            {taskIds[i],-8}");
            for (int c = 0; c < Qualifications.Count; c++)
                sb.Append($" {(requiredIds.Contains(Qualifications[c].Id) ? "1" : "0").PadRight(colWidths[c])}");

            sb.AppendLine(i < TaskItems.Count - 1 ? "" : ";");
        }
    }

    // ─── Param: requiredTools ─────────────────────────────────────────────────

    private void WriteParamRequiredTools(
        StringBuilder sb,
        List<string> taskIds)
    {
        var colWidths = Tools.Select(t => Sanitize(t.Name).Length + 2).ToList();

        sb.Append("param requiredTools:");
        for (int c = 0; c < Tools.Count; c++)
            sb.Append($" {Sanitize(Tools[c].Name).PadRight(colWidths[c])}");
        sb.AppendLine(":=");

        for (int i = 0; i < TaskItems.Count; i++)
        {
            var toolAmounts = TaskItems[i].RequiredTools
                .ToDictionary(tt => tt.ToolId, tt => tt.RequiredAmount);

            sb.Append($"            {taskIds[i],-8}");
            for (int c = 0; c < Tools.Count; c++)
            {
                int amount = toolAmounts.TryGetValue(Tools[c].Id, out var amt) ? amt : 0;
                sb.Append($" {amount.ToString().PadRight(colWidths[c])}");
            }

            sb.AppendLine(i < TaskItems.Count - 1 ? "" : ";");
        }
    }

    // ─── Param: stockTools ────────────────────────────────────────────────────

    private void WriteParamStockTools(StringBuilder sb, List<Tool> tools)
    {
        int nameWidth = tools.Max(t => Sanitize(t.Name).Length) + 4;

        sb.AppendLine("param stockTools :=");
        for (int i = 0; i < tools.Count; i++)
        {
            string line = $"      {Sanitize(tools[i].Name).PadRight(nameWidth)}{tools[i].AvailableStock}";
            sb.AppendLine(i < tools.Count - 1 ? line : line + ";");
        }
    }


    private string Sanitize(string name) => name.Replace(" ", "_");
}
