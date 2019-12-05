﻿#if NET20 || NETSTANDARD2_0
#define SUPPORTS_ENVIRONMENT_VARIABLE_TARGET
#endif
#if NETSTANDARD2_0 || NETSTANDARD1_3
#define SUPPORTS_APPCONTEXT
#endif
using System;
using System.IO;

namespace LpSolveDotNet
{
    /// <summary>
    /// Class that represents a Linear Programming (LP) model and methods to solve it.
    /// <para>
    /// This class represents the <c>lprec</c> structure in the <see href="http://lpsolve.sourceforge.net/">C API of LP Solve</see>.
    /// The instance methods of this class are all the methods from the C API of LP Solve but the first parameter
    /// is implicitly referring to current model.
    /// The static methods in this class are factory methods to create a new model.
    /// </para>
    /// <remarks>
    /// You must call the method <see cref="Init"/> once before calling any other method
    /// in order to make sure the native lpsolve library will be loaded from the right location.
    /// </remarks>
    /// </summary>
    public sealed class LpSolve
        : IDisposable
    {
        #region Library initialization

        /// <summary>
        /// Initializes the library by making sure the correct <c>lpsolve55.dll</c> native library
        /// will be loaded.
        /// </summary>
        /// <param name="dllFolderPath">The (optional) folder where the native library is located.
        /// When <paramref name="dllFolderPath"/> is <c>null</c>, it will use either <c>basedir/NativeBinaries/win64</c> or <c>basedir/NativeBinaries/win32</c>
        /// based on whether application running is 32 or 64-bits. This will work for any application built on Windows platform
        /// using the NuGet package because the package because the NuGet</param>
        /// <returns><c>true</c>, if it found the native library, <c>false</c> otherwise.</returns>
        public static bool Init(string dllFolderPath = null)
        {
            if (string.IsNullOrEmpty(dllFolderPath))
            {
                bool is64Bit = IntPtr.Size == 8;
                string baseDirectory =
#if SUPPORTS_APPCONTEXT
                    AppContext.BaseDirectory;
#else
                    Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
#endif
                dllFolderPath = Path.Combine(Path.Combine(baseDirectory, "NativeBinaries"), is64Bit ? "win64" : "win32");
            }
            if (dllFolderPath.EndsWith(Path.DirectorySeparatorChar.ToString())
                || dllFolderPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                // remove trailing slash for use in PATH environment variable
                dllFolderPath = dllFolderPath.Substring(0, dllFolderPath.Length - 1);
            }
            var dllFilePath = Path.Combine(dllFolderPath + Path.DirectorySeparatorChar, "lpsolve55.dll");

            bool returnValue = File.Exists(dllFilePath);
            if (returnValue)
            {
                if (!_hasAlreadyChangedPathEnvironmentVariable)
                {
                    string pathEnvironmentVariable = GetPathEnvironmentVariable();
                    string pathWithSemiColon = pathEnvironmentVariable + Path.PathSeparator;
                    if (pathWithSemiColon.IndexOf(dllFolderPath + Path.PathSeparator) < 0)
                    {
                        SetPathEnvironmentVariable(dllFolderPath + Path.PathSeparator + pathEnvironmentVariable);
                    }
                    _hasAlreadyChangedPathEnvironmentVariable = true;
                }
            }
            return returnValue;
        }
        private static bool _hasAlreadyChangedPathEnvironmentVariable;

        private static string GetPathEnvironmentVariable()
        {
            return Environment.GetEnvironmentVariable("PATH"
#if SUPPORTS_ENVIRONMENT_VARIABLE_TARGET
                , EnvironmentVariableTarget.Process
#endif
                );
        }

        private static void SetPathEnvironmentVariable(string value)
        {
            Environment.SetEnvironmentVariable("PATH", value
#if SUPPORTS_ENVIRONMENT_VARIABLE_TARGET
                , EnvironmentVariableTarget.Process
#endif
                );
        }

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

        /// <summary>
        /// Creates and initialises a new <see cref="LpSolve"/> model.
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/make_lp.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="rows">Initial number of rows. Can be <c>0</c> as new rows can be added via 
        /// <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.</param>
        /// <param name="columns">Initial number of columns. Can be <c>0</c> as new columns can be added via
        /// <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/>.</param>
        /// <returns>A new <see cref="LpSolve"/> model with <paramref name="rows"/> rows and <paramref name="columns"/> columns.
        /// A <c>null</c> return value indicates an error. Specifically not enough memory available to setup an lprec structure.</returns>
        public static LpSolve make_lp(int rows, int columns)
        {
            IntPtr lp = Interop.make_lp(rows, columns);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Creates and initialises a new <see cref="LpSolve"/> model from a LP model file.
        /// <remarks>The model in the file must be in <see href="http://lpsolve.sourceforge.net/5.5/lp-format.htm">lp-format</see>.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_LP.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="fileName">Filename to read the LP model from.</param>
        /// <param name="verbose">The verbose level. See also <see cref="set_verbose"/> and <see cref="get_verbose"/>.</param>
        /// <param name="lpName">Initial name of the model. May be <c>null</c> if the model has no name. See also <see cref="set_lp_name"/> and <see cref="get_lp_name"/>.</param>
        /// <returns>A new <see cref="LpSolve"/> model matching the one in the file.
        /// A <c>null</c> return value indicates an error. Specifically file could not be opened, has wrong structure or not enough memory is available.</returns>
        public static LpSolve read_LP(string fileName, lpsolve_verbosity verbose, string lpName)
        {
            IntPtr lp = Interop.read_LP(fileName, verbose, lpName);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Creates and initialises a new <see cref="LpSolve"/> model from an MPS model file.
        /// <remarks>The model in the file must be in <see href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">mps-format</see>.</remarks>
        /// <para>This method is different from C API because <paramref name="verbose"/> is separate from <paramref name="options"/></para>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_mps.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="fileName">Filename to read the MPS model from.</param>
        /// <param name="verbose">Specifies the verbose level. See also <see cref="set_verbose"/> and <see cref="get_verbose"/>.</param>
        /// <param name="options">Specifies how to interprete the MPS layout. You can use multiple values.</param>
        /// <returns>A new <see cref="LpSolve"/> model matching the one in the file.
        /// A <c>null</c> return value indicates an error. Specifically file could not be opened, has wrong structure or not enough memory is available.</returns>
        public static LpSolve read_MPS(string fileName, lpsolve_verbosity verbose, lpsolve_mps_options options)
        {
            IntPtr lp = Interop.read_MPS(fileName, ((int)verbose)|((int)options));
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Creates and initialises a new <see cref="LpSolve"/> model via the eXternal Language Interface.
        /// <remarks>The method constructs a new <see cref="LpSolve"/> model by reading model from <paramref name="modelName"/> via the specified XLI.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/XLI.htm">Extnernal Language Interfaces</see>for a complete description on XLIs.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_XLI.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="xliName">Filename of the XLI package.</param>
        /// <param name="modelName">Filename to read the model from.</param>
        /// <param name="dataName">Filename to read the data from. This may be optional. In that case, set the parameter to <c>null</c>.</param>
        /// <param name="options">Extra options that can be used by the reader.</param>
        /// <param name="verbose">The verbose level. See also <see cref="set_verbose"/> and <see cref="get_verbose"/>.</param>
        /// <returns>A new <see cref="LpSolve"/> model matching the one in the file.
        /// A <c>null</c> return value indicates an error.</returns>
        public static LpSolve read_XLI(string xliName, string modelName, string dataName, string options, lpsolve_verbosity verbose)
        {
            IntPtr lp = Interop.read_XLI(xliName, modelName, dataName, options, verbose);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Copies current model to a new one.
        /// <remarks>The new model is independent from the original one.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/copy_lp.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns>A new model with the same values as current one or <c>null</c> if an error occurs (not enough memory).</returns>
        public LpSolve copy_lp()
        {
            IntPtr lp = Interop.copy_lp(_lp);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Frees all memory allocated to the model.
        /// <remarks>You don't need to call this method, the <see cref="IDisposable"/> implementation does it for you.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/delete_lp.htm">Full C API documentation.</seealso>
        /// </summary>
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

        /// <summary>
        /// Finalizer
        /// </summary>
        ~LpSolve()
        {
            Dispose(false);
        }

        #endregion

        #region Build model

        #region Column

        /// <summary>
        /// Adds a column to the model.
        /// <remarks><para>The method adds a column to the model (at the end) and sets all values of the column at once.</para>
        /// <para>Note that element 0 of the array is the value of the objective function for that column. Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>It is almost always better to use <see cref="add_columnex"/> instead of <see cref="add_column"/>. <see cref="add_columnex"/> is always at least as performant as <see cref="add_column"/>.</para>
        /// <para>Note that if you have to add many columns, performance can be improved by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_column.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">An array with 1+<see cref="get_Nrows"/> elements that contains the values of the column.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool add_column(double[] column)
        {
            return Interop.add_column(_lp, column);
        }

        /// <summary>
        /// Adds a column to the model.
        /// <remarks><para>The method adds a column to the model (at the end) and sets all values of the column at once.</para>
        /// <para>Note that when <paramref name="rowno"/> is <c>null</c>, element 0 of the array is the value of the objective function for that column. Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>This method has the possibility to specify only the non-zero elements. In that case <paramref name="rowno"/> specifies the row numbers 
        /// of the non-zero elements. Both <paramref name="column"/> and <paramref name="rowno"/> are then zero-based arrays. 
        /// This will speed up building the model considerably if there are a lot of zero values. In most cases the matrix is sparse and has many zero values.
        /// Note that <see cref="add_columnex"/> behaves the same as <see cref="add_column"/> when <paramref name="rowno"/> is <c>null</c>.
        /// It is almost always better to use<see cref="add_columnex"/> instead of<see cref="add_column"/>. <see cref = "add_columnex" /> is always at least as performant as <see cref = "add_column" />.
        /// </para>
        /// <para><paramref name="column"/> and <paramref name="rowno"/> can both be <c>null</c>. In that case an empty column is added.</para>
        /// <para>Note that if you have to add many columns, performance can be improved by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_column.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="count">Number of elements in <paramref name="column"/> and <paramref name="rowno"/>.</param>
        /// <param name="column">An array with <paramref name="count"/> elements that contains the values of the column.</param>
        /// <param name="rowno">A zero-based array with <paramref name="count"/> elements that contains the row numbers of the column. 
        /// However this variable can also be <c>null</c>. In that case element i in the variable <paramref name="column"/> is row i.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool add_columnex(int count, double[] column, int[] rowno)
        {
            return Interop.add_columnex(_lp, count, column, rowno);
        }

        /// <summary>
        /// Adds a column to the model.
        /// <remarks>This should only be used in small or demo code since it is not performant and uses more memory.
        /// Instead use <see cref="add_columnex"/> or <see cref="add_column"/>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_column.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="col_string">A string with row elements that contains the values of the column. Each element must be separated by space(s).</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool str_add_column(string col_string)
        {
            return Interop.str_add_column(_lp, col_string);
        }

        /// <summary>
        /// Deletes a column from the model.
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="set_add_rowmode"/>.</para>
        /// <para>The column is effectively deleted from the model, so all columns after this column shift one left.</para>
        /// <para>Note that column 0 (the right hand side (RHS)) cannot be deleted. There is always a RHS.</para>
        /// <para>Note that you can also delete multiple columns by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/del_column.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column to delete. Must be between <c>1</c> and the number of columns in the model.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise. An error occurs if <paramref name="column"/> 
        /// is not between <c>1</c> and the number of columns in the model</returns>
        public bool del_column(int column)
        {
            return Interop.del_column(_lp, column);
        }


        /// <summary>
        /// Sets a column in the model.
        /// <remarks>
        /// <para>The method changes the values of an existing column in the model at once.</para>
        /// <para>Note that element 0 of the array is row 0 (objective function). element 1 is row 1, ...</para>
        /// <para>It is almost always better to use <see cref="set_columnex"/> instead of <see cref="set_column"/>. <see cref="set_columnex"/> is always at least as performant as <see cref="set_column"/>.</para>
        /// <para>It is more performant to call this method than call <see cref="set_mat"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_column.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="col_no">The column number that must be changed.</param>
        /// <param name="column">An array with 1+<see cref="get_Nrows"/> elements that contains the values of the column.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool set_column(int col_no, double[] column)
        {
            return Interop.set_column(_lp, col_no, column);
        }

        /// <summary>
        /// Sets a column in the model.
        /// <remarks>
        /// <para>The method changes the values of an existing column in the model at once.</para>
        /// <para>Note that when <paramref name="rowno"/> is <c>null</c>, element 0 of the array is row 0 (objective function). element 1 is row 1, ...</para>
        /// <para>This method has the possibility to specify only the non-zero elements. In that case <paramref name="rowno"/> specifies the row numbers 
        /// of the non-zero elements. And in contrary to <see cref="set_column"/>, it reads the arrays starting at element 0.
        /// This will speed up building the model considerably if there are a lot of zero values. In most cases the matrix is sparse and has many zero values.
        /// Note that <see cref="set_columnex"/> behaves the same as <see cref="set_column"/> when <paramref name="rowno"/> is <c>null</c>.
        /// It is almost always better to use<see cref="set_columnex"/> instead of<see cref="set_column"/>. <see cref = "set_columnex" /> is always at least as performant as <see cref = "set_column" />.
        /// </para>
        /// <para>It is more performant to call this method than call <see cref="set_mat"/>.</para>
        /// <para>Note that unspecified values are set to zero.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_column.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="col_no">The column number that must be changed.</param>
        /// <param name="count">Number of elements in <paramref name="column"/> and <paramref name="rowno"/>.</param>
        /// <param name="column">An array with <paramref name="count"/> elements that contains the values of the column.</param>
        /// <param name="rowno">A zero-based array with <paramref name="count"/> elements that contains the row numbers of the column. 
        /// However this variable can also be <c>null</c>. In that case element i in the variable <paramref name="column"/> is row i.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool set_columnex(int col_no, int count, double[] column, int[] rowno)
        {
            return Interop.set_columnex(_lp, col_no, count, column, rowno);
        }

        /// <summary>
        /// Gets all column elements from the model for the given <paramref name="col_nr"/>.
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="set_add_rowmode"/>.</para>
        /// <para>Note that element 0 of the array is row 0 (objective function). element 1 is row 1, ...</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_column.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="col_nr">The column number of the matrix. Must be between 1 and number of columns in the model.</param>
        /// <param name="column">Array in which the values are returned. The array must be dimensioned with at least 1+<see cref="get_Nrows"/> elements.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool get_column(int col_nr, double[] column)
        {
            return Interop.get_column(_lp, col_nr, column);
        }

        /// <summary>
        /// Gets the non-zero column elements from the model for the given <paramref name="col_nr"/>.
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="set_add_rowmode"/>.</para>
        /// <para>Returned values in <paramref name="column"/> and <paramref name="nzrow"/> start from element 0.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_column.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="col_nr">The column number of the matrix. Must be between 1 and number of columns in the model.</param>
        /// <param name="column">Array in which the values are returned. The array must be dimensioned with at least the number of non-zero elements in the column.
        /// If that is unknown, then use 1+<see cref="get_Nrows"/>.</param>
        /// <param name="nzrow">Array in which the row numbers  are returned. The array must be dimensioned with at least the number of non-zero elements in the column.
        /// If that is unknown, then use 1+<see cref="get_Nrows"/>.</param>
        /// <returns>The number of non-zero elements returned in <paramref name="column"/> and <paramref name="nzrow"/>.</returns>
        public int get_columnex(int col_nr, double[] column, int[] nzrow)
        {
            return Interop.get_columnex(_lp, col_nr, column, nzrow);
        }

        /// <summary>
        /// Sets the name of a column in the model.
        /// <remarks>
        /// The column must already exist.
        /// Column names are optional.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_col_name.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column for which the name must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="new_name">The name for the column.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool set_col_name(int column, string new_name)
        {
            return Interop.set_col_name(_lp, column, new_name);
        }

        /// <summary>
        /// Gets the name of a column in the model.
        /// <remarks>
        /// <para>Column names are optional.
        /// If no column name was specified, the method returns Cx with x the column number.
        /// </para>
        /// <para>
        /// The difference between <see cref="get_col_name"/> and <see cref="get_origcol_name"/> is only visible when a presolve (<see cref="set_presolve"/>) was done. 
        /// Presolve can result in deletion of columns in the model. In <see cref="get_col_name"/>, column specifies the column number after presolve was done.
        /// In <see cref="get_origcol_name"/>, column specifies the column number before presolve was done, ie the original column number. 
        /// If presolve is not active then both methods are equal.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_col_name.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column for which the name must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <returns>The name of the specified column if it was specified, Cx with x the column number otherwise or <c>null</c> on error.</returns>
        public string get_col_name(int column)
        {
            return Interop.get_col_name(_lp, column);
        }

        /// <summary>
        /// Gets the name of a column in the model.
        /// <remarks>
        /// <para>Column names are optional.
        /// If no column name was specified, the method returns Cx with x the column number.
        /// </para>
        /// <para>
        /// The difference between <see cref="get_col_name"/> and <see cref="get_origcol_name"/> is only visible when a presolve (<see cref="set_presolve"/>) was done. 
        /// Presolve can result in deletion of columns in the model. In <see cref="get_col_name"/>, column specifies the column number after presolve was done.
        /// In <see cref="get_origcol_name"/>, column specifies the column number before presolve was done, ie the original column number. 
        /// If presolve is not active then both methods are equal.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_col_name.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column for which the name must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <returns>The name of the specified column if it was specified, Cx with x the column number otherwise or <c>null</c> on error.</returns>
        public string get_origcol_name(int column)
        {
            return Interop.get_origcol_name(_lp, column);
        }

        /// <summary>
        /// Returns whether the variable is negative or not.
        /// <remarks>
        /// Negative means a lower and upper bound that are both negative. Default a variable is not free because default it has a lower bound of 0 (and an upper bound of +infinity).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_negative.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable to check. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is defined as negative, <c>false</c> otherwise.</returns>
        public bool is_negative(int column)
        {
            return Interop.is_negative(_lp, column);
        }

        /// <summary>
        /// Returns whether the variable is of type Integer or not.
        /// <remarks>
        /// Default a variable is not integer. From the moment there is at least one integer variable in the model,
        /// the Branch and Bound algorithm is used to make these variables integer.
        /// Note that solving times can be considerably larger when there are integer variables.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/integer.htm">integer variables</see> for a description about integer variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_int.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable to check. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is defined as integer, <c>false</c> otherwise.</returns>
        public bool is_int(int column)
        {
            return Interop.is_int(_lp, column);
        }

        /// <summary>
        /// Sets the type of the variable to type Integer or floating point.
        /// <remarks>
        /// Default a variable is not integer. The argument <paramref name="must_be_int"/> defines what the status of the variable becomes.
        /// From the moment there is at least one integer variable in the model,
        /// the Branch and Bound algorithm is used to make these variables integer.
        /// Note that solving times can be considerably larger when there are integer variables.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/integer.htm">integer variables</see> for a description about integer variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_int.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable that must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="must_be_int"><c>true</c> if variable must be an integer, <c>false</c> otherwise.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        public bool set_int(int column, bool must_be_int)
        {
            return Interop.set_int(_lp, column, must_be_int);
        }

        /// <summary>
        /// Returns whether the variable is of type Binary or not.
        /// <remarks>
        /// Default a variable is not binary. A binary variable is an integer variable with lower bound 0 and upper bound 1.
        /// From the moment there is at least one integer variable in the model,
        /// the Branch and Bound algorithm is used to make these variables integer.
        /// Note that solving times can be considerably larger when there are integer variables.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/integer.htm">integer variables</see> for a description about integer variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_binary.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable to check. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is defined as binary, <c>false</c> otherwise.</returns>
        public bool is_binary(int column)
        {
            return Interop.is_binary(_lp, column);
        }

        /// <summary>
        /// Sets the type of the variable to type Binary or floating point.
        /// <remarks>
        /// Default a variable is not binary. A binary variable is an integer variable with lower bound 0 and upper bound 1.
        /// This method also sets these bounds.
        /// The argument <paramref name="must_be_bin"/> defines what the status of the variable becomes.
        /// From the moment there is at least one integer variable in the model,
        /// the Branch and Bound algorithm is used to make these variables integer.
        /// Note that solving times can be considerably larger when there are integer variables.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/integer.htm">integer variables</see> for a description about integer variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_binary.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable that must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="must_be_bin"><c>true</c> if variable must be a binary, <c>false</c> otherwise.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        public bool set_binary(int column, bool must_be_bin)
        {
            return Interop.set_binary(_lp, column, must_be_bin);
        }

        /// <summary>
        /// Returns whether the variable is of type semi-continuous or not.
        /// <remarks>
        /// Default a variable is not semi-continuous.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/semi-cont.htm">semi-continuous variables</see> for a description about semi-continuous variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_semicont.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable to check. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is defined as semi-continuous, <c>false</c> otherwise.</returns>
        public bool is_semicont(int column)
        {
            return Interop.is_semicont(_lp, column);
        }

        /// <summary>
        /// Sets the type of the variable to type semi-continuous or not.
        /// <remarks>
        /// By default, a variable is not semi-continuous. The argument <paramref name="must_be_sc"/> defines what the status of the variable becomes.
        /// Note that a semi-continuous variable must also have a lower bound to have effect.
        /// This because the default lower bound on variables is zero, also when defined as semi-continuous, and without
        /// a lower bound it has no point to define a variable as such.
        /// The lower bound may be set before or after setting the semi-continuous status.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/semi-cont.htm">semi-continuous variables</see> for a description about semi-continuous variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_semicont.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable that must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="must_be_sc"><c>true</c> if variable must be a semi-continuous, <c>false</c> otherwise.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        public bool set_semicont(int column, bool must_be_sc)
        {
            return Interop.set_semicont(_lp, column, must_be_sc);
        }

        /// <summary>
        /// Sets the lower and upper bound of a variable.
        /// <remarks>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// Note that the default lower bound of each variable is 0.
        /// So variables will never take negative values if no negative lower bound is set.
        /// The default upper bound of a variable is infinity(well not quite.It is a very big number, the value of <see cref="get_infinite"/>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bounds.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable on which the bounds must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="lower">The lower bound on the variable identified by <paramref name="column"/>.</param>
        /// <param name="upper">The upper bound on the variable identified by <paramref name="column"/>.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        public bool set_bounds(int column, double lower, double upper)
        {
            return Interop.set_bounds(_lp, column, lower, upper);
        }

        /// <summary>
        /// Sets if the variable is free.
        /// <remarks>
        /// Free means a lower bound of -infinity and an upper bound of +infinity.
        /// Default a variable is not free because default it has a lower bound of 0 (and an upper bound of +infinity).
        /// See <see href="http://lpsolve.sourceforge.net/5.5/free.htm">free variables</see> for a description about free variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_unbounded.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable that must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        public bool set_unbounded(int column)
        {
            return Interop.set_unbounded(_lp, column);
        }

        /// <summary>
        /// Returns whether the variable is free or not.
        /// <remarks>
        /// Free means a lower bound of -infinity and an upper bound of +infinity.
        /// Default a variable is not free because default it has a lower bound of 0 (and an upper bound of +infinity).
        /// See <see href="http://lpsolve.sourceforge.net/5.5/free.htm">free variables</see> for a description about free variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_unbounded.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable to check. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is defined as free, <c>false</c> otherwise.</returns>
        public bool is_unbounded(int column)
        {
            return Interop.is_unbounded(_lp, column);
        }

        /// <summary>
        /// Gets the upper bound of a variable.
        /// <remarks>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// The default upper bound of a variable is infinity(well not quite.It is a very big number, the value of <see cref="get_infinite"/>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_upbo.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable. Must be between 1 and the number of columns in the lp.</param>
        /// <returns>The upper bound on the specified variable. If no bound was set, it returns a very big number, 
        /// the value of <see cref="get_infinite"/>, the default upper bound</returns>
        public double get_upbo(int column)
        {
            return Interop.get_upbo(_lp, column);
        }

        /// <summary>
        /// Sets the upper bound of a variable.
        /// <remarks>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// The default upper bound of a variable is infinity(well not quite.It is a very big number, the value of <see cref="get_infinite"/>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_upbo.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable on which the bound must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="value">The upper bound on the variable identified by <paramref name="column"/>.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        public bool set_upbo(int column, double value)
        {
            return Interop.set_upbo(_lp, column, value);
        }

        /// <summary>
        /// Gets the lower bound of a variable.
        /// <remarks>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// Note that the default lower bound of each variable is 0.
        /// So variables will never take negative values if no negative lower bound is set.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_lowbo.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable. Must be between 1 and the number of columns in the lp.</param>
        /// <returns>The lower bound on the specified variable. If no bound was set, it returns 0, the default lower bound.</returns>
        public double get_lowbo(int column)
        {
            return Interop.get_lowbo(_lp, column);
        }

        /// <summary>
        /// Sets the lower bound of a variable.
        /// <remarks>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// Note that the default lower bound of each variable is 0.
        /// So variables will never take negative values if no negative lower bound is set.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_lowbo.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable on which the bound must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="value">The lower bound on the variable identified by <paramref name="column"/>.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        public bool set_lowbo(int column, double value)
        {
            return Interop.set_lowbo(_lp, column, value);
        }

        #endregion // Build model /  Column

        #region Constraint / Row

        /// <summary>
        /// Adds a constraint to the model.
        /// <remarks>
        /// <para>This method adds a row to the model (at the end) and sets all values of the row at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>It is almost always better to use <see cref="add_constraintex"/> instead of <see cref="add_constraint"/>. <see cref="add_constraintex"/> is always at least as performant as <see cref="add_constraint"/>.</para>
        /// <para>Note that it is advised to set the objective function (via <see cref="set_obj_fn"/>, <see cref="set_obj_fnex"/>, <see cref="str_set_obj_fn"/>, <see cref="set_obj"/>)
        /// before adding rows. This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// <para>Note that these routines will perform much better when <see cref="set_add_rowmode"/> is called before adding constraints.</para>
        /// <para>Note that if you have to add many constraints, performance can be improved by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_constraint.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">An array with 1+<see cref="get_Ncolumns"/> elements that contains the values of the row.</param>
        /// <param name="constr_type">The type of the constraint.</param>
        /// <param name="rh">The value of the right-hand side (RHS) fo the constraint (in)equation</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool add_constraint(double[] row, lpsolve_constr_types constr_type, double rh)
        {
            return Interop.add_constraint(_lp, row, constr_type, rh);
        }


        /// <summary>
        /// Adds a constraint to the model.
        /// <remarks>
        /// <para>This method adds a row to the model (at the end) and sets all values of the row at once.</para>
        /// <para>Note that when <paramref name="row"/> is <c>null</c>, element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>This method has the possibility to specify only the non-zero elements. In that case <paramref name="colno"/> specifies the column numbers of 
        /// the non-zero elements. Both <paramref name="row"/> and <paramref name="colno"/> are then zero-based arrays.
        /// This will speed up building the model considerably if there are a lot of zero values. In most cases the matrix is sparse and has many zero value.
        /// Note that <see cref="add_constraintex"/> behaves the same as <see cref="add_constraint"/> when <paramref name="colno"/> is <c>null</c>.</para>
        /// <para>It is almost always better to use <see cref="add_constraintex"/> instead of <see cref="add_constraint"/>. <see cref="add_constraintex"/> is always at least as performant as <see cref="add_constraint"/>.</para>
        /// <para>Note that it is advised to set the objective function (via <see cref="set_obj_fn"/>, <see cref="set_obj_fnex"/>, <see cref="str_set_obj_fn"/>, <see cref="set_obj"/>)
        /// before adding rows. This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// <para>Note that these routines will perform much better when <see cref="set_add_rowmode"/> is called before adding constraints.</para>
        /// <para>Note that if you have to add many constraints, performance can be improved by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_constraint.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="count">Number of elements in <paramref name="row"/> and <paramref name="colno"/>.</param>
        /// <param name="row">An array with <paramref name="count"/> elements (or 1+<see cref="get_Ncolumns"/> elements if <paramref name="row"/> is <c>null</c>) that contains the values of the row.</param>
        /// <param name="colno">An array with <paramref name="count"/> elements that contains the column numbers of the row. However this variable can also be <c>null</c>.
        /// In that case element <c>i</c> in the variable row is column <c>i</c> and values start at element 1.</param>
        /// <param name="constr_type">The type of the constraint.</param>
        /// <param name="rh">The value of the right-hand side (RHS) fo the constraint (in)equation</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool add_constraintex(int count, double[] row, int[] colno, lpsolve_constr_types constr_type, double rh)
        {
            return Interop.add_constraintex(_lp, count, row, colno, constr_type, rh);
        }

        /// <summary>
        /// Adds a constraint to the model.
        /// <remarks>
        /// <para>This method adds a row to the model (at the end) and sets all values of the row at once.</para>
        /// <para>This method should only be used in small or demo code since it is not performant and uses more memory than <see cref="add_constraint"/> and <see cref="add_constraintex"/>.</para>
        /// <para>Note that these routines will perform much better when <see cref="set_add_rowmode"/> is called before adding constraints.</para>
        /// <para>Note that if you have to add many constraints, performance can be improved by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_constraint.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row_string">A string with column elements that contains the values of the row. Each element must be separated by space(s).</param>
        /// <param name="constr_type">The type of the constraint.</param>
        /// <param name="rh">The value of the right-hand side (RHS) fo the constraint (in)equation</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool str_add_constraint(string row_string, lpsolve_constr_types constr_type, double rh)
        {
            return Interop.str_add_constraint(_lp, row_string, constr_type, rh);
        }

        /// <summary>
        /// Removes a constraint from the model.
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this function also fails. See <see cref="set_add_rowmode"/>.</para>
        /// <para>This method deletes a row from the model. The row is effectively deleted, so all rows after this row shift one up.</para>
        /// <para>Note that row 0 (the objective function) cannot be deleted. There is always an objective function.</para>
        /// <para>Note that you can also delete multiple constraints by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/del_constraint.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="del_row">The row to delete. Must be between 1 and the number of rows in the model.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise. An error occurs when <paramref name="del_row"/> is not between 1 and the number of rows in the model.</returns>
        public bool del_constraint(int del_row)
        {
            return Interop.del_constraint(_lp, del_row);
        }

        /// <summary>
        /// Gets all row elements from the model for the given <paramref name="row_nr"/>.
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="set_add_rowmode"/>.</para>
        /// <para>Element 0 of the row array is not filled. Element 1 will contain the value for column 1, Element 2 the value for column 2, ...</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_row.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row_nr">The row number of the matrix. Must be between 1 and number of rows in the model. Row 0 is the objective function.</param>
        /// <param name="row">Array in which the values are returned. The array must be dimensioned with at least 1+<see cref="get_Ncolumns"/> elements.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise. An error occurs when <paramref name="row_nr"/> is not between 0 and the number of rows in the model.</returns>
        public bool get_row(int row_nr, double[] row)
        {
            return Interop.get_row(_lp, row_nr, row);
        }


        /// <summary>
        /// Gets the non-zero row elements from the model for the given <paramref name="row_nr"/>.
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="set_add_rowmode"/>.</para>
        /// <para>Returned values in <paramref name="row"/> and <paramref name="colno"/> start from element 0.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_row.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row_nr">The row number of the matrix. Must be between 1 and number of rows in the model. Row 0 is the objective function.</param>
        /// <param name="row">Array in which the values are returned. The array must be dimensioned with at least the number of non-zero elements in the row.
        /// If that is unknown, then use the number of columns in the model. The return value of the method indicates how many non-zero elements there are.</param>
        /// <param name="colno">Array in which the column numbers are returned. The array must be dimensioned with at least the number of non-zero elements in the row.
        /// If that is unknown, then use the number of columns in the model. The return value of the method indicates how many non-zero elements there are.</param>
        /// <returns>The number of non-zero elements returned in <paramref name="row"/> and <paramref name="colno"/>. If an error occurs, then -1 is returned.. An error occurs when <paramref name="row_nr"/> is not between 0 and the number of rows in the model.</returns>
        public int get_rowex(int row_nr, double[] row, int[] colno)
        {
            return Interop.get_rowex(_lp, row_nr, row, colno);
        }


        /// <summary>
        /// Sets a constraint in the model.
        /// <remarks>
        /// <para>This method change the values of the row in the model at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>It is almost always better to use <see cref="set_rowex"/> instead of <see cref="set_row"/>. <see cref="set_rowex"/> is always at least as performant as <see cref="set_row"/>.</para>
        /// <para>It is more performant to call these methods than multiple times <see cref="set_mat"/>.</para>
        /// <para>Note that these routines will perform much better when <see cref="set_add_rowmode"/> is called before adding constraints.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_row.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row_no">The row number that must be changed.</param>
        /// <param name="row">An array with 1+<see cref="get_Ncolumns"/> elements that contains the values of the row.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool set_row(int row_no, double[] row)
        {
            return Interop.set_row(_lp, row_no, row);
        }

        /// <summary>
        /// Sets a constraint in the model.
        /// <remarks>
        /// <para>This method change the values of the row in the model at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>This method has the possibility to specify only the non-zero elements. In that case <paramref name="colno"/> specifies the column numbers of 
        /// the non-zero elements. Both <paramref name="row"/> and <paramref name="colno"/> are then zero-based arrays.
        /// This will speed up building the model considerably if there are a lot of zero values. In most cases the matrix is sparse and has many zero value.
        /// Note that <see cref="set_rowex"/> behaves the same as <see cref="set_row"/> when <paramref name="colno"/> is <c>null</c>.</para>
        /// <para>It is almost always better to use <see cref="set_rowex"/> instead of <see cref="set_row"/>. <see cref="set_rowex"/> is always at least as performant as <see cref="set_row"/>.</para>
        /// <para>It is more performant to call these methods than multiple times <see cref="set_mat"/>.</para>
        /// <para>Note that unspecified values by <see cref="set_rowex"/> are set to zero.</para>
        /// <para>Note that these routines will perform much better when <see cref="set_add_rowmode"/> is called before adding constraints.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_row.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row_no">The row number that must be changed.</param>
        /// <param name="count">Number of elements in <paramref name="row"/> and <paramref name="colno"/>.</param>
        /// <param name="row">An array with <paramref name="count"/> elements (or 1+<see cref="get_Ncolumns"/> elements if <paramref name="row"/> is <c>null</c>) that contains the values of the row.</param>
        /// <param name="colno">An array with <paramref name="count"/> elements that contains the column numbers of the row. However this variable can also be <c>null</c>.
        /// In that case element <c>i</c> in the variable row is column <c>i</c> and values start at element 1.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool set_rowex(int row_no, int count, double[] row, int[] colno)
        {
            return Interop.set_rowex(_lp, row_no, count, row, colno);
        }


        /// <summary>
        /// Sets the name of a constraint (row) in the model.
        /// <remarks>
        /// <para>The row must already exist. row 0 is the objective function. Row names are optional.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_row_name.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">The row for which the name must be set. Must be between 0 and the number of rows in the model.</param>
        /// <param name="new_name">The name for the constraint (row).</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool set_row_name(int row, string new_name)
        {
            return Interop.set_row_name(_lp, row, new_name);
        }

        /// <summary>
        /// Gets the name of a constraint (row) in the model.
        /// <remarks>
        /// <para>Row names are optional. If no row name was specified, the method returns Rx with x the row number. row 0 is the objective function.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_row_name.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">The row for which the name must be retrieved.
        /// Must be between 0 and the number of rows in the model. In <see cref="get_row_name"/>, row specifies the row number after presolve was done.
        /// In <see cref="get_origrow_name"/>, row specifies the row number before presolve was done, ie the original row number.</param>
        /// <returns><see cref="get_row_name"/> and <see cref="get_origrow_name"/> return the name of the specified row.
        /// A return value of <c>null</c> indicates an error.
        /// The difference between <see cref="get_row_name"/> and <see cref="get_origrow_name"/> is only visible when a presolve (<see cref="set_presolve"/>) was done.
        /// resolve can result in deletion of rows in the model. In <see cref="get_row_name"/>, row specifies the row number after presolve was done.
        /// In <see cref="get_origrow_name"/>, row specifies the row number before presolve was done, ie the original row number.
        /// If presolve is not active then both methods are equal.</returns>
        public string get_row_name(int row)
        {
            return Interop.get_row_name(_lp, row);
        }

        /// <summary>
        /// Gets the name of a constraint (row) in the model.
        /// <remarks>
        /// <para>Row names are optional. If no row name was specified, the method returns Rx with x the row number. row 0 is the objective function.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_origrow_name.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">The row for which the name must be retrieved.
        /// Must be between 0 and the number of rows in the model. In <see cref="get_row_name"/>, row specifies the row number after presolve was done.
        /// In <see cref="get_origrow_name"/>, row specifies the row number before presolve was done, ie the original row number.</param>
        /// <returns><see cref="get_row_name"/> and <see cref="get_origrow_name"/> return the name of the specified row.
        /// A return value of <c>null</c> indicates an error.
        /// The difference between <see cref="get_row_name"/> and <see cref="get_origrow_name"/> is only visible when a presolve (<see cref="set_presolve"/>) was done.
        /// resolve can result in deletion of rows in the model. In <see cref="get_row_name"/>, row specifies the row number after presolve was done.
        /// In <see cref="get_origrow_name"/>, row specifies the row number before presolve was done, ie the original row number.
        /// If presolve is not active then both methods are equal.</returns>
        public string get_origrow_name(int row)
        {
            return Interop.get_origrow_name(_lp, row);
        }

        /// <summary>
        /// Returns whether or not the constraint of the given row matches the given mask.
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_constr_type.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">The row for which the constraint type must be retrieved. Must be between 1 and number of rows in the model.</param>
        /// <param name="mask">The type of the constraint to check in <paramref name="row"/></param>
        /// <returns><c>true</c> if the containt types match, <c>false</c> otherwise.</returns>
        public bool is_constr_type(int row, lpsolve_constr_types mask)
        {
            return Interop.is_constr_type(_lp, row, mask);
        }

        /// <summary>
        /// Gets the type of a constraint.
        /// <remarks>The default constraint type is <see cref="lpsolve_constr_types.LE"/>.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_constr_type.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">The row for which the constraint type must be retrieved. Must be between 1 and number of rows in the model.</param>
        /// <returns>The type of the constraint on row <paramref name="row"/>.</returns>
        public lpsolve_constr_types get_constr_type(int row)
        {
            return Interop.get_constr_type(_lp, row);
        }

        /// <summary>
        /// Sets the type of a constraint for the specified row.
        /// <remarks>
        /// <para>The default constraint type is <see cref="lpsolve_constr_types.LE"/>.</para>
        /// <para>A free constraint (<see cref="lpsolve_constr_types.FR"/>) will act as if the constraint is not there.
        /// The lower bound is -Infinity and the upper bound is +Infinity.
        /// This can be used to temporary disable a constraint without having to delete it from the model
        /// .Note that the already set RHS and range on this constraint is overruled with Infinity by this.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_constr_type.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">The row for which the constraint type must be set. Must be between 1 and number of rows in the model.</param>
        /// <param name="con_type">The type of the constraint.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool set_constr_type(int row, lpsolve_constr_types con_type)
        {
            return Interop.set_constr_type(_lp, row, con_type);
        }


        /// <summary>
        /// Gets the value of the right hand side (RHS) vector (column 0) for one row.
        /// <remarks>
        /// <para>Note that row can also be 0 with this method. In that case it returns the initial value of the objective function.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_rh.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">The row number of the constraint on which the range must be retrieved. Must be between 1 and number of rows in the model.</param>
        /// <returns>The value of the RHS for the specified row. If row is out of range it returns 0. If no previous value was set, then it also returns 0, the default RHS value.</returns>
        public double get_rh(int row)
        {
            return Interop.get_rh(_lp, row);
        }

        /// <summary>
        /// Sets the value of the right hand side (RHS) vector (column 0) for the specified row.
        /// <remarks>
        /// <para>Note that row can also be 0 with this method.
        /// In that case an initial value for the objective value is set.
        /// methods <see cref="set_rh_vec"/>, <see cref="str_set_rh_vec"/> ignore row 0 (for historical reasons) in the specified RHS vector,
        /// but it is possible to first call one of these methods and then set the value of the objective with <see cref="set_rh"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">The row for which the RHS value must be set. Must be between 1 and number of rows in the model.</param>
        /// <param name="value">The value of the RHS.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool set_rh(int row, double value)
        {
            return Interop.set_rh(_lp, row, value);
        }


        /// <summary>
        /// Gets the range on the constraint (row) identified by <paramref name="row"/>.
        /// <remarks>
        /// <para>Setting a range on a row is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a range doesn't increase the model size that means that the model stays smaller and will be solved faster.</para>
        /// <para>If the row has a less than constraint then the range means setting a minimum on the constraint that is equal to the RHS value minus the range.
        /// If the row has a greater than constraint then the range means setting a maximum on the constraint that is equal to the RHS value plus the range.</para>
        /// <para>Note that the range value is the difference value and not the absolute value.</para>
        /// <para>If no range was set then <see cref="get_rh_range"/> returns a very big number, the value of <see cref="get_infinite"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh_range.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">The row number of the constraint on which the range must be retrieved. Must be between 1 and number of rows in the model.</param>
        /// <returns>The range on the constraint (row) identified by <paramref name="row"/>.</returns>
        public double get_rh_range(int row)
        {
            return Interop.get_rh_range(_lp, row);
        }

        /// <summary>
        /// Sets the range on the constraint (row) identified by <paramref name="row"/>.
        /// <remarks>
        /// <para>Setting a range on a row is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a range doesn't increase the model size that means that the model stays smaller and will be solved faster.</para>
        /// <para>If the row has a less than constraint then the range means setting a minimum on the constraint that is equal to the RHS value minus the range.
        /// If the row has a greater than constraint then the range means setting a maximum on the constraint that is equal to the RHS value plus the range.</para>
        /// <para>Note that the range value is the difference value and not the absolute value.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh_range.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">The row number of the constraint on which the range must be set. Must be between 1 and number of rows in the model.</param>
        /// <param name="deltavalue">The range on the constraint.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool set_rh_range(int row, double deltavalue)
        {
            return Interop.set_rh_range(_lp, row, deltavalue);
        }

        /// <summary>
        /// Sets the value of the right hand side (RHS) vector (column 0).
        /// <remarks>
        /// <para>The method sets all values of the RHS vector (column 0) at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Row 1 is element 1, row 2 is element 2, ...</para>
        /// <para>If the initial value of the objective function must also be set, use <see cref="set_rh"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh_vec.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="rh">An array with row elements that contains the values of the RHS.</param>
        public void set_rh_vec(double[] rh)
        {
            Interop.set_rh_vec(_lp, rh);
        }

        /// <summary>
        /// Sets the value of the right hand side (RHS) vector (column 0).
        /// <remarks>
        /// <para>The method sets all values of the RHS vector (column 0) at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Row 1 is element 1, row 2 is element 2, ...</para>
        /// <para>If the initial value of the objective function must also be set, use <see cref="set_rh"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh_vec.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="rh_string">A string with row elements that contains the values of the RHS. Each element must be separated by space(s).</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool str_set_rh_vec(string rh_string)
        {
            return Interop.str_set_rh_vec(_lp, rh_string);
        }

        #endregion

        #region Objective

        /// <summary>
        /// Sets the objective function (row 0) of the matrix.
        /// <remarks>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>It is better to use <see cref="set_obj_fnex"/> or <see cref="set_obj_fn"/>.</para>
        /// <para>Note that it is advised to set the objective function before adding rows via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj_fn.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number for which the value must be set.</param>
        /// <param name="value">The value that must be set.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool set_obj(int column, double value)
        {
            return Interop.set_obj(_lp, column, value);
        }

        /// <summary>
        /// Returns initial "at least better than" guess for objective function.
        /// <remarks>
        /// <para>This is only used in the branch-and-bound algorithm when integer variables exist in the model. All solutions with a worse objective value than this value are immediately rejected. This can result in faster solving times, but it can be difficult to predict what value to take for this bound. Also there is the chance that the found solution is not the most optimal one.</para>
        /// <para>The default is infinite.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_obj_bound.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns>Returns initial "at least better than" guess for objective function.</returns>
        public double get_obj_bound()
        {
            return Interop.get_obj_bound(_lp);
        }

        /// <summary>
        /// Sets initial "at least better than" guess for objective function.
        /// <remarks>
        /// <para>This is only used in the branch-and-bound algorithm when integer variables exist in the model.
        /// All solutions with a worse objective value than this value are immediately rejected.
        /// This can result in faster solving times, but it can be difficult to predict what value to take for this bound.
        /// Also there is the chance that the found solution is not the most optimal one.</para>
        /// <para>The default is infinite.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj_bound.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="obj_bound">The initial "at least better than" guess for objective function.</param>
        public void set_obj_bound(double obj_bound)
        {
            Interop.set_obj_bound(_lp, obj_bound);
        }

        /// <summary>
        /// Sets the objective function (row 0) of the matrix.
        /// <remarks>
        /// <para>This method set the values of the objective function in the model at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>It is almost always better to use <see cref="set_obj_fnex"/> instead of <see cref="set_obj_fn"/>. <see cref="set_obj_fnex"/> is always at least as performant as <see cref="set_obj_fnex"/>.</para>
        /// <para>It is more performant to call these methods than multiple times <see cref="set_obj"/>.</para>
        /// <para>Note that it is advised to set the objective function before adding rows via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj_fn.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">An array with 1+<see cref="get_Ncolumns"/> elements that contains the values of the objective function.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool set_obj_fn(double[] row)
        {
            return Interop.set_obj_fn(_lp, row);
        }

        /// <summary>
        /// Sets the objective function (row 0) of the matrix.
        /// <remarks>
        /// <para>This method set the values of the objective function in the model at once.</para>
        /// <para>Note that when <paramref name="colno"/> is <c>null</c> element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>This method has the possibility to specify only the non-zero elements. In that case colno specifies the column numbers of the non-zero elements.
        /// This will speed up building the model considerably if there are a lot of zero values. In most cases the matrix is sparse and has many zero value.</para>
        /// <para>It is almost always better to use <see cref="set_obj_fnex"/> instead of <see cref="set_obj_fn"/>. <see cref="set_obj_fnex"/> is always at least as performant as <see cref="set_obj_fnex"/>.</para>
        /// <para>It is more performant to call these methods than multiple times <see cref="set_obj"/>.</para>
        /// <para>Note that it is advised to set the objective function before adding rows via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj_fn.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="count">Number of elements in <paramref name="row"/> and <paramref name="colno"/>.</param>
        /// <param name="row">An array with <paramref name="count"/> elements (or 1+<see cref="get_Ncolumns"/> elements if <paramref name="row"/> is <c>null</c>) that contains the values of the row.</param>
        /// <param name="colno">An array with <paramref name="count"/> elements that contains the column numbers of the row. However this variable can also be <c>null</c>.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool set_obj_fnex(int count, double[] row, int[] colno)
        {
            return Interop.set_obj_fnex(_lp, count, row, colno);
        }

        /// <summary>
        /// Sets the objective function (row 0) of the matrix.
        /// <remarks>
        /// <para>This method set the values of the objective function in the model at once.</para>
        /// <para>This method should only be used in small or demo code since it is not performant and uses more memory than <see cref="set_obj_fnex"/> or <see cref="set_obj_fn"/></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj_fn.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row_string">A string with column elements that contains the values of the objective function. Each element must be separated by space(s).</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool str_set_obj_fn(string row_string)
        {
            return Interop.str_set_obj_fn(_lp, row_string);
        }

        /// <summary>
        /// Returns objective function direction.
        /// <remarks>
        /// The default of lp_solve is to minimize, except for <see cref="read_LP"/> where the default is to maximize.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_maxim.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns><c>true</c> if the objective function is maximize, <c>false</c> if it is minimize.</returns>
        public bool is_maxim()
        {
            return Interop.is_maxim(_lp);
        }

        /// <summary>
        /// Sets the objective function to <c>maximize</c>.
        /// <remarks>
        /// The default of lp_solve is to minimize, except for <see cref="read_LP"/> where the default is to maximize.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_maxim.htm">Full C API documentation.</seealso>
        /// </summary>
        public void set_maxim()
        {
            Interop.set_maxim(_lp);
        }

        /// <summary>
        /// Sets the objective function to <c>minimize</c>.
        /// <remarks>
        /// The default of lp_solve is to minimize, except for <see cref="read_LP"/> where the default is to maximize.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_minim.htm">Full C API documentation.</seealso>
        /// </summary>
        public void set_minim()
        {
            Interop.set_minim(_lp);
        }

        /// <summary>
        /// Sets the objective function sense.
        /// <remarks>
        /// The default of lp_solve is to minimize, except for <see cref="read_LP"/> where the default is to maximize.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_sense.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="maximize">When <c>true</c>, the objective function sense is maximize, when <c>false</c> it is minimize.</param>
        public void set_sense(bool maximize)
        {
            Interop.set_sense(_lp, maximize);
        }

        #endregion

        /// <summary>
        /// Gets the name of the model.
        /// <remarks>
        /// Giving the lp a name is optional. The default name is "Unnamed".
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_lp_name.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns>The name of the model.</returns>
        public string get_lp_name()
        {
            return Interop.get_lp_name(_lp);
        }

        /// <summary>
        /// Sets the name of the model.
        /// <remarks>
        /// Giving the lp a name is optional. The default name is "Unnamed".
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_lp_name.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="lpname">The name of the model.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool set_lp_name(string lpname)
        {
            return Interop.set_lp_name(_lp, lpname);
        }

        /// <summary>
        /// Allocates memory for the specified size.
        /// <remarks>
        /// <para>This method deletes the last rows/columns of the model if the new number of rows/columns is less than the number of rows/columns before the call.</para>
        /// <para>However, the function does **not** add rows/columns to the model if the new number of rows/columns is larger.
        /// It does however changes internal memory allocations to the new specified sizes.
        /// This to make the <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/> and <see cref="add_column"/>,
        /// <see cref="add_columnex"/>, <see cref="str_add_column"/> routines faster. Without <see cref="resize_lp"/>, these methods have to reallocated
        /// memory at each call for the new dimensions. However if <see cref="resize_lp "/> is used, then memory reallocation
        /// must be done only once resulting in better performance. So if the number of rows/columns that will be added is known in advance, then performance can be improved by using this function.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/resize_lp.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="rows">Allocates memory for this amount of rows.</param>
        /// <param name="columns">Allocates memory for this amount of columns.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        public bool resize_lp(int rows, int columns)
        {
            return Interop.resize_lp(_lp, rows, columns);
        }

        /// <summary>
        /// Returns a flag telling which of the add methods perform best. Whether <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/>
        /// or <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// <remarks>
        /// <para>Default, this is <c>false</c>, meaning that <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/> perform best.
        /// If the model is build via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/> calls,
        /// then these methods will be much faster if this method returns <c>true</c>.
        /// This is also called row entry mode. The speed improvement is spectacular, especially for bigger models, so it is 
        /// advisable to call this method to set the mode. Normally a model is build either column by column or row by row.</para>
        /// <para>Note that there are several restrictions with this mode:
        /// Only use this method after a <see cref="make_lp"/> call. Not when the model is read from file.
        /// Also, if this function is used, first add the objective function via <see cref="set_obj_fn"/>, <see cref="set_obj_fnex"/>, <see cref="str_set_obj_fn"/>
        /// and after that add the constraints via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// Don't call other API functions while in row entry mode.
        /// No other data matrix access is allowed while in row entry mode.
        /// After adding the contraints, turn row entry mode back off.
        /// Once turned of, you cannot switch back to row entry mode. So in short:<list type="bullet">
        /// <item>turn row entry mode on</item>
        /// <item>set the objective function</item>
        /// <item>create the constraints</item>
        /// <item>turn row entry mode off</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_add_rowmode.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns>If <c>false</c> then <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/>
        /// perform best. If <c>true</c> then <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>
        /// perform best.</returns>
        public bool is_add_rowmode()
        {
            return Interop.is_add_rowmode(_lp);
        }

        /// <summary>
        /// Specifies which add methods perform best. Whether <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/>
        /// or <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// <remarks>
        /// <para>Default, this is <c>false</c>, meaning that <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/> perform best.
        /// If the model is build via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/> calls,
        /// then these methods will be much faster if this method is called with <paramref name="turnon"/> set to <c>true</c>.
        /// This is also called row entry mode. The speed improvement is spectacular, especially for bigger models, so it is 
        /// advisable to call this method to set the mode. Normally a model is build either column by column or row by row.</para>
        /// <para>Note that there are several restrictions with this mode:
        /// Only use this method after a <see cref="make_lp"/> call. Not when the model is read from file.
        /// Also, if this function is used, first add the objective function via <see cref="set_obj_fn"/>, <see cref="set_obj_fnex"/>, <see cref="str_set_obj_fn"/>
        /// and after that add the constraints via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// Don't call other API functions while in row entry mode.
        /// No other data matrix access is allowed while in row entry mode.
        /// After adding the contraints, turn row entry mode back off.
        /// Once turned of, you cannot switch back to row entry mode. So in short:<list type="bullet">
        /// <item>turn row entry mode on</item>
        /// <item>set the objective function</item>
        /// <item>create the constraints</item>
        /// <item>turn row entry mode off</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_add_rowmode.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="turnon">If <c>false</c> then <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/>
        /// perform best. If <c>true</c> then <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>
        /// perform best.</param>
        /// <returns><c>true</c> if the rowmode was changed, <c>false</c> if the given mode was already set.</returns>
        public bool set_add_rowmode(bool turnon)
        {
            return Interop.set_add_rowmode(_lp, turnon);
        }

        /// <summary>
        /// Gets the index of a given column or row name in the model.
        /// <remarks>
        /// Note that this index starts from 1.
        /// Some API routines expect zero-based indexes and thus this value must then be corrected with -1.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_nameindex.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="name">The name of the column or row for which the index (column/row number) must be retrieved.</param>
        /// <param name="isrow">Use <c>false</c> when column information is needed and <c>true</c> when row information is needed.</param>
        /// <returns>Returns the index (column/row number) of the given column/row name.
        /// A return value of -1 indicates that the name does not exist.
        /// Note that the index is the original index number.
        /// So if presolve is active, it has no effect.
        /// It is the original column/row number that is returned.</returns>
        public int get_nameindex(string name, bool isrow)
        {
            return Interop.get_nameindex(_lp, name, isrow);
        }

        /// <summary>
        /// Returns the value of "infinite".
        /// <remarks>
        /// <para>This value is used for very big numbers. For example the upper bound of a variable without an upper bound.</para>
        /// <para>The default is 1e30</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_infinite.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns>Returns the practical value of "infinite".</returns>
        public double get_infinite()
        {
            return Interop.get_infinite(_lp);
        }

        /// <summary>
        /// Specifies the practical value of "infinite".
        /// <remarks>
        /// <para>This value is used for very big numbers. For example the upper bound of a variable without an upper bound.</para>
        /// <para>The default is 1e30</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_infinite.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="infinite">The value that must be used for "infinite".</param>
        public void set_infinite(double infinite)
        {
            Interop.set_infinite(_lp, infinite);
        }

        /// <summary>
        /// Checks if the provided absolute of the value is larger or equal to "infinite".
        /// <remarks>
        /// Note that the absolute of the provided value is checked against the value set by <see cref="set_infinite"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_infinite.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="value">The value to check against "infinite".</param>
        /// <returns><c>true</c> if the value is equal or larger to "infinite", <c>false</c> otherwise.</returns>
        public bool is_infinite(double value)
        {
            return Interop.is_infinite(_lp, value);
        }

        /// <summary>
        /// Gets a single element from the matrix.
        /// <remarks>
        /// <para>This function is not efficient if many values are to be retrieved.
        /// Consider to use <see cref="get_row"/>, <see cref="get_rowex"/>, <see cref="get_column"/>, <see cref="get_columnex"/>.</para>
        /// <para>
        /// If row and/or column are outside the allowed range, the function returns 0.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_mat.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">Row number of the matrix. Must be between 0 and number of rows in the model. Row 0 is objective function.</param>
        /// <param name="column">Column number of the matrix. Must be between 1 and number of columns in the model.</param>
        /// <returns>
        /// <para>Returns the value of the element on row <paramref name="row"/>, column <paramref name="column"/>.
        /// If no value was set for this element, the function returns 0.</para>
        /// <para>Note that row entry mode must be off, else this function also fails.
        /// See <see cref="set_add_rowmode"/>.</para></returns>
        public double get_mat(int row, int column)
        {
            return Interop.get_mat(_lp, row, column);
        }

        /// <summary>
        /// Sets a single element in the matrix.
        /// <remarks>
        /// <para>If there was already a value for this element, it is replaced and if there was no value, it is added.</para>
        /// <para>This function is not efficient if many values are to be set.
        /// Consider to use <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>,
        /// <see cref="set_row"/>, <see cref="set_rowex"/>, <see cref="set_obj_fn"/>, <see cref="set_obj_fnex"/>,
        /// <see cref="str_set_obj_fn"/>, <see cref="set_obj"/>, <see cref="add_column"/>, <see cref="add_columnex"/>,
        /// <see cref="str_add_column"/>, <see cref="set_column"/>, <see cref="set_columnex"/>.</para>
        /// <para>
        /// If row and/or column are outside the allowed range, the function returns 0.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_mat.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="row">Row number of the matrix. Must be between 0 and number of rows in the model. Row 0 is objective function.</param>
        /// <param name="column">Column number of the matrix. Must be between 1 and number of columns in the model.</param>
        /// <param name="value">Value to set on row <paramref name="row"/>, column <paramref name="column"/>.</param>
        /// <returns><para><c>true</c> if operation was successful, <c>false</c> otherwise.</para>
        /// <para>Note that row entry mode must be off, else this function also fails.
        /// See <see cref="set_add_rowmode"/>.</para></returns>
        public bool set_mat(int row, int column, double value)
        {
            return Interop.set_mat(_lp, row, column, value);
        }

        /// <summary>
        /// Specifies if set bounds may only be tighter or also less restrictive.
        /// <remarks>
        /// <para>If set to <c>true</c> then bounds may only be tighter.
        /// This means that when <see cref="set_lowbo"/> or <see cref="set_lowbo"/> is used to set a bound
        /// and the bound is less restrictive than an already set bound, then this new bound will be ignored.
        /// If tighten is set to <c>false</c>, the new bound is accepted.
        /// This functionality is useful when several bounds are set on a variable and at the end you want
        /// the most restrictive ones. By default, this setting is <c>false</c>.
        /// Note that this setting does not affect <see cref="set_bounds"/>.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bounds_tighter.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="tighten">Specifies if set bounds may only be tighter <c>true</c> or also less restrictive <c>false</c>.</param>
        public void set_bounds_tighter(bool tighten)
        {
            Interop.set_bounds_tighter(_lp, tighten);
        }

        /// <summary>
        /// Returns if set bounds may only be tighter or also less restrictive.
        /// <remarks>
        /// <para>If it returns <c>true</c> then bounds may only be tighter.
        /// This means that when <see cref="set_lowbo"/> or <see cref="set_lowbo"/> is used to set a bound
        /// and the bound is less restrictive than an already set bound, then this new bound will be ignored.
        /// If it returns <c>false</c>, the new bound is accepted.
        /// This functionality is useful when several bounds are set on a variable and at the end you want
        /// the most restrictive ones. By default, this setting is <c>false</c>.
        /// Note that this setting does not affect <see cref="set_bounds"/>.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_bounds_tighter.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns>Returns <c>true</c> if set bounds may only be tighter or <c>false</c> if they can also be less restrictive.</returns>
        public bool get_bounds_tighter()
        {
            return Interop.get_bounds_tighter(_lp);
        }

        /// <summary>
        /// Returns, for the specified variable, the priority the variable has in the branch-and-bound algorithm.
        /// <remarks>
        /// The method returns the priority the variable has in the branch-and-bound algorithm.
        /// This priority is determined by the weights set by <see cref="set_var_weights"/>.
        /// The default priorities are the column positions of the variables in the model.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_var_priority.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable on which the priority must be returned.
        /// It must be between 1 and the number of columns in the model. If it is not within this range, the return value is 0.</param>
        /// <returns>Returns the priority of the variable.</returns>
        public int get_var_priority(int column)
        {
            return Interop.get_var_priority(_lp, column);
        }

        /// <summary>
        /// Sets the weights on variables.
        /// <remarks>
        /// <para>The <see cref="set_var_weights"/> sets a weight factor on each variable. 
        /// This is only used for the integer variables in the branch-and-bound algorithm.
        /// Array members are unique, real-valued numbers indicating the "weight" of the variable at the given index.
        /// The array is 0-based. So variable 1 is at index 0, variable 2 at index 1. 
        /// The array must contain <see cref="get_Ncolumns"/> elements.</para>
        /// <para>The weights define which variable the branch-and-bound algorithm must select first.
        /// The lower the weight, the sooner the variable is chosen to make it integer.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_var_weights.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="weights">The weights array.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool set_var_weights(double[] weights)
        {
            return Interop.set_var_weights(_lp, weights);
        }

        /// <summary>
        /// Adds a SOS (Special Ordered Sets) constraint.
        /// <remarks>
        /// <para>The <see cref="add_SOS"/> function adds an SOS constraint.</para>
        /// <para>See <see href="http://lpsolve.sourceforge.net/5.5/SOS.htm">Special Ordered Sets</see> for a description about SOS variables.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_SOS.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="name">The name of the SOS constraint.</param>
        /// <param name="sostype">The type of the SOS constraint. Must be >= 1</param>
        /// <param name="priority">Priority of the SOS constraint in the SOS set.</param>
        /// <param name="count">The number of variables in the SOS list.</param>
        /// <param name="sosvars">An array specifying the <paramref name="count"/> variables (their column numbers).</param>
        /// <param name="weights">An array specifying the <paramref name="count"/> variable weights. May also be <c>null</c>.
        /// In that case, lp_solve will weight the variables in the order they are specified.</param>
        /// <returns>Returns the list index of the new SOS if the operation was successful.
        /// A return value of 0 indicates an error.</returns>
        public int add_SOS(string name, int sostype, int priority, int count, int[] sosvars, double[] weights)
        {
            return Interop.add_SOS(_lp, name, sostype, priority, count, sosvars, weights);
        }

        /// <summary>
        /// Returns if the variable is SOS (Special Ordered Set) or not.
        /// <remarks>
        /// <para>The <see cref="is_SOS_var "/> method returns if a variable is a SOS variable or not.
        /// Default a variable is not SOS. A variable becomes a SOS variable via <see cref="add_SOS"/>.</para>
        /// <para>See <see href="http://lpsolve.sourceforge.net/5.5/SOS.htm">Special Ordered Sets</see> for a description about SOS variables.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_SOS.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable that must be checked.
        /// It must be between 1 and the number of columns in the model.</param>
        /// <returns><c>true</c> if variable is a SOS var, <c>false</c> otherwise.</returns>
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

        /// <summary>
        /// Returns, for the specified variable, which branch to take first in branch-and-bound algorithm.
        /// <remarks>
        /// This method returns which branch to take first in branch-and-bound algorithm.
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.
        /// When no value was set via <see cref="set_var_branch"/>, the return value is the value of <see cref="get_bb_floorfirst"/>.
        /// It also returns the value of <see cref="get_bb_floorfirst"/> when <see cref="set_var_branch"/> was called with branch mode <see cref="lpsolve_branch.BRANCH_DEFAULT">BRANCH_DEFAULT (3)</see>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_var_branch.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable on which the mode must be returned.
        /// It must be between 1 and the number of columns in the model.
        /// If it is not within this range, the return value is the value of <see cref="get_bb_floorfirst"/>.</param>
        /// <returns>Returns which branch to take first in branch-and-bound algorithm.</returns>
        public lpsolve_branch get_var_branch(int column)
        {
            return Interop.get_var_branch(_lp, column);
        }

        /// <summary>
        /// Specifies, for the specified variable, which branch to take first in branch-and-bound algorithm.
        /// <remarks>
        /// <para>
        /// This method specifies which branch to take first in branch-and-bound algorithm.
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.
        /// </para>
        /// <para>The default is <see cref="lpsolve_branch.BRANCH_DEFAULT">BRANCH_DEFAULT (3)</see> which means that 
        /// the branch mode specified with <see cref="set_bb_floorfirst"/> function must be used.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_var_branch.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="column">The column number of the variable on which the mode must be set.
        /// It must be between 1 and the number of columns in the model.</param>
        /// <param name="branch_mode">Specifies, for the specified variable, which branch to take first in branch-and-bound algorithm.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool set_var_branch(int column, lpsolve_branch branch_mode)
        {
            return Interop.set_var_branch(_lp, column, branch_mode);
        }

        /// <summary>
        /// Returns whether the branch-and-bound algorithm stops at first found solution or not.
        /// <remarks>
        /// <para>The method returns if the branch-and-bound algorithm stops at the first found solution or not.
        /// Stopping at the first found solution can be useful if you are only interested for a solution,
        /// but not necessarily (and most probably) the most optimal solution.</para>
        /// <para>The default is <c>false</c>: not stop at first found solution.</para></remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_break_at_first.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns><c>true</c> if the branch-and-bound algorithm stops at first found solution, <c>false</c> otherwise.</returns>
        public bool is_break_at_first()
        {
            return Interop.is_break_at_first(_lp);
        }

        /// <summary>
        /// Specifies whether the branch-and-bound algorithm stops at first found solution or not.
        /// <remarks>
        /// <para>The method specifies if the branch-and-bound algorithm stops at the first found solution or not.
        /// Stopping at the first found solution can be useful if you are only interested for a solution,
        /// but not necessarily (and most probably) the most optimal solution.</para>
        /// <para>The default is <c>false</c>: not stop at first found solution.</para></remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_break_at_first.htm">Full C API documentation.</seealso>
        /// </summary>
        public void set_break_at_first(bool break_at_first)
        {
            Interop.set_break_at_first(_lp, break_at_first);
        }

        /// <summary>
        /// Returns the value which would cause the branch-and-bound algorithm to stop when the objective value reaches it.
        /// <remarks>
        /// <para>The method returns the value at which the branch-and-bound algorithm stops when the objective value
        /// is better than this value. Stopping at a given objective value can be useful if you are only interested
        /// for a solution that has an objective value which is at least a given value, but not necessarily
        /// (and most probably not) the most optimal solution.</para>
        /// <para>The default value is (-) infinity (or +infinity when maximizing).</para></remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_break_at_value.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns>Returns the value to break on.</returns>
        public double get_break_at_value()
        {
            return Interop.get_break_at_value(_lp);
        }

        /// <summary>
        /// Specifies the value which would cause the branch-and-bound algorithm to stop when the objective value reaches it.
        /// <remarks>
        /// <para>The method sets the value at which the branch-and-bound algorithm stops when the objective value
        /// is better than this value. Stopping at a given objective value can be useful if you are only interested
        /// for a solution that has an objective value which is at least a given value, but not necessarily
        /// (and most probably not) the most optimal solution.</para>
        /// <para>The default value is (-) infinity (or +infinity when maximizing).</para></remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_break_at_value.htm">Full C API documentation.</seealso>
        /// </summary>
        public void set_break_at_value(double break_at_value)
        {
            Interop.set_break_at_value(_lp, break_at_value);
        }

        /// <summary>
        /// Returns the branch-and-bound rule.
        /// <remarks>
        /// <para>The method returns the branch-and-bound rule for choosing which non-integer variable is to be selected.
        /// This rule can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is NODE_PSEUDONONINTSELECT + NODE_GREEDYMODE + NODE_DYNAMICMODE + NODE_RCOSTFIXING(17445).</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_bb_rule.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns>Returns the <see cref="lpsolve_BBstrategies">branch-and-bound rule</see>.</returns>
        public lpsolve_BBstrategies get_bb_rule()
        {
            return Interop.get_bb_rule(_lp);
        }

        /// <summary>
        /// Specifies the branch-and-bound rule.
        /// <remarks>
        /// <para>The method specifies the branch-and-bound rule for choosing which non-integer variable is to be selected.
        /// This rule can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is NODE_PSEUDONONINTSELECT + NODE_GREEDYMODE + NODE_DYNAMICMODE + NODE_RCOSTFIXING(17445).</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bb_rule.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="bb_rule">The <see cref="lpsolve_BBstrategies">branch-and-bound rule</see> to set.</param>
        public void set_bb_rule(lpsolve_BBstrategies bb_rule)
        {
            Interop.set_bb_rule(_lp, bb_rule);
        }

        /// <summary>
        /// Returns the maximum branch-and-bound depth.
        /// <remarks>
        /// <para>The method returns the maximum branch-and-bound depth.</para>
        /// <para>This is only useful if there are integer, semi-continuous or SOS variables in the model so that the
        /// branch-and-bound algorithm must be used to solve them.
        /// The branch-and-bound algorithm will not go deeper than this level.
        /// When 0 then there is no limit to the depth.
        /// Limiting the depth will speed up solving time, but there is a chance that the found solution is not the most optimal one.
        /// Be aware of this. It can also result in not finding a solution at all.
        /// A positive value means that the depth is absolute.
        /// A negative value means a relative B&amp;B depth limit.
        /// The "order" of a MIP problem is defined to be 2x the number of binary variables plus the number of SC and SOS variables.
        /// A relative value of -x results in a maximum depth of x times the order of the MIP problem.</para>
        /// <para>The default is -50.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_bb_depthlimit.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns>Returns the maximum branch-and-bound depth</returns>
        public int get_bb_depthlimit()
        {
            return Interop.get_bb_depthlimit(_lp);
        }

        /// <summary>
        /// Sets the maximum branch-and-bound depth.
        /// <remarks>
        /// <para>The method specifies the maximum branch-and-bound depth.</para>
        /// <para>This is only useful if there are integer, semi-continuous or SOS variables in the model so that the
        /// branch-and-bound algorithm must be used to solve them.
        /// The branch-and-bound algorithm will not go deeper than this level.
        /// When 0 then there is no limit to the depth.
        /// Limiting the depth will speed up solving time, but there is a chance that the found solution is not the most optimal one.
        /// Be aware of this. It can also result in not finding a solution at all.</para>
        /// <para>The default is -50.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bb_depthlimit.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="bb_maxlevel">Specifies the maximum branch-and-bound depth.
        /// A positive value means that the depth is absoluut.
        /// A negative value means a relative B&amp;B depth limit.
        /// The "order" of a MIP problem is defined to be 2x the number of binary variables plus the number of SC and SOS variables.
        /// A relative value of -x results in a maximum depth of x times the order of the MIP problem.</param>
        public void set_bb_depthlimit(int bb_maxlevel)
        {
            Interop.set_bb_depthlimit(_lp, bb_maxlevel);
        }

        /// <summary>
        /// Returns which branch to take first in branch-and-bound algorithm.
        /// <remarks>
        /// <para>The method returns which branch to take first in branch-and-bound algorithm.
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is <see cref="lpsolve_branch.BRANCH_AUTOMATIC"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_bb_floorfirst.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <returns>Returns which branch to take first in branch-and-bound algorithm.</returns>
        public lpsolve_branch get_bb_floorfirst()
        {
            return Interop.get_bb_floorfirst(_lp);
        }

        /// <summary>
        /// Specifies which branch to take first in branch-and-bound algorithm.
        /// <remarks>
        /// <para>The method specifies which branch to take first in branch-and-bound algorithm.
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is <see cref="lpsolve_branch.BRANCH_AUTOMATIC"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_bb_floorfirst.htm">Full C API documentation.</seealso>
        /// </summary>
        /// <param name="bb_floorfirst">Specifies which branch to take first in branch-and-bound algorithm.</param>
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

        public lpsolve_verbosity get_verbose()
        {
            return Interop.get_verbose(_lp);
        }

        public void set_verbose(lpsolve_verbosity verbose)
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
