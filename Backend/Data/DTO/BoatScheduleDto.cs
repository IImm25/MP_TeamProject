namespace Backend.Data.DTO
{
    public record BoatScheduleDto(
        TimeOnly Departure,    
        TimeOnly Arrival
    );
}