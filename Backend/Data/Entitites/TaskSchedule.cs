using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Data.Entitites
{
    public class TaskSchedule
    {
        public int TaskId { get; set; }  // composite primary key (TaskId,(PlanId,BoatNumber))
        [ForeignKey("TaskId")]
        public TaskItem TaskItem { get; set; } = null!;

        public int PlanId { get; set; } // composite foreign key (PlanId,BoatNumber)
        public int BoatNumber { get; set; }
        public PlanBoat Boat { get; set; } = null!;

        public TimeOnly StartTime { get; set; }
    }
}
