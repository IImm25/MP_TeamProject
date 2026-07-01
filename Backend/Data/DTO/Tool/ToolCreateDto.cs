namespace Backend.Data.DTO.Tool;

public record ToolCreateDto(
	string Name,
	int AvailableStock
);