namespace Backend.GMPL;

public class ColumnNameParser
{
    public static (string VarName, string[] Indices) Parse(string colName)
    {
        int bracket = colName.IndexOf('[');
        if (bracket < 0)
            return (colName, Array.Empty<string>());

        string varName = colName[..bracket];
        string innerRaw = colName[(bracket + 1)..].TrimEnd(']');
        string[] indizes = innerRaw.Split(',');
        return (varName, indizes);
    }
}
// ════════════════════════════════ ═══════════════════════════════
//  Utility methods: parse column names
//
//  GLPK names indexed variables as follows:
//    taskOnBoat[1,a1]
//    boatUsage[2]
//    personOnBoat[1,p2]
//    toolOnBoat[3,hammer]
//
//  Format:  varName[index1]  or  varName[index1,index2]
// ════════════════════════════════ ═══════════════════════════════