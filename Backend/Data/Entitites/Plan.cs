namespace Backend.Data.Entitites
{
    public class Plan
    {
        public int Id { get; set; }
        public DateOnly date { get; set; }
        public DateTimeOffset createdAt { get; set; }
        public ICollection<PlanBoat> Boats { get; set; } = [];

    }
}
