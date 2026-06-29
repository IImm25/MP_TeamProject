// Author: Erik Schellenberger and Alexander Gewinnus

namespace Backend.Data.Entitites
{
    public class TaskItem
    {
        public TaskItem() { }
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public float DurationHours { get; set; }

        public bool IsCompleted { get; set; }
        public DateOnly ExecutionIntervalStart { get; set; }
        public DateOnly ExecutionIntervalEnd { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; } = null!;
        public ICollection<TaskQualification> RequiredQualifications { get; set; } = [];
        public ICollection<TaskTool> RequiredTools { get; set; } = [];
        public TaskItem(string name, float durationHours, DateOnly exectionIntervalStart, DateOnly executionIntervalEnd)
        {
            Name = name;
            DurationHours = durationHours;
            ExecutionIntervalStart = exectionIntervalStart;
            ExecutionIntervalEnd = executionIntervalEnd;
        }
    } 
}