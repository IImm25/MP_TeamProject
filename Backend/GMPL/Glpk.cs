using System.Runtime.InteropServices;
namespace Backend.GMPL;

public static class Glpk
{
    private const string Lib = "glpk_4_65"; // Windows: "glpk_4_65" | Linux/macOS: "glpk"

    public const int GLP_OPT = 5;
    public const int GLP_FEAS = 6;
    public const int GLP_INFEAS = 7;
    public const int GLP_UNBND = 8;
    public const int GLP_NOFEAS = 4;
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
