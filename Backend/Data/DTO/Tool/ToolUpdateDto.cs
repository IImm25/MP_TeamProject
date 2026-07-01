namespace Backend.Data.DTO.Tool;

public record ToolUpdateDto(
	string? Name,
	int? AvailableStock
);