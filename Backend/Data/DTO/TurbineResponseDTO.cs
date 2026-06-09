namespace Backend.Data.DTO
{
    public record TurbineResponseDto(
        int Id, 
        string Name, 
        float Latitude, 
        float Longitude
        );
}
