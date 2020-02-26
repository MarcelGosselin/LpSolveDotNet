using System;

#pragma warning disable 1591  // documentation
#pragma warning disable 618   // Obsolete warning
#pragma warning disable IDE1006 // Naming rule violations
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1717 // Only FlagsAttribute enums should have plural names
#pragma warning disable CA1714 // Flags enums should have plural names
#pragma warning disable CA1801 //  Parameter fileName of method read_MPS is never used
#pragma warning disable CA1822 // Mark members as static

namespace LpSolveDotNet.Idiomatic
{
    public sealed partial class LpSolve
    {
        #region Callbacks

        [Obsolete("Replaced by " + nameof(PutBranchAndBoundNodeSelector), true)]
        public void put_bb_nodefunc(params object[] _)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(PutBranchAndBoundBranchSelector), true)]
        public void put_bb_branchfunc(params object[] _)
            => throw new NotImplementedException();

        // Marked as false to let enum be updated first
        [Obsolete("Replaced by " + nameof(PutMessageHandler), false)]
        public void put_msgfunc(msgfunctemp handler, params object[] _)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(PutLogHandler), true)]
        public void put_logfunc(logfunctemp handler, params object[] _)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(PutAbortHandler), true)]
        public void put_abortfunc(ctrlcfunctemp handler, params object[] _)
            => throw new NotImplementedException();

        #endregion

        #region Enums Phase 1 // No Obsolete on method but rather on enum values (error) and enum type (warning)

        public void put_msgfunc(msgfunctemp newmsg, IntPtr msghandle, lpsolve_msgmask mask)
            => throw new NotImplementedException();

        public void set_print_sol(lpsolve_print_sol_option _)
            => throw new NotImplementedException();

        public static LpSolve read_MPS(string fileName, lpsolve_verbosity verbose, lpsolve_mps_options options)
            => throw new NotImplementedException();

        public static LpSolve read_LP(string fileName, lpsolve_verbosity verbose, string lpName)
            => throw new NotImplementedException();

        public static LpSolve read_XLI(string xliName, string modelName, string dataName, string options, lpsolve_verbosity verbose)
            => throw new NotImplementedException();

        public void set_verbose(lpsolve_verbosity verbose)
            => throw new NotImplementedException();

        public bool set_var_branch(int column, lpsolve_branch branch_mode)
            => throw new NotImplementedException();

        public void set_bb_floorfirst(lpsolve_branch bb_floorfirst)
            => throw new NotImplementedException();

        public void set_bb_rule(lpsolve_BBstrategies bb_rule)
            => throw new NotImplementedException();

        public void set_simplextype(lpsolve_simplextypes simplextype)
            => throw new NotImplementedException();

        public void set_basiscrash(lpsolve_basiscrash mode)
            => throw new NotImplementedException();

        public void set_anti_degen(lpsolve_anti_degen anti_degen)
            => throw new NotImplementedException();

        public bool is_anti_degen(lpsolve_anti_degen testmask)
            => throw new NotImplementedException();

        public void set_presolve(lpsolve_presolve do_presolve, int maxloops)
            => throw new NotImplementedException();

        public bool is_presolve(lpsolve_presolve testmask)
            => throw new NotImplementedException();

        public void set_improve(lpsolve_improves improve)
            => throw new NotImplementedException();

        public bool add_constraintex(int count, double[] row, int[] colno, lpsolve_constr_types constr_type, double rh)
            => throw new NotImplementedException();

        public bool add_constraint(double[] row, lpsolve_constr_types constr_type, double rh)
            => throw new NotImplementedException();

        public bool set_constr_type(int row, lpsolve_constr_types con_type)
            => throw new NotImplementedException();

        public bool is_constr_type(int row, lpsolve_constr_types mask)
            => throw new NotImplementedException();

        public bool set_epslevel(lpsolve_epsilon_level level)
            => throw new NotImplementedException();

        public void set_scaling(lpsolve_scale_algorithm algorithm, lpsolve_scale_parameters parameters)
            => throw new NotImplementedException();

        public bool is_scalemode(lpsolve_scale_algorithm algorithmMask, lpsolve_scale_parameters parameterMask)
            => throw new NotImplementedException();

        public bool is_scaletype(lpsolve_scale_algorithm algorithm)
            => throw new NotImplementedException();

        public void set_pivoting(lpsolve_pivot_rule rule, lpsolve_pivot_modes modes)
            => throw new NotImplementedException();

        public bool is_piv_rule(lpsolve_pivot_rule rule)
            => throw new NotImplementedException();

        public bool is_piv_mode(lpsolve_pivot_modes testmask)
            => throw new NotImplementedException();

        #endregion

        #region Enums Phase 1.5 // Methods returning enums cannot have overloads so Obsolete with warning to prevent compilation errors before enum values have been fixed

        [Obsolete("Replaced by " + nameof(Solve), false)]
        public lpsolve_return solve()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(PrintSolutionOption) + " property", false)]
        public PrintSolutionOption get_print_sol()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Verbosity) + " property", false)]
        public Verbosity get_verbose()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(FirstBranch) + " property", false)]
        public BranchMode get_bb_floorfirst()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(BranchAndBoundRuleAndModes) + " property", false)]
        public lpsolve_BBstrategies get_bb_rule()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(SimplexType) + " property", false)]
        public lpsolve_simplextypes get_simplextype()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Basis) + "." + nameof(ModelBasis.CrashMode) + " property", false)]
        public lpsolve_basiscrash get_basiscrash()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(AntiDegeneracyRules) + " property", false)]
        public lpsolve_anti_degen get_anti_degen()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(PreSolveLevels) + " property", false)]
        public lpsolve_presolve get_presolve()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(IterativeImprovementLevels) + " property", false)]
        public lpsolve_improves get_improve()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.ConstraintOperator), true)]
        public lpsolve_constr_types get_constr_type(int row)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.ConstraintOperator), true)]
        public bool is_constr_type(int row, ConstraintOperator mask)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.ConstraintOperator), true)]
        public bool set_constr_type(int row, ConstraintOperator con_type)
            => throw new NotImplementedException();

        #endregion

        #region Enum phase 2 // Use new methods / properties

        [Obsolete("Replaced by " + nameof(PrintSolutionOption) + " property", true)]
        public void set_print_sol(PrintSolutionOption _)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Verbosity) + " property", true)]
        public void set_verbose(Verbosity _)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(FirstBranch) + " property", true)]
        public void set_bb_floorfirst(BranchMode bb_floorfirst)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(BranchAndBoundRuleAndModes) + " property", true)]
        public void set_bb_rule(BranchAndBoundRuleAndModes bb_rule)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(SimplexType) + " = " + nameof(SimplexType) + "." + nameof(SimplexType.DualDual) + " or " + nameof(SimplexType) + " = " + nameof(SimplexType) + "." + nameof(SimplexType.PrimalPrimal), true)]
        public void set_preferdual(bool dodual)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(SimplexType) + " property", true)]
        public void set_simplextype(SimplexType _)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(BasisCrashMode) + " property", true)]
        public void set_basiscrash(BasisCrashMode mode)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(AntiDegeneracyRules) + " property", true)]
        public void set_anti_degen(AntiDegeneracyRules anti_degen)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(PreSolveLevels) + " and " + nameof(PreSolveMaxLoops) + " properties", true)]
        public void set_presolve(PreSolveLevels do_presolve, int maxloops)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(PreSolveMaxLoops) + " property", true)]
        public int get_presolveloops()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(IterativeImprovementLevels) + " property", true)]
        public void set_improve(IterativeImprovementLevels improve)
            => throw new NotImplementedException();

        #endregion

        #region Renames

        [Obsolete("Replaced by " + nameof(Create) + " method", true)]
        public static LpSolve make_lp(int rows, int columns)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(CreateFromLPFile) + " method", true)]
        public static LpSolve read_LP(string fileName, Verbosity verbosity, string lpName)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(CreateFromMPSFile) + " method", true)]
        public static LpSolve read_MPS(string fileName, Verbosity verbosity, MPSOptions options)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(CreateFromXLIFile) + " method", true)]
        public static LpSolve read_XLI(string xliName, string modelName, string dataName, string options, Verbosity verbosity)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Clone) + " method", true)]
        public LpSolve copy_lp()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Dispose) + " method", true)]
        public void delete_lp()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ModelName) + " property", true)]
        public string get_lp_name()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ModelName) + " property", true)]
        public bool set_lp_name(string lpname)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ResizeMatrix) + " method", true)]
        public bool resize_lp(int rows, int columns)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(EntryMode) + " property", true)]
        public bool set_add_rowmode(bool turnon)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(EntryMode) + " property", true)]
        public bool is_add_rowmode()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(InfiniteValue) + " property", true)]
        public double get_infinite()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(InfiniteValue) + " property", true)]
        public void set_infinite(double infinite)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(IsDebug) + " property", true)]
        public bool is_debug()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(IsDebug) + " property", true)]
        public void set_debug(bool debug)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(IsTrace) + " property", true)]
        public bool is_trace()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(IsTrace) + " property", true)]
        public void set_trace(bool trace)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.RightHandSideEpsilon), true)]
        public double get_epsb()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.RightHandSideEpsilon), true)]
        public void set_epsb(double epsb)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.ReducedCostEpsilon), true)]
        public double get_epsd()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.ReducedCostEpsilon), true)]
        public void set_epsd(double epsd)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.DefaultEpsilon), true)]
        public double get_epsel()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.DefaultEpsilon), true)]
        public void set_epsel(double epsel)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.IntegerEpsilon), true)]
        public double get_epsint()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.IntegerEpsilon), true)]
        public void set_epsint(double epsint)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.PerturbationScalarEpsilon), true)]
        public double get_epsperturb()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.PerturbationScalarEpsilon), true)]
        public void set_epsperturb(double epsperturb)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.PivotEpsilon), true)]
        public double get_epspivot()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.PivotEpsilon), true)]
        public void set_epspivot(double epspivot)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.RelativeMipGap) + " and " + nameof(Tolerance) + "." + nameof(ModelTolerance.AbsoluteMipGap), true)]
        public void set_mip_gap(bool absolute, double mip_gap)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.RelativeMipGap) + " and " + nameof(Tolerance) + "." + nameof(ModelTolerance.AbsoluteMipGap), true)]
        public double get_mip_gap(bool absolute)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Tolerance) + "." + nameof(ModelTolerance.SetEpsilonLevel), true)]
        public bool set_epslevel(ToleranceEpsilonLevel level)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Basis) + "." + nameof(ModelBasis.SetDefault), true)]
        public void default_basis()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Basis) + "." + nameof(ModelBasis.ReadFromFile), true)]
        public bool read_basis(string filename, string info)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Basis) + "." + nameof(ModelBasis.WriteToFile), true)]
        public bool write_basis(string filename)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Basis) + "." + nameof(ModelBasis.Set), true)]
        public bool set_basis(int[] bascolumn, bool nonbasic)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Basis) + "." + nameof(ModelBasis.Get), true)]
        public bool get_basis(int[] bascolumn, bool nonbasic)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Basis) + "." + nameof(ModelBasis.HasBasisFactorizationPackage), true)]
        public bool has_BFP()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Basis) + "." + nameof(ModelBasis.IsNativeBasisFactorizationPackage), true)]
        public bool is_nativeBFP()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Basis) + "." + nameof(ModelBasis.SetBasisFactorizationPackage), true)]
        public bool set_BFP(string filename)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Basis) + "." + nameof(ModelBasis.Guess), true)]
        public bool guess_basis(double[] guessvector, int[] basisvector)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(NumberOfColumns), true)]
        public int get_Ncolumns()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(NumberOfColumnsOriginally), true)]
        public int get_Norig_columns()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(NumberOfRowsOriginally), true)]
        public int get_Norig_rows()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(NumberOfRows), true)]
        public int get_Nrows()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ObjectiveFunction) + "." + nameof(ModelObjectiveFunction.SetValue), true)]
        public bool set_obj(int column, double value)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ObjectiveFunction) + "." + nameof(ModelObjectiveFunction.Bound), true)]
        public double get_obj_bound()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ObjectiveFunction) + "." + nameof(ModelObjectiveFunction.Bound), true)]
        public void set_obj_bound(double obj_bound)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ObjectiveFunction) + "." + nameof(ModelObjectiveFunction.SetValues), true)]
        public bool set_obj_fn(double[] row)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ObjectiveFunction) + "." + nameof(ModelObjectiveFunction.SetValues), true)]
        public bool set_obj_fnex(int count, double[] row, int[] colno)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ObjectiveFunction) + "." + nameof(ModelObjectiveFunction.IsMaximizing), true)]
        public bool is_maxim()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ObjectiveFunction) + "." + nameof(ModelObjectiveFunction.IsMaximizing), true)]
        public void set_maxim()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ObjectiveFunction) + "." + nameof(ModelObjectiveFunction.IsMaximizing), true)]
        public void set_minim()
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(ObjectiveFunction) + "." + nameof(ModelObjectiveFunction.IsMaximizing), true)]
        public void set_sense(bool maximize)
            => throw new NotImplementedException();



        [Obsolete("Replaced by " + nameof(Rows) + "." + nameof(ModelRows.GetNameFromOriginalIndex), true)]
        public string get_origrow_name(int row)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "." + nameof(ModelRows.GetOriginalIndexFromName) + " or " + nameof(Columns) + "." + nameof(ModelColumns.GetOriginalIndexFromName), true)]
        public int get_nameindex(string name, bool isrow)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "." + nameof(ModelRows.Add), true)]
        public bool add_constraint(double[] row, ConstraintOperator constraintOperator, double rh)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "." + nameof(ModelRows.Add), true)]
        public bool add_constraintex(int count, double[] row, int[] colno, ConstraintOperator constraintOperator, double rh)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "." + nameof(ModelRows.Delete), true)]
        public bool del_constraint(int del_row)
            => throw new NotImplementedException();



        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.GetValues), true)]
        public bool get_row(int row_nr, double[] row)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.GetNonZeroValues), true)]
        public int get_rowex(int row_nr, double[] row, int[] colno)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.SetValues), true)]
        public bool set_row(int row_no, double[] row)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.SetValues), true)]
        public bool set_rowex(int row_no, int count, double[] row, int[] colno)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.Name), true)]
        public bool set_row_name(int row, string new_name)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.Name), true)]
        public string get_row_name(int row)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.RightHandSide), true)]
        public double get_rh(int row)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.RightHandSide), true)]
        public bool set_rh(int row, double value)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.Range), true)]
        public double get_rh_range(int row)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Rows) + "[row]." + nameof(ModelRow.Range), true)]
        public bool set_rh_range(int row, double deltavalue)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Columns) + "." + nameof(ModelColumns.Add), true)]
        public bool add_column(double[] column)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Columns) + "." + nameof(ModelColumns.Add), true)]
        public bool add_columnex(int count, double[] column, int[] rowno)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Columns) + "." + nameof(ModelColumns.Delete), true)]
        public bool del_column(int column)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Columns) + "." + nameof(ModelColumns.GetNameFromOriginalIndex), true)]
        public string get_origcol_name(int column)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.SetValues), true)]
        public bool set_column(int col_no, double[] column)
            => NativeMethods.set_column(_lp, col_no, column);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.SetValues), true)]
        public bool set_columnex(int col_no, int count, double[] column, int[] rowno)
            => NativeMethods.set_columnex(_lp, col_no, count, column, rowno);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.GetValues), true)]
        public bool get_column(int col_nr, double[] column)
            => NativeMethods.get_column(_lp, col_nr, column);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.GetNonZeroValues), true)]
        public int get_columnex(int col_nr, double[] column, int[] nzrow)
            => NativeMethods.get_columnex(_lp, col_nr, column, nzrow);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.Name), true)]
        public bool set_col_name(int column, string new_name)
            => NativeMethods.set_col_name(_lp, column, new_name);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.Name), true)]
        public string get_col_name(int column)
            => NativeMethods.get_col_name(_lp, column);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.IsNegative), true)]
        public bool is_negative(int column)
            => NativeMethods.is_negative(_lp, column);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.IsInteger), true)]
        public bool is_int(int column)
            => NativeMethods.is_int(_lp, column);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.IsInteger), true)]
        public bool set_int(int column, bool must_be_int)
            => NativeMethods.set_int(_lp, column, must_be_int);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.IsBinary), true)]
        public bool is_binary(int column)
            => NativeMethods.is_binary(_lp, column);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.IsBinary), true)]
        public bool set_binary(int column, bool must_be_bin)
            => NativeMethods.set_binary(_lp, column, must_be_bin);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.IsSemiContinuous), true)]
        public bool is_semicont(int column)
            => NativeMethods.is_semicont(_lp, column);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.IsSemiContinuous), true)]
        public bool set_semicont(int column, bool must_be_sc)
            => NativeMethods.set_semicont(_lp, column, must_be_sc);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.SetBounds), true)]
        public bool set_bounds(int column, double lower, double upper)
            => NativeMethods.set_bounds(_lp, column, lower, upper);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.IsUnbounded), true)]
        public bool set_unbounded(int column)
            => NativeMethods.set_unbounded(_lp, column);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.IsUnbounded), true)]
        public bool is_unbounded(int column)
            => NativeMethods.is_unbounded(_lp, column);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.UpperBound), true)]
        public double get_upbo(int column)
            => NativeMethods.get_upbo(_lp, column);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.UpperBound), true)]
        public bool set_upbo(int column, double value)
            => NativeMethods.set_upbo(_lp, column, value);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.LowerBound), true)]
        public double get_lowbo(int column)
            => NativeMethods.get_lowbo(_lp, column);

        [Obsolete("Replaced by " + nameof(Columns) + "[row]." + nameof(ModelColumn.LowerBound), true)]
        public bool set_lowbo(int column, double value)
            => NativeMethods.set_lowbo(_lp, column, value);
        #endregion

        #region Support removed

        [Obsolete("Support for string version of model building was removed, use " + nameof(Rows) + "." + nameof(ModelRows.Add) + " instead", true)]
        public bool str_add_constraint(params object[] _)
            => throw new NotImplementedException();

        [Obsolete("Support for string version of model building was removed, use " + nameof(Columns) + "." + nameof(ModelColumns.Add) + " instead", true)]
        public bool str_add_column(params object[] _)
            => throw new NotImplementedException();

        [Obsolete("Support for string version of model building was removed, use " + nameof(ObjectiveFunction) + "." + nameof(ModelObjectiveFunction.SetValues) + " instead", true)]
        public bool str_set_obj_fn(params object[] _)
            => throw new NotImplementedException();

        [Obsolete("Support for string version of model building was removed, use " + nameof(set_rh_vec) + " instead", true)]
        public bool str_set_rh_vec(params object[] _)
            => throw new NotImplementedException();

        [Obsolete("Removed as this was not meant to be used http://lpsolve.sourceforge.net/5.5/reset_basis.htm", true)]
        public void reset_basis()
            => throw new NotImplementedException();
        #endregion

    }
    [Obsolete("Replaced by " + nameof(AbortHandler), false)]
    public delegate bool ctrlcfunctemp(IntPtr lp, IntPtr userhandle);

    [Obsolete("Replaced by " + nameof(MessageHandler), false)]
    public delegate void msgfunctemp(IntPtr lp, IntPtr userhandle, lpsolve_msgmask message);

    [Obsolete("Replaced by " + nameof(LogHandler), false)]
    public delegate void logfunctemp(IntPtr lp, IntPtr userhandle, string buf);

    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(ToleranceEpsilonLevel), false)]
    public enum lpsolve_epsilon_level
    {
        [Obsolete("Replace by " + nameof(Tight), true)]
        EPS_TIGHT = 0,
        [Obsolete("Replace by " + nameof(Medium), true)]
        EPS_MEDIUM = 1,
        [Obsolete("Replace by " + nameof(Loose), true)]
        EPS_LOOSE = 2,
        [Obsolete("Replace by " + nameof(Baggy), true)]
        EPS_BAGGY = 3,
        [Obsolete("Replace by " + nameof(Default), true)]
        EPS_DEFAULT = EPS_TIGHT,

        Tight = 0,
        Medium = 1,
        Loose = 2,
        Baggy = 3,
        Default = Tight,
    }

    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(ConstraintOperator), false)]
    public enum lpsolve_constr_types
    {
        [Obsolete("Replace by " + nameof(Free), true)]
        FR = 0,
        [Obsolete("Replace by " + nameof(LessOrEqual), true)]
        LE = 1,
        [Obsolete("Replace by " + nameof(GreaterOrEqual), true)]
        GE = 2,
        [Obsolete("Replace by " + nameof(Equal), true)]
        EQ = 3,
        [Obsolete("Replace by " + nameof(ObjectiveFunction), true)]
        OF = 4,

        Free = 0,
        LessOrEqual = 1,
        GreaterOrEqual = 2,
        Equal = 3,
        ObjectiveFunction = 4,
    }

    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(ScaleAlgorithm), false)]
    public enum lpsolve_scale_algorithm
    {
        [Obsolete("Replace by " + nameof(None), true)]
        SCALE_NONE = 0,
        [Obsolete("Replace by " + nameof(Extreme), true)]
        SCALE_EXTREME = 1,
        [Obsolete("Replace by " + nameof(Range), true)]
        SCALE_RANGE = 2,
        [Obsolete("Replace by " + nameof(Mean), true)]
        SCALE_MEAN = 3,
        [Obsolete("Replace by " + nameof(Geometric), true)]
        SCALE_GEOMETRIC = 4,
        [Obsolete("Replace by " + nameof(CurtisReid), true)]
        SCALE_CURTISREID = 7,

        None = 0,
        Extreme = 1,
        Range = 2,
        Mean = 3,
        Geometric = 4,
        CurtisReid = 7,
    }

    [Flags]
    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(ScaleParameters), false)]
    public enum lpsolve_scale_parameters
    {
        [Obsolete("Replace by " + nameof(None), true)]
        SCALE_NONE = 0,
        [Obsolete("Replace by " + nameof(Quadratic), true)]
        SCALE_QUADRATIC = 8,
        [Obsolete("Replace by " + nameof(Logarithmic), true)]
        SCALE_LOGARITHMIC = 16,
        [Obsolete("Replace by " + nameof(UserWeight), true)]
        SCALE_USERWEIGHT = 31,
        [Obsolete("Replace by " + nameof(Power2), true)]
        SCALE_POWER2 = 32,
        [Obsolete("Replace by " + nameof(Equilibrate), true)]
        SCALE_EQUILIBRATE = 64,
        [Obsolete("Replace by " + nameof(Integers), true)]
        SCALE_INTEGERS = 128,
        [Obsolete("Replace by " + nameof(DynanicUpdate), true)]
        SCALE_DYNUPDATE = 256,
        [Obsolete("Replace by " + nameof(RowsOnly), true)]
        SCALE_ROWSONLY = 512,
        [Obsolete("Replace by " + nameof(ColumnsOnly), true)]
        SCALE_COLSONLY = 1024,

        None = 0,
        Quadratic = 8,
        Logarithmic = 16,
        UserWeight = 31,
        Power2 = 32,
        Equilibrate = 64,
        Integers = 128,
        DynanicUpdate = 256,
        RowsOnly = 512,
        ColumnsOnly = 1024,
    }

    [Flags]
    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(IterativeImprovementLevels), false)]
    public enum lpsolve_improves
    {
        [Obsolete("Replace by " + nameof(None), true)]
        IMPROVE_NONE = 0,
        [Obsolete("Replace by " + nameof(Solution), true)]
        IMPROVE_SOLUTION = 1,
        [Obsolete("Replace by " + nameof(DualFeasibility), true)]
        IMPROVE_DUALFEAS = 2,
        [Obsolete("Replace by " + nameof(ThetaGap), true)]
        IMPROVE_THETAGAP = 4,
        [Obsolete("Replace by " + nameof(BBSimplex), true)]
        IMPROVE_BBSIMPLEX = 8,
        [Obsolete("Replace by " + nameof(Default), true)]
        IMPROVE_DEFAULT = (IMPROVE_DUALFEAS + IMPROVE_THETAGAP),
        [Obsolete("Replace by " + nameof(Inverse), true)]
        IMPROVE_INVERSE = (IMPROVE_SOLUTION + IMPROVE_THETAGAP),

        None = 0,
        Solution = 1,
        DualFeasibility = 2,
        ThetaGap = 4,
        BBSimplex = 8,
        Default = (DualFeasibility + ThetaGap),
        Inverse = (Solution + ThetaGap)
    }

    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(PivotRule), false)]
    public enum lpsolve_pivot_rule
    {
        [Obsolete("Replace by " + nameof(FirstIndex), true)]
        PRICER_FIRSTINDEX = 0,
        [Obsolete("Replace by " + nameof(Dantzig), true)]
        PRICER_DANTZIG = 1,
        [Obsolete("Replace by " + nameof(Devex), true)]
        PRICER_DEVEX = 2,
        [Obsolete("Replace by " + nameof(SteepestEdge), true)]
        PRICER_STEEPESTEDGE = 3,

        FirstIndex = 0,
        Dantzig = 1,
        Devex = 2,
        SteepestEdge = 3,
    }

    [Flags]
    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(PivotModes), false)]
    public enum lpsolve_pivot_modes
    {
        [Obsolete("Replace by " + nameof(PrimalFallback), true)]
        PRICE_PRIMALFALLBACK = 4,
        [Obsolete("Replace by " + nameof(Multiple), true)]
        PRICE_MULTIPLE = 8,
        [Obsolete("Replace by " + nameof(Partial), true)]
        PRICE_PARTIAL = 16,
        [Obsolete("Replace by " + nameof(Adaptive), true)]
        PRICE_ADAPTIVE = 32,
        [Obsolete("Replace by " + nameof(Randomize), true)]
        PRICE_RANDOMIZE = 128,
        [Obsolete("Replace by " + nameof(AutoPartial), true)]
        PRICE_AUTOPARTIAL = 256,
        [Obsolete("Replace by " + nameof(AutoMultiple), true)]
        PRICE_AUTOMULTIPLE = 512,
        [Obsolete("Replace by " + nameof(LoopLeft), true)]
        PRICE_LOOPLEFT = 1024,
        [Obsolete("Replace by " + nameof(LoopAlternate), true)]
        PRICE_LOOPALTERNATE = 2048,
        [Obsolete("Replace by " + nameof(HarrisTwoPass), true)]
        PRICE_HARRISTWOPASS = 4096,
        [Obsolete("Replace by " + nameof(TrueNormInit), true)]
        PRICE_TRUENORMINIT = 16384,
        [Obsolete("Replace by " + nameof(NoBoundFlip), true)]
        PRICE_NOBOUNDFLIP = 65536,

        PrimalFallback = 4,
        Multiple = 8,
        Partial = 16,
        Adaptive = 32,
        Randomize = 128,
        AutoPartial = 256,
        AutoMultiple = 512,
        LoopLeft = 1024,
        LoopAlternate = 2048,
        HarrisTwoPass = 4096,
        TrueNormInit = 16384,
        NoBoundFlip = 65536,
    }

    [Flags]
    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(PreSolveLevels), false)]
    public enum lpsolve_presolve
    {
        [Obsolete("Replace by " + nameof(None), true)]
        PRESOLVE_NONE = 0,
        [Obsolete("Replace by " + nameof(Rows), true)]
        PRESOLVE_ROWS = 1,
        [Obsolete("Replace by " + nameof(Columns), true)]
        PRESOLVE_COLS = 2,
        [Obsolete("Replace by " + nameof(LinearlyDependentRows), true)]
        PRESOLVE_LINDEP = 4,
        [Obsolete("Replace by " + nameof(SOS), true)]
        PRESOLVE_SOS = 32,
        [Obsolete("Replace by " + nameof(ReduceMIP), true)]
        PRESOLVE_REDUCEMIP = 64,
        [Obsolete("Replace by " + nameof(Knapsack), true)]
        PRESOLVE_KNAPSACK = 128,
        [Obsolete("Replace by " + nameof(ElimEQ2), true)]
        PRESOLVE_ELIMEQ2 = 256,
        [Obsolete("Replace by " + nameof(ImpliedFreeVariables), true)]
        PRESOLVE_IMPLIEDFREE = 512,
        [Obsolete("Replace by " + nameof(ReduceGCD), true)]
        PRESOLVE_REDUCEGCD = 1024,
        [Obsolete("Replace by " + nameof(ProbeFix), true)]
        PRESOLVE_PROBEFIX = 2048,
        [Obsolete("Replace by " + nameof(ProbeReduce), true)]
        PRESOLVE_PROBEREDUCE = 4096,
        [Obsolete("Replace by " + nameof(RowDominate), true)]
        PRESOLVE_ROWDOMINATE = 8192,
        [Obsolete("Replace by " + nameof(ColumnDominate), true)]
        PRESOLVE_COLDOMINATE = 16384,
        [Obsolete("Replace by " + nameof(MergeRows), true)]
        PRESOLVE_MERGEROWS = 32768,
        [Obsolete("Replace by " + nameof(ImpliedSlack), true)]
        PRESOLVE_IMPLIEDSLK = 65536,
        [Obsolete("Replace by " + nameof(ColumnFixDual), true)]
        PRESOLVE_COLFIXDUAL = 131072,
        [Obsolete("Replace by " + nameof(BoundsTightening), true)]
        PRESOLVE_BOUNDS = 262144,
        [Obsolete("Replace by " + nameof(CalculateDuals), true)]
        PRESOLVE_DUALS = 524288,
        [Obsolete("Replace by " + nameof(CalculateSensitivityDuals), true)]
        PRESOLVE_SENSDUALS = 1048576,

        None = 0,
        Rows = 1,
        Columns = 2,
        LinearlyDependentRows = 4,
        SOS = 32,
        ReduceMIP = 64,
        Knapsack = 128,
        ElimEQ2 = 256,
        ImpliedFreeVariables = 512,
        ReduceGCD = 1024,
        ProbeFix = 2048,
        ProbeReduce = 4096,
        RowDominate = 8192,
        ColumnDominate = 16384,
        MergeRows = 32768,
        ImpliedSlack = 65536,
        ColumnFixDual = 131072,
        BoundsTightening = 262144,
        CalculateDuals = 524288,
        CalculateSensitivityDuals = 1048576
    }

    [Flags]
    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(AntiDegeneracyRules), false)]
    public enum lpsolve_anti_degen
    {
        [Obsolete("Replace by " + nameof(None), true)]
        ANTIDEGEN_NONE = 0,
        [Obsolete("Replace by " + nameof(FixedVariables), true)]
        ANTIDEGEN_FIXEDVARS = 1,
        [Obsolete("Replace by " + nameof(ColumnCheck), true)]
        ANTIDEGEN_COLUMNCHECK = 2,
        [Obsolete("Replace by " + nameof(Stalling), true)]
        ANTIDEGEN_STALLING = 4,
        [Obsolete("Replace by " + nameof(NumericalFailure), true)]
        ANTIDEGEN_NUMFAILURE = 8,
        [Obsolete("Replace by " + nameof(LostDualFeasibility), true)]
        ANTIDEGEN_LOSTFEAS = 16,
        [Obsolete("Replace by " + nameof(Infeasible), true)]
        ANTIDEGEN_INFEASIBLE = 32,
        [Obsolete("Replace by " + nameof(Dynamic), true)]
        ANTIDEGEN_DYNAMIC = 64,
        [Obsolete("Replace by " + nameof(DuringBranchAndBound), true)]
        ANTIDEGEN_DURINGBB = 128,
        [Obsolete("Replace by " + nameof(RHSPerturbation), true)]
        ANTIDEGEN_RHSPERTURB = 256,
        [Obsolete("Replace by " + nameof(BoundFlips), true)]
        ANTIDEGEN_BOUNDFLIP = 512,

        None = 0,
        FixedVariables = 1,
        ColumnCheck = 2,
        Stalling = 4,
        NumericalFailure = 8,
        LostDualFeasibility = 16,
        Infeasible = 32,
        Dynamic = 64,
        DuringBranchAndBound = 128,
        RHSPerturbation = 256,
        BoundFlips = 512
    }

    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(BasisCrashMode), false)]
    public enum lpsolve_basiscrash
    {
        [Obsolete("Replace by " + nameof(None), true)]
        CRASH_NONE = 0,
        [Obsolete("Replace by " + nameof(None), true)]
        CRASH_NOTHING = 0,
        [Obsolete("Replace by " + nameof(MostFeasible), true)]
        CRASH_MOSTFEASIBLE = 2,
        [Obsolete("Replace by " + nameof(LeastDegenerate), true)]
        CRASH_LEASTDEGENERATE = 3,

        None = 0,
        MostFeasible = 2,
        LeastDegenerate = 3,
    }

    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(SimplexType), false)]
    public enum lpsolve_simplextypes
    {
        [Obsolete("Replace by " + nameof(PrimalPrimal), true)]
        SIMPLEX_PRIMAL_PRIMAL = 5,
        [Obsolete("Replace by " + nameof(DualPrimal), true)]
        SIMPLEX_DUAL_PRIMAL = 6,
        [Obsolete("Replace by " + nameof(PrimalDual), true)]
        SIMPLEX_PRIMAL_DUAL = 9,
        [Obsolete("Replace by " + nameof(DualDual), true)]
        SIMPLEX_DUAL_DUAL = 10,

        PrimalPrimal = 5,
        DualPrimal = 6,
        PrimalDual = 9,
        DualDual = 10,
    }

    [Flags]
    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(BranchAndBoundRule) + " or " + nameof(BranchAndBoundModes)
        + " and combine into a " + nameof(BranchAndBoundRuleAndModes), false)]
    public enum lpsolve_BBstrategies
    {
        [Obsolete("Replace by " + nameof(FirstSelect), true)]
        NODE_FIRSTSELECT = 0,
        [Obsolete("Replace by " + nameof(GapSelect), true)]
        NODE_GAPSELECT = 1,
        [Obsolete("Replace by " + nameof(RangeSelect), true)]
        NODE_RANGESELECT = 2,
        [Obsolete("Replace by " + nameof(FractionSelect), true)]
        NODE_FRACTIONSELECT = 3,
        [Obsolete("Replace by " + nameof(PseudoCostSelect), true)]
        NODE_PSEUDOCOSTSELECT = 4,
        [Obsolete("Replace by " + nameof(PseudoNonIntegerSelect), true)]
        NODE_PSEUDONONINTSELECT = 5,
        [Obsolete("Replace by " + nameof(PseudoRatioSelect), true)]
        NODE_PSEUDORATIOSELECT = 6,
        [Obsolete("Replace by " + nameof(UserSelect), true)]
        NODE_USERSELECT = 7,
        [Obsolete("Replace by " + nameof(WeightReverseMode), true)]
        NODE_WEIGHTREVERSEMODE = 8,
        [Obsolete("Replace by " + nameof(BranchReverseMode), true)]
        NODE_BRANCHREVERSEMODE = 16,
        [Obsolete("Replace by " + nameof(GreedyMode), true)]
        NODE_GREEDYMODE = 32,
        [Obsolete("Replace by " + nameof(PseudoCostMode), true)]
        NODE_PSEUDOCOSTMODE = 64,
        [Obsolete("Replace by " + nameof(DepthFirstMode), true)]
        NODE_DEPTHFIRSTMODE = 128,
        [Obsolete("Replace by " + nameof(RandomizeMode), true)]
        NODE_RANDOMIZEMODE = 256,
        [Obsolete("Replace by " + nameof(GUBMode), true)]
        NODE_GUBMODE = 512,
        [Obsolete("Replace by " + nameof(DynamicMode), true)]
        NODE_DYNAMICMODE = 1024,
        [Obsolete("Replace by " + nameof(RestartMode), true)]
        NODE_RESTARTMODE = 2048,
        [Obsolete("Replace by " + nameof(BreadthFirstMode), true)]
        NODE_BREADTHFIRSTMODE = 4096,
        [Obsolete("Replace by " + nameof(AutoOrder), true)]
        NODE_AUTOORDER = 8192,
        [Obsolete("Replace by " + nameof(RCostFixing), true)]
        NODE_RCOSTFIXING = 16384,
        [Obsolete("Replace by " + nameof(StrongInit), true)]
        NODE_STRONGINIT = 32768,
        //
        FirstSelect = 0,
        GapSelect = 1,
        RangeSelect = 2,
        FractionSelect = 3,
        PseudoCostSelect = 4,
        PseudoNonIntegerSelect = 5,
        PseudoRatioSelect = 6,
        UserSelect = 7,
        WeightReverseMode = 8,
        BranchReverseMode = 16,
        GreedyMode = 32,
        PseudoCostMode = 64,
        DepthFirstMode = 128,
        RandomizeMode = 256,
        GUBMode = 512,
        DynamicMode = 1024,
        RestartMode = 2048,
        BreadthFirstMode = 4096,
        AutoOrder = 8192,
        RCostFixing = 16384,
        StrongInit = 32768
    }

    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(SolveResult), false)]
    public enum lpsolve_return
    {
        [Obsolete("Replace by " + nameof(UnknownError), true)]
        UNKNOWNERROR = -5,
        [Obsolete("Replace by " + nameof(DataIgnored), true)]
        DATAIGNORED = -4,
        [Obsolete("Replace by " + nameof(NoBFP), true)]
        NOBFP = -3,
        [Obsolete("Replace by " + nameof(NoMemory), true)]
        NOMEMORY = -2,
        [Obsolete("Replace by " + nameof(NotRun), true)]
        NOTRUN = -1,
        [Obsolete("Replace by " + nameof(Optimal), true)]
        OPTIMAL = 0,
        [Obsolete("Replace by " + nameof(SubOptimal), true)]
        SUBOPTIMAL = 1,
        [Obsolete("Replace by " + nameof(Infeasible), true)]
        INFEASIBLE = 2,
        [Obsolete("Replace by " + nameof(Unbounded), true)]
        UNBOUNDED = 3,
        [Obsolete("Replace by " + nameof(Degenerate), true)]
        DEGENERATE = 4,
        [Obsolete("Replace by " + nameof(NumericalFailure), true)]
        NUMFAILURE = 5,
        [Obsolete("Replace by " + nameof(UserAborted), true)]
        USERABORT = 6,
        [Obsolete("Replace by " + nameof(TimedOut), true)]
        TIMEOUT = 7,
        [Obsolete("Replace by " + nameof(PreSolved), true)]
        PRESOLVED = 9,
        [Obsolete("Replace by " + nameof(AccuracyError), true)]
        ACCURACYERROR = 25,


        UnknownError = -5,
        DataIgnored = -4,
        NoBFP = -3,
        NoMemory = -2,
        NotRun = -1,
        Optimal = 0,
        SubOptimal = 1,
        Infeasible = 2,
        Unbounded = 3,
        Degenerate = 4,
        NumericalFailure = 5,
        UserAborted = 6,
        TimedOut = 7,
        PreSolved = 9,
        AccuracyError = 25,
    }

    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(BranchMode), false)]
    public enum lpsolve_branch
    {
        [Obsolete("Replace by " + nameof(Ceiling), true)]
        BRANCH_CEILING = 0,
        [Obsolete("Replace by " + nameof(Floor), true)]
        BRANCH_FLOOR = 1,
        [Obsolete("Replace by " + nameof(Automatic), true)]
        BRANCH_AUTOMATIC = 2,
        [Obsolete("Replace by " + nameof(Default), true)]
        BRANCH_DEFAULT = 3,
        Ceiling = 0,
        Floor = 1,
        Automatic = 2,
        Default = 3,
    }

    [Flags]
    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(MessageMasks), false)]
    public enum lpsolve_msgmask
    {
        [Obsolete("Replace by " + nameof(None), true)]
        MSG_NONE = 0,
        [Obsolete("Replace by " + nameof(PreSolve), true)]
        MSG_PRESOLVE = 1,
        [Obsolete("Replace by " + nameof(LPFeasible), true)]
        MSG_LPFEASIBLE = 8,
        [Obsolete("Replace by " + nameof(LPOptimal), true)]
        MSG_LPOPTIMAL = 16,
        [Obsolete("Replace by " + nameof(MILPFeasible), true)]
        MSG_MILPFEASIBLE = 128,
        [Obsolete("Replace by " + nameof(MILPEqual), true)]
        MSG_MILPEQUAL = 256,
        [Obsolete("Replace by " + nameof(MILPBetter), true)]
        MSG_MILPBETTER = 512,
        //new values
        None = 0,
        PreSolve = 1,
        LPFeasible = 8,
        LPOptimal = 16,
        MILPFeasible = 128,
        MILPEqual = 256,
        MILPBetter = 512,
    }

    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(Verbosity), false)]
    public enum lpsolve_verbosity
    {
        [Obsolete("Replace by " + nameof(Neutral), true)]
        NEUTRAL = 0,
        [Obsolete("Replace by " + nameof(Critical), true)]
        CRITICAL = 1,
        [Obsolete("Replace by " + nameof(Severe), true)]
        SEVERE = 2,
        [Obsolete("Replace by " + nameof(Important), true)]
        IMPORTANT = 3,
        [Obsolete("Replace by " + nameof(Normal), true)]
        NORMAL = 4,
        [Obsolete("Replace by " + nameof(Detailed), true)]
        DETAILED = 5,
        [Obsolete("Replace by " + nameof(Full), true)]
        FULL = 6,

        Neutral = 0,
        Critical = 1,
        Severe = 2,
        Important = 3,
        Normal = 4,
        Detailed = 5,
        Full = 6,
    }

    [Flags]
    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(MPSOptions), false)]
    public enum lpsolve_mps_options
    {
        [Obsolete("Replace by " + nameof(FixedMPSFormat), true)]
        MPS_FIXED = 0,
        [Obsolete("Replace by " + nameof(FreeMPSFormat), true)]
        MPS_FREE = 8,
        [Obsolete("Replace by " + nameof(IBM), true)]
        MPS_IBM = 16,
        [Obsolete("Replace by " + nameof(NegateObjectiveConstant), true)]
        MPS_NEGOBJCONST = 32,

        FixedMPSFormat = 0,
        FreeMPSFormat = 8,
        IBM = 16,
        NegateObjectiveConstant = 32,
    }

    [Obsolete("When all Obsolete warnings are fixed, change this enum to " + nameof(PrintSolutionOption), false)]
    public enum lpsolve_print_sol_option
    {
        [Obsolete("Replace by " + nameof(False), true)]
        FALSE = 0,
        [Obsolete("Replace by " + nameof(True), true)]
        TRUE = 1,
        [Obsolete("Replace by " + nameof(Automatic), true)]
        AUTOMATIC = 2,
        False = 0,
        True = 1,
        Automatic = 2,
    }
}

#pragma warning restore 1591  // documentation
#pragma warning restore 618   // Obsolete warning
#pragma warning restore IDE1006 // Naming rule violations
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore CA1717 // Only FlagsAttribute enums should have plural names
#pragma warning restore CA1714 // Flags enums should have plural names
#pragma warning restore CA1801 //  Parameter fileName of method read_MPS is never used
#pragma warning restore CA1822 // Mark members as static
