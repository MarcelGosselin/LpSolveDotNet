using System;
using System.IO;
using System.Reflection;

namespace LpSolveDotNet
{
    public sealed class LpSolve
        : IDisposable
    {
        #region Library initialization

        public static bool Init()
        {
            return Init(null);
        }

        public static bool Init(string dllFolderPath)
        {
            if (string.IsNullOrEmpty(dllFolderPath))
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                bool is64Bit = IntPtr.Size == 8;
                dllFolderPath = Path.GetDirectoryName(exePath) + (is64Bit ? @"\NativeBinaries\win64\" : @"\NativeBinaries\win32\");
            }
            var dllFilePath = dllFolderPath;
            if (dllFilePath.Substring(dllFilePath.Length - 1, 1) != "\\")
            {
                dllFilePath += "\\";
            }
            dllFilePath += "lpsolve55.dll";

            bool returnValue = File.Exists(dllFilePath);
            if (returnValue)
            {
                if (!_hasAlreadyChangedPathEnvironmentVariable)
                {
                    string pathEnvironmentVariable = Environment.GetEnvironmentVariable("PATH");
                    string pathWithSemiColon = pathEnvironmentVariable + ";";
                    if (pathWithSemiColon.IndexOf(dllFolderPath + ";", StringComparison.InvariantCultureIgnoreCase) < 0)
                    {
                        Environment.SetEnvironmentVariable("PATH", dllFolderPath + ";" + pathEnvironmentVariable, EnvironmentVariableTarget.Process);
                    }
                    _hasAlreadyChangedPathEnvironmentVariable = true;
                }
            }
            return returnValue;
        }
        private static bool _hasAlreadyChangedPathEnvironmentVariable;

        #endregion

        #region Fields

        private IntPtr _lp;

        #endregion

        #region Create/destroy model

        /// <summary>
        /// Constructor, to be called from <see cref="CreateFromLpRecStructurePointer"/> only.
        /// </summary>
        private LpSolve(IntPtr lp)
        {
            if (lp == IntPtr.Zero)
            {
                throw new ArgumentException("'lp' must be a valid pointer.", "lp");
            }
            _lp = lp;
        }

        private static LpSolve CreateFromLpRecStructurePointer(IntPtr lp)
        {
            if (lp == IntPtr.Zero)
            {
                return null;
            }
            return new LpSolve(lp);
        }

        public static LpSolve make_lp(int rows, int columns)
        {
            IntPtr lp = Interop.make_lp(rows, columns);
            return CreateFromLpRecStructurePointer(lp);
        }

        public static LpSolve read_LP(string fileName, int verbose, string lpName)
        {
            IntPtr lp = Interop.read_LP(fileName, verbose, lpName);
            return CreateFromLpRecStructurePointer(lp);
        }

        public static LpSolve read_freeMPS(string fileName, int options)
        {
            IntPtr lp = Interop.read_freeMPS(fileName, options);
            return CreateFromLpRecStructurePointer(lp);
        }

        public static LpSolve read_MPS(string fileName, int options)
        {
            IntPtr lp = Interop.read_MPS(fileName, options);
            return CreateFromLpRecStructurePointer(lp);
        }

        public static LpSolve read_XLI(string xliName, string modelName, string dataName, string options, int verbose)
        {
            IntPtr lp = Interop.read_XLI(xliName, modelName, dataName, options, verbose);
            return CreateFromLpRecStructurePointer(lp);
        }

        public LpSolve copy_lp()
        {
            IntPtr lp = Interop.copy_lp(_lp);
            return CreateFromLpRecStructurePointer(lp);
        }

        public void delete_lp()
        {
            // implement Dispose pattern according to https://msdn.microsoft.com/en-us/library/b1yfkh5e.aspx
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            // According to https://msdn.microsoft.com/en-us/library/b1yfkh5e.aspx
            //      CONSIDER providing method Close(), in addition to the Dispose(), if close is standard terminology in the area.
            //      When doing so, it is important that you make the Close implementation identical to Dispose and consider implementing the IDisposable.Dispose method explicitly
            delete_lp();
        }

        private void Dispose(bool disposing)
        {
            // release unmanaged memory
            if (_lp != IntPtr.Zero)
            {
                Interop.delete_lp(_lp);
                _lp = IntPtr.Zero;
            }

            // release other disposable objects
            if (disposing)
            {
            }
        }

        ~LpSolve()
        {
            Dispose(false);
        }

        #endregion

        #region Build model

        #region Column

        // http://lpsolve.sourceforge.net/5.5/add_column.htm

        public bool add_column(double[] column)
        {
            return Interop.add_column(_lp, column);
        }

        public bool add_columnex(int count, double[] column, int[] rowno)
        {
            return Interop.add_columnex(_lp, count, column, rowno);
        }

        public bool str_add_column(string col_string)
        {
            return Interop.str_add_column(_lp, col_string);
        }

        // http://lpsolve.sourceforge.net/5.5/del_column.htm

        public bool del_column(int column)
        {
            return Interop.del_column(_lp, column);
        }

        // http://lpsolve.sourceforge.net/5.5/set_column.htm

        public bool set_column(int col_no, double[] column)
        {
            return Interop.set_column(_lp, col_no, column);
        }

        public bool set_columnex(int col_no, int count, double[] column, int[] rowno)
        {
            return Interop.set_columnex(_lp, col_no, count, column, rowno);
        }

        // http://lpsolve.sourceforge.net/5.5/get_column.htm

        public bool get_column(int col_nr, double[] column)
        {
            return Interop.get_column(_lp, col_nr, column);
        }

        public int get_columnex(int col_nr, double[] column, int[] nzrow)
        {
            return Interop.get_columnex(_lp, col_nr, column, nzrow);
        }

        // http://lpsolve.sourceforge.net/5.5/set_col_name.htm

        public bool set_col_name(int column, string new_name)
        {
            return Interop.set_col_name(_lp, column, new_name);
        }

        // http://lpsolve.sourceforge.net/5.5/get_col_name.htm

        public string get_col_name(int column)
        {
            return Interop.get_col_name(_lp, column);
        }

        public string get_origcol_name(int column)
        {
            return Interop.get_origcol_name(_lp, column);
        }

        public bool is_negative(int column)
        {
            return Interop.is_negative(_lp, column);
        }

        // http://lpsolve.sourceforge.net/5.5/is_add_rowmode.htm

        public bool is_int(int column)
        {
            return Interop.is_int(_lp, column);
        }

        public bool set_int(int column, bool must_be_int)
        {
            return Interop.set_int(_lp, column, must_be_int);
        }

        public bool is_binary(int column)
        {
            return Interop.is_binary(_lp, column);
        }

        public bool set_binary(int column, bool must_be_bin)
        {
            return Interop.set_binary(_lp, column, must_be_bin);
        }


        public bool is_semicont(int column)
        {
            return Interop.is_semicont(_lp, column);
        }

        public bool set_semicont(int column, bool must_be_sc)
        {
            return Interop.set_semicont(_lp, column, must_be_sc);
        }

        public bool set_bounds(int column, double lower, double upper)
        {
            return Interop.set_bounds(_lp, column, lower, upper);
        }

        public bool set_unbounded(int column)
        {
            return Interop.set_unbounded(_lp, column);
        }

        public bool is_unbounded(int column)
        {
            return Interop.is_unbounded(_lp, column);
        }

        public double get_upbo(int column)
        {
            return Interop.get_upbo(_lp, column);
        }

        public bool set_upbo(int column, double value)
        {
            return Interop.set_upbo(_lp, column, value);
        }

        public double get_lowbo(int column)
        {
            return Interop.get_lowbo(_lp, column);
        }

        public bool set_lowbo(int column, double value)
        {
            return Interop.set_lowbo(_lp, column, value);
        }

        #endregion // Build model /  Column

        #region Constraint / Row

        public bool add_constraint(double[] row, lpsolve_constr_types constr_type, double rh)
        {
            return Interop.add_constraint(_lp, row, constr_type, rh);
        }

        public bool add_constraintex(int count, double[] row, int[] colno, lpsolve_constr_types constr_type, double rh)
        {
            return Interop.add_constraintex(_lp, count, row, colno, constr_type, rh);
        }

        public bool str_add_constraint(string row_string, lpsolve_constr_types constr_type, double rh)
        {
            return Interop.str_add_constraint(_lp, row_string, constr_type, rh);
        }

        public bool del_constraint(int del_row)
        {
            return Interop.del_constraint(_lp, del_row);
        }

        public bool get_row(int row_nr, double[] row)
        {
            return Interop.get_row(_lp, row_nr, row);
        }

        public int get_rowex(int row_nr, double[] row, int[] colno)
        {
            return Interop.get_rowex(_lp, row_nr, row, colno);
        }

        // http://lpsolve.sourceforge.net/5.5/set_row.htm

        public bool set_row(int row_no, double[] row)
        {
            return Interop.set_row(_lp, row_no, row);
        }

        public bool set_rowex(int row_no, int count, double[] row, int[] colno)
        {
            return Interop.set_rowex(_lp, row_no, count, row, colno);
        }

        // http://lpsolve.sourceforge.net/5.5/set_row_name.htm

        public bool set_row_name(int row, string new_name)
        {
            return Interop.set_row_name(_lp, row, new_name);
        }
        // http://lpsolve.sourceforge.net/5.5/get_row_name.htm

        public string get_row_name(int row)
        {
            return Interop.get_row_name(_lp, row);
        }

        public string get_origrow_name(int row)
        {
            return Interop.get_origrow_name(_lp, row);
        }

        public bool is_constr_type(int row, int mask)
        {
            return Interop.is_constr_type(_lp, row, mask);
        }

        public lpsolve_constr_types get_constr_type(int row)
        {
            return Interop.get_constr_type(_lp, row);
        }

        public bool set_constr_type(int row, lpsolve_constr_types con_type)
        {
            return Interop.set_constr_type(_lp, row, con_type);
        }

        public double get_rh(int row)
        {
            return Interop.get_rh(_lp, row);
        }

        public bool set_rh(int row, double value)
        {
            return Interop.set_rh(_lp, row, value);
        }

        public double get_rh_range(int row)
        {
            return Interop.get_rh_range(_lp, row);
        }

        public bool set_rh_range(int row, double deltavalue)
        {
            return Interop.set_rh_range(_lp, row, deltavalue);
        }

        public void set_rh_vec(double[] rh)
        {
            Interop.set_rh_vec(_lp, rh);
        }

        public bool str_set_rh_vec(string rh_string)
        {
            return Interop.str_set_rh_vec(_lp, rh_string);
        }

        // http://lpsolve.sourceforge.net/5.5/add_constraint.htm

        // http://lpsolve.sourceforge.net/5.5/del_constraint.htm

        // http://lpsolve.sourceforge.net/5.5/get_row.htm

        #endregion

        #region Objective

        public bool set_obj(int Column, double Value)
        {
            return Interop.set_obj(_lp, Column, Value);
        }

        public double get_obj_bound()
        {
            return Interop.get_obj_bound(_lp);
        }

        public void set_obj_bound(double obj_bound)
        {
            Interop.set_obj_bound(_lp, obj_bound);
        }

        public bool set_obj_fn(double[] row)
        {
            return Interop.set_obj_fn(_lp, row);
        }

        public bool set_obj_fnex(int count, double[] row, int[] colno)
        {
            return Interop.set_obj_fnex(_lp, count, row, colno);
        }

        public bool str_set_obj_fn(string row_string)
        {
            return Interop.str_set_obj_fn(_lp, row_string);
        }

        public bool is_maxim()
        {
            return Interop.is_maxim(_lp);
        }

        public void set_maxim()
        {
            Interop.set_maxim(_lp);
        }

        public void set_minim()
        {
            Interop.set_minim(_lp);
        }

        #endregion

        public string get_lp_name()
        {
            return Interop.get_lp_name(_lp);
        }

        public bool set_lp_name(string lpname)
        {
            return Interop.set_lp_name(_lp, lpname);
        }

        public bool resize_lp(int rows, int columns)
        {
            return Interop.resize_lp(_lp, rows, columns);
        }

        public bool is_add_rowmode()
        {
            return Interop.is_add_rowmode(_lp);
        }

        // http://lpsolve.sourceforge.net/5.5/set_add_rowmode.htm
        public bool set_add_rowmode(bool turnon)
        {
            return Interop.set_add_rowmode(_lp, turnon);
        }

        public int get_nameindex(string name, bool isrow)
        {
            return Interop.get_nameindex(_lp, name, isrow);
        }

        public double get_infinite()
        {
            return Interop.get_infinite(_lp);
        }

        public void set_infinite(double infinite)
        {
            Interop.set_infinite(_lp, infinite);
        }

        public bool is_infinite(double value)
        {
            return Interop.is_infinite(_lp, value);
        }


        public double get_mat(int row, int column)
        {
            return Interop.get_mat(_lp, row, column);
        }

        public bool set_mat(int row, int column, double value)
        {
            return Interop.set_mat(_lp, row, column, value);
        }

        public void set_bounds_tighter(bool tighten)
        {
            Interop.set_bounds_tighter(_lp, tighten);
        }

        public bool get_bounds_tighter()
        {
            return Interop.get_bounds_tighter(_lp);
        }

        public lpsolve_branch get_var_branch(int column)
        {
            return Interop.get_var_branch(_lp, column);
        }

        public bool set_var_branch(int column, lpsolve_branch branch_mode)
        {
            return Interop.set_var_branch(_lp, column, branch_mode);
        }

        public int get_var_priority(int column)
        {
            return Interop.get_var_priority(_lp, column);
        }

        public bool set_var_weights(double[] weights)
        {
            return Interop.set_var_weights(_lp, weights);
        }


        // http://lpsolve.sourceforge.net/5.5/add_SOS.htm

        public int add_SOS(string name, int sostype, int priority, int count, int[] sosvars, double[] weights)
        {
            return Interop.add_SOS(_lp, name, sostype, priority, count, sosvars, weights);
        }

        // http://lpsolve.sourceforge.net/5.5/is_SOS_var.htm

        public bool is_SOS_var(int column)
        {
            return Interop.is_SOS_var(_lp, column);
        }


        #endregion

        #region Solver settings

        #region Epsilon / Tolerance

        public double get_epsb()
        {
            return Interop.get_epsb(_lp);
        }

        public void set_epsb(double epsb)
        {
            Interop.set_epsb(_lp, epsb);
        }

        public double get_epsd()
        {
            return Interop.get_epsd(_lp);
        }

        public void set_epsd(double epsd)
        {
            Interop.set_epsd(_lp, epsd);
        }

        public double get_epsel()
        {
            return Interop.get_epsel(_lp);
        }

        public void set_epsel(double epsel)
        {
            Interop.set_epsel(_lp, epsel);
        }

        public double get_epsint()
        {
            return Interop.get_epsint(_lp);
        }

        public void set_epsint(double epsint)
        {
            Interop.set_epsint(_lp, epsint);
        }

        public double get_epsperturb()
        {
            return Interop.get_epsperturb(_lp);
        }

        public void set_epsperturb(double epsperturb)
        {
            Interop.set_epsperturb(_lp, epsperturb);
        }

        public double get_epspivot()
        {
            return Interop.get_epspivot(_lp);
        }

        public void set_epspivot(double epspivot)
        {
            Interop.set_epspivot(_lp, epspivot);
        }

        public bool set_epslevel(lpsolve_epsilon_level level)
        {
            return Interop.set_epslevel(_lp, level);
        }

        #endregion

        #region Basis

        public void reset_basis()
        {
            Interop.reset_basis(_lp);
        }

        public void default_basis()
        {
            Interop.default_basis(_lp);
        }

        public bool read_basis(string filename, string info)
        {
            return Interop.read_basis(_lp, filename, info);
        }

        public bool write_basis(string filename)
        {
            return Interop.write_basis(_lp, filename);
        }

        public bool set_basis(int[] bascolumn, bool nonbasic)
        {
            return Interop.set_basis(_lp, bascolumn, nonbasic);
        }

        public bool get_basis(int[] bascolumn, bool nonbasic)
        {
            return Interop.get_basis(_lp, bascolumn, nonbasic);
        }

        public bool guess_basis(double[] guessvector, int[] basisvector)
        {
            return Interop.guess_basis(_lp, guessvector, basisvector);
        }

        public lpsolve_basiscrash get_basiscrash()
        {
            return Interop.get_basiscrash(_lp);
        }

        public void set_basiscrash(lpsolve_basiscrash mode)
        {
            Interop.set_basiscrash(_lp, mode);
        }

        public bool has_BFP()
        {
            return Interop.has_BFP(_lp);
        }

        public bool is_nativeBFP()
        {
            return Interop.is_nativeBFP(_lp);
        }

        public bool set_BFP(string filename)
        {
            return Interop.set_BFP(_lp, filename);
        }

        #endregion

        #region Pivoting

        public int get_maxpivot()
        {
            return Interop.get_maxpivot(_lp);
        }

        public void set_maxpivot(int max_num_inv)
        {
            Interop.set_maxpivot(_lp, max_num_inv);
        }

        public lpsolve_piv_rules get_pivoting()
        {
            return Interop.get_pivoting(_lp);
        }

        public void set_pivoting(lpsolve_piv_rules piv_rule)
        {
            Interop.set_pivoting(_lp, piv_rule);
        }

        public bool is_piv_rule(lpsolve_piv_rules rule)
        {
            return Interop.is_piv_rule(_lp, rule);
        }

        public bool is_piv_mode(lpsolve_piv_rules testmask)
        {
            return Interop.is_piv_mode(_lp, testmask);
        }

        #endregion

        #region Scaling

        public double get_scalelimit()
        {
            return Interop.get_scalelimit(_lp);
        }

        public void set_scalelimit(double scalelimit)
        {
            Interop.set_scalelimit(_lp, scalelimit);
        }

        public lpsolve_scales get_scaling()
        {
            return Interop.get_scaling(_lp);
        }

        public void set_scaling(lpsolve_scales scalemode)
        {
            Interop.set_scaling(_lp, scalemode);
        }

        public bool is_scalemode(lpsolve_scales testmask)
        {
            return Interop.is_scalemode(_lp, testmask);
        }

        public bool is_scaletype(lpsolve_scales scaletype)
        {
            return Interop.is_scaletype(_lp, scaletype);
        }

        public bool is_integerscaling()
        {
            return Interop.is_integerscaling(_lp);
        }

        public void unscale()
        {
            Interop.unscale(_lp);
        }

        #endregion

        #region Branching

        public bool is_break_at_first()
        {
            return Interop.is_break_at_first(_lp);
        }

        public void set_break_at_first(bool break_at_first)
        {
            Interop.set_break_at_first(_lp, break_at_first);
        }

        public double get_break_at_value()
        {
            return Interop.get_break_at_value(_lp);
        }

        public void set_break_at_value(double break_at_value)
        {
            Interop.set_break_at_value(_lp, break_at_value);
        }

        public lpsolve_BBstrategies get_bb_rule()
        {
            return Interop.get_bb_rule(_lp);
        }

        public void set_bb_rule(lpsolve_BBstrategies bb_rule)
        {
            Interop.set_bb_rule(_lp, bb_rule);
        }

        public int get_bb_depthlimit()
        {
            return Interop.get_bb_depthlimit(_lp);
        }

        public void set_bb_depthlimit(int bb_maxlevel)
        {
            Interop.set_bb_depthlimit(_lp, bb_maxlevel);
        }

        public lpsolve_branch get_bb_floorfirst()
        {
            return Interop.get_bb_floorfirst(_lp);
        }

        public void set_bb_floorfirst(lpsolve_branch bb_floorfirst)
        {
            Interop.set_bb_floorfirst(_lp, bb_floorfirst);
        }

        #endregion

        public lpsolve_improves get_improve()
        {
            return Interop.get_improve(_lp);
        }

        public void set_improve(lpsolve_improves improve)
        {
            Interop.set_improve(_lp, improve);
        }

        public void set_mip_gap(bool absolute, double mip_gap)
        {
            Interop.set_mip_gap(_lp, absolute, mip_gap);
        }

        public double get_mip_gap(bool absolute)
        {
            return Interop.get_mip_gap(_lp, absolute);
        }

        public double get_negrange()
        {
            return Interop.get_negrange(_lp);
        }

        public void set_negrange(double negrange)
        {
            Interop.set_negrange(_lp, negrange);
        }

        public void set_preferdual(bool dodual)
        {
            Interop.set_preferdual(_lp, dodual);
        }

        public lpsolve_anti_degen get_anti_degen()
        {
            return Interop.get_anti_degen(_lp);
        }

        public bool is_anti_degen(lpsolve_anti_degen testmask)
        {
            return Interop.is_anti_degen(_lp, testmask);
        }

        public void set_anti_degen(lpsolve_anti_degen anti_degen)
        {
            Interop.set_anti_degen(_lp, anti_degen);
        }

        public void reset_params()
        {
            Interop.reset_params(_lp);
        }

        public bool read_params(string filename, string options)
        {
            return Interop.read_params(_lp, filename, options);
        }

        public bool write_params(string filename, string options)
        {
            return Interop.write_params(_lp, filename, options);
        }


        public void set_sense(bool maximize)
        {
            Interop.set_sense(_lp, maximize);
        }

        public lpsolve_simplextypes get_simplextype()
        {
            return Interop.get_simplextype(_lp);
        }

        public void set_simplextype(lpsolve_simplextypes simplextype)
        {
            Interop.set_simplextype(_lp, simplextype);
        }

        public int get_solutionlimit()
        {
            return Interop.get_solutionlimit(_lp);
        }

        public void set_solutionlimit(int limit)
        {
            Interop.set_solutionlimit(_lp, limit);
        }

        public int get_timeout()
        {
            return Interop.get_timeout(_lp);
        }

        public void set_timeout(int sectimeout)
        {
            Interop.set_timeout(_lp, sectimeout);
        }

        public bool is_use_names(bool isrow)
        {
            return Interop.is_use_names(_lp, isrow);
        }

        public void set_use_names(bool isrow, bool use_names)
        {
            Interop.set_use_names(_lp, isrow, use_names);
        }

        public bool is_presolve(lpsolve_presolve testmask)
        {
            return Interop.is_presolve(_lp, testmask);
        }

        public int get_presolveloops()
        {
            return Interop.get_presolveloops(_lp);
        }

        public lpsolve_presolve get_presolve()
        {
            return Interop.get_presolve(_lp);
        }

        public void set_presolve(lpsolve_presolve do_presolve, int maxloops)
        {
            Interop.set_presolve(_lp, do_presolve, maxloops);
        }

        #endregion

        #region Callback routines

        public void put_abortfunc(ctrlcfunc newctrlc, IntPtr ctrlchandle)
        {
            Interop.put_abortfunc(_lp, newctrlc, ctrlchandle);
        }

        public void put_logfunc(logfunc newlog, IntPtr loghandle)
        {
            Interop.put_logfunc(_lp, newlog, loghandle);
        }

        public void put_msgfunc(msgfunc newmsg, IntPtr msghandle, int mask)
        {
            Interop.put_msgfunc(_lp, newmsg, msghandle, mask);
        }

        #endregion

        #region Solve

        public lpsolve_return solve()
        {
            return Interop.solve(_lp);
        }

        #endregion

        #region Solution

        public double get_constr_value(int row, int count, double[] primsolution, int[] nzindex)
        {
            return Interop.get_constr_value(_lp, row, count, primsolution, nzindex);
        }

        public bool get_constraints(double[] constr)
        {
            return Interop.get_constraints(_lp, constr);
        }

        public bool get_dual_solution(double[] rc)
        {
            return Interop.get_dual_solution(_lp, rc);
        }

        public int get_max_level()
        {
            return Interop.get_max_level(_lp);
        }

        public double get_objective()
        {
            return Interop.get_objective(_lp);
        }

        public bool get_primal_solution(double[] pv)
        {
            return Interop.get_primal_solution(_lp, pv);
        }

        public bool get_sensitivity_obj(double[] objfrom, double[] objtill)
        {
            return Interop.get_sensitivity_obj(_lp, objfrom, objtill);
        }

        public bool get_sensitivity_objex(double[] objfrom, double[] objtill, double[] objfromvalue,
            double[] objtillvalue)
        {
            return Interop.get_sensitivity_objex(_lp, objfrom, objtill, objfromvalue, objtillvalue);
        }

        public bool get_sensitivity_rhs(double[] duals, double[] dualsfrom, double[] dualstill)
        {
            return Interop.get_sensitivity_rhs(_lp, duals, dualsfrom, dualstill);
        }

        public int get_solutioncount()
        {
            return Interop.get_solutioncount(_lp);
        }

        public long get_total_iter()
        {
            return Interop.get_total_iter(_lp);
        }

        public long get_total_nodes()
        {
            return Interop.get_total_nodes(_lp);
        }

        public double get_var_dualresult(int index)
        {
            return Interop.get_var_dualresult(_lp, index);
        }

        public double get_var_primalresult(int index)
        {
            return Interop.get_var_primalresult(_lp, index);
        }

        public bool get_variables(double[] var)
        {
            return Interop.get_variables(_lp, var);
        }

        public double get_working_objective()
        {
            return Interop.get_working_objective(_lp);
        }

        public bool is_feasible(double[] values, double threshold)
        {
            return Interop.is_feasible(_lp, values, threshold);
        }

        #endregion

        #region Debug/print settings

        public bool set_outputfile(string filename)
        {
            return Interop.set_outputfile(_lp, filename);
        }

        public int get_print_sol()
        {
            return Interop.get_print_sol(_lp);
        }

        public void set_print_sol(int print_sol)
        {
            Interop.set_print_sol(_lp, print_sol);
        }

        public int get_verbose()
        {
            return Interop.get_verbose(_lp);
        }

        public void set_verbose(int verbose)
        {
            Interop.set_verbose(_lp, verbose);
        }

        public bool is_debug()
        {
            return Interop.is_debug(_lp);
        }

        public void set_debug(bool debug)
        {
            Interop.set_debug(_lp, debug);
        }

        public bool is_trace()
        {
            return Interop.is_trace(_lp);
        }

        public void set_trace(bool trace)
        {
            Interop.set_trace(_lp, trace);
        }

        #endregion

        #region Debug/print

        public void print_constraints(int columns)
        {
            Interop.print_constraints(_lp, columns);
        }

        public bool print_debugdump(string filename)
        {
            return Interop.print_debugdump(_lp, filename);
        }

        public void print_duals()
        {
            Interop.print_duals(_lp);
        }

        public void print_lp()
        {
            Interop.print_lp(_lp);
        }

        public void print_objective()
        {
            Interop.print_objective(_lp);
        }

        public void print_scales()
        {
            Interop.print_scales(_lp);
        }

        public void print_solution(int columns)
        {
            Interop.print_solution(_lp, columns);
        }

        public void print_str(string str)
        {
            Interop.print_str(_lp, str);
        }

        public void print_tableau()
        {
            Interop.print_tableau(_lp);
        }

        #endregion

        #region Write model to file

        public bool write_lp(string filename)
        {
            return Interop.write_lp(_lp, filename);
        }

        public bool write_freemps(string filename)
        {
            return Interop.write_freemps(_lp, filename);
        }

        public bool write_mps(string filename)
        {
            return Interop.write_mps(_lp, filename);
        }

        public bool is_nativeXLI()
        {
            return Interop.is_nativeXLI(_lp);
        }

        public bool has_XLI()
        {
            return Interop.has_XLI(_lp);
        }

        public bool set_XLI(string filename)
        {
            return Interop.set_XLI(_lp, filename);
        }

        public bool write_XLI(string filename, string options, bool results)
        {
            return Interop.write_XLI(_lp, filename, options, results);
        }

        #endregion

        #region Miscellaneous routines

        public static Version LpSolveVersion
        {
            get
            {
                int major = 0;
                int minor = 0;
                int release = 0;
                int build = 0;
                Interop.lp_solve_version(ref major, ref minor, ref release, ref build);
                return new Version(major, minor, release, build);
            }
        }

        public int column_in_lp(double[] column)
        {
            return Interop.column_in_lp(_lp, column);
        }

        public bool dualize_lp()
        {
            return Interop.dualize_lp(_lp);
        }

        public int get_nonzeros()
        {
            return Interop.get_nonzeros(_lp);
        }

        public int get_Lrows()
        {
            return Interop.get_Lrows(_lp);
        }

        public int get_Ncolumns()
        {
            return Interop.get_Ncolumns(_lp);
        }

        public int get_Norig_columns()
        {
            return Interop.get_Norig_columns(_lp);
        }

        public int get_Norig_rows()
        {
            return Interop.get_Norig_rows(_lp);
        }

        public int get_Nrows()
        {
            return Interop.get_Nrows(_lp);
        }

        public int get_status()
        {
            return Interop.get_status(_lp);
        }

        public string get_statustext(int statuscode)
        {
            return Interop.get_statustext(_lp, statuscode);
        }

        public double time_elapsed()
        {
            return Interop.time_elapsed(_lp);
        }

        public int get_lp_index(int orig_index)
        {
            return Interop.get_lp_index(_lp, orig_index);
        }

        public int get_orig_index(int lp_index)
        {
            return Interop.get_orig_index(_lp, lp_index);
        }

        #endregion


    }
}
