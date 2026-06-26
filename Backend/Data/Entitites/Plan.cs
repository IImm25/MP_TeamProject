namespace Backend.Data.Entitites
{
    public class Plan
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public ICollection<PlanBoat> Boats { get; set; } = [];
        public Plan()
        {
            
        }

        public Plan(DateOnly date, DateTimeOffset createdAt, ICollection<PlanBoat> boats)
        {
            Date = date;
            CreatedAt = createdAt;
            Boats = boats;
        }
    }
}
