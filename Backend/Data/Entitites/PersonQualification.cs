// Author: Erik Schellenberger

public class PersonQualification
{
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public int QualificationId { get; set; }
    public Qualification Qualification { get; set; } = null!;
}