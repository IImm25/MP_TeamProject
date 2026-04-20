// Author: Erik Schellenberger

public class TaskTool
{
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public int ToolId { get; set; }
    public Tool Tool { get; set; } = null!;

    public int RequiredAmount { get; set; }

}