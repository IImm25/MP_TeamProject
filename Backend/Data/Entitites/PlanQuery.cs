namespace Backend.Data.Entitites
{
    public class PlanQuery
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public string JsonQuery { get; set; } = "";
        public DateOnly PlanDate {  get; set; }

        public PlanQuery(int planId, string JsonQuery)
        {
            this.PlanId = planId;
            this.JsonQuery = JsonQuery;
        }
    }
}
