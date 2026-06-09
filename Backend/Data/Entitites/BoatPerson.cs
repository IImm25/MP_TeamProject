namespace Backend.Data.Entitites
{
    public class BoatPerson
    {
        public int PlanId { get; set; }
        public int BoatNumber { get; set; }
        public PlanBoat Boat { get; set; } = null!;

        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;
    }
}