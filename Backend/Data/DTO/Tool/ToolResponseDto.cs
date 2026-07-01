namespace Backend.Data.DTO.Tool;

public record ToolResponseDto(
	int Id,
	string Name,
	int AvailableStock
);