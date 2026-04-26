// Author: Erik Schellenberger and Alexander Gewinnus

public class TaskItem
{
    public TaskItem() { }
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public float DurationHours { get; set; }

    public ICollection<TaskQualification> RequiredQualifications { get; set; } = [];
    public ICollection<TaskTool> RequiredTools { get; set; } = [];
    public TaskItem(string name, float durationHours)
    {
        Name = name;
        DurationHours = durationHours;
    }
}