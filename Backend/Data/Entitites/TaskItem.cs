// Author: Erik Schellenberger and Alexander Gewinnus

public class TaskItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int DurationHours { get; set; }

    public ICollection<TaskQualification> RequiredQualifications { get; set; } = [];
    public ICollection<TaskTool> RequiredTools { get; set; } = [];
    public TaskItem(string name, int durationHours)
    {
        Name = name;
        DurationHours = durationHours;
    }
}