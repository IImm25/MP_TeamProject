namespace Backend.Data.Entitites
{
    public class BoatSchedule
    {
        public int PlanId { get; set; }
        public int BoatNumber { get; set; }
        public PlanBoat Boat { get; set; } = null!;
        public int TripNumber { get; set; }

        public TimeOnly departure {  get; set; }
        public TimeOnly arrival { get; set; }

    }
}