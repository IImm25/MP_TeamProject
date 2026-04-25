namespace Backend.Data.DTO.Read
{
    public class TaskItemSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public float DurationHours { get; set; }
    }
}
