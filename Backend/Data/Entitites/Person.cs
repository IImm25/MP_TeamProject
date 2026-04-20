// Authors: Erik Schellenberger and Alexander Gewinnus

public class Person
{
    public Person(string firstname, string lastname)
    {
        Firstname = firstname;
        Lastname = lastname;
    }

    public int Id { get; set; }
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }

    public ICollection<PersonQualification> Qualifications { get; set; } = [];
}