namespace Backend.Web.Dto;

public class TaskItemCreateDto
{
    public required string Name { get; set; }
    public float DurationHours { get; set; }
    public List<int> RequiredQualificationIds { get; set; } = [];
    public List<TaskToolDto> RequiredTools { get; set; } = [];
}

public class TaskItemUpdateDto
{
    public string? Name { get; set; }
    public float? DurationHours { get; set; }
}

public class TaskToolDto
{
    public int ToolId { get; set; }
    public int RequiredAmount { get; set; } = 1;
}

public class PersonCreateDto
{
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public List<int> QualificationIds { get; set; } = [];
}

public class PersonUpdateDto
{
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
}

public class QualificationCreateDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = "";
}

public class QualificationUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class ToolCreateDto
{
    public required string Name { get; set; }
    public int AvailableStock { get; set; }
}

public class ToolUpdateDto
{
    public string? Name { get; set; }
    public int? AvailableStock { get; set; }
}

public class QualificationResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public class ToolResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int AvailableStock { get; set; }
}

public class TaskToolResponseDto
{
    public ToolResponseDto Tool { get; set; } = null!;
    public int RequiredAmount { get; set; }
}

public class TaskItemResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public float DurationHours { get; set; }
    public List<QualificationResponseDto> RequiredQualifications { get; set; } = [];
    public List<TaskToolResponseDto> RequiredTools { get; set; } = [];
}

public class PersonResponseDto
{
    public int Id { get; set; }
    public string Firstname { get; set; } = "";
    public string Lastname { get; set; } = "";
    public List<QualificationResponseDto> Qualifications { get; set; } = [];
}