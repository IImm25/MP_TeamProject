namespace Backend.Data.Entitites
{
    public class PlanBoat
    {
        public int BoatNumber { get; set; } // composite primary key of (BoatIndex,PlanId)

        public int PlanId { get; set; }
        public Plan Plan { get; set; } = null!;

        public ICollection<BoatPerson> Persons { get; set; } = [];
        public ICollection<BoatTools> Tools { get; set; } = [];
        public ICollection<BoatSchedule> BoatSchedule { get; set; } = [];
        public ICollection<TaskSchedule> TaskSchedule { get; set; } = [];

    }
}
