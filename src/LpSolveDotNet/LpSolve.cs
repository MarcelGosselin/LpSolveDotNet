#if NETCOREAPP3_0 || NET471 || NETSTANDARD2_0 || NETSTANDARD1_5
    #define SUPPORTS_OS_PLATFORM
#endif
#if NETCOREAPP3_0
    #define SUPPORTS_NATIVELIBRARY
    #define SUPPORTS_ASSEMBLYLOADCONTEXT_RESOLVINGUNMANAGEDDLL
#endif

using System;
using System.IO;
using System.Reflection;
#if SUPPORTS_OS_PLATFORM || SUPPORTS_NATIVELIBRARY
using System.Runtime.InteropServices;
#endif
#if SUPPORTS_ASSEMBLYLOADCONTEXT_RESOLVINGUNMANAGEDDLL
using System.Runtime.Loader;
#endif

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
    /// </summary>
    /// <remarks>
    /// You must call the method <see cref="Init"/> once before calling any other method
    /// in order to make sure the native lpsolve library will be loaded from the right location.
    /// </remarks>
    public sealed class LpSolve
        : IDisposable
    {
        #region Library initialization

        /// <summary>
        /// Initializes the library by making sure the correct native library will be loaded.
        /// <remarks>
        /// If you use this method with a platform other than .NET framework or for a version of .NET Core before 3.0,
        /// you <strong>must</strong> either:<list>
        /// <item>provide a value for <see cref="CustomLoadNativeLibrary"/></item>
        /// <item>put the native library in a place where the runtime will pick it up.</item>
        /// </list> 
        /// </remarks>
        /// </summary>
        /// <param name="nativeLibraryFolderPath">The (optional) folder where the native library is located.
        /// When <paramref name="nativeLibraryFolderPath"/> is <c>null</c>, it will infer one of
        /// <list type="bullet">
        /// <item>basedir/NativeBinaries/win-x64</item>
        /// <item>basedir/NativeBinaries/win-x86</item>
        /// <item>basedir/NativeBinaries/linux-x64</item>
        /// <item>basedir/NativeBinaries/linux-x86</item>
        /// <item>basedir/NativeBinaries/osx-x64</item>
        /// <item>basedir/NativeBinaries/osx-x86</item>
        /// </list>
        /// </param>
        /// <param name="nativeLibraryNamePrefix">The prefix for the native library file name. It defaults to <c>null</c> but when left <c>null</c>,
        /// it will be inferred by <see href="https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.osplatform">OSPlatform</see>.
        /// On Linux, OSX and other Unixes it will be <c>lib</c>, and on Windows it will be empty string.</param>
        /// <param name="nativeLibraryExtension">The file extension for the native library (you must include the dot). 
        /// It defaults to <c>null</c> but when left <c>null</c>, it will be inferred by <see href="https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.osplatform">OSPlatform</see>
        /// to <c>.dll</c> on Windows, <c>.so</c> on Unix and <c>.dylib</c> on OSX.</param>
        /// <returns><c>true</c>, if it found the native library, <c>false</c> otherwise.</returns>
        public static bool Init(string nativeLibraryFolderPath = null, string nativeLibraryNamePrefix = null, string nativeLibraryExtension = null)
        {
            if (string.IsNullOrEmpty(nativeLibraryFolderPath))
            {
                Assembly thisAssembly = typeof(LpSolve)
#if NETSTANDARD1_5
                    .GetTypeInfo()
#endif
                    .Assembly;
                string baseDirectory = Path.GetDirectoryName(new Uri(thisAssembly.CodeBase).LocalPath);
                string subFolder = GetFolderNameByOSAndArchitecture();
                nativeLibraryFolderPath = Path.Combine(Path.Combine(baseDirectory, "NativeBinaries"), subFolder);
            }

            if (nativeLibraryFolderPath.EndsWith(Path.DirectorySeparatorChar.ToString())
                || nativeLibraryFolderPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                // remove trailing slash for use in PATH environment variable
                nativeLibraryFolderPath = nativeLibraryFolderPath.Substring(0, nativeLibraryFolderPath.Length - 1);
            }

            nativeLibraryNamePrefix ??= GetLibraryNamePrefix();
            nativeLibraryExtension ??= GetLibraryExtension();

            var nativeLibraryFileName = nativeLibraryNamePrefix + Interop.LibraryName + nativeLibraryExtension;
            var nativeLibraryFilePath = Path.Combine(nativeLibraryFolderPath + Path.DirectorySeparatorChar, nativeLibraryFileName);

            bool returnValue = File.Exists(nativeLibraryFilePath);
            if (returnValue)
            {
                var customLoadNativeLibrary = CustomLoadNativeLibrary;
                if (customLoadNativeLibrary != null)
                {
                    customLoadNativeLibrary(nativeLibraryFilePath);
                }
                else
                {
                    LoadNativeLibrary(nativeLibraryFilePath);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// When not <c>null</c>, defines a method which receives the expected path of the native library
        /// and will tell the runtime to load it.
        /// </summary>
        public static Action<string> CustomLoadNativeLibrary { get; set; }

#if NET20 // That was Windows only 

        private static string GetFolderNameByOSAndArchitecture() => IntPtr.Size == 8
            ? "win-x64"
            : "win-x86";

        private static string GetLibraryNamePrefix() => "";

        private static string GetLibraryExtension() => ".dll";

#elif SUPPORTS_OS_PLATFORM

        private static string GetFolderNameByOSAndArchitecture()
        {
            string arch = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"win-{arch}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return $"linux-{arch}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return $"osx-{arch}";
            }
            else
            {
                return "UNKNOWN";
            }
        }

        private static string GetLibraryNamePrefix()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "lib";
            }
            return null;
        }

        private static string GetLibraryExtension()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ".dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return ".dylib";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return ".so";
            }
            return null;
        }
#endif

#if SUPPORTS_NATIVELIBRARY && SUPPORTS_ASSEMBLYLOADCONTEXT_RESOLVINGUNMANAGEDDLL

        private static void LoadNativeLibrary(string nativeLibraryFilePath)
        {
            if (_nativeLibraryFilePath == null)
            {
                Assembly thisAssembly = typeof(LpSolve).Assembly;
                var loadContext = AssemblyLoadContext.GetLoadContext(thisAssembly) ?? AssemblyLoadContext.Default;
                loadContext.ResolvingUnmanagedDll += ResolvingLpSolveUnmanagedDll;
            }
            _nativeLibraryFilePath = nativeLibraryFilePath;

            static IntPtr ResolvingLpSolveUnmanagedDll(Assembly arg1, string arg2)
            {
                if (arg2 == Interop.LibraryName)
                {
                    return NativeLibrary.Load(_nativeLibraryFilePath);
                }
                return IntPtr.Zero;
            }
        }
        private static string _nativeLibraryFilePath;

#elif NET20 || NET471

        private static void LoadNativeLibrary(string nativeLibraryFilePath)
        {
            if (!_hasAlreadyChangedPathEnvironmentVariable)
            {
                string pathEnvironmentVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
                string pathWithSeparator = pathEnvironmentVariable + Path.PathSeparator;
                string nativeLibraryFolderPath = Path.GetDirectoryName(nativeLibraryFilePath);

                if (pathWithSeparator.IndexOf(nativeLibraryFolderPath + Path.PathSeparator) < 0)
                {
                    Environment.SetEnvironmentVariable(
                        "PATH",
                        nativeLibraryFolderPath + Path.PathSeparator + pathEnvironmentVariable,
                        EnvironmentVariableTarget.Process);
                }
                _hasAlreadyChangedPathEnvironmentVariable = true;
            }
        }
        
        private static bool _hasAlreadyChangedPathEnvironmentVariable;

#else

        private static void LoadNativeLibrary(string nativeLibraryFilePath)
        {
            // Nothing to do, hopefully  CustomLoadNativeLibrary handled it
        }

#endif

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
        /// </summary>
        /// <param name="rows">Initial number of rows. Can be <c>0</c> as new rows can be added via 
        /// <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.</param>
        /// <param name="columns">Initial number of columns. Can be <c>0</c> as new columns can be added via
        /// <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/>.</param>
        /// <returns>A new <see cref="LpSolve"/> model with <paramref name="rows"/> rows and <paramref name="columns"/> columns.
        /// A <c>null</c> return value indicates an error. Specifically not enough memory available to setup an lprec structure.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/make_lp.htm">Full C API documentation.</seealso>
        public static LpSolve make_lp(int rows, int columns)
        {
            IntPtr lp = Interop.make_lp(rows, columns);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Creates and initialises a new <see cref="LpSolve"/> model from a LP model file.
        /// </summary>
        /// <param name="fileName">Filename to read the LP model from.</param>
        /// <param name="verbose">The verbose level. See also <see cref="set_verbose"/> and <see cref="get_verbose"/>.</param>
        /// <param name="lpName">Initial name of the model. May be <c>null</c> if the model has no name. See also <see cref="set_lp_name"/> and <see cref="get_lp_name"/>.</param>
        /// <returns>A new <see cref="LpSolve"/> model matching the one in the file.
        /// A <c>null</c> return value indicates an error. Specifically file could not be opened, has wrong structure or not enough memory is available.</returns>
        /// <remarks>The model in the file must be in <see href="http://lpsolve.sourceforge.net/5.5/lp-format.htm">lp-format</see>.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_lp.htm">Full C API documentation.</seealso>
        public static LpSolve read_LP(string fileName, lpsolve_verbosity verbose, string lpName)
        {
            IntPtr lp = Interop.read_LP(fileName, verbose, lpName);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Creates and initialises a new <see cref="LpSolve"/> model from an MPS model file.
        /// </summary>
        /// <param name="fileName">Filename to read the MPS model from.</param>
        /// <param name="verbose">Specifies the verbose level. See also <see cref="set_verbose"/> and <see cref="get_verbose"/>.</param>
        /// <param name="options">Specifies how to interprete the MPS layout. You can use multiple values.</param>
        /// <returns>A new <see cref="LpSolve"/> model matching the one in the file.
        /// A <c>null</c> return value indicates an error. Specifically file could not be opened, has wrong structure or not enough memory is available.</returns>
        /// <remarks>The model in the file must be in <see href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">mps-format</see>.</remarks>
        /// <para>This method is different from C API because <paramref name="verbose"/> is separate from <paramref name="options"/></para>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_mps.htm">Full C API documentation.</seealso>
        public static LpSolve read_MPS(string fileName, lpsolve_verbosity verbose, lpsolve_mps_options options)
        {
            IntPtr lp = Interop.read_MPS(fileName, ((int)verbose)|((int)options));
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Creates and initialises a new <see cref="LpSolve"/> model via the eXternal Language Interface.
        /// </summary>
        /// <param name="xliName">Filename of the XLI package.</param>
        /// <param name="modelName">Filename to read the model from.</param>
        /// <param name="dataName">Filename to read the data from. This may be optional. In that case, set the parameter to <c>null</c>.</param>
        /// <param name="options">Extra options that can be used by the reader.</param>
        /// <param name="verbose">The verbose level. See also <see cref="set_verbose"/> and <see cref="get_verbose"/>.</param>
        /// <returns>A new <see cref="LpSolve"/> model matching the one in the file.
        /// A <c>null</c> return value indicates an error.</returns>
        /// <remarks>The method constructs a new <see cref="LpSolve"/> model by reading model from <paramref name="modelName"/> via the specified XLI.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/XLI.htm">External Language Interfaces</see>for a complete description on XLIs.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_XLI.htm">Full C API documentation.</seealso>
        public static LpSolve read_XLI(string xliName, string modelName, string dataName, string options, lpsolve_verbosity verbose)
        {
            IntPtr lp = Interop.read_XLI(xliName, modelName, dataName, options, verbose);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Copies current model to a new one.
        /// </summary>
        /// <returns>A new model with the same values as current one or <c>null</c> if an error occurs (not enough memory).</returns>
        /// <remarks>The new model is independent from the original one.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/copy_lp.htm">Full C API documentation.</seealso>
        public LpSolve copy_lp()
        {
            IntPtr lp = Interop.copy_lp(_lp);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Frees all memory allocated to the model.
        /// </summary>
        /// <remarks>You don't need to call this method, the <see cref="IDisposable"/> implementation does it for you.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/delete_lp.htm">Full C API documentation.</seealso>
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
        /// </summary>
        /// <param name="column">An array with 1+<see cref="get_Nrows"/> elements that contains the values of the column.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks><para>The method adds a column to the model (at the end) and sets all values of the column at once.</para>
        /// <para>Note that element 0 of the array is the value of the objective function for that column. Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>It is almost always better to use <see cref="add_columnex"/> instead of <see cref="add_column"/>. <see cref="add_columnex"/> is always at least as performant as <see cref="add_column"/>.</para>
        /// <para>Note that if you have to add many columns, performance can be improved by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_column.htm">Full C API documentation.</seealso>
        public bool add_column(double[] column)
        {
            return Interop.add_column(_lp, column);
        }

        /// <summary>
        /// Adds a column to the model.
        /// </summary>
        /// <param name="count">Number of elements in <paramref name="column"/> and <paramref name="rowno"/>.</param>
        /// <param name="column">An array with <paramref name="count"/> elements that contains the values of the column.</param>
        /// <param name="rowno">A zero-based array with <paramref name="count"/> elements that contains the row numbers of the column. 
        /// However this variable can also be <c>null</c>. In that case element i in the variable <paramref name="column"/> is row i.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
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
        public bool add_columnex(int count, double[] column, int[] rowno)
        {
            return Interop.add_columnex(_lp, count, column, rowno);
        }

        /// <summary>
        /// Adds a column to the model.
        /// </summary>
        /// <param name="col_string">A string with row elements that contains the values of the column. Each element must be separated by space(s).</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>This should only be used in small or demo code since it is not performant and uses more memory.
        /// Instead use <see cref="add_columnex"/> or <see cref="add_column"/>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_column.htm">Full C API documentation.</seealso>
        public bool str_add_column(string col_string)
        {
            return Interop.str_add_column(_lp, col_string);
        }

        /// <summary>
        /// Deletes a column from the model.
        /// </summary>
        /// <param name="column">The column to delete. Must be between <c>1</c> and the number of columns in the model.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise. An error occurs if <paramref name="column"/> 
        /// is not between <c>1</c> and the number of columns in the model</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="set_add_rowmode"/>.</para>
        /// <para>The column is effectively deleted from the model, so all columns after this column shift one left.</para>
        /// <para>Note that column 0 (the right hand side (RHS)) cannot be deleted. There is always a RHS.</para>
        /// <para>Note that you can also delete multiple columns by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/del_column.htm">Full C API documentation.</seealso>
        public bool del_column(int column)
        {
            return Interop.del_column(_lp, column);
        }


        /// <summary>
        /// Sets a column in the model.
        /// </summary>
        /// <param name="col_no">The column number that must be changed.</param>
        /// <param name="column">An array with 1+<see cref="get_Nrows"/> elements that contains the values of the column.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The method changes the values of an existing column in the model at once.</para>
        /// <para>Note that element 0 of the array is row 0 (objective function). element 1 is row 1, ...</para>
        /// <para>It is almost always better to use <see cref="set_columnex"/> instead of <see cref="set_column"/>. <see cref="set_columnex"/> is always at least as performant as <see cref="set_column"/>.</para>
        /// <para>It is more performant to call this method than call <see cref="set_mat"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_column.htm">Full C API documentation.</seealso>
        public bool set_column(int col_no, double[] column)
        {
            return Interop.set_column(_lp, col_no, column);
        }

        /// <summary>
        /// Sets a column in the model.
        /// </summary>
        /// <param name="col_no">The column number that must be changed.</param>
        /// <param name="count">Number of elements in <paramref name="column"/> and <paramref name="rowno"/>.</param>
        /// <param name="column">An array with <paramref name="count"/> elements that contains the values of the column.</param>
        /// <param name="rowno">A zero-based array with <paramref name="count"/> elements that contains the row numbers of the column. 
        /// However this variable can also be <c>null</c>. In that case element i in the variable <paramref name="column"/> is row i.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
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
        public bool set_columnex(int col_no, int count, double[] column, int[] rowno)
        {
            return Interop.set_columnex(_lp, col_no, count, column, rowno);
        }

        /// <summary>
        /// Gets all column elements from the model for the given <paramref name="col_nr"/>.
        /// </summary>
        /// <param name="col_nr">The column number of the matrix. Must be between 1 and number of columns in the model.</param>
        /// <param name="column">Array in which the values are returned. The array must be dimensioned with at least 1+<see cref="get_Nrows"/> elements.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="set_add_rowmode"/>.</para>
        /// <para>Note that element 0 of the array is row 0 (objective function). element 1 is row 1, ...</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_column.htm">Full C API documentation.</seealso>
        public bool get_column(int col_nr, double[] column)
        {
            return Interop.get_column(_lp, col_nr, column);
        }

        /// <summary>
        /// Gets the non-zero column elements from the model for the given <paramref name="col_nr"/>.
        /// </summary>
        /// <param name="col_nr">The column number of the matrix. Must be between 1 and number of columns in the model.</param>
        /// <param name="column">Array in which the values are returned. The array must be dimensioned with at least the number of non-zero elements in the column.
        /// If that is unknown, then use 1+<see cref="get_Nrows"/>.</param>
        /// <param name="nzrow">Array in which the row numbers  are returned. The array must be dimensioned with at least the number of non-zero elements in the column.
        /// If that is unknown, then use 1+<see cref="get_Nrows"/>.</param>
        /// <returns>The number of non-zero elements returned in <paramref name="column"/> and <paramref name="nzrow"/>.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="set_add_rowmode"/>.</para>
        /// <para>Returned values in <paramref name="column"/> and <paramref name="nzrow"/> start from element 0.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_column.htm">Full C API documentation.</seealso>
        public int get_columnex(int col_nr, double[] column, int[] nzrow)
        {
            return Interop.get_columnex(_lp, col_nr, column, nzrow);
        }

        /// <summary>
        /// Sets the name of a column in the model.
        /// </summary>
        /// <param name="column">The column for which the name must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="new_name">The name for the column.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// The column must already exist.
        /// Column names are optional.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_col_name.htm">Full C API documentation.</seealso>
        public bool set_col_name(int column, string new_name)
        {
            return Interop.set_col_name(_lp, column, new_name);
        }

        /// <summary>
        /// Gets the name of a column in the model.
        /// </summary>
        /// <param name="column">The column for which the name must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <returns>The name of the specified column if it was specified, Cx with x the column number otherwise or <c>null</c> on error.</returns>
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
        public string get_col_name(int column)
        {
            return Interop.get_col_name(_lp, column);
        }

        /// <summary>
        /// Gets the name of a column in the model.
        /// </summary>
        /// <param name="column">The column for which the name must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <returns>The name of the specified column if it was specified, Cx with x the column number otherwise or <c>null</c> on error.</returns>
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
        public string get_origcol_name(int column)
        {
            return Interop.get_origcol_name(_lp, column);
        }

        /// <summary>
        /// Returns whether the variable is negative or not.
        /// </summary>
        /// <param name="column">The column number of the variable to check. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is defined as negative, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Negative means a lower and upper bound that are both negative.
        /// By default a variable is not negative because it has a lower bound of 0 (and an upper bound of +infinity).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_negative.htm">Full C API documentation.</seealso>
        public bool is_negative(int column)
        {
            return Interop.is_negative(_lp, column);
        }

        /// <summary>
        /// Returns whether the variable is of type Integer or not.
        /// </summary>
        /// <param name="column">The column number of the variable to check. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is defined as integer, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Default a variable is not integer. From the moment there is at least one integer variable in the model,
        /// the Branch and Bound algorithm is used to make these variables integer.
        /// Note that solving times can be considerably larger when there are integer variables.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/integer.htm">integer variables</see> for a description about integer variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_int.htm">Full C API documentation.</seealso>
        public bool is_int(int column)
        {
            return Interop.is_int(_lp, column);
        }

        /// <summary>
        /// Sets the type of the variable to type Integer or floating point.
        /// </summary>
        /// <param name="column">The column number of the variable that must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="must_be_int"><c>true</c> if variable must be an integer, <c>false</c> otherwise.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Default a variable is not integer. The argument <paramref name="must_be_int"/> defines what the status of the variable becomes.
        /// From the moment there is at least one integer variable in the model,
        /// the Branch and Bound algorithm is used to make these variables integer.
        /// Note that solving times can be considerably larger when there are integer variables.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/integer.htm">integer variables</see> for a description about integer variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_int.htm">Full C API documentation.</seealso>
        public bool set_int(int column, bool must_be_int)
        {
            return Interop.set_int(_lp, column, must_be_int);
        }

        /// <summary>
        /// Returns whether the variable is of type Binary or not.
        /// </summary>
        /// <param name="column">The column number of the variable to check. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is defined as binary, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Default a variable is not binary. A binary variable is an integer variable with lower bound 0 and upper bound 1.
        /// From the moment there is at least one integer variable in the model,
        /// the Branch and Bound algorithm is used to make these variables integer.
        /// Note that solving times can be considerably larger when there are integer variables.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/integer.htm">integer variables</see> for a description about integer variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_binary.htm">Full C API documentation.</seealso>
        public bool is_binary(int column)
        {
            return Interop.is_binary(_lp, column);
        }

        /// <summary>
        /// Sets the type of the variable to type Binary or floating point.
        /// </summary>
        /// <param name="column">The column number of the variable that must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="must_be_bin"><c>true</c> if variable must be a binary, <c>false</c> otherwise.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
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
        public bool set_binary(int column, bool must_be_bin)
        {
            return Interop.set_binary(_lp, column, must_be_bin);
        }

        /// <summary>
        /// Returns whether the variable is of type semi-continuous or not.
        /// </summary>
        /// <param name="column">The column number of the variable to check. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is defined as semi-continuous, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Default a variable is not semi-continuous.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/semi-cont.htm">semi-continuous variables</see> for a description about semi-continuous variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_semicont.htm">Full C API documentation.</seealso>
        public bool is_semicont(int column)
        {
            return Interop.is_semicont(_lp, column);
        }

        /// <summary>
        /// Sets the type of the variable to type semi-continuous or not.
        /// </summary>
        /// <param name="column">The column number of the variable that must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="must_be_sc"><c>true</c> if variable must be a semi-continuous, <c>false</c> otherwise.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// By default, a variable is not semi-continuous. The argument <paramref name="must_be_sc"/> defines what the status of the variable becomes.
        /// Note that a semi-continuous variable must also have a lower bound to have effect.
        /// This because the default lower bound on variables is zero, also when defined as semi-continuous, and without
        /// a lower bound it has no point to define a variable as such.
        /// The lower bound may be set before or after setting the semi-continuous status.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/semi-cont.htm">semi-continuous variables</see> for a description about semi-continuous variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_semicont.htm">Full C API documentation.</seealso>
        public bool set_semicont(int column, bool must_be_sc)
        {
            return Interop.set_semicont(_lp, column, must_be_sc);
        }

        /// <summary>
        /// Sets the lower and upper bound of a variable.
        /// </summary>
        /// <param name="column">The column number of the variable on which the bounds must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="lower">The lower bound on the variable identified by <paramref name="column"/>.</param>
        /// <param name="upper">The upper bound on the variable identified by <paramref name="column"/>.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// Note that the default lower bound of each variable is 0.
        /// So variables will never take negative values if no negative lower bound is set.
        /// The default upper bound of a variable is infinity(well not quite.It is a very big number, the value of <see cref="get_infinite"/>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bounds.htm">Full C API documentation.</seealso>
        public bool set_bounds(int column, double lower, double upper)
        {
            return Interop.set_bounds(_lp, column, lower, upper);
        }

        /// <summary>
        /// Sets if the variable is free.
        /// </summary>
        /// <param name="column">The column number of the variable that must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>
        /// By default a variable is not free because it has a lower bound of 0 (and an upper bound of +infinity).
        /// </para>
        /// <para>
        /// Free means a lower bound of -infinity and an upper bound of +infinity.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/free.htm">free variables</see> for a description about free variables.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_unbounded.htm">Full C API documentation.</seealso>
        public bool set_unbounded(int column)
        {
            return Interop.set_unbounded(_lp, column);
        }

        /// <summary>
        /// Returns whether the variable is free or not.
        /// </summary>
        /// <param name="column">The column number of the variable to check. Must be between 1 and the number of columns in the lp.</param>
        /// <returns><c>true</c> if variable is defined as free, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Free means a lower bound of -infinity and an upper bound of +infinity.
        /// Default a variable is not free because default it has a lower bound of 0 (and an upper bound of +infinity).
        /// See <see href="http://lpsolve.sourceforge.net/5.5/free.htm">free variables</see> for a description about free variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_unbounded.htm">Full C API documentation.</seealso>
        public bool is_unbounded(int column)
        {
            return Interop.is_unbounded(_lp, column);
        }

        /// <summary>
        /// Gets the upper bound of a variable.
        /// </summary>
        /// <param name="column">The column number of the variable. Must be between 1 and the number of columns in the lp.</param>
        /// <returns>The upper bound on the specified variable. If no bound was set, it returns a very big number, 
        /// the value of <see cref="get_infinite"/>, the default upper bound</returns>
        /// <remarks>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// The default upper bound of a variable is infinity(well not quite.It is a very big number, the value of <see cref="get_infinite"/>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_upbo.htm">Full C API documentation.</seealso>
        public double get_upbo(int column)
        {
            return Interop.get_upbo(_lp, column);
        }

        /// <summary>
        /// Sets the upper bound of a variable.
        /// </summary>
        /// <param name="column">The column number of the variable on which the bound must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="value">The upper bound on the variable identified by <paramref name="column"/>.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// The default upper bound of a variable is infinity(well not quite.It is a very big number, the value of <see cref="get_infinite"/>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_upbo.htm">Full C API documentation.</seealso>
        public bool set_upbo(int column, double value)
        {
            return Interop.set_upbo(_lp, column, value);
        }

        /// <summary>
        /// Gets the lower bound of a variable.
        /// </summary>
        /// <param name="column">The column number of the variable. Must be between 1 and the number of columns in the lp.</param>
        /// <returns>The lower bound on the specified variable. If no bound was set, it returns 0, the default lower bound.</returns>
        /// <remarks>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// Note that the default lower bound of each variable is 0.
        /// So variables will never take negative values if no negative lower bound is set.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_lowbo.htm">Full C API documentation.</seealso>
        public double get_lowbo(int column)
        {
            return Interop.get_lowbo(_lp, column);
        }

        /// <summary>
        /// Sets the lower bound of a variable.
        /// </summary>
        /// <param name="column">The column number of the variable on which the bound must be set. Must be between 1 and the number of columns in the lp.</param>
        /// <param name="value">The lower bound on the variable identified by <paramref name="column"/>.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// Note that the default lower bound of each variable is 0.
        /// So variables will never take negative values if no negative lower bound is set.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_lowbo.htm">Full C API documentation.</seealso>
        public bool set_lowbo(int column, double value)
        {
            return Interop.set_lowbo(_lp, column, value);
        }

#endregion // Build model /  Column

#region Constraint / Row

        /// <summary>
        /// Adds a constraint to the model.
        /// </summary>
        /// <param name="row">An array with 1+<see cref="get_Ncolumns"/> elements that contains the values of the row.</param>
        /// <param name="constr_type">The type of the constraint.</param>
        /// <param name="rh">The value of the right-hand side (RHS) fo the constraint (in)equation</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method adds a row to the model (at the end) and sets all values of the row at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>It is almost always better to use <see cref="add_constraintex"/> instead of <see cref="add_constraint"/>. <see cref="add_constraintex"/> is always at least as performant as <see cref="add_constraint"/>.</para>
        /// <para>Note that it is advised to set the objective function (via <see cref="set_obj_fn"/>, <see cref="set_obj_fnex"/>, <see cref="str_set_obj_fn"/>, <see cref="set_obj"/>)
        /// before adding rows. This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// <para>Note that these methods will perform much better when <see cref="set_add_rowmode"/> is called before adding constraints.</para>
        /// <para>Note that if you have to add many constraints, performance can be improved by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_constraint.htm">Full C API documentation.</seealso>
        public bool add_constraint(double[] row, lpsolve_constr_types constr_type, double rh)
        {
            return Interop.add_constraint(_lp, row, constr_type, rh);
        }


        /// <summary>
        /// Adds a constraint to the model.
        /// </summary>
        /// <param name="count">Number of elements in <paramref name="row"/> and <paramref name="colno"/>.</param>
        /// <param name="row">An array with <paramref name="count"/> elements (or 1+<see cref="get_Ncolumns"/> elements if <paramref name="row"/> is <c>null</c>) that contains the values of the row.</param>
        /// <param name="colno">An array with <paramref name="count"/> elements that contains the column numbers of the row. However this variable can also be <c>null</c>.
        /// In that case element <c>i</c> in the variable row is column <c>i</c> and values start at element 1.</param>
        /// <param name="constr_type">The type of the constraint.</param>
        /// <param name="rh">The value of the right-hand side (RHS) fo the constraint (in)equation</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method adds a row to the model (at the end) and sets all values of the row at once.</para>
        /// <para>Note that when <paramref name="colno"/> is <c>null</c>, element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>This method has the possibility to specify only the non-zero elements. In that case <paramref name="colno"/> specifies the column numbers of 
        /// the non-zero elements. Both <paramref name="row"/> and <paramref name="colno"/> are then zero-based arrays.
        /// This will speed up building the model considerably if there are a lot of zero values. In most cases the matrix is sparse and has many zero value.
        /// Note that <see cref="add_constraintex"/> behaves the same as <see cref="add_constraint"/> when <paramref name="colno"/> is <c>null</c>.</para>
        /// <para>It is almost always better to use <see cref="add_constraintex"/> instead of <see cref="add_constraint"/>. <see cref="add_constraintex"/> is always at least as performant as <see cref="add_constraint"/>.</para>
        /// <para>Note that it is advised to set the objective function (via <see cref="set_obj_fn"/>, <see cref="set_obj_fnex"/>, <see cref="str_set_obj_fn"/>, <see cref="set_obj"/>)
        /// before adding rows. This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// <para>Note that these methods will perform much better when <see cref="set_add_rowmode"/> is called before adding constraints.</para>
        /// <para>Note that if you have to add many constraints, performance can be improved by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_constraint.htm">Full C API documentation.</seealso>
        public bool add_constraintex(int count, double[] row, int[] colno, lpsolve_constr_types constr_type, double rh)
        {
            return Interop.add_constraintex(_lp, count, row, colno, constr_type, rh);
        }

        /// <summary>
        /// Adds a constraint to the model.
        /// </summary>
        /// <param name="row_string">A string with column elements that contains the values of the row. Each element must be separated by space(s).</param>
        /// <param name="constr_type">The type of the constraint.</param>
        /// <param name="rh">The value of the right-hand side (RHS) fo the constraint (in)equation</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method adds a row to the model (at the end) and sets all values of the row at once.</para>
        /// <para>This method should only be used in small or demo code since it is not performant and uses more memory than <see cref="add_constraint"/> and <see cref="add_constraintex"/>.</para>
        /// <para>Note that these methods will perform much better when <see cref="set_add_rowmode"/> is called before adding constraints.</para>
        /// <para>Note that if you have to add many constraints, performance can be improved by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_constraint.htm">Full C API documentation.</seealso>
        public bool str_add_constraint(string row_string, lpsolve_constr_types constr_type, double rh)
        {
            return Interop.str_add_constraint(_lp, row_string, constr_type, rh);
        }

        /// <summary>
        /// Removes a constraint from the model.
        /// </summary>
        /// <param name="del_row">The row to delete. Must be between 1 and the number of rows in the model.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise. An error occurs when <paramref name="del_row"/>
        /// is not between 1 and the number of rows in the model.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method also fails. See <see cref="set_add_rowmode"/>.</para>
        /// <para>This method deletes a row from the model. The row is effectively deleted, so all rows after this row shift one up.</para>
        /// <para>Note that row 0 (the objective function) cannot be deleted. There is always an objective function.</para>
        /// <para>Note that you can also delete multiple constraints by a call to <see cref="resize_lp"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/del_constraint.htm">Full C API documentation.</seealso>
        public bool del_constraint(int del_row)
        {
            return Interop.del_constraint(_lp, del_row);
        }

        /// <summary>
        /// Gets all row elements from the model for the given <paramref name="row_nr"/>.
        /// </summary>
        /// <param name="row_nr">The row number of the matrix. Must be between 1 and number of rows in the model. Row 0 is the objective function.</param>
        /// <param name="row">Array in which the values are returned. The array must be dimensioned with at least 1+<see cref="get_Ncolumns"/> elements.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise. An error occurs when <paramref name="row_nr"/>
        /// is not between 0 and the number of rows in the model.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="set_add_rowmode"/>.</para>
        /// <para>Element 0 of the row array is not filled. Element 1 will contain the value for column 1, Element 2 the value for column 2, ...</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_row.htm">Full C API documentation.</seealso>
        public bool get_row(int row_nr, double[] row)
        {
            return Interop.get_row(_lp, row_nr, row);
        }

        /// <summary>
        /// Gets the non-zero row elements from the model for the given <paramref name="row_nr"/>.
        /// </summary>
        /// <param name="row_nr">The row number of the matrix. Must be between 1 and number of rows in the model. Row 0 is the objective function.</param>
        /// <param name="row">Array in which the values are returned. The array must be dimensioned with at least the number of non-zero elements in the row.
        /// If that is unknown, then use the number of columns in the model. The return value of the method indicates how many non-zero elements there are.</param>
        /// <param name="colno">Array in which the column numbers are returned. The array must be dimensioned with at least the number of non-zero elements in the row.
        /// If that is unknown, then use the number of columns in the model. The return value of the method indicates how many non-zero elements there are.</param>
        /// <returns>The number of non-zero elements returned in <paramref name="row"/> and <paramref name="colno"/>. If an error occurs, then -1 is returned.. An error occurs when <paramref name="row_nr"/> is not between 0 and the number of rows in the model.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="set_add_rowmode"/>.</para>
        /// <para>Returned values in <paramref name="row"/> and <paramref name="colno"/> start from element 0.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_row.htm">Full C API documentation.</seealso>
        public int get_rowex(int row_nr, double[] row, int[] colno)
        {
            return Interop.get_rowex(_lp, row_nr, row, colno);
        }

        /// <summary>
        /// Sets a constraint in the model.
        /// </summary>
        /// <param name="row_no">The row number that must be changed.</param>
        /// <param name="row">An array with 1+<see cref="get_Ncolumns"/> elements that contains the values of the row.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method change the values of the row in the model at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>It is almost always better to use <see cref="set_rowex"/> instead of <see cref="set_row"/>. <see cref="set_rowex"/> is always at least as performant as <see cref="set_row"/>.</para>
        /// <para>It is more performant to call these methods than multiple times <see cref="set_mat"/>.</para>
        /// <para>Note that these methods will perform much better when <see cref="set_add_rowmode"/> is called before adding constraints.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_row.htm">Full C API documentation.</seealso>
        public bool set_row(int row_no, double[] row)
        {
            return Interop.set_row(_lp, row_no, row);
        }

        /// <summary>
        /// Sets a constraint in the model.
        /// </summary>
        /// <param name="row_no">The row number that must be changed.</param>
        /// <param name="count">Number of elements in <paramref name="row"/> and <paramref name="colno"/>.</param>
        /// <param name="row">An array with <paramref name="count"/> elements (or 1+<see cref="get_Ncolumns"/> elements if <paramref name="row"/> is <c>null</c>) that contains the values of the row.</param>
        /// <param name="colno">An array with <paramref name="count"/> elements that contains the column numbers of the row. However this variable can also be <c>null</c>.
        /// In that case element <c>i</c> in the variable row is column <c>i</c> and values start at element 1.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
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
        /// <para>Note that these methods will perform much better when <see cref="set_add_rowmode"/> is called before adding constraints.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_row.htm">Full C API documentation.</seealso>
        public bool set_rowex(int row_no, int count, double[] row, int[] colno)
        {
            return Interop.set_rowex(_lp, row_no, count, row, colno);
        }

        /// <summary>
        /// Sets the name of a constraint (row) in the model.
        /// </summary>
        /// <param name="row">The row for which the name must be set. Must be between 0 and the number of rows in the model.</param>
        /// <param name="new_name">The name for the constraint (row).</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The row must already exist. row 0 is the objective function. Row names are optional.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_row_name.htm">Full C API documentation.</seealso>
        public bool set_row_name(int row, string new_name)
        {
            return Interop.set_row_name(_lp, row, new_name);
        }

        /// <summary>
        /// Gets the name of a constraint (row) in the model.
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
        /// <remarks>
        /// <para>Row names are optional. If no row name was specified, the method returns Rx with x the row number. row 0 is the objective function.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_row_name.htm">Full C API documentation.</seealso>
        public string get_row_name(int row)
        {
            return Interop.get_row_name(_lp, row);
        }

        /// <summary>
        /// Gets the name of a constraint (row) in the model.
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
        /// <remarks>
        /// <para>Row names are optional. If no row name was specified, the method returns Rx with x the row number. row 0 is the objective function.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_row_name.htm">Full C API documentation.</seealso>
        public string get_origrow_name(int row)
        {
            return Interop.get_origrow_name(_lp, row);
        }

        /// <summary>
        /// Returns whether or not the constraint of the given row matches the given mask.
        /// </summary>
        /// <param name="row">The row for which the constraint type must be retrieved. Must be between 1 and number of rows in the model.</param>
        /// <param name="mask">The type of the constraint to check in <paramref name="row"/></param>
        /// <returns><c>true</c> if the containt types match, <c>false</c> otherwise.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_constr_type.htm">Full C API documentation.</seealso>
        public bool is_constr_type(int row, lpsolve_constr_types mask)
        {
            return Interop.is_constr_type(_lp, row, mask);
        }

        /// <summary>
        /// Gets the type of a constraint.
        /// </summary>
        /// <param name="row">The row for which the constraint type must be retrieved. Must be between 1 and number of rows in the model.</param>
        /// <returns>The type of the constraint on row <paramref name="row"/>.</returns>
        /// <remarks>The default constraint type is <see cref="lpsolve_constr_types.LE"/>.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_constr_type.htm">Full C API documentation.</seealso>
        public lpsolve_constr_types get_constr_type(int row)
        {
            return Interop.get_constr_type(_lp, row);
        }

        /// <summary>
        /// Sets the type of a constraint for the specified row.
        /// </summary>
        /// <param name="row">The row for which the constraint type must be set. Must be between 1 and number of rows in the model.</param>
        /// <param name="con_type">The type of the constraint.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The default constraint type is <see cref="lpsolve_constr_types.LE"/>.</para>
        /// <para>A free constraint (<see cref="lpsolve_constr_types.FR"/>) will act as if the constraint is not there.
        /// The lower bound is -Infinity and the upper bound is +Infinity.
        /// This can be used to temporary disable a constraint without having to delete it from the model
        /// .Note that the already set RHS and range on this constraint is overruled with Infinity by this.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_constr_type.htm">Full C API documentation.</seealso>
        public bool set_constr_type(int row, lpsolve_constr_types con_type)
        {
            return Interop.set_constr_type(_lp, row, con_type);
        }

        /// <summary>
        /// Gets the value of the right hand side (RHS) vector (column 0) for one row.
        /// </summary>
        /// <param name="row">The row number of the constraint on which the range must be retrieved. Must be between 1 and number of rows in the model.</param>
        /// <returns>The value of the RHS for the specified row. If row is out of range it returns 0. If no previous value was set, then it also returns 0, the default RHS value.</returns>
        /// <remarks>
        /// <para>Note that row can also be 0 with this method. In that case it returns the initial value of the objective function.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_rh.htm">Full C API documentation.</seealso>
        public double get_rh(int row)
        {
            return Interop.get_rh(_lp, row);
        }

        /// <summary>
        /// Sets the value of the right hand side (RHS) vector (column 0) for the specified row.
        /// </summary>
        /// <param name="row">The row for which the RHS value must be set. Must be between 1 and number of rows in the model.</param>
        /// <param name="value">The value of the RHS.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that row can also be 0 with this method.
        /// In that case an initial value for the objective value is set.
        /// methods <see cref="set_rh_vec"/>, <see cref="str_set_rh_vec"/> ignore row 0 (for historical reasons) in the specified RHS vector,
        /// but it is possible to first call one of these methods and then set the value of the objective with <see cref="set_rh"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh.htm">Full C API documentation.</seealso>
        public bool set_rh(int row, double value)
        {
            return Interop.set_rh(_lp, row, value);
        }

        /// <summary>
        /// Gets the range on the constraint (row) identified by <paramref name="row"/>.
        /// </summary>
        /// <param name="row">The row number of the constraint on which the range must be retrieved. Must be between 1 and number of rows in the model.</param>
        /// <returns>The range on the constraint (row) identified by <paramref name="row"/>.</returns>
        /// <remarks>
        /// <para>Setting a range on a row is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a range doesn't increase the model size that means that the model stays smaller and will be solved faster.</para>
        /// <para>If the row has a less than constraint then the range means setting a minimum on the constraint that is equal to the RHS value minus the range.
        /// If the row has a greater than constraint then the range means setting a maximum on the constraint that is equal to the RHS value plus the range.</para>
        /// <para>Note that the range value is the difference value and not the absolute value.</para>
        /// <para>If no range was set then <see cref="get_rh_range"/> returns a very big number, the value of <see cref="get_infinite"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_rh_range.htm">Full C API documentation.</seealso>
        public double get_rh_range(int row)
        {
            return Interop.get_rh_range(_lp, row);
        }

        /// <summary>
        /// Sets the range on the constraint (row) identified by <paramref name="row"/>.
        /// </summary>
        /// <param name="row">The row number of the constraint on which the range must be set. Must be between 1 and number of rows in the model.</param>
        /// <param name="deltavalue">The range on the constraint.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Setting a range on a row is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a range doesn't increase the model size that means that the model stays smaller and will be solved faster.</para>
        /// <para>If the row has a less than constraint then the range means setting a minimum on the constraint that is equal to the RHS value minus the range.
        /// If the row has a greater than constraint then the range means setting a maximum on the constraint that is equal to the RHS value plus the range.</para>
        /// <para>Note that the range value is the difference value and not the absolute value.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh_range.htm">Full C API documentation.</seealso>
        public bool set_rh_range(int row, double deltavalue)
        {
            return Interop.set_rh_range(_lp, row, deltavalue);
        }

        /// <summary>
        /// Sets the value of the right hand side (RHS) vector (column 0).
        /// </summary>
        /// <param name="rh">An array with row elements that contains the values of the RHS.</param>
        /// <remarks>
        /// <para>The method sets all values of the RHS vector (column 0) at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Row 1 is element 1, row 2 is element 2, ...</para>
        /// <para>If the initial value of the objective function must also be set, use <see cref="set_rh"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh_vec.htm">Full C API documentation.</seealso>
        public void set_rh_vec(double[] rh)
        {
            Interop.set_rh_vec(_lp, rh);
        }

        /// <summary>
        /// Sets the value of the right hand side (RHS) vector (column 0).
        /// </summary>
        /// <param name="rh_string">A string with row elements that contains the values of the RHS. Each element must be separated by space(s).</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The method sets all values of the RHS vector (column 0) at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Row 1 is element 1, row 2 is element 2, ...</para>
        /// <para>If the initial value of the objective function must also be set, use <see cref="set_rh"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh_vec.htm">Full C API documentation.</seealso>
        public bool str_set_rh_vec(string rh_string)
        {
            return Interop.str_set_rh_vec(_lp, rh_string);
        }

#endregion

#region Objective

        /// <summary>
        /// Sets the objective function (row 0) of the matrix.
        /// </summary>
        /// <param name="column">The column number for which the value must be set.</param>
        /// <param name="value">The value that must be set.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>It is better to use <see cref="set_obj_fnex"/> or <see cref="set_obj_fn"/>.</para>
        /// <para>Note that it is advised to set the objective function before adding rows via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj_fn.htm">Full C API documentation.</seealso>
        public bool set_obj(int column, double value)
        {
            return Interop.set_obj(_lp, column, value);
        }

        /// <summary>
        /// Returns initial "at least better than" guess for objective function.
        /// </summary>
        /// <returns>Returns initial "at least better than" guess for objective function.</returns>
        /// <remarks>
        /// <para>This is only used in the branch-and-bound algorithm when integer variables exist in the model. All solutions with a worse objective value than this value are immediately rejected. This can result in faster solving times, but it can be difficult to predict what value to take for this bound. Also there is the chance that the found solution is not the most optimal one.</para>
        /// <para>The default is infinite.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_obj_bound.htm">Full C API documentation.</seealso>
        public double get_obj_bound()
        {
            return Interop.get_obj_bound(_lp);
        }

        /// <summary>
        /// Sets initial "at least better than" guess for objective function.
        /// </summary>
        /// <param name="obj_bound">The initial "at least better than" guess for objective function.</param>
        /// <remarks>
        /// <para>This is only used in the branch-and-bound algorithm when integer variables exist in the model.
        /// All solutions with a worse objective value than this value are immediately rejected.
        /// This can result in faster solving times, but it can be difficult to predict what value to take for this bound.
        /// Also there is the chance that the found solution is not the most optimal one.</para>
        /// <para>The default is infinite.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj_bound.htm">Full C API documentation.</seealso>
        public void set_obj_bound(double obj_bound)
        {
            Interop.set_obj_bound(_lp, obj_bound);
        }

        /// <summary>
        /// Sets the objective function (row 0) of the matrix.
        /// </summary>
        /// <param name="row">An array with 1+<see cref="get_Ncolumns"/> elements that contains the values of the objective function.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method set the values of the objective function in the model at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>It is almost always better to use <see cref="set_obj_fnex"/> instead of <see cref="set_obj_fn"/>. <see cref="set_obj_fnex"/> is always at least as performant as <see cref="set_obj_fnex"/>.</para>
        /// <para>It is more performant to call these methods than multiple times <see cref="set_obj"/>.</para>
        /// <para>Note that it is advised to set the objective function before adding rows via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj_fn.htm">Full C API documentation.</seealso>
        public bool set_obj_fn(double[] row)
        {
            return Interop.set_obj_fn(_lp, row);
        }

        /// <summary>
        /// Sets the objective function (row 0) of the matrix.
        /// </summary>
        /// <param name="count">Number of elements in <paramref name="row"/> and <paramref name="colno"/>.</param>
        /// <param name="row">An array with <paramref name="count"/> elements (or 1+<see cref="get_Ncolumns"/> elements if <paramref name="row"/> is <c>null</c>) that contains the values of the row.</param>
        /// <param name="colno">An array with <paramref name="count"/> elements that contains the column numbers of the row. However this variable can also be <c>null</c>.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
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
        public bool set_obj_fnex(int count, double[] row, int[] colno)
        {
            return Interop.set_obj_fnex(_lp, count, row, colno);
        }

        /// <summary>
        /// Sets the objective function (row 0) of the matrix.
        /// </summary>
        /// <param name="row_string">A string with column elements that contains the values of the objective function. Each element must be separated by space(s).</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method set the values of the objective function in the model at once.</para>
        /// <para>This method should only be used in small or demo code since it is not performant and uses more memory than <see cref="set_obj_fnex"/> or <see cref="set_obj_fn"/></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj_fn.htm">Full C API documentation.</seealso>
        public bool str_set_obj_fn(string row_string)
        {
            return Interop.str_set_obj_fn(_lp, row_string);
        }

        /// <summary>
        /// Returns objective function direction.
        /// </summary>
        /// <returns><c>true</c> if the objective function is maximize, <c>false</c> if it is minimize.</returns>
        /// <remarks>
        /// The default of lp_solve is to minimize, except for <see cref="read_LP"/> where the default is to maximize.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_maxim.htm">Full C API documentation.</seealso>
        public bool is_maxim()
        {
            return Interop.is_maxim(_lp);
        }

        /// <summary>
        /// Sets the objective function to <c>maximize</c>.
        /// </summary>
        /// <remarks>
        /// The default of lp_solve is to minimize, except for <see cref="read_LP"/> where the default is to maximize.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_maxim.htm">Full C API documentation.</seealso>
        public void set_maxim()
        {
            Interop.set_maxim(_lp);
        }

        /// <summary>
        /// Sets the objective function to <c>minimize</c>.
        /// </summary>
        /// <remarks>
        /// The default of lp_solve is to minimize, except for <see cref="read_LP"/> where the default is to maximize.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_minim.htm">Full C API documentation.</seealso>
        public void set_minim()
        {
            Interop.set_minim(_lp);
        }

        /// <summary>
        /// Sets the objective function sense.
        /// </summary>
        /// <param name="maximize">When <c>true</c>, the objective function sense is maximize, when <c>false</c> it is minimize.</param>
        /// <remarks>
        /// The default of lp_solve is to minimize, except for <see cref="read_LP"/> where the default is to maximize.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_sense.htm">Full C API documentation.</seealso>
        public void set_sense(bool maximize)
        {
            Interop.set_sense(_lp, maximize);
        }

#endregion

        /// <summary>
        /// Gets the name of the model.
        /// </summary>
        /// <returns>The name of the model.</returns>
        /// <remarks>
        /// Giving the lp a name is optional. The default name is "Unnamed".
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_lp_name.htm">Full C API documentation.</seealso>
        public string get_lp_name()
        {
            return Interop.get_lp_name(_lp);
        }

        /// <summary>
        /// Sets the name of the model.
        /// </summary>
        /// <param name="lpname">The name of the model.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Giving the lp a name is optional. The default name is "Unnamed".
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_lp_name.htm">Full C API documentation.</seealso>
        public bool set_lp_name(string lpname)
        {
            return Interop.set_lp_name(_lp, lpname);
        }

        /// <summary>
        /// Allocates memory for the specified size.
        /// </summary>
        /// <param name="rows">Allocates memory for this amount of rows.</param>
        /// <param name="columns">Allocates memory for this amount of columns.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method deletes the last rows/columns of the model if the new number of rows/columns is less than the number of rows/columns before the call.</para>
        /// <para>However, the function does **not** add rows/columns to the model if the new number of rows/columns is larger.
        /// It does however changes internal memory allocations to the new specified sizes.
        /// This to make the <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/> and <see cref="add_column"/>,
        /// <see cref="add_columnex"/>, <see cref="str_add_column"/> methods faster. Without <see cref="resize_lp"/>, these methods have to reallocated
        /// memory at each call for the new dimensions. However if <see cref="resize_lp "/> is used, then memory reallocation
        /// must be done only once resulting in better performance. So if the number of rows/columns that will be added is known in advance, then performance can be improved by using this method.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/resize_lp.htm">Full C API documentation.</seealso>
        public bool resize_lp(int rows, int columns)
        {
            return Interop.resize_lp(_lp, rows, columns);
        }

        /// <summary>
        /// Returns a flag telling which of the add methods perform best. Whether <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/>
        /// or <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// </summary>
        /// <returns>If <c>false</c> then <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/>
        /// perform best. If <c>true</c> then <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>
        /// perform best.</returns>
        /// <remarks>
        /// <para>Default, this is <c>false</c>, meaning that <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/> perform best.
        /// If the model is build via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/> calls,
        /// then these methods will be much faster if this method returns <c>true</c>.
        /// This is also called row entry mode. The speed improvement is spectacular, especially for bigger models, so it is 
        /// advisable to call this method to set the mode. Normally a model is build either column by column or row by row.</para>
        /// <para>Note that there are several restrictions with this mode:
        /// Only use this method after a <see cref="make_lp"/> call. Not when the model is read from file.
        /// Also, if this method is used, first add the objective function via <see cref="set_obj_fn"/>, <see cref="set_obj_fnex"/>, <see cref="str_set_obj_fn"/>
        /// and after that add the constraints via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// Don't call other API methods while in row entry mode.
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
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_add_rowmode.htm">Full C API documentation.</seealso>
        public bool is_add_rowmode()
        {
            return Interop.is_add_rowmode(_lp);
        }

        /// <summary>
        /// Specifies which add methods perform best. Whether <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/>
        /// or <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// </summary>
        /// <param name="turnon">If <c>false</c> then <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/>
        /// perform best. If <c>true</c> then <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>
        /// perform best.</param>
        /// <returns><c>true</c> if the rowmode was changed, <c>false</c> if the given mode was already set.</returns>
        /// <remarks>
        /// <para>Default, this is <c>false</c>, meaning that <see cref="add_column"/>, <see cref="add_columnex"/>, <see cref="str_add_column"/> perform best.
        /// If the model is build via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/> calls,
        /// then these methods will be much faster if this method is called with <paramref name="turnon"/> set to <c>true</c>.
        /// This is also called row entry mode. The speed improvement is spectacular, especially for bigger models, so it is 
        /// advisable to call this method to set the mode. Normally a model is build either column by column or row by row.</para>
        /// <para>Note that there are several restrictions with this mode:
        /// Only use this method after a <see cref="make_lp"/> call. Not when the model is read from file.
        /// Also, if this method is used, first add the objective function via <see cref="set_obj_fn"/>, <see cref="set_obj_fnex"/>, <see cref="str_set_obj_fn"/>
        /// and after that add the constraints via <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>.
        /// Don't call other API methods while in row entry mode.
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
        public bool set_add_rowmode(bool turnon)
        {
            return Interop.set_add_rowmode(_lp, turnon);
        }

        /// <summary>
        /// Gets the index of a given column or row name in the model.
        /// </summary>
        /// <param name="name">The name of the column or row for which the index (column/row number) must be retrieved.</param>
        /// <param name="isrow">Use <c>false</c> when column information is needed and <c>true</c> when row information is needed.</param>
        /// <returns>Returns the index (column/row number) of the given column/row name.
        /// A return value of -1 indicates that the name does not exist.
        /// Note that the index is the original index number.
        /// So if presolve is active, it has no effect.
        /// It is the original column/row number that is returned.</returns>
        /// <remarks>
        /// Note that this index starts from 1.
        /// Some API methods expect zero-based indexes and thus this value must then be corrected with -1.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_nameindex.htm">Full C API documentation.</seealso>
        public int get_nameindex(string name, bool isrow)
        {
            return Interop.get_nameindex(_lp, name, isrow);
        }

        /// <summary>
        /// Returns the value of "infinite".
        /// </summary>
        /// <returns>Returns the practical value of "infinite".</returns>
        /// <remarks>
        /// <para>This value is used for very big numbers. For example the upper bound of a variable without an upper bound.</para>
        /// <para>The default is 1e30</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_infinite.htm">Full C API documentation.</seealso>
        public double get_infinite()
        {
            return Interop.get_infinite(_lp);
        }

        /// <summary>
        /// Specifies the practical value of "infinite".
        /// </summary>
        /// <param name="infinite">The value that must be used for "infinite".</param>
        /// <remarks>
        /// <para>This value is used for very big numbers. For example the upper bound of a variable without an upper bound.</para>
        /// <para>The default is 1e30</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_infinite.htm">Full C API documentation.</seealso>
        public void set_infinite(double infinite)
        {
            Interop.set_infinite(_lp, infinite);
        }

        /// <summary>
        /// Checks if the provided absolute of the value is larger or equal to "infinite".
        /// </summary>
        /// <param name="value">The value to check against "infinite".</param>
        /// <returns><c>true</c> if the value is equal or larger to "infinite", <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Note that the absolute of the provided value is checked against the value set by <see cref="set_infinite"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_infinite.htm">Full C API documentation.</seealso>
        public bool is_infinite(double value)
        {
            return Interop.is_infinite(_lp, value);
        }

        /// <summary>
        /// Gets a single element from the matrix.
        /// </summary>
        /// <param name="row">Row number of the matrix. Must be between 0 and number of rows in the model. Row 0 is objective function.</param>
        /// <param name="column">Column number of the matrix. Must be between 1 and number of columns in the model.</param>
        /// <returns>
        /// <para>Returns the value of the element on row <paramref name="row"/>, column <paramref name="column"/>.
        /// If no value was set for this element, the method returns 0.</para>
        /// <para>Note that row entry mode must be off, else this method also fails.
        /// See <see cref="set_add_rowmode"/>.</para></returns>
        /// <remarks>
        /// <para>This method is not efficient if many values are to be retrieved.
        /// Consider to use <see cref="get_row"/>, <see cref="get_rowex"/>, <see cref="get_column"/>, <see cref="get_columnex"/>.</para>
        /// <para>
        /// If row and/or column are outside the allowed range, the method returns 0.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_mat.htm">Full C API documentation.</seealso>
        public double get_mat(int row, int column)
        {
            return Interop.get_mat(_lp, row, column);
        }

        /// <summary>
        /// Sets a single element in the matrix.
        /// </summary>
        /// <param name="row">Row number of the matrix. Must be between 0 and number of rows in the model. Row 0 is objective function.</param>
        /// <param name="column">Column number of the matrix. Must be between 1 and number of columns in the model.</param>
        /// <param name="value">Value to set on row <paramref name="row"/>, column <paramref name="column"/>.</param>
        /// <returns><para><c>true</c> if operation was successful, <c>false</c> otherwise.</para>
        /// <para>Note that row entry mode must be off, else this method also fails.
        /// See <see cref="set_add_rowmode"/>.</para></returns>
        /// <remarks>
        /// <para>If there was already a value for this element, it is replaced and if there was no value, it is added.</para>
        /// <para>This method is not efficient if many values are to be set.
        /// Consider to use <see cref="add_constraint"/>, <see cref="add_constraintex"/>, <see cref="str_add_constraint"/>,
        /// <see cref="set_row"/>, <see cref="set_rowex"/>, <see cref="set_obj_fn"/>, <see cref="set_obj_fnex"/>,
        /// <see cref="str_set_obj_fn"/>, <see cref="set_obj"/>, <see cref="add_column"/>, <see cref="add_columnex"/>,
        /// <see cref="str_add_column"/>, <see cref="set_column"/>, <see cref="set_columnex"/>.</para>
        /// <para>
        /// If row and/or column are outside the allowed range, the method returns 0.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_mat.htm">Full C API documentation.</seealso>
        public bool set_mat(int row, int column, double value)
        {
            return Interop.set_mat(_lp, row, column, value);
        }

        /// <summary>
        /// Specifies if set bounds may only be tighter or also less restrictive.
        /// </summary>
        /// <param name="tighten">Specifies if set bounds may only be tighter <c>true</c> or also less restrictive <c>false</c>.</param>
        /// <remarks>
        /// <para>If set to <c>true</c> then bounds may only be tighter.
        /// This means that when <see cref="set_lowbo"/> or <see cref="set_upbo"/> is used to set a bound
        /// and the bound is less restrictive than an already set bound, then this new bound will be ignored.
        /// If tighten is set to <c>false</c>, the new bound is accepted.
        /// This functionality is useful when several bounds are set on a variable and at the end you want
        /// the most restrictive ones. By default, this setting is <c>false</c>.
        /// Note that this setting does not affect <see cref="set_bounds"/>.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bounds_tighter.htm">Full C API documentation.</seealso>
        public void set_bounds_tighter(bool tighten)
        {
            Interop.set_bounds_tighter(_lp, tighten);
        }

        /// <summary>
        /// Returns if set bounds may only be tighter or also less restrictive.
        /// </summary>
        /// <returns>Returns <c>true</c> if set bounds may only be tighter or <c>false</c> if they can also be less restrictive.</returns>
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
        public bool get_bounds_tighter()
        {
            return Interop.get_bounds_tighter(_lp);
        }

        /// <summary>
        /// Returns, for the specified variable, the priority the variable has in the branch-and-bound algorithm.
        /// </summary>
        /// <param name="column">The column number of the variable on which the priority must be returned.
        /// It must be between 1 and the number of columns in the model. If it is not within this range, the return value is 0.</param>
        /// <returns>Returns the priority of the variable.</returns>
        /// <remarks>
        /// The method returns the priority the variable has in the branch-and-bound algorithm.
        /// This priority is determined by the weights set by <see cref="set_var_weights"/>.
        /// The default priorities are the column positions of the variables in the model.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_var_priority.htm">Full C API documentation.</seealso>
        public int get_var_priority(int column)
        {
            return Interop.get_var_priority(_lp, column);
        }

        /// <summary>
        /// Sets the weights on variables.
        /// </summary>
        /// <param name="weights">The weights array.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
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
        public bool set_var_weights(double[] weights)
        {
            return Interop.set_var_weights(_lp, weights);
        }

        /// <summary>
        /// Adds a SOS (Special Ordered Sets) constraint.
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
        /// <remarks>
        /// <para>The <see cref="add_SOS"/> method adds an SOS constraint.</para>
        /// <para>See <see href="http://lpsolve.sourceforge.net/5.5/SOS.htm">Special Ordered Sets</see> for a description about SOS variables.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_SOS.htm">Full C API documentation.</seealso>
        public int add_SOS(string name, int sostype, int priority, int count, int[] sosvars, double[] weights)
        {
            return Interop.add_SOS(_lp, name, sostype, priority, count, sosvars, weights);
        }

        /// <summary>
        /// Returns if the variable is SOS (Special Ordered Set) or not.
        /// </summary>
        /// <param name="column">The column number of the variable that must be checked.
        /// It must be between 1 and the number of columns in the model.</param>
        /// <returns><c>true</c> if variable is a SOS var, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The <see cref="is_SOS_var "/> method returns if a variable is a SOS variable or not.
        /// Default a variable is not SOS. A variable becomes a SOS variable via <see cref="add_SOS"/>.</para>
        /// <para>See <see href="http://lpsolve.sourceforge.net/5.5/SOS.htm">Special Ordered Sets</see> for a description about SOS variables.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_SOS_var.htm">Full C API documentation.</seealso>
        public bool is_SOS_var(int column)
        {
            return Interop.is_SOS_var(_lp, column);
        }

#endregion

#region Solver settings

#region Epsilon / Tolerance

        /// <summary>
        /// Returns the value that is used as a tolerance for the Right Hand Side (RHS) to determine whether a value should be considered as 0.
        /// </summary>
        /// <returns>Returns the value that is used as a tolerance for the Right Hand Side (RHS) to determine whether a value should be considered as 0.</returns>
        /// <remarks>
        /// The default value for <c>epsb</c> is <c>1e-10</c>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epsb.htm">Full C API documentation.</seealso>
        public double get_epsb()
        {
            return Interop.get_epsb(_lp);
        }

        /// <summary>
        /// Specifies the value that is used as a tolerance for the Right Hand Side (RHS) to determine whether a value should be considered as 0.
        /// </summary>
        /// <param name="epsb">The value that is used as a tolerance for the Right Hand Side (RHS) to determine whether a value should be considered as 0.</param>
        /// <remarks>
        /// <para>Floating-point calculations always result in loss of precision and rounding errors.
        /// Therefore a very small value (example <c>1e-99</c>) could be the result of such errors and should be considered as 0 for the algorithm. epsb specifies the tolerance to determine if a RHS value should be considered as 0. If abs(value) is less than this epsb value in the RHS, it is considered as 0.</para>
        /// <para>The default <c>epsb</c> value is <c>1e-10</c></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epsb.htm">Full C API documentation.</seealso>
        public void set_epsb(double epsb)
        {
            Interop.set_epsb(_lp, epsb);
        }

        /// <summary>
        /// Returns the value that is used as a tolerance for the reduced costs to determine whether a value should be considered as 0.
        /// </summary>
        /// <returns>Returns the value that is used as a tolerance for the reduced costs to determine whether a value should be considered as 0.</returns>
        /// <remarks>
        /// The default value for <c>epsd</c> is <c>1e-9</c>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epsd.htm">Full C API documentation.</seealso>
        public double get_epsd()
        {
            return Interop.get_epsd(_lp);
        }

        /// <summary>
        /// Specifies the value that is used as a tolerance for reduced costs to determine whether a value should be considered as 0.
        /// </summary>
        /// <param name="epsd">The value that is used as a tolerance for reduced costs to determine whether a value should be considered as 0.</param>
        /// <remarks>
        /// <para>Floating-point calculations always result in loss of precision and rounding errors.
        /// Therefore a very small value (example <c>1e-99</c>) could be the result of such errors and should be considered as 0 for the algorithm. epsd specifies the tolerance to determine if a reducedcost value should be considered as 0. If abs(value) is less than this epsd value, it is considered as 0.</para>
        /// <para>The default <c>epsd</c> value is <c>1e-9</c></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epsd.htm">Full C API documentation.</seealso>
        public void set_epsd(double epsd)
        {
            Interop.set_epsd(_lp, epsd);
        }

        /// <summary>
        /// Returns the value that is used as a tolerance for rounding values to zero.
        /// </summary>
        /// <returns>Returns the value that is used as a tolerance for rounding values to zero.</returns>
        /// <remarks>
        /// <para><c>epsel</c> is used on all other places where <c>epsint</c>, <c>epsb</c>, <c>epsd</c>, <c>epspivot</c>, <c>epsperturb</c> are not used.</para>
        /// <para>The default value for <c>epsel</c> is <c>1e-12</c></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epsel.htm">Full C API documentation.</seealso>
        public double get_epsel()
        {
            return Interop.get_epsel(_lp);
        }

        /// <summary>
        /// Specifies the value that is used as a tolerance for rounding values to zero.
        /// </summary>
        /// <param name="epsel">The value that is used as a tolerance for rounding values to zero.</param>
        /// <remarks>
        /// <para>Floating-point calculations always result in loss of precision and rounding errors. Therefore a very small value (example 1e-99) could be the result of such errors and should be considered as 0 for the algorithm. epsel specifies the tolerance to determine if a value should be considered as 0. If abs(value) is less than this epsel value, it is considered as 0.</para>
        /// <para><c>epsel</c> is used on all other places where <c>epsint</c>, <c>epsb</c>, <c>epsd</c>, <c>epspivot</c>, <c>epsperturb</c> are not used.</para>
        /// <para>The default value for <c>epsel</c> is <c>1e-12</c></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epsel.htm">Full C API documentation.</seealso>
        public void set_epsel(double epsel)
        {
            Interop.set_epsel(_lp, epsel);
        }

        /// <summary>
        /// Returns the tolerance that is used to determine whether a floating-point number is in fact an integer.
        /// </summary>
        /// <returns>Returns the tolerance that is used to determine whether a floating-point number is in fact an integer.</returns>
        /// <remarks>
        /// The default value for <c>epsint</c> is <c>1e-7</c>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epsint.htm">Full C API documentation.</seealso>
        public double get_epsint()
        {
            return Interop.get_epsint(_lp);
        }

        /// <summary>
        /// Specifies the tolerance that is used to determine whether a floating-point number is in fact an integer.
        /// </summary>
        /// <param name="epsint">The tolerance that is used to determine whether a floating-point number is in fact an integer.</param>
        /// <remarks>
        /// <para>This is only used when there is at least one integer variable and the branch and bound algorithm is used to make variables integer.</para>
        /// <para>Integer variables are internally in the algorithm also stored as floating point.
        /// Therefore a tolerance is needed to determine if a value is to be considered as integer or not.
        /// If the absolute value of the variable minus the closed integer value is less than <c>epsint</c>, it is considered as integer.
        /// For example if a variable has the value 0.9999999 and epsint is 0.000001 then it is considered integer because abs(0.9999999 - 1) = 0.0000001 and this is less than 0.000001</para>
        /// <para>The default value for epsint is 1e-7</para>
        /// <para>So by changing epsint you determine how close a value must approximate the nearest integer.
        /// Changing this tolerance value to for example 0.001 will generally result in faster solving times, but your solution is less integer.</para>
        /// <para>So it is a compromise.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epsint.htm">Full C API documentation.</seealso>
        public void set_epsint(double epsint)
        {
            Interop.set_epsint(_lp, epsint);
        }

        /// <summary>
        /// Returns the value that is used as perturbation scalar for degenerative problems.
        /// </summary>
        /// <returns>Returns the perturbation scalar.</returns>
        /// <remarks>The default epsperturb value is 1e-5</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epsperturb.htm">Full C API documentation.</seealso>
        public double get_epsperturb()
        {
            return Interop.get_epsperturb(_lp);
        }

        /// <summary>
        /// Specifies the value that is used as perturbation scalar for degenerative problems.
        /// </summary>
        /// <param name="epsperturb">The perturbation scalar.</param>
        /// <remarks>The default epsperturb value is 1e-5</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epsperturb.htm">Full C API documentation.</seealso>
        public void set_epsperturb(double epsperturb)
        {
            Interop.set_epsperturb(_lp, epsperturb);
        }

        /// <summary>
        /// Returns the value that is used as a tolerance for the pivot element to determine whether a value should be considered as 0.
        /// </summary>
        /// <returns>Returns the value that is used as a tolerance for the pivot element to determine whether a value should be considered as 0.</returns>
        /// <remarks>
        /// <para>Floating-point calculations always result in loss of precision and rounding errors.
        /// Therefore a very small value (example 1e-99) could be the result of such errors and should be considered as 0 
        /// for the algorithm. epspivot specifies the tolerance to determine if a pivot element should be considered as 0.
        /// If abs(value) is less than this epspivot value it is considered as 0 and at first instance rejected as pivot element.
        /// Only when no larger other pivot element can be found and the value is different from 0 it will be used as pivot element.</para>
        /// <para>The default epspivot value is 2e-7</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epspivot.htm">Full C API documentation.</seealso>
        public double get_epspivot()
        {
            return Interop.get_epspivot(_lp);
        }

        /// <summary>
        /// Specifies the value that is used as a tolerance pivot element to determine whether a value should be considered as 0.
        /// </summary>
        /// <param name="epspivot">The value that is used as a tolerance for the pivot element to determine whether a value should be considered as 0.</param>
        /// <remarks>
        /// <para>Floating-point calculations always result in loss of precision and rounding errors.
        /// Therefore a very small value (example 1e-99) could be the result of such errors and should be considered as 0 
        /// for the algorithm. epspivot specifies the tolerance to determine if a pivot element should be considered as 0.
        /// If abs(value) is less than this epspivot value it is considered as 0 and at first instance rejected as pivot element.
        /// Only when no larger other pivot element can be found and the value is different from 0 it will be used as pivot element.</para>
        /// <para>The default epspivot value is 2e-7</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epspivot.htm">Full C API documentation.</seealso>
        public void set_epspivot(double epspivot)
        {
            Interop.set_epspivot(_lp, epspivot);
        }

        /// <summary>
        /// Specifies the MIP gap value.
        /// </summary>
        /// <param name="absolute">If <c>true</c> then the absolute MIP gap is set, else the relative MIP gap.</param>
        /// <param name="mip_gap">The MIP gap.</param>
        /// <remarks>
        /// <para>The <see cref="set_mip_gap"/> method sets the MIP gap that specifies a tolerance for the branch and bound algorithm.
        /// This tolerance is the difference between the best-found solution yet and the current solution.
        /// If the difference is smaller than this tolerance then the solution (and all the sub-solutions) is rejected.
        /// This can result in faster solving times, but results in a solution which is not the perfect solution.
        /// So be careful with this tolerance.</para>
        /// <para>The default is 1e-11.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_mip_gap.htm">Full C API documentation.</seealso>
        public void set_mip_gap(bool absolute, double mip_gap)
        {
            Interop.set_mip_gap(_lp, absolute, mip_gap);
        }

        /// <summary>
        /// Returns the MIP gap value.
        /// </summary>
        /// <param name="absolute">If <c>true</c> then the absolute MIP gap is returned, else the relative MIP gap.</param>
        /// <returns>The MIP gap value</returns>
        /// <remarks>
        /// <para>The <see cref="get_mip_gap"/> method returns the MIP gap that specifies a tolerance for the branch and bound algorithm.
        /// This tolerance is the difference between the best-found solution yet and the current solution.
        /// If the difference is smaller than this tolerance then the solution (and all the sub-solutions) is rejected.
        /// This can result in faster solving times, but results in a solution which is not the perfect solution.
        /// So be careful with this tolerance.</para>
        /// <para>The default is 1e-11.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_mip_gap.htm">Full C API documentation.</seealso>
        public double get_mip_gap(bool absolute)
        {
            return Interop.get_mip_gap(_lp, absolute);
        }

        /// <summary>
        /// This is a simplified way of specifying multiple eps thresholds that are "logically" consistent.
        /// </summary>
        /// <param name="level">The level to set.</param>
        /// <returns><c>true</c> if level is accepted and <c>false</c> if an invalid epsilon level was provided.</returns>
        /// <remarks>
        /// <para>It sets the following values: <see cref="set_epsel"/>, <see cref="set_epsb"/>, <see cref="set_epsd"/>, <see cref="set_epspivot"/>, <see cref="set_epsint"/>, <see cref="set_mip_gap"/>.</para>
        /// <para>The default is <see cref="lpsolve_epsilon_level.EPS_TIGHT"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epslevel.htm">Full C API documentation.</seealso>
        public bool set_epslevel(lpsolve_epsilon_level level)
        {
            return Interop.set_epslevel(_lp, level);
        }

#endregion

#region Basis
        /// <summary>
        /// Causes reinversion at next opportunity.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is meant for internal use and development.
        /// It causes a reinversion of the matrix at a next opportunity.
        /// The method should only be used by people deeply understanding the code.
        /// </para>
        /// <para>
        /// In the past, this method was documented as the method to set an initial base.
        /// <strong>This is incorrect.</strong>
        /// <see cref="default_basis"/> must be used for this purpose.
        /// It is very unlikely that you must call this method.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/reset_basis.htm">Full C API documentation.</seealso>
        public void reset_basis()
        {
            Interop.reset_basis(_lp);
        }

        /// <summary>
        /// Sets the starting base to an all slack basis (the default simplex starting basis).
        /// </summary>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/default_basis.htm">Full C API documentation.</seealso>
        public void default_basis()
        {
            Interop.default_basis(_lp);
        }

        /// <summary>
        /// Read basis from a file and set as default basis.
        /// </summary>
        /// <param name="filename">Name of file containing the basis to read.</param>
        /// <param name="info">When not <c>null</c>, returns the information of the INFO card in <paramref name="filename"/>.
        /// When <c>null</c>, the information is ignored.
        /// Note that when not <c>null</c>, that you must make sure that this variable is long enough,
        /// else a memory overrun could occur.</param>
        /// <returns><c>true</c> if basis could be read from <paramref name="filename"/> and <c>false</c> if not.
        /// A <c>false</c> return value indicates an error.
        /// Specifically file could not be opened or file has wrong structure or wrong number/names rows/variables or invalid basis.</returns>
        /// <remarks>
        /// <para>Setting an initial basis can speed up the solver considerably.
        /// It is the starting point from where the algorithm continues to find an optimal solution.</para>
        /// <para>When a restart is done, lp_solve continues at the last basis, except if <see cref="set_basis"/>,
        /// <see cref="default_basis"/>, <see cref="guess_basis"/> or <see cref="read_basis"/> is called.</para>
        /// <para>The basis in the file must be in <see href="http://lpsolve.sourceforge.net/5.5/bas-format.htm">MPS bas file format</see>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_basis.htm">Full C API documentation.</seealso>
        public bool read_basis(string filename, string info)
        {
            return Interop.read_basis(_lp, filename, info);
        }

        /// <summary>
        /// Writes current basis to a file.
        /// </summary>
        /// <param name="filename">Name of file to write the basis to.</param>
        /// <returns><c>true</c> if basis could be written from <paramref name="filename"/> and <c>false</c> if not.
        /// A <c>false</c> return value indicates an error.
        /// Specifically file could not be opened or written to.</returns>
        /// <remarks>
        /// <para>This method writes current basis to a file which can later be reused by <see cref="read_basis"/> to reset the basis.</para>
        /// <para>Setting an initial basis can speed up the solver considerably.
        /// It is the starting point from where the algorithm continues to find an optimal solution.</para>
        /// <para>When a restart is done, lp_solve continues at the last basis, except if <see cref="set_basis"/>,
        /// <see cref="default_basis"/>, <see cref="guess_basis"/> or <see cref="read_basis"/> is called.</para>
        /// <para>The basis in the file is written in <see href="http://lpsolve.sourceforge.net/5.5/bas-format.htm">MPS bas file format</see>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/write_basis.htm">Full C API documentation.</seealso>
        public bool write_basis(string filename)
        {
            return Interop.write_basis(_lp, filename);
        }

        /// <summary>
        /// Sets an initial basis of the model.
        /// </summary>
        /// <param name="bascolumn">An array with 1+<see cref="get_Nrows"/> or
        /// 1+<see cref="get_Nrows"/>+<see cref="get_Ncolumns"/> elements that specifies the basis.</param>
        /// <param name="nonbasic">If <c>false</c>, then <paramref name="bascolumn"/> must have 
        /// 1+<see cref="get_Nrows"/> elements and only contains the basic variables. 
        /// If <c>true</c>, then <paramref name="bascolumn"/> must have 1+<see cref="get_Nrows"/>+<see cref="get_Ncolumns"/> 
        /// elements and will also contain the non-basic variables.</param>
        /// <returns><c>true</c> if provided basis was set. <c>false</c> if not.
        /// If <c>false</c> then provided data was invalid.</returns>
        /// <remarks>
        /// <para>The array receives the basic variables and if <paramref name="nonbasic"/> is <c>true</c>,
        /// then also the non-basic variables.
        /// If an element is less then zero then it means on lower bound, else on upper bound.</para>
        /// <para>Element 0 of the array is unused.</para>
        /// <para>The default initial basis is bascolumn[x] = -x.</para>
        /// <para>Each element represents a basis variable.
        /// If the absolute value is between 1 and <see cref="get_Nrows"/>, it represents a slack variable 
        /// and if it is between <see cref="get_Nrows"/>+1 and <see cref="get_Nrows"/>+<see cref="get_Ncolumns"/>
        /// then it represents a regular variable.
        /// If the value is negative, then the variable is on its lower bound.
        /// If positive it is on its upper bound.</para>
        /// <para>Setting an initial basis can speed up the solver considerably.
        /// It is the starting point from where the algorithm continues to find an optimal solution.</para>
        /// <para>When a restart is done, lp_solve continues at the last basis, except if except if <see cref="set_basis"/>,
        /// <see cref="default_basis"/>, <see cref="guess_basis"/> or <see cref="read_basis"/> is called.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_basis.htm">Full C API documentation.</seealso>
        public bool set_basis(int[] bascolumn, bool nonbasic)
        {
            return Interop.set_basis(_lp, bascolumn, nonbasic);
        }

        /// <summary>
        /// Returns the basis of the model.
        /// </summary>
        /// <param name="bascolumn">An array with 1+<see cref="get_Nrows"/> or
        /// 1+<see cref="get_Nrows"/>+<see cref="get_Ncolumns"/> elements that will contain the basis after the call.</param>
        /// <param name="nonbasic">If <c>false</c>, then <paramref name="bascolumn"/> must have 
        /// 1+<see cref="get_Nrows"/> elements and only contains the basic variables. 
        /// If <c>true</c>, then <paramref name="bascolumn"/> must have 1+<see cref="get_Nrows"/>+<see cref="get_Ncolumns"/> 
        /// elements and will also contain the non-basic variables.</param>
        /// <returns><c>true</c> if a basis could be returned, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This can only be done after a successful solve.
        /// If the model is not successively solved then the method will return <c>false</c>.</para>
        /// <para>The array receives the basic variables and if <paramref name="nonbasic"/> is <c>true</c>,
        /// then also the non-basic variables.
        /// If an element is less then zero then it means on lower bound, else on upper bound.</para>
        /// <para>Element 0 of the array is set to 0.</para>
        /// <para>The default initial basis is bascolumn[x] = -x.</para>
        /// <para>Each element represents a basis variable.
        /// If the absolute value is between 1 and <see cref="get_Nrows"/>, it represents a slack variable 
        /// and if it is between <see cref="get_Nrows"/>+1 and <see cref="get_Nrows"/>+<see cref="get_Ncolumns"/>
        /// then it represents a regular variable.
        /// If the value is negative, then the variable is on its lower bound.
        /// If positive it is on its upper bound.</para>
        /// <para>Setting an initial basis can speed up the solver considerably.
        /// It is the starting point from where the algorithm continues to find an optimal solution.</para>
        /// <para>When a restart is done, lp_solve continues at the last basis, except if except if <see cref="set_basis"/>,
        /// <see cref="default_basis"/>, <see cref="guess_basis"/> or <see cref="read_basis"/> is called.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_basis.htm">Full C API documentation.</seealso>
        public bool get_basis(int[] bascolumn, bool nonbasic)
        {
            return Interop.get_basis(_lp, bascolumn, nonbasic);
        }

        /// <summary>
        /// Create a starting base from the provided guess vector.
        /// </summary>
        /// <param name="guessvector">A vector that must contain a feasible solution vector.
        /// It must contain at least 1+<see cref="get_Ncolumns"/> elements.
        /// Element 0 is not used.</param>
        /// <param name="basisvector">When successful, this vector contains a feasible basis corresponding to guessvector.
        /// The array must already be dimensioned for at least 1+<see cref="get_Nrows"/>+<see cref="get_Ncolumns"/> elements.
        /// When the method returns successfully, <paramref name="basisvector"/> is filled with the basis.
        /// This array can be provided to <see cref="set_basis"/>.</param>
        /// <returns><c>true</c> if a valid base could be determined, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method is meant to find a basis based on provided variable values.
        /// This basis can be provided to lp_solve via <see cref="set_basis"/>.
        /// This can result in getting faster to an optimal solution.
        /// However the simplex algorithm doesn't guarantee you that.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/guess_basis.htm">Full C API documentation.</seealso>
        public bool guess_basis(double[] guessvector, int[] basisvector)
        {
            return Interop.guess_basis(_lp, guessvector, basisvector);
        }

        /// <summary>
        /// Returns which basis crash mode must be used.
        /// </summary>
        /// <returns><c>true</c> if a valid base could be determined, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Default is <see cref="lpsolve_basiscrash.CRASH_NONE"/></para>
        /// <para>When no base crash is done (the default), the initial basis from which lp_solve 
        /// starts to solve the model is the basis containing all slack or artificial variables that 
        /// is automatically associated with each constraint.</para>
        /// <para>When base crash is enabled, a heuristic "crash procedure" is executed before the 
        /// first simplex iteration to quickly choose a basis matrix that has fewer artificial variables.
        /// This procedure tends to reduce the number of iterations to optimality since a number of 
        /// iterations are skipped.
        /// lp_solve starts iterating from this basis until optimality.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_basiscrash.htm">Full C API documentation.</seealso>
        public lpsolve_basiscrash get_basiscrash()
        {
            return Interop.get_basiscrash(_lp);
        }

        /// <summary>
        /// Returns which basis crash mode must be used.
        /// </summary>
        /// <param name="mode">Specifies which basis crash mode must be used.</param>
        /// <remarks>
        /// <para>Default is <see cref="lpsolve_basiscrash.CRASH_NONE"/></para>
        /// <para>When no base crash is done (the default), the initial basis from which lp_solve 
        /// starts to solve the model is the basis containing all slack or artificial variables that 
        /// is automatically associated with each constraint.</para>
        /// <para>When base crash is enabled, a heuristic "crash procedure" is executed before the 
        /// first simplex iteration to quickly choose a basis matrix that has fewer artificial variables.
        /// This procedure tends to reduce the number of iterations to optimality since a number of 
        /// iterations are skipped.
        /// lp_solve starts iterating from this basis until optimality.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_basiscrash.htm">Full C API documentation.</seealso>
        public void set_basiscrash(lpsolve_basiscrash mode)
        {
            Interop.set_basiscrash(_lp, mode);
        }

        /// <summary>
        /// Returns if there is a basis factorization package (BFP) available.
        /// </summary>
        /// <returns><c>true</c> if there is a BFP available, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>There should always be a BFP available, else lpsolve can not solve.
        /// Normally lpsolve is compiled with a default BFP.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/BFP.htm">Basis Factorization Packages</see> for a complete description on BFPs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/has_BFP.htm">Full C API documentation.</seealso>
        public bool has_BFP()
        {
            return Interop.has_BFP(_lp);
        }

        /// <summary>
        /// Returns if the native (build-in) basis factorization package (BFP) is used, or an external package.
        /// </summary>
        /// <returns><c>true</c> if the native (build-in) BFP is used, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method checks if an external basis factorization package (BFP) is set or not.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/BFP.htm">Basis Factorization Packages</see> for a complete description on BFPs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_nativeBFP.htm">Full C API documentation.</seealso>
        public bool is_nativeBFP()
        {
            return Interop.is_nativeBFP(_lp);
        }

        /// <summary>
        /// Sets the basis factorization package.
        /// </summary>
        /// <param name="filename">The name of the BFP package. Currently following BFPs are implemented:
        /// <list type="table">
        /// <item>
        /// <term>"bfp_etaPFI"</term><description>original lp_solve product form of the inverse.</description>
        /// <term>"bfp_LUSOL"</term><description>LU decomposition.</description>
        /// <term>"bfp_GLPK"</term><description>GLPK LU decomposition.</description>
        /// <term><c>null</c></term><description>The default BFP package.</description>
        /// </item>
        /// </list>
        /// However the user can also build his own BFP packages ...
        /// </param>
        /// <returns><c>true</c> if the call succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method sets the basis factorization package (BFP).
        /// See <see href="http://lpsolve.sourceforge.net/5.5/BFP.htm">Basis Factorization Packages</see> for a complete description on BFPs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_BFP.htm">Full C API documentation.</seealso>
        public bool set_BFP(string filename)
        {
            return Interop.set_BFP(_lp, filename);
        }

#endregion

#region Pivoting

        /// <summary>
        /// Returns the maximum number of pivots between a re-inversion of the matrix.
        /// </summary>
        /// <returns>Returns the maximum number of pivots between a re-inversion of the matrix.</returns>
        /// <remarks>
        /// <para>For stability reasons, lp_solve re-inverts the matrix on regular times. max_num_inv determines how frequently this inversion is done. This can influence numerical stability. However, the more often this is done, the slower the solver becomes.</para>
        /// <para>The default is 250 for the LUSOL bfp and 42 for the other BFPs.</para></remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_maxpivot.htm">Full C API documentation.</seealso>
        public int get_maxpivot()
        {
            return Interop.get_maxpivot(_lp);
        }

        /// <summary>
        /// Sets the maximum number of pivots between a re-inversion of the matrix.
        /// </summary>
        /// <param name="max_num_inv">The maximum number of pivots between a re-inversion of the matrix.</param>
        /// <remarks>
        /// <para>For stability reasons, lp_solve re-inverts the matrix on regular times. max_num_inv determines how frequently this inversion is done. This can influence numerical stability. However, the more often this is done, the slower the solver becomes.</para>
        /// <para>The default is 250 for the LUSOL bfp and 42 for the other BFPs.</para></remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_maxpivot.htm">Full C API documentation.</seealso>
        public void set_maxpivot(int max_num_inv)
        {
            Interop.set_maxpivot(_lp, max_num_inv);
        }

        /// <summary>
        /// Returns the pivot rule and modes. See <see cref="lpsolve_pivot_rule"/> and <see cref="lpsolve_pivot_modes"/> for possible values.
        /// </summary>
        /// <returns>The pivot rule (rule for selecting row and column entering/leaving) and mode.</returns>
        /// <remarks>
        /// <para>The rule is an exclusive option and the mode is a modifier to the rule.
        /// This rule/mode can influence solving times considerably.
        /// Depending on the model one rule/mode can be best and for another model another rule/mode.</para>
        /// <para>The default rule is <see cref="lpsolve_pivot_rule.PRICER_DEVEX"/> and the default mode is <see cref="lpsolve_pivot_modes.PRICE_ADAPTIVE"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_pivoting.htm">Full C API documentation.</seealso>
        public PivotRuleAndModes get_pivoting()
        {
            int pivoting = Interop.get_pivoting(_lp);
            int mask = (int)lpsolve_pivot_rule.PRICER_STEEPESTEDGE;
            int rule = pivoting & mask;
            int modes = pivoting & ~mask;
            return new PivotRuleAndModes(
                (lpsolve_pivot_rule)rule,
                (lpsolve_pivot_modes)modes
                );
        }

        /// <summary>
        /// Sets the pivot rule and modes.
        /// </summary>
        /// <param name="rule">The pivot <see cref="lpsolve_pivot_rule">rule</see> (rule for selecting row and column entering/leaving).</param>
        /// <param name="modes">The <see cref="lpsolve_pivot_modes">modes</see> modifying the <see cref="lpsolve_pivot_rule">rule</see>.</param>
        /// <remarks>
        /// <para>The rule is an exclusive option and the mode is a modifier to the rule.
        /// This rule/mode can influence solving times considerably.
        /// Depending on the model one rule/mode can be best and for another model another rule/mode.</para>
        /// <para>The default rule is <see cref="lpsolve_pivot_rule.PRICER_DEVEX"/> and the default mode is <see cref="lpsolve_pivot_modes.PRICE_ADAPTIVE"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_pivoting.htm">Full C API documentation.</seealso>
        public void set_pivoting(lpsolve_pivot_rule rule, lpsolve_pivot_modes modes)
        {
            Interop.set_pivoting(_lp, ((int)rule)| ((int)modes));
        }

        /// <summary>
        /// Checks if the specified pivot rule is active.
        /// </summary>
        /// <param name="rule">Rule to check.</param>
        /// <returns><c>true</c> if the specified pivot rule is active, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>
        /// This rule/mode can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is <see cref="lpsolve_pivot_rule.PRICER_DEVEX"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_piv_rule.htm">Full C API documentation.</seealso>
        public bool is_piv_rule(lpsolve_pivot_rule rule)
        {
            return Interop.is_piv_rule(_lp, rule);
        }


        /// <summary>
        /// Checks if the pivot mode specified in <paramref name="testmask"/> is active.
        /// </summary>
        /// <param name="testmask">Any combination of <see cref="lpsolve_pivot_modes"/> to check if they are active.</param>
        /// <returns><c>true</c> if all the specified modes are active, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// The pivot mode is an extra modifier to the pivot rule. Any combination (OR) of the defined values is possible.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_piv_mode.htm">Full C API documentation.</seealso>
        public bool is_piv_mode(lpsolve_pivot_modes testmask)
        {
            return Interop.is_piv_mode(_lp, testmask);
        }

#endregion

#region Scaling

        /// <summary>
        /// Gets the relative scaling convergence criterion for the active scaling mode.
        /// </summary>
        /// <returns>The relative scaling convergence criterion for the active scaling mode;
        /// the integer part specifies the maximum number of iterations.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_scalelimit.htm">Full C API documentation.</seealso>
        public double get_scalelimit()
        {
            return Interop.get_scalelimit(_lp);
        }

        /// <summary>
        /// Sets the relative scaling convergence criterion for the active scaling mode;
        /// the integer part specifies the maximum number of iterations.
        /// </summary>
        /// <param name="scalelimit">The relative scaling convergence criterion for the active scaling mode;
        /// the integer part specifies the maximum number of iterations.</param>
        /// <remarks>
        /// Default is 5.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_scalelimit.htm">Full C API documentation.</seealso>
        public void set_scalelimit(double scalelimit)
        {
            Interop.set_scalelimit(_lp, scalelimit);
        }

        /// <summary>
        /// Specifies which scaling algorithm and parameters are used.
        /// </summary>
        /// <returns>The scaling algorithm and parameters that are used.</returns>
        /// <remarks>
        /// <para>
        /// This can influence numerical stability considerably.
        /// It is advisable to always use some sort of scaling.</para>
        /// <para><see cref="set_scaling"/> must be called before solve is called.</para>
        /// See <see cref="ScalingAlgorithmAndParameters" /> for more information on scaling.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_scaling.htm">Full C API documentation.</seealso>
        public ScalingAlgorithmAndParameters get_scaling()
        {
            int scaling = Interop.get_scaling(_lp);
            int mask = (int)lpsolve_scale_algorithm.SCALE_CURTISREID;
            int algorithm = scaling & mask;
            int parameters = scaling & ~mask;
            return new ScalingAlgorithmAndParameters(
                (lpsolve_scale_algorithm)algorithm,
                (lpsolve_scale_parameters)parameters
                );
        }

        /// <summary>
        /// Specifies which scaling algorithm and parameters must be used.
        /// </summary>
        /// <param name="algorithm">Specifies which scaling algorithm must be used.</param>
        /// <param name="parameters">Specifies which parameters to apply to scaling <paramref name="algorithm"/>.</param>
        /// <remarks>
        /// <para>
        /// This can influence numerical stability considerably.
        /// It is advisable to always use some sort of scaling.</para>
        /// <para><see cref="set_scaling"/> must be called before solve is called.</para>
        /// See <see cref="ScalingAlgorithmAndParameters" /> for more information on scaling.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_scaling.htm">Full C API documentation.</seealso>
        public void set_scaling(lpsolve_scale_algorithm algorithm, lpsolve_scale_parameters parameters)
        {
            Interop.set_scaling(_lp, ((int)algorithm)|((int)parameters));
        }

        /// <summary>
        /// Returns if scaling algorithm and parameters specified are active.
        /// </summary>
        /// <param name="algorithmMask">Specifies which scaling algorithm to verify.
        /// Optional with default = <see cref="lpsolve_scale_algorithm.SCALE_NONE"/></param>
        /// <param name="parameterMask">Specifies which parameters must be verified.
        /// Optional with default = <see cref="lpsolve_scale_parameters.SCALE_NONE"/></param>
        /// <returns><c>true</c> if scaling algorithm and parameters specified are active, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// See <see cref="ScalingAlgorithmAndParameters" /> for more information on scaling.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_scalemode.htm">Full C API documentation.</seealso>
        public bool is_scalemode(
            lpsolve_scale_algorithm algorithmMask = lpsolve_scale_algorithm.SCALE_NONE,
            lpsolve_scale_parameters parameterMask = lpsolve_scale_parameters.SCALE_NONE)
        {
            return Interop.is_scalemode(_lp, ((int)algorithmMask) | ((int)parameterMask));
        }

        /// <summary>
        /// Returns if scaling algorithm specified is active.
        /// </summary>
        /// <param name="algorithm">Specifies which scaling algorithm to verify.</param>
        /// <returns><c>true</c> if scaling algorithm specified is active, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// See <see cref="ScalingAlgorithmAndParameters" /> for more information on scaling.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_scaletype.htm">Full C API documentation.</seealso>
        public bool is_scaletype(lpsolve_scale_algorithm algorithm)
        {
            return Interop.is_scaletype(_lp, algorithm);
        }

        /// <summary>
        /// Returns if integer scaling is active.
        /// </summary>
        /// <returns><c>true</c> if <see cref="lpsolve_scale_parameters.SCALE_INTEGERS"/> was set with <see cref="set_scaling"/>.</returns>
        /// <remarks>
        /// By default, integers are not scaled, you mus call <see cref="set_scaling"/>
        /// with <see cref="lpsolve_scale_parameters.SCALE_INTEGERS"/> to activate this feature.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_integerscaling.htm">Full C API documentation.</seealso>
        public bool is_integerscaling()
        {
            return Interop.is_integerscaling(_lp);
        }

        /// <summary>
        /// Unscales the model.
        /// </summary>
        /// <remarks>
        /// The unscale method unscales the model.
        /// Scaling can influence numerical stability considerably.
        /// It is advisable to always use some sort of scaling.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/unscale.htm">Full C API documentation.</seealso>
        public void unscale()
        {
            Interop.unscale(_lp);
        }

#endregion

#region Branching

        /// <summary>
        /// Returns, for the specified variable, which branch to take first in branch-and-bound algorithm.
        /// </summary>
        /// <param name="column">The column number of the variable on which the mode must be returned.
        /// It must be between 1 and the number of columns in the model.
        /// If it is not within this range, the return value is the value of <see cref="get_bb_floorfirst"/>.</param>
        /// <returns>Returns which branch to take first in branch-and-bound algorithm.</returns>
        /// <remarks>
        /// This method returns which branch to take first in branch-and-bound algorithm.
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.
        /// When no value was set via <see cref="set_var_branch"/>, the return value is the value of <see cref="get_bb_floorfirst"/>.
        /// It also returns the value of <see cref="get_bb_floorfirst"/> when <see cref="set_var_branch"/> was called with branch mode <see cref="lpsolve_branch.BRANCH_DEFAULT">BRANCH_DEFAULT (3)</see>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_var_branch.htm">Full C API documentation.</seealso>
        public lpsolve_branch get_var_branch(int column)
        {
            return Interop.get_var_branch(_lp, column);
        }

        /// <summary>
        /// Specifies, for the specified variable, which branch to take first in branch-and-bound algorithm.
        /// </summary>
        /// <param name="column">The column number of the variable on which the mode must be set.
        /// It must be between 1 and the number of columns in the model.</param>
        /// <param name="branch_mode">Specifies, for the specified variable, which branch to take first in branch-and-bound algorithm.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>
        /// This method specifies which branch to take first in branch-and-bound algorithm.
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.
        /// </para>
        /// <para>The default is <see cref="lpsolve_branch.BRANCH_DEFAULT">BRANCH_DEFAULT (3)</see> which means that 
        /// the branch mode specified with <see cref="set_bb_floorfirst"/> method must be used.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_var_branch.htm">Full C API documentation.</seealso>
        public bool set_var_branch(int column, lpsolve_branch branch_mode)
        {
            return Interop.set_var_branch(_lp, column, branch_mode);
        }

        /// <summary>
        /// Returns whether the branch-and-bound algorithm stops at first found solution or not.
        /// </summary>
        /// <returns><c>true</c> if the branch-and-bound algorithm stops at first found solution, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The method returns if the branch-and-bound algorithm stops at the first found solution or not.
        /// Stopping at the first found solution can be useful if you are only interested for a solution,
        /// but not necessarily (and most probably) the most optimal solution.</para>
        /// <para>The default is <c>false</c>: not stop at first found solution.</para></remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_break_at_first.htm">Full C API documentation.</seealso>
        public bool is_break_at_first()
        {
            return Interop.is_break_at_first(_lp);
        }

        /// <summary>
        /// Specifies whether the branch-and-bound algorithm stops at first found solution or not.
        /// </summary>
        /// <param name="break_at_first"><c>true</c> if the branch-and-bound algorithm should break at first found solution, <c>false</c> if not.</param>
        /// <remarks>
        /// <para>The method specifies if the branch-and-bound algorithm stops at the first found solution or not.
        /// Stopping at the first found solution can be useful if you are only interested for a solution,
        /// but not necessarily (and most probably) the most optimal solution.</para>
        /// <para>The default is <c>false</c>: not stop at first found solution.</para></remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_break_at_first.htm">Full C API documentation.</seealso>
        public void set_break_at_first(bool break_at_first)
        {
            Interop.set_break_at_first(_lp, break_at_first);
        }

        /// <summary>
        /// Returns the value which would cause the branch-and-bound algorithm to stop when the objective value reaches it.
        /// </summary>
        /// <returns>Returns the value to break on.</returns>
        /// <remarks>
        /// <para>The method returns the value at which the branch-and-bound algorithm stops when the objective value
        /// is better than this value. Stopping at a given objective value can be useful if you are only interested
        /// for a solution that has an objective value which is at least a given value, but not necessarily
        /// (and most probably not) the most optimal solution.</para>
        /// <para>The default value is (-) infinity (or +infinity when maximizing).</para></remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_break_at_value.htm">Full C API documentation.</seealso>
        public double get_break_at_value()
        {
            return Interop.get_break_at_value(_lp);
        }

        /// <summary>
        /// Specifies the value which would cause the branch-and-bound algorithm to stop when the objective value reaches it.
        /// </summary>
        /// <param name="break_at_value">The value to break on.</param>
        /// <remarks>
        /// <para>The method sets the value at which the branch-and-bound algorithm stops when the objective value
        /// is better than this value. Stopping at a given objective value can be useful if you are only interested
        /// for a solution that has an objective value which is at least a given value, but not necessarily
        /// (and most probably not) the most optimal solution.</para>
        /// <para>The default value is (-) infinity (or +infinity when maximizing).</para></remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_break_at_value.htm">Full C API documentation.</seealso>
        public void set_break_at_value(double break_at_value)
        {
            Interop.set_break_at_value(_lp, break_at_value);
        }

        /// <summary>
        /// Returns the branch-and-bound rule.
        /// </summary>
        /// <returns>Returns the <see cref="lpsolve_BBstrategies">branch-and-bound rule</see>.</returns>
        /// <remarks>
        /// <para>The method returns the branch-and-bound rule for choosing which non-integer variable is to be selected.
        /// This rule can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is NODE_PSEUDONONINTSELECT + NODE_GREEDYMODE + NODE_DYNAMICMODE + NODE_RCOSTFIXING(17445).</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_bb_rule.htm">Full C API documentation.</seealso>
        public lpsolve_BBstrategies get_bb_rule()
        {
            return Interop.get_bb_rule(_lp);
        }

        /// <summary>
        /// Specifies the branch-and-bound rule.
        /// </summary>
        /// <param name="bb_rule">The <see cref="lpsolve_BBstrategies">branch-and-bound rule</see> to set.</param>
        /// <remarks>
        /// <para>The method specifies the branch-and-bound rule for choosing which non-integer variable is to be selected.
        /// This rule can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is NODE_PSEUDONONINTSELECT + NODE_GREEDYMODE + NODE_DYNAMICMODE + NODE_RCOSTFIXING(17445).</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bb_rule.htm">Full C API documentation.</seealso>
        public void set_bb_rule(lpsolve_BBstrategies bb_rule)
        {
            Interop.set_bb_rule(_lp, bb_rule);
        }

        /// <summary>
        /// Returns the maximum branch-and-bound depth.
        /// </summary>
        /// <returns>Returns the maximum branch-and-bound depth</returns>
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
        public int get_bb_depthlimit()
        {
            return Interop.get_bb_depthlimit(_lp);
        }

        /// <summary>
        /// Sets the maximum branch-and-bound depth.
        /// </summary>
        /// <param name="bb_maxlevel">Specifies the maximum branch-and-bound depth.
        /// A positive value means that the depth is absoluut.
        /// A negative value means a relative B&amp;B depth limit.
        /// The "order" of a MIP problem is defined to be 2x the number of binary variables plus the number of SC and SOS variables.
        /// A relative value of -x results in a maximum depth of x times the order of the MIP problem.</param>
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
        public void set_bb_depthlimit(int bb_maxlevel)
        {
            Interop.set_bb_depthlimit(_lp, bb_maxlevel);
        }

        /// <summary>
        /// Returns which branch to take first in branch-and-bound algorithm.
        /// </summary>
        /// <returns>Returns which branch to take first in branch-and-bound algorithm.</returns>
        /// <remarks>
        /// <para>The method returns which branch to take first in branch-and-bound algorithm.
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is <see cref="lpsolve_branch.BRANCH_AUTOMATIC"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_bb_floorfirst.htm">Full C API documentation.</seealso>
        public lpsolve_branch get_bb_floorfirst()
        {
            return Interop.get_bb_floorfirst(_lp);
        }

        /// <summary>
        /// Specifies which branch to take first in branch-and-bound algorithm.
        /// </summary>
        /// <param name="bb_floorfirst">Specifies which branch to take first in branch-and-bound algorithm.</param>
        /// <remarks>
        /// <para>The method specifies which branch to take first in branch-and-bound algorithm.
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is <see cref="lpsolve_branch.BRANCH_AUTOMATIC"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bb_floorfirst.htm">Full C API documentation.</seealso>
        public void set_bb_floorfirst(lpsolve_branch bb_floorfirst)
        {
            Interop.set_bb_floorfirst(_lp, bb_floorfirst);
        }

#endregion

        /// <summary>
        /// Returns the iterative improvement level.
        /// </summary>
        /// <returns>The iterative improvement level</returns>
        /// <remarks>
        /// The default is <see cref="lpsolve_improves.IMPROVE_DUALFEAS"/> + <see cref="lpsolve_improves.IMPROVE_THETAGAP"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_improve.htm">Full C API documentation.</seealso>
        public lpsolve_improves get_improve()
        {
            return Interop.get_improve(_lp);
        }

        /// <summary>
        /// Specifies the iterative improvement level.
        /// </summary>
        /// <param name="improve">The iterative improvement level.</param>
        /// <remarks>
        /// The default is <see cref="lpsolve_improves.IMPROVE_DUALFEAS"/> + <see cref="lpsolve_improves.IMPROVE_THETAGAP"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_improve.htm">Full C API documentation.</seealso>
        public void set_improve(lpsolve_improves improve)
        {
            Interop.set_improve(_lp, improve);
        }

        /// <summary>
        /// Returns the negative value below which variables are split into a negative and a positive part.
        /// </summary>
        /// <return>The negative value below which variables are split into a negative and a positive part.</return>
        /// <remarks>
        ///  <para>This value is always zero or negative.</para>
        ///  <para>In some cases, negative variables must be split in a positive part and a negative part.
        ///  This is when a negative lower or upper bound is set on a variable.
        ///  If a bound is less than this value, it is <strong>possibly</strong> split.</para>
        ///  <para>The default is -1e6.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_negrange.htm">Full C API documentation.</seealso>
        public double get_negrange()
        {
            return Interop.get_negrange(_lp);
        }

        /// <summary>
        /// Sets the negative value below which variables are split into a negative and a positive part.
        /// </summary>
        /// <param name="negrange">The negative value below which variables are split into a negative and a positive part.</param>
        /// <remarks>
        ///  <para>This value must always be zero or negative. If a positive value is specified, then 0 is taken.</para>
        ///  <para>In some cases, negative variables must be split in a positive part and a negative part.
        ///  This is when a negative lower or upper bound is set on a variable.
        ///  If a bound is less than this value, it is possibly split.</para>
        ///  <para>The default is -1e6.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_negrange.htm">Full C API documentation.</seealso>
        public void set_negrange(double negrange)
        {
            Interop.set_negrange(_lp, negrange);
        }

        /// <summary>
        /// Returns the used degeneracy rule.
        /// </summary>
        /// <returns>The used degeneracy rule (can be any combination of <see cref="lpsolve_anti_degen"/>).</returns>
        /// <remarks>
        ///  <para>The default is <see cref="lpsolve_anti_degen.ANTIDEGEN_INFEASIBLE"/>
        ///  + <see cref="lpsolve_anti_degen.ANTIDEGEN_STALLING"/>
        ///  + <see cref="lpsolve_anti_degen.ANTIDEGEN_FIXEDVARS"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_anti_degen.htm">Full C API documentation.</seealso>
        public lpsolve_anti_degen get_anti_degen()
        {
            return Interop.get_anti_degen(_lp);
        }

        /// <summary>
        /// Returns if the degeneracy rules specified in <paramref name="testmask"/> are active.
        /// </summary>
        /// <param name="testmask">Any combination of <see cref="lpsolve_anti_degen"/> to check if they are active.</param>
        /// <returns><c>true</c> if all rules specified in <paramref name="testmask"/> are active, <c>false</c> otherwise.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_anti_degen.htm">Full C API documentation.</seealso>
        public bool is_anti_degen(lpsolve_anti_degen testmask)
        {
            return Interop.is_anti_degen(_lp, testmask);
        }

        /// <summary>
        /// Specifies if special handling must be done to reduce degeneracy/cycling while solving.
        /// </summary>
        /// <param name="anti_degen">The degeneracy rule that must be used (can be any combination of <see cref="lpsolve_anti_degen"/>).</param>
        /// <remarks>
        ///  <para>Setting this flag can avoid cycling, but can also increase numerical instability.</para>
        ///  <para>The default is <see cref="lpsolve_anti_degen.ANTIDEGEN_INFEASIBLE"/>
        ///  + <see cref="lpsolve_anti_degen.ANTIDEGEN_STALLING"/>
        ///  + <see cref="lpsolve_anti_degen.ANTIDEGEN_FIXEDVARS"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_anti_degen.htm">Full C API documentation.</seealso>
        public void set_anti_degen(lpsolve_anti_degen anti_degen)
        {
            Interop.set_anti_degen(_lp, anti_degen);
        }

        /// <summary>
        /// Resets parameters back to their default values.
        /// </summary>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/reset_params.htm">Full C API documentation.</seealso>
        public void reset_params()
        {
            Interop.reset_params(_lp);
        }

        /// <summary>
        /// Read settings from a parameter file.
        /// </summary>
        /// <param name="filename">Name of file from which to read parameters.</param>
        /// <param name="options">Optional options. Can be:
        /// <c>-h header</c>: Read parameters at specified header.
        /// By default this is header named <c>Default</c></param>
        /// <returns><c>true</c> if parameters could be read, <c>false</c> otherwise.</returns>
        /// <remarks>
        ///  <para>This file has an ini-format as used by Windows applications.
        ///  All parameters are read from a header.
        ///  This is by default <c>[Default]</c>.
        ///  The header can be specified in the options parameter.
        ///  Other headers are ignored.</para>
        ///  <para>Example parameter file:</para>
        ///  <c>
        ///  [Default]
        ///  ; lp_solve version 5.5 settings
        ///  
        ///  anti_degen = ANTIDEGEN_FIXEDVARS + ANTIDEGEN_STALLING + ANTIDEGEN_INFEASIBLE
        ///  basiscrash=CRASH_NONE
        ///  improve = IMPROVE_DUALFEAS + IMPROVE_THETAGAP
        ///  maxpivot=250
        ///  negrange=-1e+006
        ///  pivoting=PRICER_DEVEX + PRICE_ADAPTIVE
        ///  presolve = PRESOLVE_NONE
        ///  presolveloops=2147483647
        ///  scalelimit=5
        ///  scaling=SCALE_GEOMETRIC + SCALE_EQUILIBRATE + SCALE_INTEGERS
        ///  simplextype = SIMPLEX_DUAL_PRIMAL
        ///  bb_depthlimit=-50
        ///  bb_floorfirst=BRANCH_AUTOMATIC
        ///  bb_rule = NODE_PSEUDONONINTSELECT + NODE_GREEDYMODE + NODE_DYNAMICMODE + NODE_RCOSTFIXING
        ///  ; break_at_first=0
        ///  ;break_at_value=-1e+030
        ///  mip_gap_abs=1e-011
        ///  mip_gap_rel=1e-011
        ///  epsint=1e-007
        ///  epsb=1e-010
        ///  epsd=1e-009
        ///  epsel=1e-012
        ///  epsperturb=1e-005
        ///  epspivot=2e-007
        ///  infinite=1e+030
        ///  ;debug=0
        ///  ;obj_bound=1e+030
        ///  ;print_sol=0
        ///  ;timeout=0
        ///  ;trace=0
        ///  ;verbose=NORMAL
        ///  </c>
        ///  <para>
        ///  Note that there are some options commented out (;).
        ///  This is done because these options can not be used in general for all models or because they are debug/trace/print options.
        ///  These options can be made active and will be read by <see cref="read_params"/> but note again that they are possible 
        ///  dangerous to be used in general (except for the debug/trace/print options). Note that there are two kind of entries:
        ///  <list type="bullet">
        ///  <item>Numerical values</item>
        ///  <item>Options</item>
        ///  </list>
        ///  Numerical values can be integer values like <c>maxpivot</c> or floating point values like <c>epsel</c></para>
        ///  <para>Options are a combination of constants as defined in the manual.
        ///  Multiple options are added with +. 
        ///  For example option <c>anti_degen</c>.</para>
        /// </remarks>
        /// <example>
        /// </example>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_params.htm">Full C API documentation.</seealso>
        public bool read_params(string filename, string options)
        {
            return Interop.read_params(_lp, filename, options);
        }

        /// <summary>
        /// Write settings from a parameter file.
        /// </summary>
        /// <param name="filename">Name of file into which to write parameters.</param>
        /// <param name="options">Optional options. Can be:
        /// <c>-h header</c>: Write parameters at specified header.
        /// By default this is header named <c>Default</c></param>
        /// <returns><c>true</c> if parameters could be written, <c>false</c> otherwise.</returns>
        /// <remarks>
        ///  <para>This file has an ini-format as used by Windows applications.
        ///  All parameters are written under a header.
        ///  This is by default <c>[Default]</c>.
        ///  The header can be specified in the options parameter.
        ///  Other headers are preserved.</para>
        ///  <para>Example parameter file:</para>
        ///  <c>
        ///  [Default]
        ///  ; lp_solve version 5.5 settings
        ///  
        ///  anti_degen = ANTIDEGEN_FIXEDVARS + ANTIDEGEN_STALLING + ANTIDEGEN_INFEASIBLE
        ///  basiscrash=CRASH_NONE
        ///  improve = IMPROVE_DUALFEAS + IMPROVE_THETAGAP
        ///  maxpivot=250
        ///  negrange=-1e+006
        ///  pivoting=PRICER_DEVEX + PRICE_ADAPTIVE
        ///  presolve = PRESOLVE_NONE
        ///  presolveloops=2147483647
        ///  scalelimit=5
        ///  scaling=SCALE_GEOMETRIC + SCALE_EQUILIBRATE + SCALE_INTEGERS
        ///  simplextype = SIMPLEX_DUAL_PRIMAL
        ///  bb_depthlimit=-50
        ///  bb_floorfirst=BRANCH_AUTOMATIC
        ///  bb_rule = NODE_PSEUDONONINTSELECT + NODE_GREEDYMODE + NODE_DYNAMICMODE + NODE_RCOSTFIXING
        ///  ; break_at_first=0
        ///  ;break_at_value=-1e+030
        ///  mip_gap_abs=1e-011
        ///  mip_gap_rel=1e-011
        ///  epsint=1e-007
        ///  epsb=1e-010
        ///  epsd=1e-009
        ///  epsel=1e-012
        ///  epsperturb=1e-005
        ///  epspivot=2e-007
        ///  infinite=1e+030
        ///  ;debug=0
        ///  ;obj_bound=1e+030
        ///  ;print_sol=0
        ///  ;timeout=0
        ///  ;trace=0
        ///  ;verbose=NORMAL
        ///  </c>
        ///  <para>
        ///  Note that there are some options commented out (;).
        ///  This is done because these options can not be used in general for all models or because they are debug/trace/print options.
        ///  These options can be made active and will be read by <see cref="read_params"/> but note again that they are possible 
        ///  dangerous to be used in general (except for the debug/trace/print options). Note that there are two kind of entries:
        ///  <list type="bullet">
        ///  <item>Numerical values</item>
        ///  <item>Options</item>
        ///  </list>
        ///  Numerical values can be integer values like <c>maxpivot</c> or floating point values like <c>epsel</c></para>
        ///  <para>Options are a combination of constants as defined in the manual.
        ///  Multiple options are added with +. 
        ///  For example option <c>anti_degen</c>.</para>
        /// </remarks>
        /// <example>
        /// </example>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/write_params.htm">Full C API documentation.</seealso>
        public bool write_params(string filename, string options)
        {
            return Interop.write_params(_lp, filename, options);
        }

        /// <summary>
        /// Returns the desired combination of primal and dual simplex algorithms.
        /// </summary>
        /// <returns>The desired combination of primal and dual simplex algorithms.</returns>
        /// <remarks>
        ///  The default is <see cref="lpsolve_simplextypes.SIMPLEX_DUAL_PRIMAL"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_simplextype.htm">Full C API documentation.</seealso>
        public lpsolve_simplextypes get_simplextype()
        {
            return Interop.get_simplextype(_lp);
        }

        /// <summary>
        /// Sets the desired combination of primal and dual simplex algorithms.
        /// </summary>
        /// <param name="simplextype">The desired combination of primal and dual simplex algorithms.</param>
        /// <remarks>
        ///  The default is <see cref="lpsolve_simplextypes.SIMPLEX_DUAL_PRIMAL"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_simplextype.htm">Full C API documentation.</seealso>
        public void set_simplextype(lpsolve_simplextypes simplextype)
        {
            Interop.set_simplextype(_lp, simplextype);
        }

        /// <summary>
        /// Sets the desired combination of primal and dual simplex algorithms.
        /// </summary>
        /// <param name="dodual">
        ///  <para>When <c>true</c>, the simplex strategy is set to <see cref="lpsolve_simplextypes.SIMPLEX_DUAL_DUAL"/>.</para>
        ///  <para>When <c>false</c>, the simplex strategy is set to <see cref="lpsolve_simplextypes.SIMPLEX_PRIMAL_PRIMAL"/>.</para>
        /// </param>
        /// <remarks>
        ///  <para>The method <see cref="set_preferdual"/> with <paramref name="dodual"/> = <c>true</c> is a shortcut for <c>set_simplextype(lpsolve_simplextypes.SIMPLEX_DUAL_DUAL)</c></para>
        ///  <para>The method <see cref="set_preferdual"/> with <paramref name="dodual"/> = <c>false</c> is a shortcut for <c>set_simplextype(lpsolve_simplextypes.SIMPLEX_PRIMAL_PRIMAL)</c></para>
        ///  <para>The default is <see cref="lpsolve_simplextypes.SIMPLEX_DUAL_PRIMAL"/></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_preferdual.htm">Full C API documentation.</seealso>
        public void set_preferdual(bool dodual)
        {
            Interop.set_preferdual(_lp, dodual);
        }

        /// <summary>
        /// Returns the solution number that must be returned.
        /// </summary>
        /// <returns>The solution number that must be returned. This value gives the number of the solution that must be returned.</returns>
        /// <remarks>
        /// <para>This method is only valid if there are integer, semi-continious or SOS variables in the 
        /// model so that the branch-and-bound algoritm is used.
        /// If there are more solutions with the same objective value, then this number specifies 
        /// which solution must be returned. This can be used to retrieve all possible solutions. 
        /// Start with 1 till <see cref="get_solutioncount"/>.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_solutionlimit.htm">Full C API documentation.</seealso>
        public int get_solutionlimit()
        {
            return Interop.get_solutionlimit(_lp);
        }

        /// <summary>
        /// Sets the solution number that must be returned.
        /// </summary>
        /// <param name="limit">The solution number that must be returned. This value gives the number of the solution that must be returned.</param>
        /// <remarks>
        /// <para>This method is only valid if there are integer, semi-continious or SOS variables in the 
        /// model so that the branch-and-bound algoritm is used.
        /// If there are more solutions with the same objective value, then this number specifies 
        /// which solution must be returned. This can be used to retrieve all possible solutions. 
        /// Start with 1 till <see cref="get_solutioncount"/>.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_solutionlimit.htm">Full C API documentation.</seealso>
        public void set_solutionlimit(int limit)
        {
            Interop.set_solutionlimit(_lp, limit);
        }

        /// <summary>
        /// Gets the timeout.
        /// </summary>
        /// <returns>The number of seconds after which a timeout occurs.</returns>
        /// <remarks>
        /// <para>The <see cref="solve"/> method may not last longer than this time or
        /// the method returns with a timeout. There is no valid solution at this time.
        /// The default timeout is 0, resulting in no timeout.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_timeout.htm">Full C API documentation.</seealso>
        public int get_timeout()
        {
            return Interop.get_timeout(_lp);
        }

        /// <summary>
        /// Sets a timeout.
        /// </summary>
        /// <param name="sectimeout">The number of seconds after which a timeout occurs. If zero, then no timeout will occur.</param>
        /// <remarks>
        /// <para>The <see cref="solve"/> method may not last longer than this time or
        /// the method returns with a timeout. The default timeout is 0, resulting in no timeout.</para>
        /// <para>If a timout occurs, but there was already an integer solution found (that is possibly not the best),
        /// then solve will return <see cref="lpsolve_return.SUBOPTIMAL"/>.
        /// If there was no integer solution found yet or there are no integers or the solvers is still in the
        /// first phase where a REAL optimal solution is searched for, then solve will return <see cref="lpsolve_return.TIMEOUT"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_timeout.htm">Full C API documentation.</seealso>
        public void set_timeout(int sectimeout)
        {
            Interop.set_timeout(_lp, sectimeout);
        }

        /// <summary>
        /// Returns if variable or constraint names are used.
        /// </summary>
        /// <param name="isrow">Set to <c>false</c> if column information is needed and <c>true</c> if row information is needed.</param>
        /// <returns>A boolean value indicating if variable or constraint names are used.</returns>
        /// <remarks>
        /// <para>When a model is read from file or created via the API, variables and constraints can be named.
        /// These names are used to report information or to save the model in a given format.
        /// However, sometimes it is required to ignore these names and to use the internal names of lp_solve.
        /// This is for example the case when the names do not comply to the syntax rules of the format
        /// that will be used to write the model to.</para>
        /// <para>Names are used by default.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_use_names.htm">Full C API documentation.</seealso>
        public bool is_use_names(bool isrow)
        {
            return Interop.is_use_names(_lp, isrow);
        }

        /// <summary>
        /// Sets if variable or constraint names are used.
        /// </summary>
        /// <param name="isrow">Set to <c>false</c> if column information is needed and <c>true</c> if row information is needed.</param>
        /// <param name="use_names">If <c>false</c>, the names are not used, else they are.</param>
        /// <remarks>
        /// <para>When a model is read from file or created via the API, variables and constraints can be named.
        /// These names are used to report information or to save the model in a given format.
        /// However, sometimes it is required to ignore these names and to use the internal names of lp_solve.
        /// This is for example the case when the names do not comply to the syntax rules of the format
        /// that will be used to write the model to.</para>
        /// <para>Names are used by default.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_use_names.htm">Full C API documentation.</seealso>
        public void set_use_names(bool isrow, bool use_names)
        {
            Interop.set_use_names(_lp, isrow, use_names);
        }

        /// <summary>
        /// Returns if presolve level specified in <paramref name="testmask"/> is active.
        /// </summary>
        /// <param name="testmask">The combination of any of the <see cref="lpsolve_presolve"/> values to check whether they are active or not.</param>
        /// <returns><c>true</c>, if all levels specified in <paramref name="testmask"/> are active, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Presolve looks at the model and tries to simplify it so that solving times are shorter.
        /// For example a constraint on only one variable is converted to a bound on this variable
        /// (and the constraint is deleted). Note that the model dimensions can change because of this,
        /// so be careful with this. Both rows and columns can be deleted by the presolve.</para>
        /// <para>The default is not (<see cref="lpsolve_presolve.PRESOLVE_NONE"/>) doing a presolve.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_presolve.htm">Full C API documentation.</seealso>
        public bool is_presolve(lpsolve_presolve testmask)
        {
            return Interop.is_presolve(_lp, testmask);
        }

        /// <summary>
        /// Returns the number of times presolve is done.
        /// </summary>
        /// <returns>The number of times presolve is done.</returns>
        /// <remarks>
        /// After a presolve is done, another presolve can again result in elimination of extra rows and/or columns.
        /// This number specifies the maximum number of times this process is repeated.
        /// By default this is until presolve has nothing to do anymore.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_presolveloops.htm">Full C API documentation.</seealso>
        public int get_presolveloops()
        {
            return Interop.get_presolveloops(_lp);
        }

        /// <summary>
        /// Returns if a presolve must be done before solving.
        /// </summary>
        /// <returns>Can be the combination of any of the <see cref="lpsolve_presolve"/> values.</returns>
        /// <remarks>
        /// <para>Presolve looks at the model and tries to simplify it so that solving times are shorter.
        /// For example a constraint on only one variable is converted to a bound on this variable
        /// (and the constraint is deleted). Note that the model dimensions can change because of this,
        /// so be careful with this. Both rows and columns can be deleted by the presolve.</para>
        /// <para>
        /// The default is not (<see cref="lpsolve_presolve.PRESOLVE_NONE"/>) doing a presolve.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_presolve.htm">Full C API documentation.</seealso>
        public lpsolve_presolve get_presolve()
        {
            return Interop.get_presolve(_lp);
        }

        /// <summary>
        /// Specifies if a presolve must be done before solving.
        /// </summary>
        /// <param name="do_presolve">Specifies presolve level. Can be the combination of any of the <see cref="lpsolve_presolve"/> values.</param>
        /// <param name="maxloops">The maximum number of times presolve may be done.
        /// Use <see cref="get_presolveloops"/> if you don't want to change this value.</param>
        /// <remarks>
        /// <para>Presolve looks at the model and tries to simplify it so that solving times are shorter.
        /// For example a constraint on only one variable is converted to a bound on this variable
        /// (and the constraint is deleted). Note that the model dimensions can change because of this,
        /// so be careful with this. Both rows and columns can be deleted by the presolve.</para>
        /// <para>The <paramref name="maxloops"/> variable specifies the maximum number of times presolve
        /// is done. After a presolve is done, another presolve can again result in elimination of
        /// extra rows and/or columns.
        /// This number specifies the maximum number of times this process is repeated.
        /// By default this is until presolve has nothing to do anymore.
        /// Use <see cref="get_presolveloops"/> if you don't want to change this value.</para>
        /// <para>Note that <see cref="lpsolve_presolve.PRESOLVE_LINDEP"/> can result in deletion of rows
        /// (the linear dependent ones).
        /// <see cref="get_constraints"/> will then return only the values of the rows that are
        /// kept and the values of the deleted rows are not known anymore.
        /// The default is not (<see cref="lpsolve_presolve.PRESOLVE_NONE"/>) doing a presolve.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_presolve.htm">Full C API documentation.</seealso>
        public void set_presolve(lpsolve_presolve do_presolve, int maxloops)
        {
            Interop.set_presolve(_lp, do_presolve, maxloops);
        }

#endregion

#region Callback methods

        /// <summary>
        /// Sets a callback called regularly while solving the model to verify if solving should abort.
        /// </summary>
        /// <param name="newctrlc">The handler to call regularly while solving the model to verify if solving should abort.</param>
        /// <param name="ctrlchandle">A parameter that will be provided back to the abort callback.</param>
        /// <remarks>
        /// <para>When set, the abort callback is called regularly.
        /// The user can do whatever he wants in this callback.
        /// For example check if the user pressed abort.
        /// When the return value of this callback is <c>true</c>, then lp_solve aborts the solver and returns with an appropriate code.
        /// The abort callback can be cleared by specifying <c>null</c> as <paramref name="newctrlc"/>.</para>
        /// <para>The default is no abort callback (<c>null</c>).</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/put_abortfunc.htm">Full C API documentation.</seealso>
        public void put_abortfunc(ctrlcfunc newctrlc, IntPtr ctrlchandle)
        {
            Interop.put_abortfunc(_lp, newctrlc, ctrlchandle);
        }

        /// <summary>
        /// Sets a log callback.
        /// </summary>
        /// <param name="newlog">The log callback.</param>
        /// <param name="loghandle">A parameter that will be provided back to the log callback.</param>
        /// <remarks>
        /// <para>When set, the log callback is called when lp_solve has someting to report.
        /// The log callback can be cleared by specifying <c>null</c> as <paramref name="newlog"/>.</para>
        /// <para>This method is called at the same time as something is written to the file set via <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/put_logfunc.htm">Full C API documentation.</seealso>
        public void put_logfunc(logfunc newlog, IntPtr loghandle)
        {
            Interop.put_logfunc(_lp, newlog, loghandle);
        }

        /// <summary>
        /// Sets a message callback called upon certain events.
        /// </summary>
        /// <param name="newmsg">A handler to call when events defined in <paramref name="mask"/> occur.</param>
        /// <param name="msghandle">A parameter that will be provided back to the message handler.</param>
        /// <param name="mask">The mask of event types that should trigger a call to the <paramref name="newmsg"/> handler.</param>
        /// <remarks>
        /// This callback is called when a situation specified in mask occurs.
        /// Note that this callback is called while solving the model.
        /// This can be useful to follow the solving progress.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/put_msgfunc.htm">Full C API documentation.</seealso>
        public void put_msgfunc(msgfunc newmsg, IntPtr msghandle, lpsolve_msgmask mask)
        {
            Interop.put_msgfunc(_lp, newmsg, msghandle, mask);
        }


#endregion

#region Solve

        /// <summary>
        /// Solve the model.
        /// </summary>
        /// <returns>One of the <see cref="lpsolve_return"/> enum values.</returns>
        /// <remarks>
        /// <para><see cref="solve"/> can be called more than once.
        /// Between calls, the model may be modified in every way.
        /// Restrictions may be changed, matrix values may be changed and even rows and/or columns 
        /// may be added or deleted.</para>
        /// <para>If <see cref="set_timeout"/> was called before solve with a non-zero timeout and a timout occurs,
        /// and there was already an integer solution found (that is possibly not the best), 
        /// then solve will return <see cref="lpsolve_return.SUBOPTIMAL"/>.
        /// If there was no integer solution found yet or there are no integers or the solvers is still 
        /// in the first phase where a REAL optimal solution is searched for, then solve will return <see cref="lpsolve_return.TIMEOUT"/>.</para>
        /// <para>If <see cref="set_presolve"/> was called before solve, then it can happen that presolve 
        /// eliminates all rows and columns such that the solution is known by presolve.
        /// In that case, no solve is done.
        /// This also means that values of constraints and sensitivity are unknown.
        /// solve will return <see cref="lpsolve_return.PRESOLVED"/> in this case.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/solve.htm">Full C API documentation.</seealso>
        public lpsolve_return solve()
        {
            return Interop.solve(_lp);
        }

#endregion

#region Solution

        /// <summary>
        /// Gets the value of a constraint according to provided variable values.
        /// </summary>
        /// <param name="row">The row for which the constraint value must be calculated. Must be between 1 and number of rows in the lp model.</param>
        /// <param name="count">The number of items in <paramref name="primsolution"/> and <paramref name="nzindex"/>.</param>
        /// <param name="primsolution">The values of the variables.</param>
        /// <param name="nzindex">The variable indexes.</param>
        /// <returns>The value of the constraint as calculated with the provided variable values.</returns>
        /// <remarks>
        /// <para>If <paramref name="primsolution"/> is <c>null</c>, then the solution
        /// of the last solve is taken. <paramref name="count"/> and <paramref name="nzindex"/> are then ignored.</para>
        /// <para>If <paramref name="primsolution"/> is not <c>null</c>, and <paramref name="nzindex"/> is <c>null</c>,
        /// then the variable values are taken from <paramref name="primsolution"/> and element i must specify
        /// the value for variable i.
        /// Element 0 is not used and thus data starts from element 1. 
        /// The variable must then contain 1+<see cref="get_Ncolumns"/> elements.
        /// <paramref name="count"/> is ignored in that case.</para>
        /// <para>If <paramref name="primsolution"/> is not <c>null</c>, and <paramref name="nzindex"/> is not <c>null</c>,
        /// then the variable values are taken from <paramref name="primsolution"/>. 
        /// <paramref name="nzindex"/> contains the indexes of the variables and <paramref name="count"/> specifies 
        /// how many elements there are in <paramref name="primsolution"/> and <paramref name="nzindex"/>.
        /// So the data is then provided in a sparse vector.
        /// Elements start from index 0 in that case.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_constr_value.htm">Full C API documentation.</seealso>
        public double get_constr_value(int row, int count, double[] primsolution, int[] nzindex)
        {
            return Interop.get_constr_value(_lp, row, count, primsolution, nzindex);
        }

        /// <summary>
        /// Returns the values of the constraints.
        /// </summary>
        /// <param name="constr">An array that will contain the values of the constraints.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>These values are only valid after a successful <see cref="solve"/>. 
        /// The array must already be dimensioned with <see cref="get_Nrows"/> elements.
        /// Element 0 will contain the value of the first row, element 1 of the second row, ...</para>
        /// <para>Note that when <see cref="set_presolve"/> was called with parameter <see cref="lpsolve_presolve.PRESOLVE_LINDEP"/>
        /// that this can result in deletion of rows (the linear dependent ones). 
        /// This method will then return only the values of the rows that are kept and 
        /// the values of the deleted rows are not known anymore.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_constraints.htm">Full C API documentation.</seealso>
        public bool get_constraints(double[] constr)
        {
            return Interop.get_constraints(_lp, constr);
        }

        /// <summary>
        /// Returns the value(s) of the dual variables aka reduced costs.
        /// </summary>
        /// <param name="rc">An array that will contain the values of the dual variables aka reduced costs.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The <see cref="get_dual_solution"/> method return only the value(s) of the dual variables aka reduced costs.</para>
        /// <para>These values are only valid after a successful <see cref="solve"/> and if there are integer variables in the model then only if <see cref="set_presolve"/>
        /// is called before <see cref="solve"/> with parameter <see cref="lpsolve_presolve.PRESOLVE_SENSDUALS"/>.</para>
        /// <para><paramref name="rc"/> needs to already be dimensioned with 1+<see cref="get_Nrows"/>+<see cref="get_Ncolumns"/> elements.</para>
        /// <para>For method <see cref="get_dual_solution"/>, the index starts from 1 and element 0 is not used.
        /// The first <see cref="get_Nrows"/> elements contain the duals of the constraints, 
        /// the next <see cref="get_Ncolumns"/> elements contain the duals of the variables.</para>
        /// <para>The dual values or reduced costs values indicate that the objective function will change with the value of the reduced cost
        /// if the restriction is changed with 1 unit.
        /// There will only be a reduced cost if the value is bounded by the restriction, else it is zero.
        /// Note that the sign indicates if the objective function will increase or decrease.
        /// The reduced costs remains constant as long as the restriction stays within the lower/upper range also provided with these methods (dualsfrom, dualstill).
        /// If there is no reduced cost, or no lower/upper limit, then these values are (-)infinity.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_sensitivity_rhs.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/sensitivity.htm">Sensitivity explanation.</seealso>
        public bool get_dual_solution(double[] rc)
        {
            return Interop.get_dual_solution(_lp, rc);
        }

        /// <summary>
        /// Returns the deepest Branch-and-bound level of the last solution.
        /// </summary>
        /// <returns>The deepest Branch-and-bound level of the last solution.</returns>
        /// <remarks>
        /// Is only applicable if the model contains integer variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_max_level.htm">Full C API documentation.</seealso>
        public int get_max_level()
        {
            return Interop.get_max_level(_lp);
        }

        /// <summary>
        /// Returns the value of the objective function.
        /// </summary>
        /// <returns>The value of the objective function.</returns>
        /// <remarks>
        /// <para>This value is only valid after a successful <see cref="solve"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_objective.htm">Full C API documentation.</seealso>
        public double get_objective()
        {
            return Interop.get_objective(_lp);
        }

        /// <summary>
        /// Returns the solution of the model.
        /// </summary>
        /// <param name="pv">An array that will contain the value of the objective function (element 0),
        /// values of the constraints (elements 1 till Nrows),
        /// and the values of the variables (elements Nrows+1 till Nrows+NColumns).
        /// </param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>These values are only valid after a successful <see cref="solve"/>.
        /// <paramref name="pv"/> needs to be already dimensioned with 1 + <see cref="get_Nrows"/> + <see cref="get_Ncolumns"/> elements. 
        /// Element 0 is the value of the objective function, elements 1 till Nrows the values of the constraints and elements Nrows+1 till Nrows+NColumns the values of the variables.
        /// </para>
        /// <para>Special considerations when presolve was done. When <see cref="set_presolve"/> is called before solve, 
        /// then presolve can have deleted both rows and columns from the model because they could be eliminated.
        /// This influences <see cref="get_primal_solution"/>.
        /// This method only reports the values of the remaining variables and constraints.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_primal_solution.htm">Full C API documentation.</seealso>
        public bool get_primal_solution(double[] pv)
        {
            return Interop.get_primal_solution(_lp, pv);
        }

        /// <summary>
        /// Returns the sensitivity of the objective function.
        /// </summary>
        /// <param name="objfrom">An array that will contain the values of the lower limits on the objective function.</param>
        /// <param name="objtill">An array that will contain the values of the upper limits of the objective function.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The <see cref="get_sensitivity_obj"/> and <see cref="get_sensitivity_objex"/> methods return 
        /// the sensitivity of the objective function.</para>
        /// <para>These values are only valid after a successful <see cref="solve"/> and if there are integer
        /// variables in the model then only if <see cref="set_presolve"/> is called before <see cref="solve"/>
        /// with parameter <see cref="lpsolve_presolve.PRESOLVE_SENSDUALS"/>.
        /// The arrays must already be dimensioned with <see cref="get_Ncolumns"/> elements.
        /// Element 0 will contain the value of the first variable, element 1 of the second variable, ...</para>
        /// <para>The meaning of these limits are the following. As long as the value of the coefficient of 
        /// the objective function stays between the lower limit (<paramref name="objfrom"/>) and the upper limit (<paramref name="objtill"/>),
        /// the solution stays the same.
        /// Only the objective value itself changes with a value equal to the difference multiplied by
        /// the amount of this variable.
        /// If there is no lower/upper limit, then these values are (-)infinity.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_sensitivity_obj.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/sensitivity.htm">Sensitivity explanation.</seealso>
        public bool get_sensitivity_obj(double[] objfrom, double[] objtill)
        {
            return Interop.get_sensitivity_obj(_lp, objfrom, objtill);
        }

        /// <summary>
        /// Returns the sensitivity of the objective function.
        /// </summary>
        /// <param name="objfrom">An array that will contain the values of the lower limits on the objective function.</param>
        /// <param name="objtill">An array that will contain the values of the upper limits of the objective function.</param>
        /// <param name="objfromvalue">An array that will contain the values of the variables at their lower limit. Only applicable when the value of the variable is 0 (rejected).</param>
        /// <param name="objtillvalue">Not used at this time.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The <see cref="get_sensitivity_obj"/> and <see cref="get_sensitivity_objex"/> methods return 
        /// the sensitivity of the objective function.</para>
        /// <para>These values are only valid after a successful <see cref="solve"/> and if there are integer
        /// variables in the model then only if <see cref="set_presolve"/> is called before <see cref="solve"/>
        /// with parameter <see cref="lpsolve_presolve.PRESOLVE_SENSDUALS"/>.
        /// The arrays must already be dimensioned with <see cref="get_Ncolumns"/> elements.
        /// Element 0 will contain the value of the first variable, element 1 of the second variable, ...</para>
        /// <para>The meaning of these limits are the following. As long as the value of the coefficient of 
        /// the objective function stays between the lower limit (<paramref name="objfrom"/>) and the upper limit (<paramref name="objtill"/>),
        /// the solution stays the same.
        /// Only the objective value itself changes with a value equal to the difference multiplied by
        /// the amount of this variable.
        /// If there is no lower/upper limit, then these values are (-)infinity.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_sensitivity_obj.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/sensitivity.htm">Sensitivity explanation.</seealso>
        public bool get_sensitivity_objex(double[] objfrom, double[] objtill, double[] objfromvalue,
            double[] objtillvalue)
        {
            return Interop.get_sensitivity_objex(_lp, objfrom, objtill, objfromvalue, objtillvalue);
        }

        /// <summary>
        /// Returns the sensitivity of the constraints and the variables.
        /// </summary>
        /// <param name="duals">An array that will contain the values of the dual variables aka reduced costs.</param>
        /// <param name="dualsfrom">An array that will contain the values of the lower limits on the dual variables aka reduced costs.</param>
        /// <param name="dualstill">An array that will contain the values of the upper limits on the dual variables aka reduced costs.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The method returns the values of the dual variables aka reduced costs and their limits.</para>
        /// <para>These values are only valid after a successful solve and if there are integer variables in the model then only if <see cref="set_presolve"/>
        /// is called before <see cref="solve"/> with parameter <see cref="lpsolve_presolve.PRESOLVE_SENSDUALS"/>.</para>
        /// <para>The arrays need to be alread already dimensioned with <see cref="get_Nrows"/>+<see cref="get_Ncolumns"/> elements.</para>
        /// <para>Element 0 will contain the value of the first row, element 1 of the second row, ...
        /// Element <see cref="get_Nrows"/> contains the value for the first variable, element <see cref="get_Nrows"/>+1 the value for the second variable and so on.</para>
        /// <para>The dual values or reduced costs values indicate that the objective function will change with the value of the reduced cost
        /// if the restriction is changed with 1 unit.
        /// There will only be a reduced cost if the value is bounded by the restriction, else it is zero.
        /// Note that the sign indicates if the objective function will increase or decrease.
        /// The reduced costs remains constant as long as the restriction stays within the lower/upper range also provided with these methods (dualsfrom, dualstill).
        /// If there is no reduced cost, or no lower/upper limit, then these values are (-)infinity.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_sensitivity_rhs.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/sensitivity.htm">Sensitivity explanation.</seealso>
        public bool get_sensitivity_rhs(double[] duals, double[] dualsfrom, double[] dualstill)
        {
            return Interop.get_sensitivity_rhs(_lp, duals, dualsfrom, dualstill);
        }

        /// <summary>
        /// Returns the number of equal solutions.
        /// </summary>
        /// <returns>The number of equal solutions up to <see cref="get_solutionlimit"/></returns>
        /// <remarks>
        /// <para>This is only valid if there are integer, semi-continious or SOS variables in the model
        /// so that the branch-and-bound algoritm is used.
        /// This count gives the number of solutions with the same optimal objective value.
        /// If there is only one optimal solution, the result is 1.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_solutioncount.htm">Full C API documentation.</seealso>
        public int get_solutioncount()
        {
            return Interop.get_solutioncount(_lp);
        }

        /// <summary>
        /// Returns the total number of iterations.
        /// </summary>
        /// <returns>The total number of iterations.</returns>
        /// <remarks>
        /// <para>If the model contains integer variables then it returns the number of iterations to find a relaxed solution plus the number of iterations in the B&amp;B process.</para>
        /// <para>If the model contains no integer variables then it returns the number of iterations to find a solution.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_total_iter.htm">Full C API documentation.</seealso>
        public long get_total_iter()
        {
            return Interop.get_total_iter(_lp);
        }

        /// <summary>
        /// Returns the total number of nodes processed in branch-and-bound of the last solution.
        /// </summary>
        /// <returns>The total number of nodes processed in branch-and-bound of the last solution.</returns>
        /// <remarks>
        /// Is only applicable if the model contains integer variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_total_nodes.htm">Full C API documentation.</seealso>
        public long get_total_nodes()
        {
            return Interop.get_total_nodes(_lp);
        }

        /// <summary>
        /// Returns the reduced cost on a variable.
        /// </summary>
        /// <param name="index">The column of the variable for which the reduced cost is required.
        /// Note that this is the column number before presolve was done, if active.
        /// If index is 0, then the value of the objective function is returned.</param>
        /// <returns>The reduced cost on the variable at <paramref name="index"/>.</returns>
        /// <remarks>
        /// <para>The method returns only the value of the dual variables aka reduced costs.</para>
        /// <para>This value is only valid after a successful <see cref="solve"/> and if there are integer variables in the model then only if <see cref="set_presolve"/>
        /// is called before <see cref="solve"/> with parameter <see cref="lpsolve_presolve.PRESOLVE_SENSDUALS"/>.</para>
        /// <para>The dual values or reduced costs values indicate that the objective function will change with the value of the reduced cost
        /// if the restriction is changed with 1 unit.
        /// There will only be a reduced cost if the value is bounded by the restriction, else it is zero.
        /// Note that the sign indicates if the objective function will increase or decrease.
        /// The reduced costs remains constant as long as the restriction stays within the lower/upper range also provided with these methods (dualsfrom, dualstill).
        /// If there is no reduced cost, or no lower/upper limit, then these values are (-)infinity.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_sensitivity_rhs.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/sensitivity.htm">Sensitivity explanation.</seealso>
        public double get_var_dualresult(int index)
        {
            return Interop.get_var_dualresult(_lp, index);
        }

        /// <summary>
        /// Returns the solution of the model.
        /// </summary>
        /// <param name="index">The original index of the variable in the model no matter if <see cref="set_presolve"/> is called before <see cref="solve"/>.</param>
        /// <returns>The value of the solution for variable at <paramref name="index"/>.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_primal_solution.htm">Full C API documentation.</seealso>
        public double get_var_primalresult(int index)
        {
            return Interop.get_var_primalresult(_lp, index);
        }

        /// <summary>
        /// Returns the values of the variables.
        /// </summary>
        /// <param name="var">An array that will contain the values of the variables.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>These values are only valid after a successful <see cref="solve"/>. 
        /// <paramref name="var"/> must already be dimensioned with <see cref="get_Ncolumns"/> elements.
        /// Element 0 will contain the value of the first variable, element 1 of the second variable, ...</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_variables.htm">Full C API documentation.</seealso>
        public bool get_variables(double[] var)
        {
            return Interop.get_variables(_lp, var);
        }

        /// <summary>
        /// Returns the value of the objective function.
        /// </summary>
        /// <returns>The current value of the objective while solving the model.</returns>
        /// <remarks>This value can be retrieved while solving in a callback method.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_working_objective.htm">Full C API documentation.</seealso>
        public double get_working_objective()
        {
            return Interop.get_working_objective(_lp);
        }

        /// <summary>
        /// Checks if provided solution is a feasible solution.
        /// </summary>
        /// <param name="values">
        ///   <para>An array of row/column values that are checked against the bounds and ranges.</para>
        ///   <para>The array must have <see cref="get_Nrows"/>+<see cref="get_Ncolumns"/> elements. Element 0 is not used.</para>
        /// </param>
        /// <param name="threshold">A tolerance value. The values may differ that much. Recommended to use <see cref="get_epsint"/> for this value.</param>
        /// <returns><c>true</c> if <paramref name="values"/> represent a solution to the model, <c>false</c> otherwise</returns>
        /// <remarks>
        /// <para>All values of the values array must be between the bounds and ranges to be a feasible solution.</para>
        /// <para>This value is only valid after a successful <see cref="solve"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_feasible.htm">Full C API documentation.</seealso>
        public bool is_feasible(double[] values, double threshold)
        {
            return Interop.is_feasible(_lp, values, threshold);
        }

#endregion

#region Debug/print settings

        /// <summary>
        /// Defines the output when lp_solve has something to report.
        /// </summary>
        /// <param name="filename">The file to print the results to.
        /// If <c>null</c>, then output is stdout again.
        /// If "", then output is ignored.
        /// It doesn't go to the console or to a file then.
        /// This is useful in combination with <see cref="put_logfunc"/> to redirect output to somewhere completely different.</param>
        /// <returns><c>true</c> if the file could be opened, else <c>false</c>.</returns>
        /// <remarks>
        /// <para>This is done at the same time as something is reported via <see cref="put_logfunc"/>.
        /// The default reporting output is screen (stdout). 
        /// If <see cref="set_outputfile"/> is called to change output to the specified file, then the file is automatically closed when <see cref="LpSolve"/> is disposed.
        /// Note that this was not the case in previous versions of lp_solve.
        /// If filename is "", then output is ignored.
        /// It doesn't go to the console or to a file then.
        /// This is useful in combination with put_logfunc to redirect output to somewhere completely different.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_output.htm">Full C API documentation.</seealso>
        public bool set_outputfile(string filename)
        {
            return Interop.set_outputfile(_lp, filename);
        }

        /// <summary>
        /// Returns  a flag if all intermediate valid solutions must be printed while solving.
        /// </summary>
        /// <returns>A <see cref="lpsolve_print_sol_option"/>, default is to not print.</returns>
        /// <remarks>
        /// This method is meant for debugging purposes. The default is not to print <see cref="lpsolve_print_sol_option.FALSE"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_print_sol.htm">Full C API documentation.</seealso>
        public lpsolve_print_sol_option get_print_sol()
        {
            return Interop.get_print_sol(_lp);
        }

        /// <summary>
        /// Sets a flag if all intermediate valid solutions must be printed while solving.
        /// </summary>
        /// <param name="print_sol">A <see cref="lpsolve_print_sol_option"/>, default is to not print.</param>
        /// <remarks>
        /// This method is meant for debugging purposes. The default is not to print <see cref="lpsolve_print_sol_option.FALSE"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_print_sol.htm">Full C API documentation.</seealso>
        public void set_print_sol(lpsolve_print_sol_option print_sol)
        {
            Interop.set_print_sol(_lp, print_sol);
        }

        /// <summary>
        /// Returns the verbose level.
        /// </summary>
        /// <returns>The <see cref="lpsolve_verbosity"/> level.</returns>
        /// <remarks>
        /// <para>lp_solve reports information back to the user.
        /// How much information is reported depends on the verbose level.
        /// The default verbose level is <see cref="lpsolve_verbosity.NORMAL"/>.
        /// lp_solve determines how verbose a given message is.
        /// For example specifying a wrong row/column index values is considered as a <see cref="lpsolve_verbosity.SEVERE"/> error.
        /// verbose determines how much of the lp_solve message are reported.
        /// All messages equal to and below the set level are reported.</para>
        /// <para>The default reporting device is the console screen.
        /// It is possible to set a used defined reporting callback via <see cref="put_logfunc"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_verbose.htm">Full C API documentation.</seealso>
        public lpsolve_verbosity get_verbose()
        {
            return Interop.get_verbose(_lp);
        }

        /// <summary>
        /// Set the verbose level.
        /// </summary>
        /// <param name="verbose">The <see cref="lpsolve_verbosity"/> level.</param>
        /// <remarks>
        /// <para>lp_solve reports information back to the user.
        /// How much information is reported depends on the verbose level.
        /// The default verbose level is <see cref="lpsolve_verbosity.NORMAL"/>.
        /// lp_solve determines how verbose a given message is.
        /// For example specifying a wrong row/column index values is considered as a <see cref="lpsolve_verbosity.SEVERE"/> error.
        /// <paramref name="verbose"/> determines how much of the lp_solve message are reported.
        /// All messages equal to and below the set level are reported.</para>
        /// <para>The default reporting device is the console screen.
        /// It is possible to set a used defined reporting callback via <see cref="put_logfunc"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_verbose.htm">Full C API documentation.</seealso>
        public void set_verbose(lpsolve_verbosity verbose)
        {
            Interop.set_verbose(_lp, verbose);
        }

        /// <summary>
        /// Returns a flag if all intermediate results and the branch-and-bound decisions must be printed while solving.
        /// </summary>
        /// <returns><c>true</c> to print intermediate results, <c>false</c> to not print.</returns>
        /// <remarks>
        /// This method is meant for debugging purposes. The default is not to print (<c>false</c>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_debug.htm">Full C API documentation.</seealso>
        public bool is_debug()
        {
            return Interop.is_debug(_lp);
        }

        /// <summary>
        /// Sets a flag if all intermediate results and the branch-and-bound decisions must be printed while solving.
        /// </summary>
        /// <param name="debug"><c>true</c> to print intermediate results, <c>false</c> to not print.</param>
        /// <remarks>
        /// This method is meant for debugging purposes. The default is not to print (<c>false</c>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_debug.htm">Full C API documentation.</seealso>
        public void set_debug(bool debug)
        {
            Interop.set_debug(_lp, debug);
        }

        /// <summary>
        /// Returns a flag if pivot selection must be printed while solving.
        /// </summary>
        /// <returns><c>true</c> if pivot selection must be printed, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This method is meant for debugging purposes. The default is not to print (<c>false</c>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_trace.htm">Full C API documentation.</seealso>
        public bool is_trace()
        {
            return Interop.is_trace(_lp);
        }

        /// <summary>
        /// Sets a flag if pivot selection must be printed while solving.
        /// </summary>
        /// <param name="trace"><c>true</c> to set trace, <c>false</c> to remove it.</param>
        /// <remarks>
        /// This method is meant for debugging purposes. The default is not to print (<c>false</c>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_trace.htm">Full C API documentation.</seealso>
        public void set_trace(bool trace)
        {
            Interop.set_trace(_lp, trace);
        }

#endregion

#region Debug/print

        /// <summary>
        /// Prints the values of the constraints of the lp model.
        /// </summary>
        /// <param name="columns">Number of columns to print solution.</param>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="solve"/>.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_constraints.htm">Full C API documentation.</seealso>
        public void print_constraints(int columns)
        {
            Interop.print_constraints(_lp, columns);
        }

        /// <summary>
        /// Do a generic readable data dump of key lp_solve model variables; principally for run difference and debugging purposes.
        /// </summary>
        /// <param name="filename">Name of file to write to.</param>
        /// <returns><c>true</c> if data could be written, else <c>false</c>.</returns>
        /// <remarks>
        /// <para>This method is meant for debugging purposes.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_debugdump.htm">Full C API documentation.</seealso>
        public bool print_debugdump(string filename)
        {
            return Interop.print_debugdump(_lp, filename);
        }

        /// <summary>
        /// Prints the values of the duals of the lp model.
        /// </summary>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="solve"/>.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_duals.htm">Full C API documentation.</seealso>
        public void print_duals()
        {
            Interop.print_duals(_lp);
        }

        /// <summary>
        /// Prints the lp model.
        /// </summary>
        /// <remarks>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_lp.htm">Full C API documentation.</seealso>
        public void print_lp()
        {
            Interop.print_lp(_lp);
        }

        /// <summary>
        /// Prints the objective value of the lp model.
        /// </summary>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="solve"/>.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_objective.htm">Full C API documentation.</seealso>
        public void print_objective()
        {
            Interop.print_objective(_lp);
        }

        /// <summary>
        /// Prints the scales of the lp model.
        /// </summary>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="solve"/>.</para>
        /// <para>It will only output something when the model is scaled.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_scales.htm">Full C API documentation.</seealso>
        public void print_scales()
        {
            Interop.print_scales(_lp);
        }

        /// <summary>
        /// Prints the solution (variables) of the lp model.
        /// </summary>
        /// <param name="columns">Number of columns to print solution.</param>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="solve"/>.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_solution.htm">Full C API documentation.</seealso>
        public void print_solution(int columns)
        {
            Interop.print_solution(_lp, columns);
        }

        /// <summary>
        /// Prints a string.
        /// </summary>
        /// <param name="str">The string to print</param>
        /// <remarks>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_str.htm">Full C API documentation.</seealso>
        public void print_str(string str)
        {
            Interop.print_str(_lp, str);
        }

        /// <summary>
        /// Prints the tableau.
        /// </summary>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="solve"/>.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_tableau.htm">Full C API documentation.</seealso>
        public void print_tableau()
        {
            Interop.print_tableau(_lp);
        }

#endregion

#region Write model to file

        /// <summary>
        /// Write the model in the lp format to <paramref name="filename"/>.
        /// </summary>
        /// <param name="filename">Filename to write the model to.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. See <see cref="set_add_rowmode"/>.</para>
        /// <para>The model in the file will be in <seealso href="http://lpsolve.sourceforge.net/5.5/lp-format.htm">lp-format</seealso></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/write_lp.htm">Full C API documentation.</seealso>
        public bool write_lp(string filename)
        {
            return Interop.write_lp(_lp, filename);
        }

        /// <summary>
        /// Write the model in the Free MPS format to <paramref name="filename"/> or 
        /// if <paramref name="filename"/> is <c>null</c>, to default output.
        /// </summary>
        /// <param name="filename">Filename to write the model to or <c>null</c> to write to default output.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. See <see cref="set_add_rowmode"/>.</para>
        /// <para>When <paramref name="filename"/> is <c>null</c>, then output is written to the output 
        /// set by <see cref="set_outputfile"/>. By default this is stdout.</para>
        /// <para>The model in the file will be in <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">mps-format</seealso></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/write_mps.htm">Full C API documentation.</seealso>
        public bool write_freemps(string filename)
        {
            return Interop.write_freemps(_lp, filename);
        }

        /// <summary>
        /// Write the model in the Fixed MPS format to <paramref name="filename"/> or 
        /// if <paramref name="filename"/> is <c>null</c>, to default output.
        /// </summary>
        /// <param name="filename">Filename to write the model to or <c>null</c> to write to default output.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. See <see cref="set_add_rowmode"/>.</para>
        /// <para>When <paramref name="filename"/> is <c>null</c>, then output is written to the output 
        /// set by <see cref="set_outputfile"/>. By default this is stdout.</para>
        /// <para>The model in the file will be in <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">mps-format</seealso></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/write_mps.htm">Full C API documentation.</seealso>
        public bool write_mps(string filename)
        {
            return Interop.write_mps(_lp, filename);
        }

        /// <summary>
        /// Returns if a built-in External Language Interfaces (XLI) is available or not.
        /// </summary>
        /// <returns><c>true</c> if there is a built-in XLI is available, <c>false</c> if not.</returns>
        /// <remarks>
        /// <para>At this moment, this method always returns <c>false</c> since no built-in XLI is available.</para>
        /// <para>See <seealso href="http://lpsolve.sourceforge.net/5.5/XLI.htm">External Language Interfaces</seealso>
        /// for a complete description on XLIs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_nativeXLI.htm">Full C API documentation.</seealso>
        public bool is_nativeXLI()
        {
            return Interop.is_nativeXLI(_lp);
        }

        /// <summary>
        /// Returns if there is an external language interface (XLI) set.
        /// </summary>
        /// <returns><c>true</c> if there is an XLI is set, else <c>false</c>.</returns>
        /// <remarks>
        /// <para>See <seealso href="http://lpsolve.sourceforge.net/5.5/XLI.htm">External Language Interfaces</seealso>
        /// for a complete description on XLIs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/has_XLI.htm">Full C API documentation.</seealso>
        public bool has_XLI()
        {
            return Interop.has_XLI(_lp);
        }

        /// <summary>
        /// Sets External Language Interfaces package.
        /// </summary>
        /// <param name="filename">The name of the XLI package.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This call is normally only needed when <see cref="write_XLI"/> will be called. 
        /// <see cref="read_XLI"/> automatically calls this method</para>
        /// <para>See <seealso href="http://lpsolve.sourceforge.net/5.5/XLI.htm">External Language Interfaces</seealso>
        /// for a complete description on XLIs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_XLI.htm">Full C API documentation.</seealso>
        public bool set_XLI(string filename)
        {
            return Interop.set_XLI(_lp, filename);
        }

        /// <summary>
        /// Writes a model to a file via the External Language Interface.
        /// </summary>
        /// <param name="filename">Filename to write the model to.</param>
        /// <param name="options">Extra options that can be used by the writer.</param>
        /// <param name="results"><c>false</c> to generate a model file, <c>true</c> to generate a solution file.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that <see cref="set_XLI"/> must be called before this method to set an XLI.</para>
        /// <para>See <seealso href="http://lpsolve.sourceforge.net/5.5/XLI.htm">External Language Interfaces</seealso>
        /// for a complete description on XLIs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/write_XLI.htm">Full C API documentation.</seealso>
        public bool write_XLI(string filename, string options, bool results)
        {
            return Interop.write_XLI(_lp, filename, options, results);
        }

#endregion

#region Miscellaneous methods

        /// <summary>
        /// Returns the version of the lpsolve library loaded ar runtime.
        /// </summary>
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

        /// <summary>
        /// Checks if a column is already present in the lp model.
        /// </summary>
        /// <param name="column">An array with 1+<see cref="get_Nrows"/> elements that are checked against the existing columns in the lp model.</param>
        /// <returns>The (first) column number if the column is already in the lp model and 0 if not.</returns>
        /// <remarks>
        /// <para>It does not look at bounds and types, only at matrix values.</para>
        /// <para>The first matched column is returned. If there is no column match, then 0 is returned.</para>
        /// <para>Note that element 0 is the objective function value. Element 1 is column 1, and so on.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/column_in_lp.htm">Full C API documentation.</seealso>
        public int column_in_lp(double[] column)
        {
            return Interop.column_in_lp(_lp, column);
        }

        /// <summary>
        /// Creates the dual of the current model.
        /// </summary>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/dualize_lp.htm">Full C API documentation.</seealso>
        public bool dualize_lp()
        {
            return Interop.dualize_lp(_lp);
        }

        /// <summary>
        /// Returns the number of non-zero elements in the matrix.
        /// </summary>
        /// <returns>The number of non-zeros in the matrix.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_nonzeros.htm">Full C API documentation.</seealso>
        public int get_nonzeros()
        {
            return Interop.get_nonzeros(_lp);
        }

        /// <summary>
        /// Returns the number of columns (variables) in the lp model.
        /// </summary>
        /// <returns>The number of columns (variables) in the lp model.</returns>
        /// <remarks>
        /// <para>Note that the number of columns can change when a presolve is done
        /// or when negative variables are split in a positive and a negative part.</para>
        /// <para>Therefore it is advisable to use this method to determine how many columns there are
        /// in the lp model instead of relying on an own count.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_Ncolumns.htm">Full C API documentation.</seealso>
        public int get_Ncolumns()
        {
            return Interop.get_Ncolumns(_lp);
        }

        /// <summary>
        /// Returns the number of original columns (variables) in the lp model.
        /// </summary>
        /// <returns>The number of original columns (variables) in the lp model.</returns>
        /// <remarks>
        /// <para>Note that the number of columns (<see cref="get_Ncolumns"/>) can change when a presolve is done
        /// or when negative variables are split in a positive and a negative part.</para>
        /// <para><see cref="get_Norig_columns"/> does not change and thus returns the original number of columns in the lp model.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_Norig_columns.htm">Full C API documentation.</seealso>
        public int get_Norig_columns()
        {
            return Interop.get_Norig_columns(_lp);
        }

        /// <summary>
        /// Returns the number of original rows (constraints) in the lp model.
        /// </summary>
        /// <returns>The number of original rows (constraints) in the lp model.</returns>
        /// <remarks>
        /// <para>Note that the number of rows (<see cref="get_Nrows"/>) can change when a presolve is done.</para>
        /// <para><see cref="get_Norig_rows"/> does not change and thus returns the original number of rows in the lp model.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_Norig_rows.htm">Full C API documentation.</seealso>
        public int get_Norig_rows()
        {
            return Interop.get_Norig_rows(_lp);
        }

        /// <summary>
        /// Returns the number of rows (constraints) in the lp model.
        /// </summary>
        /// <returns>The number of rows (constraints) in the lp model.</returns>
        /// <remarks>
        /// <para>Note that the number of rows can change when a presolve is done.</para>
        /// <para>Therefore it is advisable to use this method to determine how many rows there are
        /// in the lp model instead of relying on an own count.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_Nrows.htm">Full C API documentation.</seealso>
        public int get_Nrows()
        {
            return Interop.get_Nrows(_lp);
        }

        /// <summary>
        /// Returns an extra status after a call to a method.
        /// </summary>
        /// <returns>Extra status which indicates type of problem after call to method.</returns>
        /// <remarks>
        /// Some methods return <c>false</c> when they have failed.
        /// To have more information on the reason of the failure, this method can be used to get an extended error code.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_status.htm">Full C API documentation.</seealso>
        public int get_status()
        {
            return Interop.get_status(_lp);
        }

        /// <summary>
        /// Returns the description of a returncode of the <see cref="solve"/> method.
        /// </summary>
        /// <param name="statuscode">Returncode of <see cref="solve"/></param>
        /// <returns>The description of a returncode of the <see cref="solve"/> method</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_statustext.htm">Full C API documentation.</seealso>
        public string get_statustext(int statuscode)
        {
            return Interop.get_statustext(_lp, statuscode);
        }

        /// <summary>
        /// Gets the time elapsed since start of solve.
        /// </summary>
        /// <returns>The number of seconds after <see cref="solve"/> has started.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/time_elapsed.htm">Full C API documentation.</seealso>
        public double time_elapsed()
        {
            return Interop.time_elapsed(_lp);
        }

        /// <summary>
        /// Returns the index in the lp of the original row/column.
        /// </summary>
        /// <param name="orig_index">Original constraint or column number. 
        /// If <paramref name="orig_index"/> is between 1 and <see cref="get_Norig_rows"/> then the index is a constraint (row) number.
        /// If <paramref name="orig_index"/> is between 1+<see cref="get_Norig_rows"/> and <see cref="get_Norig_rows"/> + <see cref="get_Norig_columns"/>
        /// then the index is a column number.</param>
        /// <returns>The index in the lp of the original row/column.</returns>
        /// <remarks>
        /// <para>Note that the number of constraints(<see cref="get_Nrows"/>) and columns(<see cref="get_Ncolumns"/>) can change when
        /// a presolve is done or when negative variables are split in a positive and a negative part.
        /// <see cref="get_lp_index"/> returns the position of the constraint/variable in the lp model.
        /// If <paramref name="orig_index"/> is not a legal index  or the constraint/column is deleted,
        /// the return value is <c>0</c>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_lp_index.htm">Full C API documentation.</seealso>
        public int get_lp_index(int orig_index)
        {
            return Interop.get_lp_index(_lp, orig_index);
        }

        /// <summary>
        /// Returns the original row/column where a constraint/variable was before presolve.
        /// </summary>
        /// <param name="lp_index">Constraint or column number.
        /// If <paramref name="lp_index"/> is between 1 and <see cref="get_Nrows"/> then the index is a constraint (row) number.
        /// If <paramref name="lp_index"/> is between 1+<see cref="get_Nrows"/> and <see cref="get_Nrows"/> + <see cref="get_Ncolumns"/>
        /// then the index is a column number.</param>
        /// <returns>The original row/column where a constraint/variable was before presolve.</returns>
        /// <remarks>
        /// <para>Note that the number of constraints(<see cref="get_Nrows"/>) and columns(<see cref="get_Ncolumns"/>) can change when
        /// a presolve is done or when negative variables are split in a positive and a negative part.
        /// <see cref="get_orig_index"/> returns the original position of the constraint/variable.
        /// If <paramref name="lp_index"/> is not a legal index, the return value is <c>0</c>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_orig_index.htm">Full C API documentation.</seealso>
        public int get_orig_index(int lp_index)
        {
            return Interop.get_orig_index(_lp, lp_index);
        }

#endregion
    }
}
