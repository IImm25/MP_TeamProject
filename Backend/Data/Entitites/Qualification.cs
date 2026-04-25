// Author: Erik Schellenberger and Alexander Gewinnus

public class Qualification
{
    Qualification() { }
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public Qualification(string name, string description)
    {
        Name = name;
        Description = description;
    }
}