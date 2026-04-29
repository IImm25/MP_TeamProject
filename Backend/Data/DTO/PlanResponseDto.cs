namespace Backend.Data.DTO
{
    public record PlanResponseDto(
        double TotalTime,
        List<BoatPlanDto> Boats,
        List<RequirementDiffDto> ToolDiff,
        List<RequirementDiffDto> QualificationDiff
    );
}
