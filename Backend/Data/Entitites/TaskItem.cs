// Author: Erik Schellenberger

public class TaskItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int DurationHours { get; set; }

    public ICollection<TaskQualification> RequiredQualifications { get; set; } = [];
    public ICollection<TaskTool> RequiredTools { get; set; } = [];
}