namespace Backend.Data.Entitites
{
    public class Plan
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public ICollection<PlanBoat> Boats { get; set; } = [];

    }
}
