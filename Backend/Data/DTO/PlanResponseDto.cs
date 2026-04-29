namespace Backend.Data.DTO
{
    public record PlanResponseDto(
        double totalTime,
        List<BoatPlanDto> boats
    );
}
