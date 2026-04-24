namespace Backend.Web.Services;

using Backend.Data.DBContext;
using Backend.Web.Repositories;
using System.Text;

public class GmplService
{
    private List<TaskItem> TaskItems = new List<TaskItem>();

    public GmplService(List<TaskItem> tasks)
    {
        this.TaskItems = tasks;
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


    public string WriteDatafile(List<Person> people, int maxWorkingHours = 8, int amoutBoats = 8)
    {
        var allQualifications = ExtractQualifications();
        var allTools = ExtractTools();
        var taskIds = BuildTaskIds();

        var sb = new StringBuilder();

        sb.AppendLine("data;");
        sb.AppendLine();

        WriteSet(sb, "TASKS", taskIds);
        WriteSet(sb, "QUALIS", allQualifications.Select(q => Sanitize(q.Name)));
        WriteSet(sb, "PEOPLE", people.Select((_, i) => $"p{i + 1}"));
        WriteSet(sb, "TOOLS", allTools.Select(t => Sanitize(t.Name)));
        sb.AppendLine();

        WriteParamDuration(sb, taskIds);
        sb.AppendLine();

        WriteParamHasQuali(sb, people, allQualifications);
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

    // ─── Extraktion ───────────────────────────────────────────────────────────

    private List<string> BuildTaskIds()
        => TaskItems.Select((_, i) => $"a{i + 1}").ToList();

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

    // ─── Set-Schreiber ────────────────────────────────────────────────────────

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
        List<Qualification> qualifications)
    {
        if (people.Count == 0)
        {
            sb.AppendLine("# param hasQuali: keine Personen übergeben");
            return;
        }

        // Spaltenbreiten berechnen
        var colWidths = qualifications.Select(q => Sanitize(q.Name).Length + 2).ToList();

        // Header
        sb.Append("param hasQuali :");
        for (int c = 0; c < qualifications.Count; c++)
            sb.Append($" {Sanitize(qualifications[c].Name).PadRight(colWidths[c])}");
        sb.AppendLine(":=");

        for (int pi = 0; pi < people.Count; pi++)
        {
            var qualIds = (people[pi].Qualifications ?? [])
                .Select(pq => pq.QualificationId)
                .ToHashSet();

            sb.Append($"         {$"p{pi + 1}",-10}");
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
