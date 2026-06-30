using System.Globalization;
using System.Text;
using Backend.Data.DTO.Plan;
using Backend.Data.Entitites;
using Backend.Util;

namespace Backend.GMPL;

public interface IGmplInputBuilder
{
    string CreateDataFileContent(
        PlanRequestDto info,
        List<TaskItem> tasks,
        List<TaskSchedule> inProgress,
        List<Person> persons,
        List<Qualification> qualifications,
        List<Tool> tools,
        List<Location> locations
    );
}

public class GmplInputBuilder : IGmplInputBuilder
{
    private static float CalculateTaskPriority(TaskItem task, DateOnly date, float scalar = 1.0f)
    {
        float priority = 0.0f;
        var current = date.DayNumber;
        var start = task.ExecutionIntervalStart.DayNumber;
        var end = task.ExecutionIntervalEnd.DayNumber;

        if (current >= start)
        {
            priority = (current - start) / (float)(end - start);
        }

        return Math.Clamp(priority, 0.0f, 1.0f) * scalar;
    }

    public string CreateDataFileContent(PlanRequestDto info, List<TaskItem> tasks, List<TaskSchedule> inProgress, List<Person> persons, List<Qualification> qualifications, List<Tool> tools, List<Location> locations)
    {
        var sb = new StringBuilder();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        sb.AppendLine("data;");
        sb.AppendLine();

        string qualificationString = string.Join(" ", qualifications.Select(q => $"q_{q.Id}"));
        string locationString = string.Join(" ", locations.Select(t => $"w_{t.Id}")); // by default harbour is location id 1
        string toolString = string.Join(" ", tools.Select(to => $"to_{to.Id}"));

        // Sets
        sb.AppendLine($"set TASKS := {string.Join(" ", tasks.Select(ta => $"ta_{ta.Id}"))};");
        sb.AppendLine($"set PEOPLE := {string.Join(" ", persons.Select(p => $"p_{p.Id}"))};");
        sb.AppendLine($"set QUALIS := {qualificationString};");
        sb.AppendLine($"set TOOLS := {toolString};");
        sb.AppendLine($"set PLACES := {locationString};");

        // Parameter
        sb.AppendLine($"param maxWorkingHours := {info.MaxWorkHours.ToString(CultureInfo.InvariantCulture)};");
        sb.AppendLine($"param amountBoats := {info.BoatNumber};");
        sb.AppendLine($"param drivingSpeed := {info.BoatSpeed.ToString(CultureInfo.InvariantCulture)};");
        sb.AppendLine();

        // Duration
        sb.AppendLine("param duration :=");
        foreach (var task in tasks)
        {
            sb.AppendLine($"\tta_{task.Id} {task.DurationHours.ToString(CultureInfo.InvariantCulture)}");
        }
        sb.AppendLine(";");




        // Person -> Qualification
        sb.AppendLine($"param hasQuali : {qualificationString} :=");
        foreach (var person in persons)
        {
            var bitMask = qualifications
                .Select(q => person.Qualifications.Select(pq => pq.QualificationId)
                .Contains(q.Id)); // for every qualification check if the person has it

            sb.AppendLine($"\tp_{person.Id} {string.Join(" ", bitMask.Select(b => b ? "1" : "0"))}"); // map from bool to string
        }
        sb.AppendLine(";");



        // Task -> Qualification
        sb.AppendLine($"param requiredQualis : {qualificationString} :=");
        foreach (var task in tasks)
        {
            var requiredCount = qualifications
                .Select(q => task.RequiredQualifications
                    .ToDictionary(tq => tq.QualificationId, tq => tq.RequiredAmount)
                    .GetValueOrDefault(q.Id))
                .ToList();  // for every qualification get the amount a task requires

            sb.AppendLine($"\tta_{task.Id} {string.Join(" ", requiredCount)}");
        }
        sb.AppendLine(";");



        // Task -> Tools
        sb.AppendLine($"param requiredTools : {toolString} :=");
        foreach (var task in tasks)
        {
            var requiredCount = tools
                .Select(t => task.RequiredTools
                    .ToDictionary(tt => tt.ToolId, tt => tt.RequiredAmount)
                    .GetValueOrDefault(t.Id))
                .ToList();  // for every tool get the amount a task requires

            sb.AppendLine($"\tta_{task.Id} {string.Join(" ", requiredCount)}");
        }
        sb.AppendLine(";");



        // Stock Tools
        sb.AppendLine("param stockTools :=");
        foreach (var tool in tools)
        {
            sb.AppendLine($"\tto_{tool.Id} {tool.AvailableStock}");
        }
        sb.AppendLine(";");



        // Priority
        sb.AppendLine("param taskPrio :=");
        foreach (var task in tasks)
        {
            sb.AppendLine($"\tta_{task.Id} {CalculateTaskPriority(task, today).ToString(CultureInfo.InvariantCulture)}");
        }
        sb.AppendLine(";");



        // Task Locations
        sb.AppendLine("param taskLocation :=");
        foreach (var task in tasks)
        {
            sb.AppendLine($"\tta_{task.Id} w_{task.Location.Id}");
        }
        sb.AppendLine(";");



        // Distances
        List<Coordinates> coords = locations.Select(l => new Coordinates(l.Latitude, l.Longitude)).ToList();
        var dist = Coordinates.CalculateDistanceMatrix(coords);

        sb.AppendLine($"param distance : {locationString} :=");

        for (int i = 0; i < locations.Count; i++)
        {
            sb.AppendLine($"\tw_{locations[i].Id} {string.Join(" ", dist[i].Select(d => d.ToString(CultureInfo.InvariantCulture)))}");
        }
        sb.AppendLine(";");

        if (false)
        {

            // StartDistances 
            sb.AppendLine($"param startDistance : {locationString} :=");
            for (int i = 0; i < info.BoatNumber; i++)
            {
                Coordinates boatCoords = coords[0]; // replace with actual implementation
                var startDist = Coordinates.CalculateDistances(boatCoords, coords);
                sb.AppendLine($"\t{i + 1} {string.Join(" ", startDist.Select(d => d.ToString(CultureInfo.InvariantCulture)))}");
            }
            sb.AppendLine(";");
        }


        // Fixed Boat
        sb.AppendLine("param fixedBoat :=");
        foreach (var schedule in inProgress)
        {
            sb.AppendLine($"\tta_{schedule.TaskItemId} {schedule.BoatNumber}");
        }
        sb.AppendLine(";");



        // Fixed Start Time
        sb.AppendLine("param fixedStartTime :=");
        foreach (var schedule in inProgress)
        {
            sb.AppendLine($"\tta_{schedule.TaskItemId} {(float)schedule.StartTime.ToTimeSpan().TotalHours}");
        }
        sb.AppendLine(";");



        // Fixed Person
        var scheduledPersons = inProgress.SelectMany(ts => ts.Boat.Persons).ToList();
        sb.AppendLine("param fixedPerson :=");
        foreach (var person in scheduledPersons)
        {
            sb.AppendLine($"\tp_{person.PersonId} {person.BoatNumber}");
        }
        sb.AppendLine(";");



        // Fixed Tool Amount
        var scheduledBoats = inProgress.Select(ts => ts.Boat).DistinctBy(b => b.BoatNumber).ToList();
        var distinctToolIds = inProgress
            .SelectMany(ts => ts.Boat.Tools)
            .Select(t => t.ToolId)
            .Distinct()
            .OrderBy(id => id)
            .ToList();
        sb.AppendLine($"param fixedToolAmount : {string.Join(" ", distinctToolIds.Select(id => $"to_{id}"))} :=");
        for (int i = 0; i < scheduledBoats.Count; i++)
        {
            var requiredAmount = distinctToolIds.Select(toolId => scheduledBoats[i].Tools
                .ToDictionary(x => x.ToolId, x => x.RequiredAmount)
                .GetValueOrDefault(toolId));

            sb.AppendLine($"\t{scheduledBoats[i].BoatNumber} {string.Join(" ", requiredAmount)}");
        }
        sb.AppendLine(";");

        sb.AppendLine("end;");
        return sb.ToString();
    }
}

