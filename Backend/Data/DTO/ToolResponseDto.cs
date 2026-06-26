namespace Backend.Data.DTO;

public record ToolResponseDto(
	int Id,
	string Name,
	int AvailableStock
);