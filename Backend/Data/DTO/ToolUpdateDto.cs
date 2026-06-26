namespace Backend.Data.DTO;

public record ToolUpdateDto(
	string? Name,
	int? AvailableStock
);