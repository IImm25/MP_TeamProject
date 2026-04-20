using Backend.GMPL;
using System.Runtime.InteropServices;

namespace Backend.GMPL;

public class GmplSolver
{
    private static readonly (string Titel, string Dat)[] Testfaelle =
    {
        ("Normalfall — erwartet 34000 Cent",           @"C:\Users\ALEX\SynologyDrive\Master\MathProgramm\Mathematische Programmierung\Mathematische Programmierung\pasta_normal.dat"),
        ("Unlösbarer Fall — zu wenig Ressourcen",      @"C:\Users\ALEX\SynologyDrive\Master\MathProgramm\Mathematische Programmierung\Mathematische Programmierung\pasta_unloesbar.dat"),
        ("Fehlerhafter Fall — negative Eingabewerte",  @"C:\Users\ALEX\SynologyDrive\Master\MathProgramm\Mathematische Programmierung\Mathematische Programmierung\pasta_fehlerhaft.dat"),
    };
    private const string MOD = @"C:\Users\ALEX\Desktop\Studium\Master\Semester 2\Mathematische Programmieren\Programs\Nudeln\Nudeln.mod";

    public static void Call()
    {
        for (int i = 0; i < Testfaelle.Length; i++)
        {
            var (titel, dat) = Testfaelle[i];
            Ausgabe.Trennlinie($"Testfall {i + 1}: {titel}");

            try
            {
                // ── Validierung VOR dem Lösen ──────────────────
                Validator.Pruefen(dat);

                // ── Lösen ──────────────────────────────────────
                PastaErgebnis ergebnis = WindpowerSolver.Loese(MOD, dat);

                // ── Ergebnis ausgeben ──────────────────────────
                Ausgabe.Ergebnis(dat, ergebnis);

                // ── Systemtest nur beim Normalfall ─────────────
                if (i == 0)
                    Systemtest.Ausfuehren(ergebnis);
            }
            catch (ValidierungsFehler ex)
            {
                Ausgabe.Fehler("VALIDIERUNGSFEHLER", dat, ex.Message);
            }
            catch (UnloesbarFehler ex)
            {
                Ausgabe.Fehler("UNZULÄSSIG", dat, ex.Message);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [UNERWARTETER FEHLER] {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}

internal static class Glpk
{
    private const string Lib = "glpk_4_65"; // Windows: "glpk_4_65" | Linux/macOS: "glpk"

    public const int GLP_OPT = 5;
    public const int GLP_FEAS = 6;
    public const int GLP_INFEAS = 7;
    public const int GLP_UNBND = 8;
    public const int GLP_NOFEAS = 4;  // Rückgabe glp_simplex/glp_intopt
    public const int GLP_SOL = 1;
    public const int GLP_MIP = 3;
    public const int GLP_MSG_OFF = 0;

    [StructLayout(LayoutKind.Sequential)]
    public struct glp_smcp
    {
        public int msg_lev, meth, pricing, r_test;
        public double tol_bnd, tol_dj, tol_piv, obj_ll, obj_ul;
        public int it_lim, tm_lim, out_frq, out_dly, presolve;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
        public int[] _reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct glp_iocp
    {
        public int msg_lev, br_tech, bt_tech, pp_tech;
        public int fp_heur, gmi_cuts, mir_cuts, cov_cuts, clq_cuts;
        public double tol_int, tol_obj, mip_gap;
        public int tm_lim, out_frq, out_dly, cb_func, cb_info, cb_size;
        public int presolve, binarize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
        public int[] _reserved;
    }

    [DllImport(Lib)] public static extern IntPtr glp_create_prob();
    [DllImport(Lib)] public static extern void glp_delete_prob(IntPtr lp);
    [DllImport(Lib)] public static extern void glp_free_env();

    [DllImport(Lib)] public static extern IntPtr glp_mpl_alloc_wksp();
    [DllImport(Lib)]
    public static extern int glp_mpl_read_model(IntPtr tran,
                         [MarshalAs(UnmanagedType.LPStr)] string fname, int skip);
    [DllImport(Lib)]
    public static extern int glp_mpl_read_data(IntPtr tran,
                         [MarshalAs(UnmanagedType.LPStr)] string fname);
    [DllImport(Lib)] public static extern int glp_mpl_generate(IntPtr tran, IntPtr fname);
    [DllImport(Lib)] public static extern void glp_mpl_build_prob(IntPtr tran, IntPtr lp);
    [DllImport(Lib)] public static extern int glp_mpl_postsolve(IntPtr tran, IntPtr lp, int sol);
    [DllImport(Lib)] public static extern void glp_mpl_free_wksp(IntPtr tran);

    [DllImport(Lib)] public static extern void glp_init_smcp(ref glp_smcp p);
    [DllImport(Lib)] public static extern int glp_simplex(IntPtr lp, ref glp_smcp p);
    [DllImport(Lib)] public static extern void glp_init_iocp(ref glp_iocp p);
    [DllImport(Lib)] public static extern int glp_intopt(IntPtr lp, ref glp_iocp p);

    [DllImport(Lib)] public static extern int glp_get_num_cols(IntPtr lp);
    [DllImport(Lib)] public static extern int glp_get_num_int(IntPtr lp);
    [DllImport(Lib)] public static extern int glp_get_status(IntPtr lp);
    [DllImport(Lib)] public static extern int glp_mip_status(IntPtr lp);
    [DllImport(Lib)] public static extern double glp_mip_obj_val(IntPtr lp);
    [DllImport(Lib)] public static extern double glp_mip_col_val(IntPtr lp, int j);

    [DllImport(Lib, EntryPoint = "glp_get_col_name")]
    private static extern IntPtr _col(IntPtr lp, int j);
    public static string GetColName(IntPtr lp, int j)
        => Marshal.PtrToStringAnsi(_col(lp, j)) ?? $"x{j}";
}

class ValidierungsFehler : Exception
{
    public ValidierungsFehler(string msg) : base(msg) { }
}

class UnloesbarFehler : Exception
{
    public UnloesbarFehler(string msg) : base(msg) { }
}

// ═══════════════════════════════════════════════════════════════
//  Ergebnis-Datenstruktur
// ═══════════════════════════════════════════════════════════════
record PastaErgebnis(
    double Gesamtkosten,
    int sE, int mE, int tE,   // Eigenherstellung
    int sF, int mF, int tF    // Fremdbezug
);


  

   



static class Ausgabe
{
    public static void Ergebnis(string datDatei, PastaErgebnis e)
    {
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║          PASTA LÖSER — ERGEBNIS              ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
        Console.WriteLine($"  Datei         : {datDatei}");
        Console.WriteLine($"  Gesamtkosten  : {e.Gesamtkosten:F0} Cent  " +
                          $"= {e.Gesamtkosten / 100.0:F2} €");
        Console.WriteLine();
        Console.WriteLine($"  {"Sorte",-14} {"Eigenherst.",12} {"Fremdbezug",12} {"Gesamt",8}");
        Console.WriteLine($"  {"─────────────",14} {"────────────",12} {"──────────",12} {"──────",8}");
        Console.WriteLine($"  {"Spaghetti",-14} {e.sE,12} {e.sF,12} {e.sE + e.sF,8}");
        Console.WriteLine($"  {"Maccheroni",-14} {e.mE,12} {e.mF,12} {e.mE + e.mF,8}");
        Console.WriteLine($"  {"Tagliolini",-14} {e.tE,12} {e.tF,12} {e.tE + e.tF,8}");
        Console.WriteLine();
    }

    public static void Fehler(string typ, string datDatei, string nachricht)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  [{typ}] {datDatei}");
        Console.WriteLine($"  Ursache: {nachricht}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  → Test erfolgreich: Fehler korrekt abgefangen. ✓");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void Trennlinie(string titel)
    {
        Console.WriteLine();
        Console.WriteLine(new string('═', 50));
        Console.WriteLine($"  {titel}");
        Console.WriteLine(new string('═', 50));
    }
}
static class Systemtest
{
    // Erwartete optimale Lösung laut Aufgabenstellung
    private const double ERWARTETE_KOSTEN = 34000.0;
    private const double TOLERANZ = 0.5;

    public static void Ausfuehren(PastaErgebnis e)
    {
        Console.WriteLine("┌─────────────────────────────────────────────┐");
        Console.WriteLine("│               SYSTEMTEST                    │");
        Console.WriteLine("└─────────────────────────────────────────────┘");

        bool alleOk = true;

        alleOk &= Test("Gesamtkosten == 34000",
            Math.Abs(e.Gesamtkosten - ERWARTETE_KOSTEN) <= TOLERANZ,
            $"{e.Gesamtkosten:F0} Cent");

        alleOk &= Test("sE (Spaghetti eigen) == 100", e.sE == 100, $"{e.sE}");
        alleOk &= Test("mE (Maccheroni eigen) == 100", e.mE == 100, $"{e.mE}");
        alleOk &= Test("tE (Tagliolini eigen) == 100", e.tE == 100, $"{e.tE}");
        alleOk &= Test("sF (Spaghetti fremd) == 0", e.sF == 0, $"{e.sF}");
        alleOk &= Test("mF (Maccheroni fremd) == 100", e.mF == 100, $"{e.mF}");
        alleOk &= Test("tF (Tagliolini fremd) == 200", e.tF == 200, $"{e.tF}");

        Console.WriteLine();
        if (alleOk)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Alle Systemtests bestanden.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✗ Mindestens ein Systemtest fehlgeschlagen.");
        }
        Console.ResetColor();
        Console.WriteLine();
    }

    private static bool Test(string beschreibung, bool ok, string istWert)
    {
        Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"  {(ok ? "✓" : "✗")} {beschreibung,-38} → {istWert}");
        Console.ResetColor();
        return ok;
    }
}

static class WindpowerSolver
{
    public static PastaErgebnis Loese(string modDatei, string datDatei)
    {
        IntPtr lp = Glpk.glp_create_prob();
        IntPtr tran = Glpk.glp_mpl_alloc_wksp();

        try
        {
            // ── Modell und Daten laden ─────────────────────────
            if (Glpk.glp_mpl_read_model(tran, modDatei, skip: 0) != 0)
                throw new Exception($"Fehler beim Lesen von '{modDatei}'");
            if (Glpk.glp_mpl_read_data(tran, datDatei) != 0)
                throw new Exception($"Fehler beim Lesen von '{datDatei}'");
            if (Glpk.glp_mpl_generate(tran, IntPtr.Zero) != 0)
                throw new Exception("Fehler beim Generieren des Modells.");

            Glpk.glp_mpl_build_prob(tran, lp);

            // ── Schritt 1: LP-Relaxation (Simplex) ────────────
            var smcp = new Glpk.glp_smcp();
            Glpk.glp_init_smcp(ref smcp);
            smcp.msg_lev = Glpk.GLP_MSG_OFF;

            int retSimplex = Glpk.glp_simplex(lp, ref smcp);
            if (retSimplex == Glpk.GLP_NOFEAS)
                throw new UnloesbarFehler("LP-Relaxation: keine zulässige Lösung (INFEASIBLE).");
            if (retSimplex != 0)
                throw new Exception($"Simplex-Fehler (Code {retSimplex}).");

            int lpStatus = Glpk.glp_get_status(lp);
            if (lpStatus == Glpk.GLP_INFEAS || lpStatus == Glpk.GLP_NOFEAS)
                throw new UnloesbarFehler("Modell ist unzulässig — Ressourcen reichen nicht aus.");
            if (lpStatus == Glpk.GLP_UNBND)
                throw new UnloesbarFehler("Modell ist unbeschränkt.");

            // ── Schritt 2: MIP (Branch & Bound) ───────────────
            // Pflicht, da Variablen als "integer" deklariert sind
            var iocp = new Glpk.glp_iocp();
            Glpk.glp_init_iocp(ref iocp);
            iocp.msg_lev = Glpk.GLP_MSG_OFF;

            int retMip = Glpk.glp_intopt(lp, ref iocp);
            if (retMip == Glpk.GLP_NOFEAS)
                throw new UnloesbarFehler("MIP: keine ganzzahlige Lösung gefunden (INFEASIBLE).");
            if (retMip != 0)
                throw new Exception($"MIP Branch-and-Bound Fehler (Code {retMip}).");

            int mipStatus = Glpk.glp_mip_status(lp);
            if (mipStatus == Glpk.GLP_INFEAS || mipStatus == Glpk.GLP_NOFEAS)
                throw new UnloesbarFehler("MIP-Lösung unzulässig.");
            if (mipStatus == Glpk.GLP_UNBND)
                throw new UnloesbarFehler("MIP-Lösung unbeschränkt.");

            // ── Schritt 3: Variablenwerte lesen ───────────────
            // Spaltenreihenfolge im Modell: sE, mE, tE, sF, mF, tF
            int sE = (int)Math.Round(Glpk.glp_mip_col_val(lp, 1));
            int mE = (int)Math.Round(Glpk.glp_mip_col_val(lp, 2));
            int tE = (int)Math.Round(Glpk.glp_mip_col_val(lp, 3));
            int sF = (int)Math.Round(Glpk.glp_mip_col_val(lp, 4));
            int mF = (int)Math.Round(Glpk.glp_mip_col_val(lp, 5));
            int tF = (int)Math.Round(Glpk.glp_mip_col_val(lp, 6));
            double kosten = Glpk.glp_mip_obj_val(lp);

            Glpk.glp_mpl_postsolve(tran, lp, Glpk.GLP_MIP);

            return new PastaErgebnis(kosten, sE, mE, tE, sF, mF, tF);
        }
        finally
        {
            Glpk.glp_mpl_free_wksp(tran);
            Glpk.glp_delete_prob(lp);
            Glpk.glp_free_env();
        }
    }
}

static class Validator
{
    public static void Pruefen(string datDatei)
    {
        if (!File.Exists(datDatei))
            throw new ValidierungsFehler($"Datei nicht gefunden: '{datDatei}'");

        int zeilenNr = 0;
        foreach (string zeile in File.ReadLines(datDatei))
        {
            zeilenNr++;
            string z = zeile.Trim();

            // Kommentare und Schlüsselwörter überspringen
            if (z.StartsWith("#") || z.StartsWith("/*") ||
                z.StartsWith("*") || z == "" || z == "end;") continue;

            // Zuweisung: "param XYZ := WERT;"
            // Wert ist das Token nach ":="
            int idx = z.IndexOf(":=", StringComparison.Ordinal);
            if (idx < 0) continue;

            string wertTeil = z[(idx + 2)..].TrimEnd(';', ' ', '\t');
            if (double.TryParse(wertTeil.Trim(),
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double zahl) && zahl < 0)
            {
                // Parameter-Name aus dem Teil vor ":=" extrahieren
                string paramName = z[..idx].Replace("param", "").Trim();
                throw new ValidierungsFehler(
                    $"Ungültiger Wert in '{datDatei}', Zeile {zeilenNr}: " +
                    $"Parameter '{paramName}' hat negativen Wert {zahl}. " +
                    $"Alle Parameter müssen >= 0 sein.");
            }
        }
    }
}