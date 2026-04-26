// Author: Erik Schellenberger

public class Tool
{
    public Tool() { }
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int AvailableStock { get; set; }

    public Tool(string name, int availableStock)
    {
        Name = name;
        AvailableStock = availableStock;
    }
}