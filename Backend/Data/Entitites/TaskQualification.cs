// Author: Erik Schellenberger

public class TaskQualification
{
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public int QualificationId { get; set; }
    public Qualification Qualification { get; set; } = null!;
    public int RequiredAmount { get; set; }
}