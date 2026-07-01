namespace Backend.Data.Entitites
{
    public class BoatSchedule
    {
        public int PlanId { get; set; }
        public int BoatNumber { get; set; }
        public PlanBoat Boat { get; set; } = null!;
        public int TripNumber { get; set; }

        public TimeOnly Departure {  get; set; }
        public TimeOnly Arrival { get; set; }

        public int OriginId { get; set; }
        public Location Origin { get; set; } = null!;
        public int DestinationId { get; set; }
        public Location Destination { get; set; } = null!;
    }
}