namespace Backend.Data.DTO;

// ─── Response DTOs (kein zirkulärer Verweis) ─────────────────────────────────

public class QualificationResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}
