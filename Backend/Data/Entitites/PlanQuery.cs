using Backend.Data.DTO;

namespace Backend.Data.Entitites
{
    public class PlanQuery
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public float MaxWorkHours { get; set; }
        public int BoatNumber { get; set; }
        public DateTime Time { get; set; }
        public float BoatSpeed { get; set; }
       
        public PlanQuery(int planId, PlanRequestDto PlanRequest)
        {
            this.PlanId = planId;
            this.MaxWorkHours = PlanRequest.MaxWorkHours;
            this.BoatNumber = PlanRequest.BoatNumber;
            this.Time = PlanRequest.Time;
            this.BoatSpeed = PlanRequest.BoatSpeed;
        }
        public PlanQuery()
        {
            
        }
    }
}
