namespace Backend.GMPL;

public class GmplOutput2Console
{
    public static void GetGmplResults(GmplResults e)
    {
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║        Boat-Optimization — Results           ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
        Console.WriteLine($"  Working Hours: {e.Workhours}");
        Console.WriteLine();

        // List all boats that are actually in use
        var usedBoats = e.BoatUsage
            .Where(kv => kv.Value == 1)
            .Select(kv => kv.Key)
            .OrderBy(b => b)
            .ToList();

        foreach (int boat in usedBoats)
        {
            Console.WriteLine($"  ┌─── Boat {boat} ───────────────────────────────┐");

            // Tasks on this boat
            if (e.TaskOnBoat.TryGetValue(boat, out var tasks))
            {
                var activeTasks = tasks
                    .Where(kv => kv.Value == 1)
                    .Select(kv => kv.Key)
                    .OrderBy(s => s)
                    .ToList();

                Console.WriteLine($"  │  Tasks    : {string.Join(", ", activeTasks)}");
            }

            // people on this boat
            if (e.PersonOnBoat.TryGetValue(boat, out var people))
            {
                var activePeople = people
                    .Where(kv => kv.Value == 1)
                    .Select(kv => kv.Key)
                    .OrderBy(s => s)
                    .ToList();

                Console.WriteLine($"  │  People    : {string.Join(", ", activePeople)}");
            }

            // tools on this boat
            if (e.ToolOnBoat.TryGetValue(boat, out var tools))
            {
                var aktiveTools = tools
                    .Where(kv => kv.Value > 0)
                    .OrderBy(kv => kv.Key)
                    .Select(kv => $"{kv.Key}={kv.Value}")
                    .ToList();

                Console.WriteLine($"  │  Tools    : {string.Join(", ", aktiveTools)}");
            }

            Console.WriteLine($"  └──────────────────────────────────────────────┘");
            Console.WriteLine();
        }
    }
}
