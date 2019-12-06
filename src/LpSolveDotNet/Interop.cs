/*
    LpSolveDotNet is a .NET wrapper allowing usage of the 
    Mixed Integer Linear Programming (MILP) solver lp_solve.

    Copyright (C) 2016 Marcel Gosselin

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

// ReSharper disable InconsistentNaming
namespace LpSolveDotNet
{
    /// <summary>
    /// Parameters constants for short-cut setting of tolerances.
    /// </summary>
    public enum lpsolve_epsilon_level
    {
        /// <summary>Very tight epsilon values (default).</summary>
        EPS_TIGHT = 0,

        /// <summary>Medium epsilon values.</summary>
        EPS_MEDIUM = 1,

        /// <summary>Loose epsilon values.</summary>
        EPS_LOOSE = 2,

        /// <summary>Very loose epsilon values.</summary>
        EPS_BAGGY = 3,

        /// <summary>Default: EPS_TIGHT</summary>
        EPS_DEFAULT = EPS_TIGHT,
    }

    /// <summary>
    /// Defines the operator used in the equation/inequation representing the constraint.
    /// </summary>
    public enum lpsolve_constr_types
    {
        /// <summary>Free. A free constraint will act as if the constraint is not there. The lower bound is -Infinity and the upper bound is +Infinity.
        /// This can be used to temporary disable a constraint without having to delete it from the model. Note that the already set RHS and range on this constraint is overruled with Infinity by this.</summary>
        FR = 0,
        /// <summary>Less than or equal (&lt;=)</summary>
        LE = 1,
        /// <summary>Greater than or equal (&gt;=)</summary>
        GE = 2,
        /// <summary>Equal (=)</summary>
        EQ = 3,
        /// <summary>Objective Function. This is not an operator to use but a marker for the function to optimize.</summary>
        OF = 4,
    }

    /// <summary>
    /// Defines scaling algorithm to use. Can be any of the values from <see cref="SCALE_NONE">SCALE_NONE (0)</see> to <see cref="SCALE_CURTISREID">SCALE_CURTISREID (7)</see>
    /// that can be OR-ed with values above <see cref="SCALE_CURTISREID">SCALE_CURTISREID (7)</see>.
    /// </summary>
    [Flags]
    public enum lpsolve_scales
    {
        /// <summary>No scaling</summary>
        SCALE_NONE = 0,
        /// <summary>Scale to convergence using largest absolute value</summary>
        SCALE_EXTREME = 1,
        /// <summary>Scale based on the simple numerical range</summary>
        SCALE_RANGE = 2,
        /// <summary>Numerical range-based scaling</summary>
        SCALE_MEAN = 3,
        /// <summary>Geometric scaling</summary>
        SCALE_GEOMETRIC = 4,
        /// <summary>Curtis-reid scaling</summary>
        SCALE_CURTISREID = 7,

        /// <summary></summary>
        SCALE_QUADRATIC = 8,
        /// <summary>Scale to convergence using logarithmic mean of all values</summary>
        SCALE_LOGARITHMIC = 16,
        /// <summary>User can specify scalars</summary>
        SCALE_USERWEIGHT = 31,
        /// <summary>also do Power scaling</summary>
        SCALE_POWER2 = 32,
        /// <summary>Make sure that no scaled number is above 1</summary>
        SCALE_EQUILIBRATE = 64,
        /// <summary>also scaling integer variables</summary>
        SCALE_INTEGERS = 128,
        /// <summary>dynamic update</summary>
        SCALE_DYNUPDATE = 256,
        /// <summary>scale only rows</summary>
        SCALE_ROWSONLY = 512,
        /// <summary>scale only columns</summary>
        SCALE_COLSONLY = 1024,
    }

    /// <summary>
    /// Specifies the iterative improvement level
    /// </summary>
    [Flags]
    public enum lpsolve_improves
    {
        /// <summary>improve none</summary>
        IMPROVE_NONE = 0,
        /// <summary>Running accuracy measurement of solved equations based on Bx=r (primal simplex), remedy is refactorization.</summary>
        IMPROVE_SOLUTION = 1,
        /// <summary>Improve initial dual feasibility by bound flips (highly recommended, and default)</summary>
        IMPROVE_DUALFEAS = 2,
        /// <summary>Low-cost accuracy monitoring in the dual, remedy is refactorization</summary>
        IMPROVE_THETAGAP = 4,
        /// <summary>By default there is a check for primal/dual feasibility at optimum only for the relaxed problem, this also activates the test at the node level</summary>
        IMPROVE_BBSIMPLEX = 8,
        /// <summary>The default is IMPROVE_DUALFEAS | IMPROVE_THETAGAP.</summary>
        IMPROVE_DEFAULT = (IMPROVE_DUALFEAS + IMPROVE_THETAGAP),
        /// <summary></summary>
        IMPROVE_INVERSE = (IMPROVE_SOLUTION + IMPROVE_THETAGAP)
    }

    /// <summary>
    /// Pivot Rule
    /// </summary>
    public enum lpsolve_pivot_rule
    {
        /// <summary>Select first</summary>
        PRICER_FIRSTINDEX = 0,
        /// <summary>Select according to Dantzig</summary>
        PRICER_DANTZIG = 1,
        /// <summary>Devex pricing from Paula Harris</summary>
        PRICER_DEVEX = 2,
        /// <summary>Steepest Edge</summary>
        PRICER_STEEPESTEDGE = 3,
    }

    /// <summary>
    /// Pivot Mode. Can be a combination of many values.
    /// </summary>
    [Flags]
    public enum lpsolve_pivot_modes
    {
        /// <summary>In case of Steepest Edge, fall back to DEVEX in primal</summary>
        PRICE_PRIMALFALLBACK = 4,
        /// <summary>Preliminary implementation of the multiple pricing scheme.This means that attractive candidate entering columns from one iteration may be used in the subsequent iteration, avoiding full updating of reduced costs.In the current implementation, lp_solve only reuses the 2nd best entering column alternative</summary>
        PRICE_MULTIPLE = 8,
        /// <summary>Enable partial pricing</summary>
        PRICE_PARTIAL = 16,
        /// <summary>Temporarily use alternative strategy if cycling is detected</summary>
        PRICE_ADAPTIVE = 32,
        // /// <summary>NOT_IMPLEMENTED</summary>
        // PRICE_HYBRID = 64,
        /// <summary>Adds a small randomization effect to the selected pricer</summary>
        PRICE_RANDOMIZE = 128,
        /// <summary>Indicates automatic detection of segmented/staged/blocked models.It refers to partial pricing rather than full pricing.With full pricing, all non-basic columns are scanned, but with partial pricing only a subset is scanned for every iteration. This can speed up several models</summary>
        PRICE_AUTOPARTIAL = 256,
        /// <summary>Automatically select multiple pricing (primal simplex)</summary>
        PRICE_AUTOMULTIPLE = 512,
        /// <summary>Scan entering/leaving columns left rather than right</summary>
        PRICE_LOOPLEFT = 1024,
        /// <summary>Scan entering/leaving columns alternatingly left/right</summary>
        PRICE_LOOPALTERNATE = 2048,
        /// <summary>Use Harris' primal pivot logic rather than the default</summary>
        PRICE_HARRISTWOPASS = 4096,
        // /// <summary>Non-user option to force full pricing</summary>
        // non user option PRICE_FORCEFULL = 8192,
        /// <summary>Use true norms for Devex and Steepest Edge initializations</summary>
        PRICE_TRUENORMINIT = 16384,
        /// <summary>Disallow automatic bound-flip during pivot</summary>
        PRICE_NOBOUNDFLIP = 65536,
    }

    /// <summary>
    /// Presolve levels. Can be the OR-combination of any of the values.
    /// </summary>
    [Flags]
    public enum lpsolve_presolve
    {
        /// <summary>No presolve at all</summary>
        PRESOLVE_NONE = 0,
        /// <summary>Presolve rows</summary>
        PRESOLVE_ROWS = 1,
        /// <summary>Presolve columns</summary>
        PRESOLVE_COLS = 2,
        /// <summary>Eliminate linearly dependent rows</summary>
        PRESOLVE_LINDEP = 4,
        /// <summary>Convert constraints to SOSes (only SOS1 handled)</summary>
        //PRESOLVE_AGGREGATE = 8, //not implemented
        //PRESOLVE_SPARSER = 16, //not implemented
        PRESOLVE_SOS = 32,
        /// <summary>If the phase 1 solution process finds that a constraint is redundant then this constraint is deleted</summary>
        PRESOLVE_REDUCEMIP = 64,
        /// <summary>Simplification of knapsack-type constraints through addition of an extra variable, which also helps bound the OF</summary>
        PRESOLVE_KNAPSACK = 128,
        /// <summary>Direct substitution of one variable in 2-element equality constraints; this requires changes to the constraint matrix.
        /// Elimeq2 simply eliminates a variable by substitution when you have 2-element equality constraints.
        /// This can sometimes cause fill-in of the constraint matrix, and also be a source of rounding errors which can lead to problems in the simplex.
        /// </summary>
        PRESOLVE_ELIMEQ2 = 256,
        /// <summary>Identify implied free variables (releasing their explicit bounds)</summary>
        PRESOLVE_IMPLIEDFREE = 512,
        /// <summary>Reduce (tighten) coefficients in integer models based on GCD argument.
        /// Reduce GCD is for mixed integer programs where it is possible to adjust the constraint coefficies due to integrality.
        /// This can cause the dual objective ("lower bound") to increase and may make it easier to prove optimality.
        /// </summary>
        PRESOLVE_REDUCEGCD = 1024,
        /// <summary>Attempt to fix binary variables at one of their bounds</summary>
        PRESOLVE_PROBEFIX = 2048,
        /// <summary>Attempt to reduce coefficients in binary models</summary>
        PRESOLVE_PROBEREDUCE = 4096,
        /// <summary>Idenfify and delete qualifying constraints that are dominated by others, also fixes variables at a bound</summary>
        PRESOLVE_ROWDOMINATE = 8192,
        /// <summary>Deletes variables (mainly binary), that are dominated by others (only one can be non-zero)</summary>
        PRESOLVE_COLDOMINATE = 16384,
        /// <summary>Merges neighboring >= or &lt;= constraints when the vectors are otherwise relatively identical into a single ranged constraint</summary>
        PRESOLVE_MERGEROWS = 32768,
        /// <summary>Converts qualifying equalities to inequalities by converting a column singleton variable to slack.
        /// The routine also detects implicit duplicate slacks from inequality constraints, fixes and removes the redundant variable.
        /// This latter removal also tends to reduce the risk of degeneracy.
        /// The combined function of this option can have a dramatic simplifying effect on some models.
        /// Implied slacks is when, for example, there is a column singleton (with zero OF) in an equality constraint.
        /// In this case, the column can be deleted and the constraint converted to a LE constraint.</summary>
        PRESOLVE_IMPLIEDSLK = 65536,
        /// <summary>Variable fixing and removal based on considering signs of the associated dual constraint.
        /// Dual fixing is when the (primal) variable can be fixed due to the implied value of the dual being infinite.</summary>
        PRESOLVE_COLFIXDUAL = 131072,
        /// <summary>Does bound tightening based on full-row constraint information. This can assist in tightening the OF bound, eliminate variables and constraints.
        /// At the end of presolve, it is checked if any variables can be deemed free, thereby reducing any chance that degeneracy is introduced via this presolve option.</summary>
        PRESOLVE_BOUNDS = 262144,
        /// <summary>Calculate duals</summary>
        PRESOLVE_DUALS = 524288,
        /// <summary>Calculate sensitivity if there are integer variables</summary>
        PRESOLVE_SENSDUALS = 1048576
    }

    /// <summary>
    /// Strategy codes to avoid or recover from degenerate pivots,
    /// infeasibility or numeric errors via randomized bound relaxation
    /// </summary>
    [Flags]
    public enum lpsolve_anti_degen
    {
        /// <summary>No anti-degeneracy handling</summary>
        ANTIDEGEN_NONE = 0,
        /// <summary>Check if there are equality slacks in the basis and try to drive them out in order to reduce chance of degeneracy in Phase 1</summary>
        ANTIDEGEN_FIXEDVARS = 1,
        /// <summary></summary>
        ANTIDEGEN_COLUMNCHECK = 2,
        /// <summary></summary>
        ANTIDEGEN_STALLING = 4,
        /// <summary></summary>
        ANTIDEGEN_NUMFAILURE = 8,
        /// <summary></summary>
        ANTIDEGEN_LOSTFEAS = 16,
        /// <summary></summary>
        ANTIDEGEN_INFEASIBLE = 32,
        /// <summary></summary>
        ANTIDEGEN_DYNAMIC = 64,
        /// <summary></summary>
        ANTIDEGEN_DURINGBB = 128,
        /// <summary>Perturbation of the working RHS at refactorization</summary>
        ANTIDEGEN_RHSPERTURB = 256,
        /// <summary>Limit bound flips that can sometimes contribute to degeneracy in some models</summary>
        ANTIDEGEN_BOUNDFLIP = 512
    }


    public enum lpsolve_basiscrash
    {
        /// <summary>No basis crash</summary>
        CRASH_NOTHING = 0,
        /// <summary>Most feasible basis</summary>
        CRASH_MOSTFEASIBLE = 2,
        /// <summary>Construct a basis that is in some measure the "least degenerate"</summary>
        CRASH_LEASTDEGENERATE = 3,
    }

    public enum lpsolve_simplextypes
    {
        /// <summary>Phase1 Primal, Phase2 Primal</summary>
        SIMPLEX_PRIMAL_PRIMAL = 5,
        /// <summary>Phase1 Dual, Phase2 Primal</summary>
        SIMPLEX_DUAL_PRIMAL = 6,
        /// <summary>Phase1 Primal, Phase2 Dual</summary>
        SIMPLEX_PRIMAL_DUAL = 9,
        /// <summary>Phase1 Dual, Phase2 Dual</summary>
        SIMPLEX_DUAL_DUAL = 10,
    }

    /// <summary>
    /// Branch-and-bound rule. Can be <em>one</em> of the values below <see cref="NODE_WEIGHTREVERSEMODE">NODE_WEIGHTREVERSEMODE (8)</see>.
    /// It can be combined with one or more of the values greater than <see cref="NODE_USERSELECT">NODE_USERSELECT (7)</see>
    /// </summary>
    [Flags]
    public enum lpsolve_BBstrategies
    {
        /// <summary></summary>
        NODE_FIRSTSELECT = 0,
        /// <summary></summary>
        NODE_GAPSELECT = 1,
        /// <summary></summary>
        NODE_RANGESELECT = 2,
        /// <summary></summary>
        NODE_FRACTIONSELECT = 3,
        /// <summary></summary>
        NODE_PSEUDOCOSTSELECT = 4,
        /// <summary></summary>
        NODE_PSEUDONONINTSELECT = 5,
        /// <summary></summary>
        NODE_PSEUDORATIOSELECT = 6,
        /// <summary></summary>
        NODE_USERSELECT = 7,
        /// <summary></summary>
        NODE_WEIGHTREVERSEMODE = 8,
        /// <summary></summary>
        NODE_BRANCHREVERSEMODE = 16,
        /// <summary></summary>
        NODE_GREEDYMODE = 32,
        /// <summary></summary>
        NODE_PSEUDOCOSTMODE = 64,
        /// <summary></summary>
        NODE_DEPTHFIRSTMODE = 128,
        /// <summary></summary>
        NODE_RANDOMIZEMODE = 256,
        /// <summary></summary>
        NODE_GUBMODE = 512,
        /// <summary></summary>
        NODE_DYNAMICMODE = 1024,
        /// <summary></summary>
        NODE_RESTARTMODE = 2048,
        /// <summary></summary>
        NODE_BREADTHFIRSTMODE = 4096,
        /// <summary></summary>
        NODE_AUTOORDER = 8192,
        /// <summary></summary>
        NODE_RCOSTFIXING = 16384,
        /// <summary></summary>
        NODE_STRONGINIT = 32768
    }

    public enum lpsolve_return
    {
        /// <summary>Undefined internal error</summary>
        UNKNOWNERROR = -5,

        /// <summary>Invalid input data provided</summary>
        DATAIGNORED = -4,

        /// <summary>No basis factorization package</summary>
        NOBFP = -3,
        
        /// <summary>
        /// Out of memory
        /// </summary>
        NOMEMORY = -2,

        /// <summary>
        /// Solver has not run, usually because of an empty model.
        /// </summary>
        NOTRUN = -1,

        /// <summary>
        /// An optimal solution was obtained
        /// </summary>
        OPTIMAL = 0,

        /// <summary>
        /// The model is sub-optimal. Only happens if there are integer variables and there is already an integer solution found. The solution is not guaranteed the most optimal one.
        /// <list>
        /// <item><description>A timeout occured (set via set_timeout or with the -timeout option in lp_solve)</description></item>
        /// <item><description>set_break_at_first was called so that the first found integer solution is found (-f option in lp_solve)</description></item>
        /// <item><description>set_break_at_value was called so that when integer solution is found that is better than the specified value that it stops (-o option in lp_solve)</description></item>
        /// <item><description>set_mip_gap was called (-g/-ga/-gr options in lp_solve) to specify a MIP gap</description></item>
        /// <item><description>An abort function is installed (<see cref="LpSolve.put_abortfunc"/>) and this function returned TRUE</description></item>
        /// <item><description>At some point not enough memory could not be allocated</description></item>
        /// </list>
        /// </summary>
        SUBOPTIMAL = 1,

        /// <summary>
        /// The model is infeasible
        /// </summary>
        INFEASIBLE = 2,

        /// <summary>
        /// The model is unbounded
        /// </summary>
        UNBOUNDED = 3,

        /// <summary>
        /// The model is degenerative
        /// </summary>
        DEGENERATE = 4,

        /// <summary>
        /// Numerical failure encountered
        /// </summary>
        NUMFAILURE = 5,

        /// <summary>
        /// The abort routine returned TRUE. <see cref="LpSolve.put_abortfunc"/>
        /// </summary>
        USERABORT = 6,

        /// <summary>
        /// A timeout occurred. A timeout was set via <see cref="LpSolve.set_timeout"/>
        /// </summary>
        TIMEOUT = 7,

        /// <summary>
        /// The model could be solved by presolve. This can only happen if presolve is active via <see cref="LpSolve.set_presolve"/>
        /// </summary>
        PRESOLVED = 9,

        // defined as internal in lp_lib.h as of 5.5.2.5
        //PROCFAIL = 10,
        //PROCBREAK = 11,
        //FEASFOUND = 12,
        //NOFEASFOUND = 13,
        //FATHOMED = 14,
        //SWITCH_TO_PRIMAL = 20,
        //SWITCH_TO_DUAL   = 21,
        //SINGULAR_BASIS   = 22,
        //LOSTFEAS         = 23,
        //MATRIXERROR      = 24,

        /// <summary>
        /// Accuracy error encountered
        /// </summary>
        ACCURACYERROR = 25,
    }

    /// <summary>
    /// Defines which branch to take first in branch-and-bound algorithm.
    /// </summary>
    public enum lpsolve_branch
    {
        /// <summary>Take ceiling branch first</summary>
        BRANCH_CEILING = 0,
        /// <summary>Take floor branch first</summary>
        BRANCH_FLOOR = 1,
        /// <summary>Algorithm decides which branch being taken first</summary>
        BRANCH_AUTOMATIC = 2,
        /// <summary>
        /// (To be used only in <see cref="LpSolve.set_var_branch"/>).
        /// Means to use value set in <see cref="LpSolve.set_bb_floorfirst"/>.
        /// </summary>
        BRANCH_DEFAULT = 3,
    }

    [Flags]
    public enum lpsolve_msgmask
    {
        /// <summary>Do not handle any message.</summary>
        MSG_NONE = 0,

        /// <summary>Presolve done.</summary>
        MSG_PRESOLVE = 1,

        ///// <summary>?? Only used on user abort.</summary>
        //MSG_ITERATION = 2,

        ///// <summary>?? Only used on user abort.</summary>
        //MSG_INVERT = 4,

        /// <summary>Feasible solution found.</summary>
        MSG_LPFEASIBLE = 8,

        /// <summary>Real optimal solution found. Only fired when there are integer variables at the start of B&amp;B</summary>
        MSG_LPOPTIMAL = 16,

        //MSG_LPEQUAL = 32, //not used in lpsolve code
        //MSG_LPBETTER = 64, //not used in lpsolve code

        /// <summary>First MILPsolution found. Only fired when there are integer variables during B&amp;B</summary>
        MSG_MILPFEASIBLE = 128,

        /// <summary>Equal MILP solution found. Only fired when there are integer variables during B&amp;B</summary>
        MSG_MILPEQUAL = 256,

        /// <summary>Better MILPsolution found. Only fired when there are integer variables during B&amp;B</summary>
        MSG_MILPBETTER = 512,

        ///// <summary>?? Only used on user abort.</summary>
        //MSG_MILPSTRATEGY = 1024,

        //MSG_MILPOPTIMAL = 2048, //not used in lpsolve code
        //MSG_PERFORMANCE = 4096, //not used in lpsolve code

        ///// <summary>?? Only used on user abort.</summary>
        //MSG_INITPSEUDOCOSR = 8192,
    }

    /// <summary>
    /// Defines the amount of information that is reported back to the user.
    /// </summary>
    public enum lpsolve_verbosity
    {
        /// <summary>Only some specific debug messages in de debug print routines are reported. (Value = 0)</summary>
        NEUTRAL = 0,
        /// <summary>Only critical messages are reported. Hard errors like instability, out of memory, ... (Value = 1)</summary>
        CRITICAL = 1,
        /// <summary>Only severe messages are reported. Errors. (Value = 2)</summary>
        SEVERE = 2,
        /// <summary>Only important messages are reported. Warnings and Errors. (Value = 3)</summary>
        IMPORTANT = 3,
        /// <summary>Normal messages are reported. This is the default. (Value = 4)</summary>
        NORMAL = 4,
        /// <summary>Detailed messages are reported. Like model size, continuing B&amp;B improvements, ... (Value = 5)</summary>
        DETAILED = 5,
        /// <summary>All messages are reported. Useful for debugging purposes and small models. (Value = 6)</summary>
        FULL = 6,
    }

    /// <summary>
    /// Options used when reading MPS files. You can combine them.
    /// </summary>
    [Flags]
    public enum lpsolve_mps_options
    {
        /// <summary>Fixed MPS Format [Default] (Value = 0)
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">MPS file format</seealso></summary>
        MPS_FIXED = 0,
        /// <summary>Free MPS Format (Value = 8)
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">MPS file format</seealso></summary>
        MPS_FREE = 8,
        /// <summary>Interprete integer variables without bounds as binary variables. That is the original IBM standard.
        /// By default lp_solve interpretes variables without bounds as having no upperbound as for real variables. (Value = 16)
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">MPS file format (section G)</seealso></summary>
        MPS_IBM = 16,
        /// <summary>Interprete the objective constant with an oposite sign. Some solvers interprete the objective constant
        /// as a value in the RHS and negate it when brought at the LHS. This option allows to let lp_solve do this also. (Value = 32)
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">MPS file format</seealso></summary>
        MPS_NEGOBJCONST = 32,
    }

    public delegate bool ctrlcfunc(IntPtr lp, IntPtr userhandle);
    public delegate void msgfunc(IntPtr lp, IntPtr userhandle, lpsolve_msgmask message);
    public delegate void logfunc(IntPtr lp, IntPtr userhandle, [MarshalAs(UnmanagedType.LPStr)] string buf);

    internal static class Interop
    {
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool add_column(IntPtr lp, double[] column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool add_columnex(IntPtr lp, int count, double[] column, int[] rowno);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool add_constraint(IntPtr lp, double[] row, lpsolve_constr_types constr_type, double rh);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool add_constraintex(IntPtr lp, int count, double[] row, int[] colno, lpsolve_constr_types constr_type, double rh);
        //[DllImport("lpsolve55.dll", SetLastError = true)]
        //public static extern bool add_lag_con(IntPtr lp, double[] row, lpsolve_constr_types con_type, double rhs);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern int add_SOS(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string name, int sostype, int priority, int count, int[] sosvars, double[] weights);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int column_in_lp(IntPtr lp, double[] column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern IntPtr copy_lp(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void default_basis(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool del_column(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool del_constraint(IntPtr lp, int del_row);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void delete_lp(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool dualize_lp(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_anti_degen get_anti_degen(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool get_basis(IntPtr lp, int[] bascolumn, bool nonbasic);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_basiscrash get_basiscrash(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_bb_depthlimit(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_branch get_bb_floorfirst(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_BBstrategies get_bb_rule(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool get_bounds_tighter(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_break_at_value(IntPtr lp);
        //[DllImport("lpsolve55.dll", SetLastError=true)] public static extern string get_col_name(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", EntryPoint = "get_col_name", SetLastError = true)]
        private static extern IntPtr get_col_name_c(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool get_column(IntPtr lp, int col_nr, double[] column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_columnex(IntPtr lp, int col_nr, double[] column, int[] nzrow);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_constr_types get_constr_type(IntPtr lp, int row);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_constr_value(IntPtr lp, int row, int count, double[] primsolution, int[] nzindex);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool get_constraints(IntPtr lp, double[] constr);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool get_dual_solution(IntPtr lp, double[] rc);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_epsb(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_epsd(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_epsel(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_epsint(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_epsperturb(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_epspivot(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_improves get_improve(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_infinite(IntPtr lp);
        //[DllImport("lpsolve55.dll", SetLastError = true)]
        //public static extern bool get_lambda(IntPtr lp, double[] lambda);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_lowbo(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_lp_index(IntPtr lp, int orig_index);
        //[DllImport("lpsolve55.dll", SetLastError=true)] public static extern string get_lp_name(IntPtr lp);
        [DllImport("lpsolve55.dll", EntryPoint = "get_lp_name", SetLastError = true)]
        private static extern IntPtr get_lp_name_c(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_Lrows(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_mat(IntPtr lp, int row, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_max_level(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_maxpivot(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_mip_gap(IntPtr lp, bool absolute);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_Ncolumns(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_negrange(IntPtr lp);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern int get_nameindex(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string name, bool isrow);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_nonzeros(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_Norig_columns(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_Norig_rows(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_Nrows(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_obj_bound(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_objective(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_orig_index(IntPtr lp, int lp_index);
        //[DllImport("lpsolve55.dll", SetLastError=true)] public static extern string get_origcol_name(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", EntryPoint = "get_origcol_name", SetLastError = true)]
        private static extern IntPtr get_origcol_name_c(IntPtr lp, int column);
        //[DllImport("lpsolve55.dll", SetLastError=true)] public static extern string get_origrow_name(IntPtr lp, int row);
        [DllImport("lpsolve55.dll", EntryPoint = "get_origrow_name", SetLastError = true)]
        private static extern IntPtr get_origrow_name_c(IntPtr lp, int row);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_pivoting(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_presolve get_presolve(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_presolveloops(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool get_primal_solution(IntPtr lp, double[] pv);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_print_sol(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_rh(IntPtr lp, int row);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_rh_range(IntPtr lp, int row);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool get_row(IntPtr lp, int row_nr, double[] row);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_rowex(IntPtr lp, int row_nr, double[] row, int[] colno);
        //[DllImport("lpsolve55.dll", SetLastError=true)] public static extern string get_row_name(IntPtr lp, int row);
        [DllImport("lpsolve55.dll", EntryPoint = "get_row_name", SetLastError = true)]
        private static extern IntPtr get_row_name_c(IntPtr lp, int row);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_scalelimit(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_scales get_scaling(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool get_sensitivity_obj(IntPtr lp, double[] objfrom, double[] objtill);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool get_sensitivity_objex(IntPtr lp, double[] objfrom, double[] objtill, double[] objfromvalue, double[] objtillvalue);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool get_sensitivity_rhs(IntPtr lp, double[] duals, double[] dualsfrom, double[] dualstill);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_simplextypes get_simplextype(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_solutioncount(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_solutionlimit(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_status(IntPtr lp);
        //[DllImport("lpsolve55.dll", SetLastError=true)] public static extern string get_statustext(IntPtr lp, int statuscode);
        [DllImport("lpsolve55.dll", EntryPoint = "get_statustext", SetLastError = true)]
        private static extern IntPtr get_statustext_c(IntPtr lp, int statuscode);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_timeout(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern long get_total_iter(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern long get_total_nodes(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_upbo(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_branch get_var_branch(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_var_dualresult(IntPtr lp, int index);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_var_primalresult(IntPtr lp, int index);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern int get_var_priority(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool get_variables(IntPtr lp, double[] var);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_verbosity get_verbose(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double get_working_objective(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool guess_basis(IntPtr lp, double[] guessvector, int[] basisvector);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool has_BFP(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool has_XLI(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_add_rowmode(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_anti_degen(IntPtr lp, lpsolve_anti_degen testmask);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_binary(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_break_at_first(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_constr_type(IntPtr lp, int row, lpsolve_constr_types mask);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_debug(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_feasible(IntPtr lp, double[] values, double threshold);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_infinite(IntPtr lp, double value);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_int(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_integerscaling(IntPtr lp);
        //[DllImport("lpsolve55.dll", SetLastError = true)]
        //public static extern bool is_lag_trace(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_maxim(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_nativeBFP(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_nativeXLI(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_negative(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_piv_mode(IntPtr lp, lpsolve_pivot_modes testmask);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_piv_rule(IntPtr lp, lpsolve_pivot_rule rule);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_presolve(IntPtr lp, lpsolve_presolve testmask);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_scalemode(IntPtr lp, lpsolve_scales testmask);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_scaletype(IntPtr lp, lpsolve_scales scaletype);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_semicont(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_SOS_var(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_trace(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_unbounded(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool is_use_names(IntPtr lp, bool isrow);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void lp_solve_version(ref int majorversion, ref int minorversion, ref int release, ref int build);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern IntPtr make_lp(int rows, int columns);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool resize_lp(IntPtr lp, int rows, int columns);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void print_constraints(IntPtr lp, int columns);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool print_debugdump(IntPtr lp,  [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void print_duals(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void print_lp(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void print_objective(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void print_scales(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void print_solution(IntPtr lp, int columns);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern void print_str(IntPtr lp,  [MarshalAs(UnmanagedType.LPStr)] string str);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void print_tableau(IntPtr lp);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern void put_abortfunc(IntPtr lp, ctrlcfunc newctrlc, IntPtr ctrlchandle);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void put_logfunc(IntPtr lp, logfunc newlog, IntPtr loghandle);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void put_msgfunc(IntPtr lp, msgfunc newmsg, IntPtr msghandle, int mask);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool read_basis(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename, [MarshalAs(UnmanagedType.LPStr)] string info);
        //[DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        //public static extern IntPtr read_freeMPS([MarshalAs(UnmanagedType.LPStr)] string filename, int options);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern IntPtr read_LP([MarshalAs(UnmanagedType.LPStr)] string filename, lpsolve_verbosity verbose, [MarshalAs(UnmanagedType.LPStr)] string lp_name);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern IntPtr read_MPS([MarshalAs(UnmanagedType.LPStr)] string filename, int options);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern IntPtr read_XLI([MarshalAs(UnmanagedType.LPStr)] string xliname, [MarshalAs(UnmanagedType.LPStr)] string modelname, [MarshalAs(UnmanagedType.LPStr)] string dataname, [MarshalAs(UnmanagedType.LPStr)] string options, lpsolve_verbosity verbose);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool read_params(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename, [MarshalAs(UnmanagedType.LPStr)] string options);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void reset_basis(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void reset_params(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_add_rowmode(IntPtr lp, bool turnon);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_anti_degen(IntPtr lp, lpsolve_anti_degen anti_degen);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_basis(IntPtr lp, int[] bascolumn, bool nonbasic);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_basiscrash(IntPtr lp, lpsolve_basiscrash mode);
        //[DllImport("lpsolve55.dll", SetLastError = true)]
        //public static extern void set_basisvar(IntPtr lp, int basisPos, int enteringCol);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_bb_depthlimit(IntPtr lp, int bb_maxlevel);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_bb_floorfirst(IntPtr lp, lpsolve_branch bb_floorfirst);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_bb_rule(IntPtr lp, lpsolve_BBstrategies bb_rule);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_BFP(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_binary(IntPtr lp, int column, bool must_be_bin);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_bounds(IntPtr lp, int column, double lower, double upper);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_bounds_tighter(IntPtr lp, bool tighten);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_break_at_first(IntPtr lp, bool break_at_first);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_break_at_value(IntPtr lp, double break_at_value);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_col_name(IntPtr lp, int column, [MarshalAs(UnmanagedType.LPStr)] string new_name);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_column(IntPtr lp, int col_no, double[] column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_columnex(IntPtr lp, int col_no, int count, double[] column, int[] rowno);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_constr_type(IntPtr lp, int row, lpsolve_constr_types con_type);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_debug(IntPtr lp, bool debug);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_epsb(IntPtr lp, double epsb);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_epsd(IntPtr lp, double epsd);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_epsel(IntPtr lp, double epsel);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_epsint(IntPtr lp, double epsint);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_epslevel(IntPtr lp, lpsolve_epsilon_level level);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_epsperturb(IntPtr lp, double epsperturb);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_epspivot(IntPtr lp, double epspivot);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_improve(IntPtr lp, lpsolve_improves improve);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_infinite(IntPtr lp, double infinite);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_int(IntPtr lp, int column, bool must_be_int);
        //[DllImport("lpsolve55.dll", SetLastError = true)]
        //public static extern void set_lag_trace(IntPtr lp, bool lag_trace);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_lowbo(IntPtr lp, int column, double value);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_lp_name(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string lpname);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_mat(IntPtr lp, int row, int column, double value);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_maxim(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_maxpivot(IntPtr lp, int max_num_inv);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_minim(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_mip_gap(IntPtr lp, bool absolute, double mip_gap);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_negrange(IntPtr lp, double negrange);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_obj(IntPtr lp, int Column, double Value);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_obj_bound(IntPtr lp, double obj_bound);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_obj_fn(IntPtr lp, double[] row);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_obj_fnex(IntPtr lp, int count, double[] row, int[] colno);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_outputfile(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_pivoting(IntPtr lp, int piv_rule);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_preferdual(IntPtr lp, bool dodual);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_presolve(IntPtr lp, lpsolve_presolve do_presolve, int maxloops);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_print_sol(IntPtr lp, int print_sol);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_rh(IntPtr lp, int row, double value);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_rh_range(IntPtr lp, int row, double deltavalue);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_rh_vec(IntPtr lp, double[] rh);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_row(IntPtr lp, int row_no, double[] row);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_row_name(IntPtr lp, int row, [MarshalAs(UnmanagedType.LPStr)] string new_name);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_rowex(IntPtr lp, int row_no, int count, double[] row, int[] colno);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_scalelimit(IntPtr lp, double scalelimit);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_scaling(IntPtr lp, lpsolve_scales scalemode);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_semicont(IntPtr lp, int column, bool must_be_sc);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_sense(IntPtr lp, bool maximize);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_simplextype(IntPtr lp, lpsolve_simplextypes simplextype);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_solutionlimit(IntPtr lp, int limit);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_timeout(IntPtr lp, int sectimeout);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_trace(IntPtr lp, bool trace);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_unbounded(IntPtr lp, int column);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_upbo(IntPtr lp, int column, double value);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_use_names(IntPtr lp, bool isrow, bool use_names);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_var_branch(IntPtr lp, int column, lpsolve_branch branch_mode);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern bool set_var_weights(IntPtr lp, double[] weights);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void set_verbose(IntPtr lp, lpsolve_verbosity verbose);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool set_XLI(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern lpsolve_return solve(IntPtr lp);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool str_add_column(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string col_string);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool str_add_constraint(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string row_string, lpsolve_constr_types constr_type, double rh);
        //[DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        //public static extern bool str_add_lag_con(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string row_string, lpsolve_constr_types con_type, double rhs);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool str_set_obj_fn(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string row_string);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool str_set_rh_vec(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string rh_string);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern double time_elapsed(IntPtr lp);
        [DllImport("lpsolve55.dll", SetLastError = true)]
        public static extern void unscale(IntPtr lp);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_basis(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_freemps(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_lp(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_mps(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_XLI(IntPtr lp, [MarshalAs(UnmanagedType.LPStr)] string filename, [MarshalAs(UnmanagedType.LPStr)] string options, bool results);
        [DllImport("lpsolve55.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern bool write_params(IntPtr lp, [MarshalAs(UnmanagedType.LPStr) ] string filename, [MarshalAs(UnmanagedType.LPStr)] string options);

        public static string get_col_name(IntPtr lp, int column)
        {
            return (Marshal.PtrToStringAnsi(get_col_name_c(lp, column)));
        }

        public static string get_lp_name(IntPtr lp)
        {
            return (Marshal.PtrToStringAnsi(get_lp_name_c(lp)));
        }

        public static string get_origcol_name(IntPtr lp, int column)
        {
            return (Marshal.PtrToStringAnsi(get_origcol_name_c(lp, column)));
        }

        public static string get_origrow_name(IntPtr lp, int row)
        {
            return (Marshal.PtrToStringAnsi(get_origrow_name_c(lp, row)));
        }

        public static string get_row_name(IntPtr lp, int row)
        {
            return (Marshal.PtrToStringAnsi(get_row_name_c(lp, row)));
        }

        public static string get_statustext(IntPtr lp, int statuscode)
        {
            return (Marshal.PtrToStringAnsi(get_statustext_c(lp, statuscode)));
        }
    }
}
