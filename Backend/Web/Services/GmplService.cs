namespace Backend.Web.Services;

using Backend.GMPL;
using System.ComponentModel.DataAnnotations;
using System.Text;

public class GmplService
{
    private List<TaskItem> TaskItems = new List<TaskItem>();

    public GmplService(List<TaskItem> tasks)
    {
        this.TaskItems = tasks;
    }

    private const string MOD = @"C:\Users\ALEX\source\repos\MP_TeamProject\GMPL\modell.mod";
    private const string DAT = @"C:\Users\ALEX\source\repos\MP_TeamProject\GMPL\data.dat";

    public async Task<GmplResults> CaculateGmplModel()
    {
        GmplResults result = new GmplResults();
        try
        {
            // ── Validate  ────────────────────────────────────
             GmplValidator.Test(DAT);

            // ── Solve ──────────────────────────────────────────
            result = await GmplSolver.Solve(MOD, DAT);

            // ── Output results ──────────────────────────────
            GmplOutput2Console.GetGmplResults(result);

            // ── processing ─────────────────────────
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

        return result;
    }

    public async Task SaveDataFile(string path, string dataFileText)
    {
        File.WriteAllText(path, dataFileText);
    }

    public async Task<string> WriteDatafile(List<Person> people, int maxWorkingHours = 8, int amoutBoats = 8)
    {
        var allQualifications = ExtractQualifications();
        var allTools = ExtractTools();
        var taskIds = BuildTaskIds();
        var personIds = people.Select(p => Sanitize($"{p.Firstname}_{p.Lastname}")).ToList();

        var sb = new StringBuilder();

        sb.AppendLine("data;");
        sb.AppendLine();

        WriteSet(sb, "TASKS", taskIds);
        WriteSet(sb, "QUALIS", allQualifications.Select(q => Sanitize(q.Name)));
        WriteSet(sb, "PEOPLE", personIds);
        WriteSet(sb, "TOOLS", allTools.Select(t => Sanitize(t.Name)));
        sb.AppendLine();

        WriteParamDuration(sb, taskIds);
        sb.AppendLine();

        WriteParamHasQuali(sb, people, personIds, allQualifications);
        sb.AppendLine();

        WriteParamRequiredQualis(sb, taskIds, allQualifications);
        sb.AppendLine();

        WriteParamRequiredTools(sb, taskIds, allTools);
        sb.AppendLine();

        WriteParamStockTools(sb, allTools);
        sb.AppendLine();

        sb.AppendLine($"param maxWorkingHours := {maxWorkingHours};");
        sb.AppendLine($"param amountBoats := {amoutBoats};");
        sb.AppendLine("end;");

        return sb.ToString();
    }

    // ─── Extraction ───────────────────────────────────────────────────────────

    private List<string> BuildTaskIds()
        => TaskItems.Select(t => Sanitize(t.Name)).ToList();

    private List<Qualification> ExtractQualifications()
        => TaskItems
            .SelectMany(t => t.RequiredQualifications.Select(tq => tq.Qualification))
            .DistinctBy(q => q.Id)
            .OrderBy(q => q.Name)
            .ToList();

    private List<Tool> ExtractTools()
        => TaskItems
            .SelectMany(t => t.RequiredTools.Select(tt => tt.Tool))
            .DistinctBy(t => t.Id)
            .OrderBy(t => t.Name)
            .ToList();

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

    private static void WriteParamHasQuali(
        StringBuilder sb,
        List<Person> people,
        List<string> personIds,
        List<Qualification> qualifications)
    {
        if (people.Count == 0)
        {
            sb.AppendLine("# param hasQuali: keine Personen übergeben");
            return;
        }

        var colWidths = qualifications.Select(q => Sanitize(q.Name).Length + 2).ToList();

        sb.Append("param hasQuali :");
        for (int c = 0; c < qualifications.Count; c++)
            sb.Append($" {Sanitize(qualifications[c].Name).PadRight(colWidths[c])}");
        sb.AppendLine(":=");

        for (int pi = 0; pi < people.Count; pi++)
        {
            var qualIds = (people[pi].Qualifications ?? [])
                .Select(pq => pq.QualificationId)
                .ToHashSet();

            sb.Append($"         {personIds[pi],-10}");
            for (int c = 0; c < qualifications.Count; c++)
                sb.Append($" {(qualIds.Contains(qualifications[c].Id) ? "1" : "0").PadRight(colWidths[c])}");

            sb.AppendLine(pi < people.Count - 1 ? "" : ";");
        }
    }

    // ─── Param: requiredQualis ────────────────────────────────────────────────

    private void WriteParamRequiredQualis(
        StringBuilder sb,
        List<string> taskIds,
        List<Qualification> qualifications)
    {
        var colWidths = qualifications.Select(q => Sanitize(q.Name).Length + 2).ToList();

        sb.Append("param requiredQualis:");
        for (int c = 0; c < qualifications.Count; c++)
            sb.Append($" {Sanitize(qualifications[c].Name).PadRight(colWidths[c])}");
        sb.AppendLine(":=");

        for (int i = 0; i < TaskItems.Count; i++)
        {
            var requiredIds = TaskItems[i].RequiredQualifications
                .Select(tq => tq.QualificationId)
                .ToHashSet();

            sb.Append($"            {taskIds[i],-8}");
            for (int c = 0; c < qualifications.Count; c++)
                sb.Append($" {(requiredIds.Contains(qualifications[c].Id) ? "1" : "0").PadRight(colWidths[c])}");

            sb.AppendLine(i < TaskItems.Count - 1 ? "" : ";");
        }
    }

    // ─── Param: requiredTools ─────────────────────────────────────────────────

    private void WriteParamRequiredTools(
        StringBuilder sb,
        List<string> taskIds,
        List<Tool> tools)
    {
        var colWidths = tools.Select(t => Sanitize(t.Name).Length + 2).ToList();

        sb.Append("param requiredTools:");
        for (int c = 0; c < tools.Count; c++)
            sb.Append($" {Sanitize(tools[c].Name).PadRight(colWidths[c])}");
        sb.AppendLine(":=");

        for (int i = 0; i < TaskItems.Count; i++)
        {
            var toolAmounts = TaskItems[i].RequiredTools
                .ToDictionary(tt => tt.ToolId, tt => tt.RequiredAmount);

            sb.Append($"            {taskIds[i],-8}");
            for (int c = 0; c < tools.Count; c++)
            {
                int amount = toolAmounts.TryGetValue(tools[c].Id, out var amt) ? amt : 0;
                sb.Append($" {amount.ToString().PadRight(colWidths[c])}");
            }

            sb.AppendLine(i < TaskItems.Count - 1 ? "" : ";");
        }
    }

    // ─── Param: stockTools ────────────────────────────────────────────────────

    private static void WriteParamStockTools(StringBuilder sb, List<Tool> tools)
    {
        int nameWidth = tools.Max(t => Sanitize(t.Name).Length) + 4;

        sb.AppendLine("param stockTools :=");
        for (int i = 0; i < tools.Count; i++)
        {
            string line = $"      {Sanitize(tools[i].Name).PadRight(nameWidth)}{tools[i].AvailableStock}";
            sb.AppendLine(i < tools.Count - 1 ? line : line + ";");
        }
    }

    private static string Sanitize(string name) => name.Replace(" ", "_");
}