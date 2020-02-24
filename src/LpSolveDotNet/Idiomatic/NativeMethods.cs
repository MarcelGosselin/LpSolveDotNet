/*
    LpSolveDotNet is a .NET wrapper allowing usage of the 
    Mixed Integer Linear Programming (MILP) solver lp_solve.

    Copyright (C) 2016-2025 Marcel Gosselin

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
    USA

    https://github.com/MarcelGosselin/LpSolveDotNet/blob/master/LICENSE

 * 
 * This file is a modified version of lpsolve55.cs from lpsolve project available at
 *      https://sourceforge.net/projects/lpsolve/files/lpsolve/5.5.2.0/lp_solve_5.5.2.0_cs.net.zip/download
 * modified to:
 *      - handle 64-bit processors better by switching to IntPtr instead of int for pointers to lprec structure.
 *      - handle 64-bit user handles in delegates
 *      - when Init() is called without a path to the dll, try to figure out right version of dll to load.
 *      - remove useless unsafe keywords
 *      - remove Lagrangian-related methods as they are not working as stated in lpsolve docs
 *      - remove methods declared internal by lpsolve
 *      - fix enum types passed to methods
 *      - extract enums and delegates out of the lpsolve static class
 *      - add Flags attribute to some enums
 *      - rename class to Interop and make internal so we can create a new class LpSolve which is a
 *        real object-oriented wrapper on top of lpsolve.
 *      - moved library initialization to new LpSolve wrapper
 */

using System;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming rule violations
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1717 // Only FlagsAttribute enums should have plural names
#pragma warning disable CA1714 // Flags enums should have plural names

// ReSharper disable InconsistentNaming
namespace LpSolveDotNet.Idiomatic
{
    internal static class NativeMethods
    {
        /// <summary>
        /// The name of the library to load, without its extension.
        /// </summary>
        public const string LibraryName = "lpsolve55";

        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool add_column(IntPtr lp, double[] column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool add_columnex(IntPtr lp, int count, double[] column, int[] rowno);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool add_constraint(IntPtr lp, double[] row, ConstraintOperator constr_type, double rh);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool add_constraintex(IntPtr lp, int count, double[] row, int[] colno, ConstraintOperator constr_type, double rh);
        //[DllImport(LibraryName, SetLastError = true)]
        //public static extern bool add_lag_con(IntPtr lp, double[] row, lpsolve_constr_types con_type, double rhs);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern int add_SOS(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string name, int sostype, int priority, int count, int[] sosvars, double[] weights);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int column_in_lp(IntPtr lp, double[] column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern IntPtr copy_lp(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void default_basis(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool del_column(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool del_constraint(IntPtr lp, int del_row);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void delete_lp(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool dualize_lp(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern AntiDegeneracyRules get_anti_degen(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool get_basis(IntPtr lp, int[] bascolumn, bool nonbasic);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern BasisCrashMode get_basiscrash(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_bb_depthlimit(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern BranchMode get_bb_floorfirst(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_bb_rule(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool get_bounds_tighter(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_break_at_value(IntPtr lp);
        //[DllImport(LibraryName, SetLastError=true)] public static extern string get_col_name(IntPtr lp, int column);
        [DllImport(LibraryName, EntryPoint = "get_col_name", SetLastError = true)]
        private static extern IntPtr get_col_name_c(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool get_column(IntPtr lp, int col_nr, double[] column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_columnex(IntPtr lp, int col_nr, double[] column, int[] nzrow);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern ConstraintOperator get_constr_type(IntPtr lp, int row);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_constr_value(IntPtr lp, int row, int count, double[] primsolution, int[] nzindex);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool get_constraints(IntPtr lp, double[] constr);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool get_dual_solution(IntPtr lp, double[] rc);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_epsb(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_epsd(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_epsel(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_epsint(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_epsperturb(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_epspivot(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern IterativeImprovementLevels get_improve(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_infinite(IntPtr lp);
        //[DllImport(LibraryName, SetLastError = true)]
        //public static extern bool get_lambda(IntPtr lp, double[] lambda);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_lowbo(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_lp_index(IntPtr lp, int orig_index);
        //[DllImport(LibraryName, SetLastError=true)] public static extern string get_lp_name(IntPtr lp);
        [DllImport(LibraryName, EntryPoint = "get_lp_name", SetLastError = true)]
        private static extern IntPtr get_lp_name_c(IntPtr lp);
        //[DllImport(LibraryName, SetLastError = true)]
        //public static extern int get_Lrows(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_mat(IntPtr lp, int row, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_max_level(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_maxpivot(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_mip_gap(IntPtr lp, bool absolute);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_Ncolumns(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_negrange(IntPtr lp);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern int get_nameindex(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string name, bool isrow);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_nonzeros(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_Norig_columns(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_Norig_rows(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_Nrows(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_obj_bound(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_objective(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_orig_index(IntPtr lp, int lp_index);
        //[DllImport(LibraryName, SetLastError=true)] public static extern string get_origcol_name(IntPtr lp, int column);
        [DllImport(LibraryName, EntryPoint = "get_origcol_name", SetLastError = true)]
        private static extern IntPtr get_origcol_name_c(IntPtr lp, int column);
        //[DllImport(LibraryName, SetLastError=true)] public static extern string get_origrow_name(IntPtr lp, int row);
        [DllImport(LibraryName, EntryPoint = "get_origrow_name", SetLastError = true)]
        private static extern IntPtr get_origrow_name_c(IntPtr lp, int row);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_pivoting(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern PreSolveLevels get_presolve(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_presolveloops(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool get_primal_solution(IntPtr lp, double[] pv);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern PrintSolutionOption get_print_sol(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_rh(IntPtr lp, int row);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_rh_range(IntPtr lp, int row);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool get_row(IntPtr lp, int row_nr, double[] row);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_rowex(IntPtr lp, int row_nr, double[] row, int[] colno);
        //[DllImport(LibraryName, SetLastError=true)] public static extern string get_row_name(IntPtr lp, int row);
        [DllImport(LibraryName, EntryPoint = "get_row_name", SetLastError = true)]
        private static extern IntPtr get_row_name_c(IntPtr lp, int row);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_scalelimit(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_scaling(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool get_sensitivity_obj(IntPtr lp, double[] objfrom, double[] objtill);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool get_sensitivity_objex(IntPtr lp, double[] objfrom, double[] objtill, double[] objfromvalue, double[] objtillvalue);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool get_sensitivity_rhs(IntPtr lp, double[] duals, double[] dualsfrom, double[] dualstill);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern SimplexType get_simplextype(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_solutioncount(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_solutionlimit(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_status(IntPtr lp);
        //[DllImport(LibraryName, SetLastError=true)] public static extern string get_statustext(IntPtr lp, int statuscode);
        [DllImport(LibraryName, EntryPoint = "get_statustext", SetLastError = true)]
        private static extern IntPtr get_statustext_c(IntPtr lp, int statuscode);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_timeout(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern long get_total_iter(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern long get_total_nodes(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_upbo(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern BranchMode get_var_branch(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_var_dualresult(IntPtr lp, int index);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_var_primalresult(IntPtr lp, int index);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern int get_var_priority(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool get_variables(IntPtr lp, double[] var);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern Verbosity get_verbose(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_working_objective(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool guess_basis(IntPtr lp, double[] guessvector, int[] basisvector);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool has_BFP(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool has_XLI(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_add_rowmode(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_anti_degen(IntPtr lp, AntiDegeneracyRules testmask);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_binary(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_break_at_first(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_constr_type(IntPtr lp, int row, ConstraintOperator mask);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_debug(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_feasible(IntPtr lp, double[] values, double threshold);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_infinite(IntPtr lp, double value);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_int(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_integerscaling(IntPtr lp);
        //[DllImport(LibraryName, SetLastError = true)]
        //public static extern bool is_lag_trace(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_maxim(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_nativeBFP(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_nativeXLI(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_negative(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_piv_mode(IntPtr lp, PivotModes testmask);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_piv_rule(IntPtr lp, PivotRule rule);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_presolve(IntPtr lp, PreSolveLevels testmask);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_scalemode(IntPtr lp, int testmask);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_scaletype(IntPtr lp, ScaleAlgorithm scaletype);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_semicont(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_SOS_var(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_trace(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_unbounded(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool is_use_names(IntPtr lp, bool isrow);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void lp_solve_version(ref int majorversion, ref int minorversion, ref int release, ref int build);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern IntPtr make_lp(int rows, int columns);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool resize_lp(IntPtr lp, int rows, int columns);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void print_constraints(IntPtr lp, int columns);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool print_debugdump(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void print_duals(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void print_lp(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void print_objective(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void print_scales(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void print_solution(IntPtr lp, int columns);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern void print_str(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string str);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void print_tableau(IntPtr lp);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern void put_abortfunc(IntPtr lp, ctrlcfunc newctrlc, IntPtr ctrlchandle);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void put_logfunc(IntPtr lp, logfunc newlog, IntPtr loghandle);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void put_msgfunc(IntPtr lp, msgfunc newmsg, IntPtr msghandle, MessageMasks mask);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool read_basis(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename, [MarshalAs(UnmanagedType.LPStr)] string info);
        //[DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        //public static extern IntPtr read_freeMPS([MarshalAs(UnmanagedType.LPStr)] string filename, int options);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern IntPtr read_LP([MarshalAs(UnmanagedType.LPStr)] string filename, Verbosity verbose, [MarshalAs(UnmanagedType.LPStr)] string lp_name);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern IntPtr read_MPS([MarshalAs(UnmanagedType.LPStr)] string filename, int options);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern IntPtr read_XLI([MarshalAs(UnmanagedType.LPStr)] string xliname, [MarshalAs(UnmanagedType.LPStr)] string modelname, [MarshalAs(UnmanagedType.LPStr)] string dataname, [MarshalAs(UnmanagedType.LPStr)] string options, Verbosity verbose);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool read_params(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename, [MarshalAs(UnmanagedType.LPStr)] string options);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void reset_basis(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void reset_params(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_add_rowmode(IntPtr lp, bool turnon);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_anti_degen(IntPtr lp, AntiDegeneracyRules anti_degen);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_basis(IntPtr lp, int[] bascolumn, bool nonbasic);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_basiscrash(IntPtr lp, BasisCrashMode mode);
        //[DllImport(LibraryName, SetLastError = true)]
        //public static extern void set_basisvar(IntPtr lp, int basisPos, int enteringCol);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_bb_depthlimit(IntPtr lp, int bb_maxlevel);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_bb_floorfirst(IntPtr lp, BranchMode bb_floorfirst);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_bb_rule(IntPtr lp, int bb_rule);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void put_bb_nodefunc(IntPtr lp, bbnodefunc newnode, IntPtr bbnodehandle);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void put_bb_branchfunc(IntPtr lp, bbbranchfunc newnode, IntPtr bbbranchhandle);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_BFP(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_binary(IntPtr lp, int column, bool must_be_bin);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_bounds(IntPtr lp, int column, double lower, double upper);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_bounds_tighter(IntPtr lp, bool tighten);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_break_at_first(IntPtr lp, bool break_at_first);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_break_at_value(IntPtr lp, double break_at_value);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_col_name(IntPtr lp, int column, [MarshalAs(UnmanagedType.LPStr)] string new_name);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_column(IntPtr lp, int col_no, double[] column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_columnex(IntPtr lp, int col_no, int count, double[] column, int[] rowno);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_constr_type(IntPtr lp, int row, ConstraintOperator con_type);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_debug(IntPtr lp, bool debug);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_epsb(IntPtr lp, double epsb);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_epsd(IntPtr lp, double epsd);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_epsel(IntPtr lp, double epsel);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_epsint(IntPtr lp, double epsint);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_epslevel(IntPtr lp, ToleranceEpsilonLevel level);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_epsperturb(IntPtr lp, double epsperturb);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_epspivot(IntPtr lp, double epspivot);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_improve(IntPtr lp, IterativeImprovementLevels improve);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_infinite(IntPtr lp, double infinite);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_int(IntPtr lp, int column, bool must_be_int);
        //[DllImport(LibraryName, SetLastError = true)]
        //public static extern void set_lag_trace(IntPtr lp, bool lag_trace);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_lowbo(IntPtr lp, int column, double value);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_lp_name(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string lpname);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_mat(IntPtr lp, int row, int column, double value);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_maxim(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_maxpivot(IntPtr lp, int max_num_inv);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_minim(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_mip_gap(IntPtr lp, bool absolute, double mip_gap);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_negrange(IntPtr lp, double negrange);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_obj(IntPtr lp, int Column, double Value);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_obj_bound(IntPtr lp, double obj_bound);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_obj_fn(IntPtr lp, double[] row);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_obj_fnex(IntPtr lp, int count, double[] row, int[] colno);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_outputfile(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_pivoting(IntPtr lp, int piv_rule);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_preferdual(IntPtr lp, bool dodual);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_presolve(IntPtr lp, PreSolveLevels do_presolve, int maxloops);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_print_sol(IntPtr lp, PrintSolutionOption print_sol);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_rh(IntPtr lp, int row, double value);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_rh_range(IntPtr lp, int row, double deltavalue);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_rh_vec(IntPtr lp, double[] rh);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_row(IntPtr lp, int row_no, double[] row);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_row_name(IntPtr lp, int row, [MarshalAs(UnmanagedType.LPStr)] string new_name);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_rowex(IntPtr lp, int row_no, int count, double[] row, int[] colno);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_scalelimit(IntPtr lp, double scalelimit);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_scaling(IntPtr lp, int scalemode);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_semicont(IntPtr lp, int column, bool must_be_sc);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_sense(IntPtr lp, bool maximize);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_simplextype(IntPtr lp, SimplexType simplextype);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_solutionlimit(IntPtr lp, int limit);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_timeout(IntPtr lp, int sectimeout);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_trace(IntPtr lp, bool trace);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_unbounded(IntPtr lp, int column);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_upbo(IntPtr lp, int column, double value);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_use_names(IntPtr lp, bool isrow, bool use_names);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_var_branch(IntPtr lp, int column, BranchMode branch_mode);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern bool set_var_weights(IntPtr lp, double[] weights);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_verbose(IntPtr lp, Verbosity verbose);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_XLI(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern SolveResult solve(IntPtr lp);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool str_add_column(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string col_string);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool str_add_constraint(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string row_string, ConstraintOperator constr_type, double rh);
        //[DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        //public static extern bool str_add_lag_con(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string row_string, lpsolve_constr_types con_type, double rhs);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool str_set_obj_fn(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string row_string);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool str_set_rh_vec(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string rh_string);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double time_elapsed(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void unscale(IntPtr lp);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_basis(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_freemps(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_lp(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_mps(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_XLI(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename, [MarshalAs(UnmanagedType.LPStr)] string options, bool results);
        [DllImport(LibraryName, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_params(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename, [MarshalAs(UnmanagedType.LPStr)] string options);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_accuracy(IntPtr lp);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern void set_break_numeric_accuracy(IntPtr lp, double accuracy);
        [DllImport(LibraryName, SetLastError = true)]
        public static extern double get_break_numeric_accuracy(IntPtr lp);

        public static string get_col_name(IntPtr lp, int column) => (Marshal.PtrToStringAnsi(get_col_name_c(lp, column)));

        public static string get_lp_name(IntPtr lp) => (Marshal.PtrToStringAnsi(get_lp_name_c(lp)));

        public static string get_origcol_name(IntPtr lp, int column) => (Marshal.PtrToStringAnsi(get_origcol_name_c(lp, column)));

        public static string get_origrow_name(IntPtr lp, int row) => (Marshal.PtrToStringAnsi(get_origrow_name_c(lp, row)));

        public static string get_row_name(IntPtr lp, int row) => (Marshal.PtrToStringAnsi(get_row_name_c(lp, row)));

        public static string get_statustext(IntPtr lp, int statuscode) => (Marshal.PtrToStringAnsi(get_statustext_c(lp, statuscode)));

        internal delegate bool ctrlcfunc(IntPtr lp, IntPtr userhandle);
        internal delegate void msgfunc(IntPtr lp, IntPtr userhandle, MessageMasks message);
        internal delegate void logfunc(IntPtr lp, IntPtr userhandle, [MarshalAs(UnmanagedType.LPStr)] string buf);
        internal delegate int bbnodefunc(IntPtr lp, IntPtr userhandle, int vartype);
        internal delegate bool bbbranchfunc(IntPtr lp, IntPtr userhandle, int column);
    }
}
#pragma warning restore IDE1006 // Naming rule violations
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore CA1717 // Only FlagsAttribute enums should have plural names
#pragma warning restore CA1714 // Flags enums should have plural names