namespace Backend.GMPL;

public static class GmplSolver
{
    public static GmplResults Solve(string modFilePath, string datFilePath)
    {
        IntPtr lp = Glpk.glp_create_prob();
        IntPtr tran = Glpk.glp_mpl_alloc_wksp();

        try
        {
            // ── Model + Load Data ───────────────────────────
            if (Glpk.glp_mpl_read_model(tran, modFilePath, skip: 0) != 0)
                throw new Exception($"Error while reading '{modFilePath}'");
            if (Glpk.glp_mpl_read_data(tran, datFilePath) != 0)
                throw new Exception($"Error while reading '{datFilePath}'");
            if (Glpk.glp_mpl_generate(tran, IntPtr.Zero) != 0)
                throw new Exception("Error while generating the Modell.");

            Glpk.glp_mpl_build_prob(tran, lp);

            // ── Step 1: LP-Relaxation (Simplex) ────────────
            var smcp = new Glpk.glp_smcp();
            Glpk.glp_init_smcp(ref smcp);
            smcp.msg_lev = Glpk.GLP_MSG_OFF;

            int retSimplex = Glpk.glp_simplex(lp, ref smcp);
            if (retSimplex == Glpk.GLP_NOFEAS)
                throw new NotSolvableError("LP-Relaxation: keine zulässige Lösung (INFEASIBLE).");
            if (retSimplex != 0)
                throw new Exception($"Simplex-Fehler (Code {retSimplex}).");

            int lpStatus = Glpk.glp_get_status(lp);
            if (lpStatus is Glpk.GLP_INFEAS or Glpk.GLP_NOFEAS)
                throw new NotSolvableError("Modell ist unzulässig.");
            if (lpStatus == Glpk.GLP_UNBND)
                throw new NotSolvableError("Modell ist unbeschränkt.");

            // ── Step 2: MIP (Branch & Bound) ───────────────
            var iocp = new Glpk.glp_iocp();
            Glpk.glp_init_iocp(ref iocp);
            iocp.msg_lev = Glpk.GLP_MSG_OFF;

            int retMip = Glpk.glp_intopt(lp, ref iocp);
            if (retMip == Glpk.GLP_NOFEAS)
                throw new NotSolvableError("MIP: keine ganzzahlige Lösung gefunden (INFEASIBLE).");
            if (retMip != 0)
                throw new Exception($"MIP Branch-and-Bound Fehler (Code {retMip}).");

            int mipStatus = Glpk.glp_mip_status(lp);
            if (mipStatus is Glpk.GLP_INFEAS or Glpk.GLP_NOFEAS)
                throw new NotSolvableError("MIP-Lösung unzulässig.");
            if (mipStatus == Glpk.GLP_UNBND)
                throw new NotSolvableError("MIP-Lösung unbeschränkt.");

            // ── Step 3: Read all columns by name ─────
            //
            //  We iterate through ALL columns and sort the values
            //  into the appropriate dictionaries based on the parsed
            //  column names.  This means the order in which
            //  GLPK internally creates columns doesn't matter.
            // ─────────────────────────────────────────────────

            var taskOnBoat = new Dictionary<int, Dictionary<string, int>>();
            var boatUsage = new Dictionary<int, int>();
            var personOnBoat = new Dictionary<int, Dictionary<string, int>>();
            var toolOnBoat = new Dictionary<int, Dictionary<string, int>>();

            int numCols = Glpk.glp_get_num_cols(lp);

            for (int j = 1; j <= numCols; j++)
            {
                string colName = Glpk.GetColName(lp, j);
                double rawVal = Glpk.glp_mip_col_val(lp, j);
                int val = (int)Math.Round(rawVal);

                var (varName, indices) = ColumnNameParser.Parse(colName);

                switch (varName)
                {
                    // ── taskOnBoat[boat, aufgabe] ──────────────
                    case "taskOnBoat" when indices.Length == 2:
                        {
                            int boat = int.Parse(indices[0]);
                            string aufgabe = indices[1];

                            if (!taskOnBoat.ContainsKey(boat))
                                taskOnBoat[boat] = new Dictionary<string, int>();

                            taskOnBoat[boat][aufgabe] = val;
                            break;
                        }

                    // ── boatUsage[boat] ────────────────────────
                    case "boatUsage" when indices.Length == 1:
                        {
                            int boat = int.Parse(indices[0]);
                            boatUsage[boat] = val;
                            break;
                        }

                    // ── personOnBoat[boat, person] ─────────────
                    case "personOnBoat" when indices.Length == 2:
                        {
                            int boat = int.Parse(indices[0]);
                            string person = indices[1];

                            if (!personOnBoat.ContainsKey(boat))
                                personOnBoat[boat] = new Dictionary<string, int>();

                            personOnBoat[boat][person] = val;
                            break;
                        }

                    // ── toolOnBoat[boat, tool] ─────────────
                    case "toolOnBoat" when indices.Length == 2:
                        {
                            int boat = int.Parse(indices[0]);
                            string tool = indices[1];

                            if (!toolOnBoat.ContainsKey(boat))
                                toolOnBoat[boat] = new Dictionary<string, int>();

                            toolOnBoat[boat][tool] = val;
                            break;
                        }

                    // Unbekannte Spalte → ignorieren / loggen
                    default:
                        Console.WriteLine($"  [INFO] unknown columns ingored: '{colName}'");
                        break;
                }
            }

            int amountBoats = (int)Math.Round(Glpk.glp_mip_obj_val(lp));

            Glpk.glp_mpl_postsolve(tran, lp, Glpk.GLP_MIP);

            return new GmplResults(amountBoats, taskOnBoat, boatUsage, personOnBoat, toolOnBoat);
        }
        finally
        {
            Glpk.glp_mpl_free_wksp(tran);
            Glpk.glp_delete_prob(lp);
            Glpk.glp_free_env();
        }
    }
}
