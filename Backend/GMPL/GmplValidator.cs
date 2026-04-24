namespace Backend.GMPL;

public static class GmplValidator
{
    public async static void Test(string datFile)
    {
        if (!File.Exists(datFile))
            throw new ValidationError($"File not found: '{datFile}'");

        int rowNumber = 0;
        foreach (string row in File.ReadLines(datFile))
        {
            rowNumber++;
            string z = row.Trim();

            if (z.StartsWith("#") || z.StartsWith("/*") ||
                z.StartsWith("*") || z == "" || z == "end;" ||
                z.StartsWith("set") || z.StartsWith("data") ||
                z.StartsWith("param") || z.StartsWith("solve")) continue;

            // Simple value assignment: token following “:=” or the last word in
            // a line containing only an identifier and a number (table cell)
            int idx = z.IndexOf(":=", StringComparison.Ordinal);
            string candidat = idx >= 0
                ? z[(idx + 2)..].TrimEnd(';', ' ', '\t')
                : z;

            // Test the last token in the row (covers table cells)
            string last = candidat.Trim().Split(' ', '\t').Last().TrimEnd(';');
            if (double.TryParse(last,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double num) && num < 0)
            {
                throw new ValidationError(
                    $"Invalid Value in'{datFile}', Row {rowNumber}: " +
                    $"Negative Value {num} found. All parameters must be >= 0.");
            }
        }
    }
}
