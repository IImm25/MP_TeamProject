// Author: Erik Schellenberger

public class Person
{
    public required int Id { get; set; }
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }

    public ICollection<PersonQualification> Qualifications { get; set; } = [];
}