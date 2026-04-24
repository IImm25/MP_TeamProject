namespace Backend.GMPL;

public class GmplResults
{
    public GmplResults(float workHours, 
        Dictionary<int, Dictionary<string, int>> taskOnBoat, 
        Dictionary<int, int> boatUsage, 
        Dictionary<int, Dictionary<string, int>> personOnBoat, 
        Dictionary<int, Dictionary<string, int>> toolOnBoat)
    {
        Workhours = workHours;
        TaskOnBoat = taskOnBoat;
        BoatUsage = boatUsage;
        PersonOnBoat = personOnBoat;
        ToolOnBoat = toolOnBoat;
    }
    public float Workhours { get; set; }
    
    // taskOnBoat[boot][task]
    public Dictionary<int, Dictionary<string, int>> TaskOnBoat {  get; set; }


    // boatUsage[boot]
    public Dictionary<int, int> BoatUsage { get; set; }
    
    // personOnBoat[boot][person]
    public Dictionary<int, Dictionary<string, int>> PersonOnBoat { get; set; }
    
    // toolOnBoat[boot][tool]
    public Dictionary<int, Dictionary<string, int>> ToolOnBoat { get; set; }
}
//═══════════════════════════════════════════════════════════════
//  Result data structure
//
//  All GLPK variables of the model are mapped here:
//
//  taskOnBoat   [boot, task]  → 1 if task is on the boat
//  boatUsage    [boat]           → 1 if boat is in use
//  personOnBoat [boat, person]   → 1 if person is on boat
//  toolOnBoat   [boat, tool] → Number of tools on boat
//  AmountBoats                   → Objective function value (number of boats)
// ═══════════════════════════════════════════════════════════════