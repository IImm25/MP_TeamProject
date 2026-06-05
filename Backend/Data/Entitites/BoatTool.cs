namespace Backend.Data.Entitites
{
    public class BoatTool
    {
        public int PlanId { get; set; } // composite foreign key
        public int BoatNumber { get; set; }
        public PlanBoat Boat { get; set; } = null!;


        public int ToolId { get; set; }
        public Tool Tool { get; set; } = null!;
        public int RequiredAmount { get; set; }
    }
}