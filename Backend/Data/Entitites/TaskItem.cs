// Author: Erik Schellenberger and Alexander Gewinnus

namespace Backend.Data.Entitites
{
    public class TaskItem
    {
        public TaskItem() { }
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public float DurationHours { get; set; }

        public DateOnly ExecutionIntervalStart { get; set; }
        public DateOnly ExecutionIntervalEnd { get; set; }
        public ICollection<TaskQualification> RequiredQualifications { get; set; } = [];
        public ICollection<TaskTool> RequiredTools { get; set; } = [];
        public TaskItem(string name, float durationHours)
        {
            Name = name;
            DurationHours = durationHours;
        }
    } 
}