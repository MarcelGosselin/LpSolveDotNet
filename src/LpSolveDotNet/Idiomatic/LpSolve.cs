// https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives
#if NETCOREAPP1_0_OR_GREATER || NET471_OR_GREATER || NETSTANDARD1_1_OR_GREATER
#define SUPPORTS_RUNTIMEINFORMATION
#endif
#if NETCOREAPP3_0_OR_GREATER
#define SUPPORTS_NATIVELIBRARY
#define SUPPORTS_ASSEMBLYLOADCONTEXT_RESOLVINGUNMANAGEDDLL
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using LpSolveDotNet.Idiomatic.Internal;
#if SUPPORTS_RUNTIMEINFORMATION || SUPPORTS_NATIVELIBRARY
using System.Runtime.InteropServices;
#endif
#if SUPPORTS_ASSEMBLYLOADCONTEXT_RESOLVINGUNMANAGEDDLL
using System.Runtime.Loader;
#endif

namespace LpSolveDotNet.Idiomatic
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
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Need to keep same names as C library.")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Need to keep same names as C library.")]
    public sealed partial class LpSolve
        : IDisposable
    {
        #region Library initialization

        /// <summary>
        /// Initializes the library by making sure the correct native library will be loaded.
        /// </summary>
        /// <param name="nativeLibraryFolderPath">The (optional) folder where the native library is located.
        /// When <paramref name="nativeLibraryFolderPath"/> is <c>null</c>, it will infer one of
        /// <list type="bullet">
        /// <item><description><c>basedir/NativeBinaries/win-x64</c></description></item>
        /// <item><description><c>basedir/NativeBinaries/win-x86</c></description></item>
        /// <item><description><c>basedir/NativeBinaries/linux-x64</c></description></item>
        /// <item><description><c>basedir/NativeBinaries/linux-x86</c></description></item>
        /// <item><description><c>basedir/NativeBinaries/osx-x86</c></description></item>
        /// </list>
        /// </param>
        /// <param name="nativeLibraryNamePrefix">The prefix for the native library file name. It defaults to <c>null</c> but when left <c>null</c>,
        /// it will be inferred by <see href="https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.osplatform">OSPlatform</see>.
        /// On Linux, OSX and other Unixes it will be <c>lib</c>, and on Windows it will be empty string.</param>
        /// <param name="nativeLibraryExtension">The file extension for the native library (you must include the dot). 
        /// It defaults to <c>null</c> but when left <c>null</c>, it will be inferred by <see href="https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.osplatform">OSPlatform</see>
        /// to <c>.dll</c> on Windows, <c>.so</c> on Unix and <c>.dylib</c> on OSX.</param>
        /// <returns><c>true</c>, if it found the native library, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// If you use this method with a platform other than .NET framework or for a version of .NET Core before 3.0,
        /// you <strong>must</strong> either:<list>
        /// <item><description>provide a value for <see cref="CustomLoadNativeLibrary"/></description></item>
        /// <item><description>put the native library in a place where the runtime will pick it up.</description></item>
        /// </list> 
        /// </remarks>
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

            nativeLibraryFolderPath = nativeLibraryFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            nativeLibraryNamePrefix ??= GetLibraryNamePrefix();
            nativeLibraryExtension ??= GetLibraryExtension();

            string nativeLibraryFileName = nativeLibraryNamePrefix + NativeMethods.LibraryName + nativeLibraryExtension;
            string nativeLibraryFilePath = Path.Combine(nativeLibraryFolderPath, nativeLibraryFileName);

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

#if SUPPORTS_RUNTIMEINFORMATION

        private static string GetFolderNameByOSAndArchitecture()
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            string arch = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
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

#elif NET20_OR_GREATER // .NET Framework is on Windows only 

        private static string GetFolderNameByOSAndArchitecture() => IntPtr.Size == 8
            ? "win-x64"
            : "win-x86";

        private static string GetLibraryNamePrefix() => "";

        private static string GetLibraryExtension() => ".dll";

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
                if (arg2 == NativeMethods.LibraryName)
                {
                    return NativeLibrary.Load(_nativeLibraryFilePath);
                }
                return IntPtr.Zero;
            }
        }
        private static string _nativeLibraryFilePath;

#elif NET20_OR_GREATER

        private static void LoadNativeLibrary(string nativeLibraryFilePath)
        {
            if (!_hasAlreadyChangedPathEnvironmentVariable)
            {
                string pathEnvironmentVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
                string pathWithSeparator = pathEnvironmentVariable + Path.PathSeparator;
                string nativeLibraryFolderPath = Path.GetDirectoryName(nativeLibraryFilePath);

                if (pathWithSeparator.IndexOf(nativeLibraryFolderPath + Path.PathSeparator, StringComparison.CurrentCultureIgnoreCase) < 0)
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

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Not used in this target framework but we need to keep same signature as other frameworks.")]
        [SuppressMessage("Performance", "CA1801:Review unused parameters", Justification = "Not used in this target framework but we need to keep same signature as other frameworks.")]
        private static void LoadNativeLibrary(string nativeLibraryFilePath)
        {
            // Nothing to do, hopefully  CustomLoadNativeLibrary handled it
        }

#endif

        #endregion

        #region Fields

        private IntPtr _lp;
        private IReturnValueHandler _returnValueHandler;

        #endregion

        #region Create/destroy model

        /// <summary>
        /// Constructor, to be called from <see cref="CreateFromLpRecStructurePointer"/> only.
        /// </summary>
        private LpSolve(IntPtr lp)
        {
            _lp = lp;
            //TODO: find a way to let user pass specific handler
            _returnValueHandler = new ThrowingReturnValueHandler(_lp);
            Tolerance = new ModelTolerance(_lp);
            Basis = new ModelBasis(_lp);
            ObjectiveFunction = new ModelObjectiveFunction(_lp);
            Rows = new ModelRows(_lp);
            Columns = new ModelColumns(_lp);
            Cells = new ModelCells(_lp);
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
        /// <see cref="ModelRows.Add"/>.</param>
        /// <param name="columns">Initial number of columns. Can be <c>0</c> as new columns can be added via
        /// <see cref="ModelColumns.Add"/>.</param>
        /// <returns>A new <see cref="LpSolve"/> model with <paramref name="rows"/> rows and <paramref name="columns"/> columns.
        /// A <c>null</c> return value indicates an error. Specifically not enough memory available to setup an lprec structure.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/make_lp.htm">Full C API documentation.</seealso>
        public static LpSolve Create(int rows, int columns)
        {
            IntPtr lp = NativeMethods.make_lp(rows, columns);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Creates and initialises a new <see cref="LpSolve"/> model from a LP model file.
        /// </summary>
        /// <param name="fileName">Filename to read the LP model from.</param>
        /// <param name="verbosity">The <see cref="Verbosity"/> level.</param>
        /// <param name="lpName">Initial name of the model. May be <c>null</c> if the model has no name. See also <see cref="ModelName"/>.</param>
        /// <returns>A new <see cref="LpSolve"/> model matching the one in the file.
        /// A <c>null</c> return value indicates an error. Specifically file could not be opened, has wrong structure or not enough memory is available.</returns>
        /// <remarks>The model in the file must be in <see href="http://lpsolve.sourceforge.net/5.5/lp-format.htm">lp-format</see>.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_lp.htm">Full C API documentation.</seealso>
        public static LpSolve CreateFromLPFile(string fileName, Verbosity verbosity, string lpName)
        {
            IntPtr lp = NativeMethods.read_LP(fileName, verbosity, lpName);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Creates and initialises a new <see cref="LpSolve"/> model from an MPS model file.
        /// </summary>
        /// <param name="fileName">Filename to read the MPS model from.</param>
        /// <param name="verbosity">Specifies the <see cref="Verbosity"/> level.</param>
        /// <param name="options">Specifies how to interprete the MPS layout. You can use multiple values.</param>
        /// <returns>A new <see cref="LpSolve"/> model matching the one in the file.
        /// A <c>null</c> return value indicates an error. Specifically file could not be opened, has wrong structure or not enough memory is available.</returns>
        /// <remarks>The model in the file must be in <see href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">mps-format</see>.</remarks>
        /// <para>This method is different from C API because <paramref name="verbosity"/> is separate from <paramref name="options"/></para>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_mps.htm">Full C API documentation.</seealso>
        public static LpSolve CreateFromMPSFile(string fileName, Verbosity verbosity, MPSOptions options)
        {
            IntPtr lp = NativeMethods.read_MPS(fileName, ((int)verbosity) | ((int)options));
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Creates and initialises a new <see cref="LpSolve"/> model via the eXternal Language Interface.
        /// </summary>
        /// <param name="xliName">Filename of the XLI package.</param>
        /// <param name="modelName">Filename to read the model from.</param>
        /// <param name="dataName">Filename to read the data from. This may be optional. In that case, set the parameter to <c>null</c>.</param>
        /// <param name="options">Extra options that can be used by the reader.</param>
        /// <param name="verbosity">The <see cref="Verbosity"/> level.</param>
        /// <returns>A new <see cref="LpSolve"/> model matching the one in the file.
        /// A <c>null</c> return value indicates an error.</returns>
        /// <remarks>The method constructs a new <see cref="LpSolve"/> model by reading model from <paramref name="modelName"/> via the specified XLI.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/XLI.htm">External Language Interfaces</see>for a complete description on XLIs.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_XLI.htm">Full C API documentation.</seealso>
        public static LpSolve CreateFromXLIFile(string xliName, string modelName, string dataName, string options, Verbosity verbosity)
        {
            IntPtr lp = NativeMethods.read_XLI(xliName, modelName, dataName, options, verbosity);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <summary>
        /// Copies current model to a new one.
        /// </summary>
        /// <returns>A new model with the same values as current one or <c>null</c> if an error occurs (not enough memory).</returns>
        /// <remarks>The new model is independent from the original one.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/copy_lp.htm">Full C API documentation.</seealso>
        public LpSolve Clone()
        {
            IntPtr lp = NativeMethods.copy_lp(_lp);
            return CreateFromLpRecStructurePointer(lp);
        }

        /// <inheritdoc cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
            // implement Dispose pattern according to https://msdn.microsoft.com/en-us/library/b1yfkh5e.aspx
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // release unmanaged memory
            if (_lp != IntPtr.Zero)
            {
                NativeMethods.delete_lp(_lp);
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

        /// <summary>
        /// Returns a sub-object to deal with everything column-related (variable-related).
        /// </summary>
        public ModelColumns Columns { get; }

        /// <summary>
        /// Returns a sub-object to deal with everything row-related (constraint-related).
        /// </summary>
        public ModelRows Rows { get; }

        /// <summary>
        /// Returns a sub-object to deal with everything cell-related.
        /// </summary>
        public ModelCells Cells { get; }

        /// <summary>
        /// Returns a sub-object to deal with everything ObjectiveFunction-related (row with index 0).
        /// </summary>
        public ModelObjectiveFunction ObjectiveFunction { get; }

        /// <summary>
        /// Returns a sub-object to deal with everything Tolerance-related.
        /// </summary>
        public ModelTolerance Tolerance { get; }

        /// <summary>
        /// Returns a sub-object to deal with everything Basis-related.
        /// </summary>
        public ModelBasis Basis { get; }


        /// <summary>
        /// The name of the model.
        /// </summary>
        /// <remarks>
        /// Giving the lp a name is optional. The default name is "Unnamed".
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_lp_name.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_lp_name.htm">Full C API documentation (set).</seealso>
        public string ModelName
        {
            get => NativeMethods.get_lp_name(_lp);
            set => NativeMethods.set_lp_name(_lp, value)
                .HandleResultAndReturnVoid(_returnValueHandler);
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
        /// This to make the <see cref="ModelRows.Add"/> and <see cref="ModelColumns.Add"/> methods faster. Without <see cref="ResizeMatrix"/>, these methods have to reallocated
        /// memory at each call for the new dimensions. However if <see cref="ResizeMatrix "/> is used, then memory reallocation
        /// must be done only once resulting in better performance. So if the number of rows/columns that will be added is known in advance, then performance can be improved by using this method.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/resize_lp.htm">Full C API documentation.</seealso>
        public bool ResizeMatrix(int rows, int columns)
            => NativeMethods.resize_lp(_lp, rows, columns);

        /// <summary>
        /// Specifies which entry methods perform best. Whether <see cref="ModelColumns.Add"/>
        /// or <see cref="ModelRows.Add"/>.
        /// </summary>
        /// <remarks>
        /// <para>Default value is <see cref="EntryMode.Column"/>, meaning that <see cref="ModelColumns.Add"/> perform best.
        /// If the model is built via calls to <see cref="ModelRows.Add"/>,
        /// then these methods will be much faster if this property is first set to <see cref="EntryMode.Row"/>.
        /// The speed improvement is spectacular, especially for bigger models, so it is 
        /// advisable to set the proper mode. Normally a model is built either column by column or row by row.</para>
        /// <para>Note that there are several restrictions with the <see cref="EntryMode.Row"/> entry mode:
        /// <list type="bullet">
        /// <item><description>Only set the mode after a <see cref="Create"/> call.</description></item>
        /// <item><description>Never set the mode when the model is read from file.</description></item>
        /// <item><description>If you use <see cref="EntryMode.Row"/> entry mode, first add the objective function via <see cref="ModelObjectiveFunction.SetValues"/> or <see cref="ModelObjectiveFunction.SetValue"/>
        /// and after that add the constraints via <see cref="ModelRows.Add"/>.</description></item>
        /// <item><description>Don't call other API methods while in row entry mode.</description></item>
        /// <item><description>No other data matrix access is allowed while in row entry mode.</description></item>
        /// <item><description>After adding the contraints, turn row entry mode back off.</description></item>
        /// <item><description>Once turned off, you cannot switch back to row entry mode.</description></item>
        /// </list>
        /// So in short:<list type="number">
        /// <item><description>var model = LpSolve.Create(rows, cols);</description></item>
        /// <item><description>model.EntryMode = EntryMode.Row;</description></item>
        /// <item><description>...model.set_obj_fn()</description></item>
        /// <item><description>...model.add_constraint()</description></item>
        /// <item><description>model.EntryMode = EntryMode.Column;</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/EntryMode.htm">Full C API documentation (set).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_add_rowmode.htm">Full C API documentation (get).</seealso>
        public EntryMode EntryMode
        {
            //TODO: update doc once all methods have been renamed
            get => NativeMethods.is_add_rowmode(_lp) ? EntryMode.Row : EntryMode.Column;
            set => _ = NativeMethods.set_add_rowmode(_lp, value == EntryMode.Row);
        }

        /// <summary>
        /// Checks if the provided absolute of the value is larger or equal to "infinite".
        /// </summary>
        /// <param name="value">The value to check against "infinite".</param>
        /// <returns><c>true</c> if the value is equal or larger to "infinite", <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Note that the absolute of the provided value is checked against the value set by <see cref="InfiniteValue"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_infinite.htm">Full C API documentation.</seealso>
        public bool IsInfinite(double value)
            => NativeMethods.is_infinite(_lp, value);

        /// <summary>
        /// Specifies the practical value of "infinite".
        /// </summary>
        /// <remarks>
        /// <para>This value is used for very big numbers. For example the upper bound of a variable without an upper bound.</para>
        /// <para>The default is <c>1e30</c></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_infinite.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_infinite.htm">Full C API documentation (set).</seealso>
        public double InfiniteValue
        {
            get => NativeMethods.get_infinite(_lp);
            set => NativeMethods.set_infinite(_lp, value);
        }

        /// <summary>
        /// Sets the value of all the right hand side (RHS) vector (column 0) at once.
        /// </summary>
        /// <param name="rh">An array with row elements that contains the values of the RHS.</param>
        /// <remarks>
        /// <para>The method sets all values of the RHS vector (column 0) at once.</para>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Row 1 is element 1, row 2 is element 2, ...</para>
        /// <para>If the initial value of the objective function must also be set, use <see cref="ModelRow.RightHandSide"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh_vec.htm">Full C API documentation.</seealso>
        public void SetRightHandSideValues(double[] rh)
            => NativeMethods.set_rh_vec(_lp, rh);




        /// <summary>
        /// Specifies if set bounds may only be tighter <c>true</c> or also less restrictive <c>false</c>.
        /// </summary>
        /// <remarks>
        /// <para>If set to <c>true</c> then bounds may only be tighter.
        /// This means that when <see cref="ModelColumn.LowerBound"/> or <see cref="ModelColumn.UpperBound"/> is used to set a bound
        /// and the bound is less restrictive than an already set bound, then this new bound will be ignored.
        /// If tighten is set to <c>false</c>, the new bound is accepted.
        /// This functionality is useful when several bounds are set on a variable and at the end you want
        /// the most restrictive ones. By default, this setting is <c>false</c>.
        /// Note that this setting does not affect <see cref="ModelColumn.SetBounds"/>.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_bounds_tighter.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bounds_tighter.htm">Full C API documentation (set).</seealso>
        public bool IsRestrictingBoundsTighter
        {
            get => NativeMethods.get_bounds_tighter(_lp);
            set => NativeMethods.set_bounds_tighter(_lp, value);
        }

        /// <summary>
        /// Specifies the minimal accuracy for a successful solve.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When accuracy from <see cref="get_accuracy"/> is larger than this value, optimization will fail with <see cref="lpsolve_return.ACCURACYERROR"/> .
        /// By default, break accuracy is <c>5e-7</c>.
        /// </para>
        /// <para>Available since v4.1.0.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_break_numeric_accuracy.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_break_numeric_accuracy.htm">Full C API documentation (set).</seealso>
        public double BreakNumericAccuracy
        {
            get => NativeMethods.get_break_numeric_accuracy(_lp);
            set => NativeMethods.set_break_numeric_accuracy(_lp, value);
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
            => NativeMethods.get_var_priority(_lp, column);

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
        /// The array must contain <see cref="NumberOfColumns"/> elements.</para>
        /// <para>The weights define which variable the branch-and-bound algorithm must select first.
        /// The lower the weight, the sooner the variable is chosen to make it integer.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_var_weights.htm">Full C API documentation.</seealso>
        public bool set_var_weights(double[] weights)
            => NativeMethods.set_var_weights(_lp, weights);

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
            => NativeMethods.add_SOS(_lp, name, sostype, priority, count, sosvars, weights);

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
            => NativeMethods.is_SOS_var(_lp, column);

        #endregion

        #region Solver settings

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
            => NativeMethods.get_maxpivot(_lp);

        /// <summary>
        /// Sets the maximum number of pivots between a re-inversion of the matrix.
        /// </summary>
        /// <param name="max_num_inv">The maximum number of pivots between a re-inversion of the matrix.</param>
        /// <remarks>
        /// <para>For stability reasons, lp_solve re-inverts the matrix on regular times. max_num_inv determines how frequently this inversion is done. This can influence numerical stability. However, the more often this is done, the slower the solver becomes.</para>
        /// <para>The default is 250 for the LUSOL bfp and 42 for the other BFPs.</para></remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_maxpivot.htm">Full C API documentation.</seealso>
        public void set_maxpivot(int max_num_inv)
            => NativeMethods.set_maxpivot(_lp, max_num_inv);

        /// <summary>
        /// Returns the pivot rule and modes. See <see cref="PivotRule"/> and <see cref="PivotModes"/> for possible values.
        /// </summary>
        /// <returns>The pivot rule (rule for selecting row and column entering/leaving) and mode.</returns>
        /// <remarks>
        /// <para>The rule is an exclusive option and the mode is a modifier to the rule.
        /// This rule/mode can influence solving times considerably.
        /// Depending on the model one rule/mode can be best and for another model another rule/mode.</para>
        /// <para>The default rule is <see cref="PivotRule.Devex"/> and the default mode is <see cref="PivotModes.Adaptive"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_pivoting.htm">Full C API documentation.</seealso>
        public PivotRuleAndModes get_pivoting()
        {
            int pivoting = NativeMethods.get_pivoting(_lp);
            int mask = (int)PivotRule.SteepestEdge;
            int rule = pivoting & mask;
            int modes = pivoting & ~mask;
            return new PivotRuleAndModes(
                (PivotRule)rule,
                (PivotModes)modes
                );
        }

        /// <summary>
        /// Sets the pivot rule and modes.
        /// </summary>
        /// <param name="rule">The pivot <see cref="PivotRule">rule</see> (rule for selecting row and column entering/leaving).</param>
        /// <param name="modes">The <see cref="PivotModes">modes</see> modifying the <see cref="PivotRule">rule</see>.</param>
        /// <remarks>
        /// <para>The rule is an exclusive option and the mode is a modifier to the rule.
        /// This rule/mode can influence solving times considerably.
        /// Depending on the model one rule/mode can be best and for another model another rule/mode.</para>
        /// <para>The default rule is <see cref="PivotRule.Devex"/> and the default mode is <see cref="PivotModes.Adaptive"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_pivoting.htm">Full C API documentation.</seealso>
        public void set_pivoting(PivotRule rule, PivotModes modes)
            => NativeMethods.set_pivoting(_lp, ((int)rule) | ((int)modes));

        /// <summary>
        /// Checks if the specified pivot rule is active.
        /// </summary>
        /// <param name="rule">Rule to check.</param>
        /// <returns><c>true</c> if the specified pivot rule is active, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>
        /// This rule/mode can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is <see cref="PivotRule.Devex"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_piv_rule.htm">Full C API documentation.</seealso>
        public bool is_piv_rule(PivotRule rule)
            => NativeMethods.is_piv_rule(_lp, rule);


        /// <summary>
        /// Checks if the pivot mode specified in <paramref name="testmask"/> is active.
        /// </summary>
        /// <param name="testmask">Any combination of <see cref="PivotModes"/> to check if they are active.</param>
        /// <returns><c>true</c> if all the specified modes are active, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// The pivot mode is an extra modifier to the pivot rule. Any combination (OR) of the defined values is possible.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_piv_mode.htm">Full C API documentation.</seealso>
        public bool is_piv_mode(PivotModes testmask)
            => NativeMethods.is_piv_mode(_lp, testmask);

        #endregion

        #region Scaling

        /// <summary>
        /// Gets the relative scaling convergence criterion for the active scaling mode.
        /// </summary>
        /// <returns>The relative scaling convergence criterion for the active scaling mode;
        /// the integer part specifies the maximum number of iterations.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_scalelimit.htm">Full C API documentation.</seealso>
        public double get_scalelimit()
            => NativeMethods.get_scalelimit(_lp);

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
            => NativeMethods.set_scalelimit(_lp, scalelimit);

        /// <summary>
        /// Specifies which scaling algorithm and parameters are used.
        /// </summary>
        /// <returns>The scaling algorithm and parameters that are used.</returns>
        /// <remarks>
        /// <para>
        /// This can influence numerical stability considerably.
        /// It is advisable to always use some sort of scaling.</para>
        /// <para><see cref="set_scaling(ScaleAlgorithm, ScaleParameters)"/> must be called before solve is called.</para>
        /// See <see cref="ScalingAlgorithmAndParameters" /> for more information on scaling.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_scaling.htm">Full C API documentation.</seealso>
        public ScalingAlgorithmAndParameters get_scaling()
        {
            int scaling = NativeMethods.get_scaling(_lp);
            int mask = (int)ScaleAlgorithm.CurtisReid;
            int algorithm = scaling & mask;
            int parameters = scaling & ~mask;
            return new ScalingAlgorithmAndParameters(
                (ScaleAlgorithm)algorithm,
                (ScaleParameters)parameters
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
        /// <para>This method must be called before <see cref="Solve"/> is called.</para>
        /// See <see cref="ScalingAlgorithmAndParameters" /> for more information on scaling.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_scaling.htm">Full C API documentation.</seealso>
        public void set_scaling(ScaleAlgorithm algorithm, ScaleParameters parameters)
            => NativeMethods.set_scaling(_lp, ((int)algorithm) | ((int)parameters));

        /// <summary>
        /// Returns if scaling algorithm and parameters specified are active.
        /// </summary>
        /// <param name="algorithmMask">Specifies which scaling algorithm to verify.
        /// Optional with default = <see cref="ScaleAlgorithm.None"/></param>
        /// <param name="parameterMask">Specifies which parameters must be verified.
        /// Optional with default = <see cref="ScaleParameters.None"/></param>
        /// <returns><c>true</c> if scaling algorithm and parameters specified are active, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// See <see cref="ScalingAlgorithmAndParameters" /> for more information on scaling.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_scalemode.htm">Full C API documentation.</seealso>
        public bool is_scalemode(
            ScaleAlgorithm algorithmMask = ScaleAlgorithm.None,
            ScaleParameters parameterMask = ScaleParameters.None)
            => NativeMethods.is_scalemode(_lp, ((int)algorithmMask) | ((int)parameterMask));

        /// <summary>
        /// Returns if scaling algorithm specified is active.
        /// </summary>
        /// <param name="algorithm">Specifies which scaling algorithm to verify.</param>
        /// <returns><c>true</c> if scaling algorithm specified is active, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// See <see cref="ScalingAlgorithmAndParameters" /> for more information on scaling.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_scaletype.htm">Full C API documentation.</seealso>
        public bool is_scaletype(ScaleAlgorithm algorithm)
            => NativeMethods.is_scaletype(_lp, algorithm);

        /// <summary>
        /// Returns if integer scaling is active.
        /// </summary>
        /// <returns><c>true</c> if <see cref="ScaleParameters.Integers"/> was set with <see cref="set_scaling(ScaleAlgorithm, ScaleParameters)"/>.</returns>
        /// <remarks>
        /// By default, integers are not scaled, you mus call <see cref="set_scaling(ScaleAlgorithm, ScaleParameters)"/>
        /// with <see cref="ScaleParameters.Integers"/> to activate this feature.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_integerscaling.htm">Full C API documentation.</seealso>
        public bool is_integerscaling()
            => NativeMethods.is_integerscaling(_lp);

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
            => NativeMethods.unscale(_lp);

        #endregion

        #region Branching

        /// <summary>
        /// Returns, for the specified variable, which branch to take first in branch-and-bound algorithm.
        /// </summary>
        /// <param name="column">The column number of the variable on which the mode must be returned.
        /// It must be between 1 and the number of columns in the model.
        /// If it is not within this range, the return value is the value of <see cref="FirstBranch"/>.</param>
        /// <returns>Returns which branch to take first in branch-and-bound algorithm.</returns>
        /// <remarks>
        /// This method returns which branch to take first in branch-and-bound algorithm.
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.
        /// When no value was set via <see cref="set_var_branch(int, BranchMode)"/>, the return value is the value of <see cref="FirstBranch"/>.
        /// It also returns the value of <see cref="FirstBranch"/> when <see cref="set_var_branch(int, BranchMode)"/> was called with branch mode <see cref="BranchMode.Default" />.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_var_branch.htm">Full C API documentation.</seealso>
        public BranchMode get_var_branch(int column)
            => NativeMethods.get_var_branch(_lp, column);

        /// <summary>
        /// Specifies, for the specified variable, which branch to take first in branch-and-bound algorithm.
        /// </summary>
        /// <param name="column">The column number of the variable on which the mode must be set.
        /// It must be between 1 and the number of columns in the model.</param>
        /// <param name="branchMode">Specifies, for the specified variable, which branch to take first in branch-and-bound algorithm.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>
        /// This method specifies which branch to take first in branch-and-bound algorithm.
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.
        /// </para>
        /// <para>The default is <see cref="BranchMode.Default" /> which means that 
        /// the branch mode specified with <see cref="FirstBranch"/> method must be used.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_var_branch.htm">Full C API documentation.</seealso>
        public bool set_var_branch(int column, BranchMode branchMode)
            => NativeMethods.set_var_branch(_lp, column, branchMode);

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
            => NativeMethods.is_break_at_first(_lp);

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
            => NativeMethods.set_break_at_first(_lp, break_at_first);

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
            => NativeMethods.get_break_at_value(_lp);

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
            => NativeMethods.set_break_at_value(_lp, break_at_value);

        /// <summary>
        /// Returns the branch-and-bound rule.
        /// </summary>
        /// <returns>Returns the <see cref="BranchAndBoundRuleAndModes">branch-and-bound rule</see>.</returns>
        /// <remarks>
        /// <para>The method returns the branch-and-bound rule for choosing which non-integer variable is to be selected.
        /// This rule can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is NODE_PSEUDONONINTSELECT + NODE_GREEDYMODE + NODE_DYNAMICMODE + NODE_RCOSTFIXING(17445).</para>
        /// </remarks>

        /// <summary>
        /// Specifies the branch-and-bound rule and modes
        /// which define which non-integer variable is to be selected.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This rule/mode can influence solving times considerably.
        /// Depending on the model one rule/mode can be best and for another model another rule/mode.</para>
        /// <para>The default rule is <see cref="BranchAndBoundRule.PseudoNonIntegerSelect"/>
        /// and the default modes are <see cref="BranchAndBoundModes.GreedyMode"/> | <see cref="BranchAndBoundModes.DynamicMode"/> | <see cref="BranchAndBoundModes.RCostFixing"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_bb_rule.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bb_rule.htm">Full C API documentation (set).</seealso>
        public BranchAndBoundRuleAndModes BranchAndBoundRuleAndModes
        {
            get
            {
                int ruleAndModes = NativeMethods.get_pivoting(_lp);
                int mask = (int)BranchAndBoundRule.UserSelect;
                int rule = ruleAndModes & mask;
                int modes = ruleAndModes & ~mask;
                return new BranchAndBoundRuleAndModes(
                    (BranchAndBoundRule)rule,
                    (BranchAndBoundModes)modes
                    );
            }
            set => NativeMethods.set_bb_rule(_lp, ((int)value.Rule) | ((int)value.Modes));
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
            => NativeMethods.get_bb_depthlimit(_lp);

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
            => NativeMethods.set_bb_depthlimit(_lp, bb_maxlevel);

        /// <summary>
        /// Defines which branch to take first in branch-and-bound algorithm.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.</para>
        /// <para>The default is <see cref="BranchMode.Automatic"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_bb_floorfirst.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bb_floorfirst.htm">Full C API documentation (set).</seealso>
        public BranchMode FirstBranch
        {
            get => NativeMethods.get_bb_floorfirst(_lp);
            set => NativeMethods.set_bb_floorfirst(_lp, value);
        }

        /// <summary>
        /// Sets a user function that specifies which non-integer variable to select next to make integer in the B&amp;B solve.
        /// </summary>
        /// <param name="nodeSelector">
        /// <para>The node selection method.</para>
        /// <para>When it returns a positive number, it is the node (column number) to make integer.</para>
        /// <para>When it returns <c>0</c> then it indicates that all variables are integers.</para>
        /// <para>When a negative value is returned, lp_solve will determine the next variable to make integer as if the routine is not set.</para>
        /// </param>
        /// <remarks>Via this routine the user can implement his own rule to select the next non-integer variable to make integer.
        /// This overrules the setting of <see cref="BranchAndBoundRuleAndModes"/>.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/put_bb_nodefunc.htm">Full C API documentation.</seealso>
        public void PutBranchAndBoundNodeSelector(BranchAndBoundNodeSelector nodeSelector)
            => NativeMethods.put_bb_nodefunc(_lp, (x, y, z) => nodeSelector(this), IntPtr.Zero);

        /// <summary>
        /// Sets a user function that specifies which B&amp;B branching to use given a column to branch on.
        /// </summary>
        /// <param name="branchSelector">The branch selection method.</param>
        /// <remarks>With this function you can specify which branch must be taken first in the B&amp;B algorithm.
        /// The floor or the ceiling.
        /// This overrules the setting of <see cref="FirstBranch"/>.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/put_bb_branchfunc.htm">Full C API documentation.</seealso>
        public void PutBranchAndBoundBranchSelector(BranchAndBoundBranchSelector branchSelector)
            => NativeMethods.put_bb_branchfunc(_lp, (x, y, column) => branchSelector(this, column) == BranchSelectorResult.Floor, IntPtr.Zero);

        #endregion

        /// <summary>
        /// Specifies the iterative improvement level.
        /// </summary>
        /// <remarks>
        /// The default is <see cref="IterativeImprovementLevels.DualFeasibility"/> + <see cref="IterativeImprovementLevels.ThetaGap"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_improve.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_improve.htm">Full C API documentation (set).</seealso>
        public IterativeImprovementLevels IterativeImprovementLevels
        {
            get => NativeMethods.get_improve(_lp);
            set => NativeMethods.set_improve(_lp, value);
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
            => NativeMethods.get_negrange(_lp);

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
            => NativeMethods.set_negrange(_lp, negrange);

        /// <summary>
        /// Returns if the degeneracy rules specified in <paramref name="testmask"/> are active.
        /// </summary>
        /// <param name="testmask">Any combination of <see cref="AntiDegeneracyRules"/> to check if they are active.</param>
        /// <returns><c>true</c> if all rules specified in <paramref name="testmask"/> are active, <c>false</c> otherwise.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_anti_degen.htm">Full C API documentation.</seealso>
        public bool is_anti_degen(AntiDegeneracyRules testmask)
            => NativeMethods.is_anti_degen(_lp, testmask);

        /// <summary>
        /// Specifies if special handling must be done to reduce degeneracy/cycling while solving.
        /// </summary>
        /// <remarks>
        ///  <para>Setting this flag can avoid cycling, but can also increase numerical instability.</para>
        ///  <para>The default is <see cref="AntiDegeneracyRules.Infeasible"/>
        ///  + <see cref="AntiDegeneracyRules.Stalling"/>
        ///  + <see cref="AntiDegeneracyRules.FixedVariables"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_anti_degen.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_anti_degen.htm">Full C API documentation (set).</seealso>
        public AntiDegeneracyRules AntiDegeneracyRules
        {
            get => NativeMethods.get_anti_degen(_lp);
            set => NativeMethods.set_anti_degen(_lp, value);
        }

        /// <summary>
        /// Resets parameters back to their default values.
        /// </summary>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/reset_params.htm">Full C API documentation.</seealso>
        public void reset_params()
            => NativeMethods.reset_params(_lp);

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
        ///  <item><description>Numerical values</description></item>
        ///  <item><description>Options</description></item>
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
            => NativeMethods.read_params(_lp, filename, options);

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
        ///  <item><description>Numerical values</description></item>
        ///  <item><description>Options</description></item>
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
            => NativeMethods.write_params(_lp, filename, options);

        /// <summary>
        /// Defines the desired combination of primal and dual simplex algorithms.
        /// </summary>
        /// <remarks>
        ///  The default is <see cref="SimplexType.DualPrimal"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_simplextype.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_simplextype.htm">Full C API documentation (set).</seealso>
        public SimplexType SimplexType
        {
            get => NativeMethods.get_simplextype(_lp);
            set => NativeMethods.set_simplextype(_lp, value);
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
            => NativeMethods.get_solutionlimit(_lp);

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
            => NativeMethods.set_solutionlimit(_lp, limit);

        /// <summary>
        /// Gets the timeout.
        /// </summary>
        /// <returns>The number of seconds after which a timeout occurs.</returns>
        /// <remarks>
        /// <para>The <see cref="Solve"/> method may not last longer than this time or
        /// the method returns with a timeout. There is no valid solution at this time.
        /// The default timeout is 0, resulting in no timeout.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_timeout.htm">Full C API documentation.</seealso>
        public int get_timeout()
            => NativeMethods.get_timeout(_lp);

        /// <summary>
        /// Sets a timeout.
        /// </summary>
        /// <param name="sectimeout">The number of seconds after which a timeout occurs. If zero, then no timeout will occur.</param>
        /// <remarks>
        /// <para>The <see cref="Solve"/> method may not last longer than this time or
        /// the method returns with a timeout. The default timeout is 0, resulting in no timeout.</para>
        /// <para>If a timout occurs, but there was already an integer solution found (that is possibly not the best),
        /// then solve will return <see cref="SolveResult.SubOptimal"/>.
        /// If there was no integer solution found yet or there are no integers or the solvers is still in the
        /// first phase where a REAL optimal solution is searched for, then solve will return <see cref="SolveResult.TimedOut"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_timeout.htm">Full C API documentation.</seealso>
        public void set_timeout(int sectimeout)
            => NativeMethods.set_timeout(_lp, sectimeout);

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
            => NativeMethods.is_use_names(_lp, isrow);

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
            => NativeMethods.set_use_names(_lp, isrow, use_names);

        /// <summary>
        /// Returns if presolve level specified in <paramref name="testmask"/> is active.
        /// </summary>
        /// <param name="testmask">The combination of any of the <see cref="PreSolveLevels"/> values to check whether they are active or not.</param>
        /// <returns><c>true</c>, if all levels specified in <paramref name="testmask"/> are active, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Presolve looks at the model and tries to simplify it so that solving times are shorter.
        /// For example a constraint on only one variable is converted to a bound on this variable
        /// (and the constraint is deleted). Note that the model dimensions can change because of this,
        /// so be careful with this. Both rows and columns can be deleted by the presolve.</para>
        /// <para>The default is not (<see cref="PreSolveLevels.None"/>) doing a presolve.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_presolve.htm">Full C API documentation.</seealso>
        public bool is_presolve(PreSolveLevels testmask)
            => NativeMethods.is_presolve(_lp, testmask);

        /// <summary>
        /// Specifies if a presolve must be done before solving.
        /// Can be the combination of any of the <see cref="PreSolveLevels"/> values.
        /// </summary>
        /// <remarks>
        /// <para>Presolve looks at the model and tries to simplify it so that solving times are shorter.
        /// For example a constraint on only one variable is converted to a bound on this variable
        /// (and the constraint is deleted). Note that the model dimensions can change because of this,
        /// so be careful with this. Both rows and columns can be deleted by the presolve.</para>
        /// <para>Note that <see cref="PreSolveLevels.LinearlyDependentRows"/> can result in deletion of rows
        /// (the linear dependent ones).
        /// <see cref="get_constraints"/> will then return only the values of the rows that are
        /// kept and the values of the deleted rows are not known anymore.
        /// </para>
        /// <para>
        /// The default is (<see cref="PreSolveLevels.None"/>) which does not presolve.
        /// </para>
        /// </remarks>
        public PreSolveLevels PreSolveLevels
        {
            get => NativeMethods.get_presolve(_lp);
            set => NativeMethods.set_presolve(_lp, value, PreSolveMaxLoops);
        }

        /// <summary>
        /// The maximum number of times presolve is done.
        /// </summary>
        /// <remarks>
        /// After a presolve is done, another presolve can again result in elimination of extra rows and/or columns.
        /// This number specifies the maximum number of times this process is repeated.
        /// By default this is until presolve has nothing to do anymore.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_presolveloops.htm">Full C API documentation.</seealso>
        public int PreSolveMaxLoops
        {
            get => NativeMethods.get_presolveloops(_lp);
            set => NativeMethods.set_presolve(_lp, PreSolveLevels, value);
        }
        #endregion

        #region Callback methods

        /// <summary>
        /// Sets a handler called regularly while solving the model to verify if solving should abort.
        /// </summary>
        /// <param name="handler">The handler to call regularly while solving the model to verify if solving should abort.</param>
        /// <remarks>
        /// <para>When set, the abort handler is called regularly.
        /// The user can do whatever he wants in this handler.
        /// For example check if the user pressed abort.
        /// When the return value of this handler is <c>true</c>, then lp_solve aborts the solver and returns with an appropriate code.
        /// The abort handler can be cleared by specifying <c>null</c> as <paramref name="handler"/>.</para>
        /// <para>The default is no abort handler (<c>null</c>).</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/put_abortfunc.htm">Full C API documentation.</seealso>
        public void PutAbortHandler(AbortHandler handler)
            => NativeMethods.put_abortfunc(_lp, (x, y) => handler(this), IntPtr.Zero);

        /// <summary>
        /// Sets a log handler.
        /// </summary>
        /// <param name="handler">The log handler.</param>
        /// <remarks>
        /// <para>When set, the log handler is called when lp_solve has someting to report.
        /// The log handler can be cleared by specifying <c>null</c> as <paramref name="handler"/>.</para>
        /// <para>This method is called at the same time as something is written to the file set via <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/put_logfunc.htm">Full C API documentation.</seealso>
        public void PutLogHandler(LogHandler handler)
            => NativeMethods.put_logfunc(_lp, (x, y, message) => handler(this, message), IntPtr.Zero);

        /// <summary>
        /// Sets a message handler called upon certain events.
        /// </summary>
        /// <param name="handler">A handler to call when events defined in <paramref name="mask"/> occur.</param>
        /// <param name="mask">The mask of event types that should trigger a call to the <paramref name="handler"/> handler.</param>
        /// <remarks>
        /// This handler is called when a situation specified in mask occurs.
        /// Note that this handler is called while solving the model.
        /// This can be useful to follow the solving progress.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/put_msgfunc.htm">Full C API documentation.</seealso>
        public void PutMessageHandler(MessageHandler handler, MessageMasks mask)
            => NativeMethods.put_msgfunc(_lp, (x, y, mask)=>handler(this,mask), IntPtr.Zero, mask);


        #endregion

        #region Solve

        /// <summary>
        /// Solve the model.
        /// </summary>
        /// <returns>One of the <see cref="SolveResult"/> enum values.</returns>
        /// <remarks>
        /// <para><see cref="Solve"/> can be called more than once.
        /// Between calls, the model may be modified in every way.
        /// Restrictions may be changed, matrix values may be changed and even rows and/or columns 
        /// may be added or deleted.</para>
        /// <para>If <see cref="set_timeout"/> was called before solve with a non-zero timeout and a timout occurs,
        /// and there was already an integer solution found (that is possibly not the best), 
        /// then solve will return <see cref="SolveResult.SubOptimal"/>.
        /// If there was no integer solution found yet or there are no integers or the solvers is still 
        /// in the first phase where a REAL optimal solution is searched for, then solve will return <see cref="SolveResult.TimedOut"/>.</para>
        /// <para>If <see cref="PreSolveLevels"/> was set before solve, then it can happen that presolve 
        /// eliminates all rows and columns such that the solution is known by presolve.
        /// In that case, no solve is done.
        /// This also means that values of constraints and sensitivity are unknown.
        /// solve will return <see cref="SolveResult.PreSolved"/> in this case.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/solve.htm">Full C API documentation.</seealso>
        public SolveResult Solve()
            => NativeMethods.solve(_lp);

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
        /// The variable must then contain 1+<see cref="NumberOfColumns"/> elements.
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
            => NativeMethods.get_constr_value(_lp, row, count, primsolution, nzindex);

        /// <summary>
        /// Returns the values of the constraints.
        /// </summary>
        /// <param name="constr">An array that will contain the values of the constraints.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>These values are only valid after a successful <see cref="Solve"/>. 
        /// The array must already be dimensioned with <see cref="NumberOfRows"/> elements.
        /// Element 0 will contain the value of the first row, element 1 of the second row, ...</para>
        /// <para>Note that when <see cref="PreSolveLevels"/> was set with parameter <see cref="PreSolveLevels.LinearlyDependentRows"/>
        /// that this can result in deletion of rows (the linear dependent ones). 
        /// This method will then return only the values of the rows that are kept and 
        /// the values of the deleted rows are not known anymore.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_constraints.htm">Full C API documentation.</seealso>
        public bool get_constraints(double[] constr)
            => NativeMethods.get_constraints(_lp, constr);

        /// <summary>
        /// Returns the value(s) of the dual variables aka reduced costs.
        /// </summary>
        /// <param name="rc">An array that will contain the values of the dual variables aka reduced costs.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The <see cref="get_dual_solution"/> method return only the value(s) of the dual variables aka reduced costs.</para>
        /// <para>These values are only valid after a successful <see cref="Solve"/> and if there are integer variables in the model then only if <see cref="PreSolveLevels"/>
        /// is set before <see cref="Solve"/> with parameter <see cref="PreSolveLevels.CalculateSensitivityDuals"/>.</para>
        /// <para><paramref name="rc"/> needs to already be dimensioned with 1+<see cref="NumberOfRows"/>+<see cref="NumberOfColumns"/> elements.</para>
        /// <para>For method <see cref="get_dual_solution"/>, the index starts from 1 and element 0 is not used.
        /// The first <see cref="NumberOfRows"/> elements contain the duals of the constraints, 
        /// the next <see cref="NumberOfColumns"/> elements contain the duals of the variables.</para>
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
            => NativeMethods.get_dual_solution(_lp, rc);

        /// <summary>
        /// Returns the deepest Branch-and-bound level of the last solution.
        /// </summary>
        /// <returns>The deepest Branch-and-bound level of the last solution.</returns>
        /// <remarks>
        /// Is only applicable if the model contains integer variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_max_level.htm">Full C API documentation.</seealso>
        public int get_max_level()
            => NativeMethods.get_max_level(_lp);

        /// <summary>
        /// Returns the value of the objective function.
        /// </summary>
        /// <returns>The value of the objective function.</returns>
        /// <remarks>
        /// <para>This value is only valid after a successful <see cref="Solve"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_objective.htm">Full C API documentation.</seealso>
        public double get_objective()
            => NativeMethods.get_objective(_lp);

        /// <summary>
        /// Returns the solution of the model.
        /// </summary>
        /// <param name="pv">An array that will contain the value of the objective function (element 0),
        /// values of the constraints (elements 1 till Nrows),
        /// and the values of the variables (elements Nrows+1 till Nrows+NColumns).
        /// </param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>These values are only valid after a successful <see cref="Solve"/>.
        /// <paramref name="pv"/> needs to be already dimensioned with 1 + <see cref="NumberOfRows"/> + <see cref="NumberOfColumns"/> elements. 
        /// Element 0 is the value of the objective function, elements 1 till Nrows the values of the constraints and elements Nrows+1 till Nrows+NColumns the values of the variables.
        /// </para>
        /// <para>Special considerations when presolve was done. When <see cref="PreSolveLevels"/> is set before solve, 
        /// then presolve can have deleted both rows and columns from the model because they could be eliminated.
        /// This influences <see cref="get_primal_solution"/>.
        /// This method only reports the values of the remaining variables and constraints.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_primal_solution.htm">Full C API documentation.</seealso>
        public bool get_primal_solution(double[] pv)
            => NativeMethods.get_primal_solution(_lp, pv);

        /// <summary>
        /// Returns the sensitivity of the objective function.
        /// </summary>
        /// <param name="objfrom">An array that will contain the values of the lower limits on the objective function.</param>
        /// <param name="objtill">An array that will contain the values of the upper limits of the objective function.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The <see cref="get_sensitivity_obj"/> and <see cref="get_sensitivity_objex"/> methods return 
        /// the sensitivity of the objective function.</para>
        /// <para>These values are only valid after a successful <see cref="Solve"/> and if there are integer
        /// variables in the model then only if <see cref="PreSolveLevels"/> is set before <see cref="Solve"/>
        /// with parameter <see cref="PreSolveLevels.CalculateSensitivityDuals"/>.
        /// The arrays must already be dimensioned with <see cref="NumberOfColumns"/> elements.
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
            => NativeMethods.get_sensitivity_obj(_lp, objfrom, objtill);

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
        /// <para>These values are only valid after a successful <see cref="Solve"/> and if there are integer
        /// variables in the model then only if <see cref="PreSolveLevels"/> is set before <see cref="Solve"/>
        /// with parameter <see cref="PreSolveLevels.CalculateSensitivityDuals"/>.
        /// The arrays must already be dimensioned with <see cref="NumberOfColumns"/> elements.
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
            => NativeMethods.get_sensitivity_objex(_lp, objfrom, objtill, objfromvalue, objtillvalue);

        /// <summary>
        /// Returns the sensitivity of the constraints and the variables.
        /// </summary>
        /// <param name="duals">An array that will contain the values of the dual variables aka reduced costs.</param>
        /// <param name="dualsfrom">An array that will contain the values of the lower limits on the dual variables aka reduced costs.</param>
        /// <param name="dualstill">An array that will contain the values of the upper limits on the dual variables aka reduced costs.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>The method returns the values of the dual variables aka reduced costs and their limits.</para>
        /// <para>These values are only valid after a successful solve and if there are integer variables in the model then only if <see cref="PreSolveLevels"/>
        /// is set before <see cref="Solve"/> with parameter <see cref="PreSolveLevels.CalculateSensitivityDuals"/>.</para>
        /// <para>The arrays need to be alread already dimensioned with <see cref="NumberOfRows"/>+<see cref="NumberOfColumns"/> elements.</para>
        /// <para>Element 0 will contain the value of the first row, element 1 of the second row, ...
        /// Element <see cref="NumberOfRows"/> contains the value for the first variable, element <see cref="NumberOfRows"/>+1 the value for the second variable and so on.</para>
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
            => NativeMethods.get_sensitivity_rhs(_lp, duals, dualsfrom, dualstill);

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
            => NativeMethods.get_solutioncount(_lp);

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
            => NativeMethods.get_total_iter(_lp);

        /// <summary>
        /// Returns the total number of nodes processed in branch-and-bound of the last solution.
        /// </summary>
        /// <returns>The total number of nodes processed in branch-and-bound of the last solution.</returns>
        /// <remarks>
        /// Is only applicable if the model contains integer variables.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_total_nodes.htm">Full C API documentation.</seealso>
        public long get_total_nodes()
            => NativeMethods.get_total_nodes(_lp);

        /// <summary>
        /// Returns the reduced cost on a variable.
        /// </summary>
        /// <param name="index">The column of the variable for which the reduced cost is required.
        /// Note that this is the column number before presolve was done, if active.
        /// If index is 0, then the value of the objective function is returned.</param>
        /// <returns>The reduced cost on the variable at <paramref name="index"/>.</returns>
        /// <remarks>
        /// <para>The method returns only the value of the dual variables aka reduced costs.</para>
        /// <para>This value is only valid after a successful <see cref="Solve"/> and if there are integer variables in the model then only if <see cref="PreSolveLevels"/>
        /// is set before <see cref="Solve"/> with parameter <see cref="PreSolveLevels.CalculateSensitivityDuals"/>.</para>
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
            => NativeMethods.get_var_dualresult(_lp, index);

        /// <summary>
        /// Returns the solution of the model.
        /// </summary>
        /// <param name="index">The original index of the variable in the model no matter if <see cref="PreSolveLevels"/> is set before <see cref="Solve"/>.</param>
        /// <returns>The value of the solution for variable at <paramref name="index"/>.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_primal_solution.htm">Full C API documentation.</seealso>
        public double get_var_primalresult(int index)
            => NativeMethods.get_var_primalresult(_lp, index);

        /// <summary>
        /// Returns the values of the variables.
        /// </summary>
        /// <param name="var">An array that will contain the values of the variables.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>These values are only valid after a successful <see cref="Solve"/>. 
        /// <paramref name="var"/> must already be dimensioned with <see cref="NumberOfColumns"/> elements.
        /// Element 0 will contain the value of the first variable, element 1 of the second variable, ...</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_variables.htm">Full C API documentation.</seealso>
        public bool get_variables(double[] var)
            => NativeMethods.get_variables(_lp, var);

        /// <summary>
        /// Returns the value of the objective function.
        /// </summary>
        /// <returns>The current value of the objective while solving the model.</returns>
        /// <remarks>This value can be retrieved while solving in a callback method.</remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_working_objective.htm">Full C API documentation.</seealso>
        public double get_working_objective()
            => NativeMethods.get_working_objective(_lp);

        /// <summary>
        /// Checks if provided solution is a feasible solution.
        /// </summary>
        /// <param name="values">
        ///   <para>An array of row/column values that are checked against the bounds and ranges.</para>
        ///   <para>The array must have <see cref="NumberOfRows"/>+<see cref="NumberOfColumns"/> elements. Element 0 is not used.</para>
        /// </param>
        /// <param name="threshold">A tolerance value. The values may differ that much. Recommended to use <see cref="ModelTolerance.IntegerEpsilon"/> for this value.</param>
        /// <returns><c>true</c> if <paramref name="values"/> represent a solution to the model, <c>false</c> otherwise</returns>
        /// <remarks>
        /// <para>All values of the values array must be between the bounds and ranges to be a feasible solution.</para>
        /// <para>This value is only valid after a successful <see cref="Solve"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_feasible.htm">Full C API documentation.</seealso>
        public bool is_feasible(double[] values, double threshold)
            => NativeMethods.is_feasible(_lp, values, threshold);

        /// <summary>
        /// Returns the accuracy of bounds and constraints.
        /// </summary>
        /// <returns>The accuracy of bounds and constraints.</returns>
        /// <remarks>
        /// <para>
        /// This value should be as close as possible to 0. The accuracy is the largest relative deviation of a bound or constraint.
        /// </para>
        /// <para>Available since v4.1.0.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_accuracy.htm">Full C API documentation.</seealso>
        public double get_accuracy()
            => NativeMethods.get_accuracy(_lp);

        #endregion

        #region Debug/print settings

        /// <summary>
        /// Defines the output when lp_solve has something to report.
        /// </summary>
        /// <param name="filename">The file to print the results to.
        /// If <c>null</c>, then output is stdout again.
        /// If "", then output is ignored.
        /// It doesn't go to the console or to a file then.
        /// This is useful in combination with <see cref="PutLogHandler"/> to redirect output to somewhere completely different.</param>
        /// <returns><c>true</c> if the file could be opened, else <c>false</c>.</returns>
        /// <remarks>
        /// <para>This is done at the same time as something is reported via <see cref="PutLogHandler"/>.
        /// The default reporting output is screen (stdout). 
        /// If <see cref="set_outputfile"/> is called to change output to the specified file, then the file is automatically closed when <see cref="LpSolve"/> is disposed.
        /// Note that this was not the case in previous versions of lp_solve.
        /// If filename is "", then output is ignored.
        /// It doesn't go to the console or to a file then.
        /// This is useful in combination with put_logfunc to redirect output to somewhere completely different.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_output.htm">Full C API documentation.</seealso>
        public bool set_outputfile(string filename)
            => NativeMethods.set_outputfile(_lp, filename);

        /// <summary>
        /// A flag defining if all intermediate valid solutions must be printed while solving.
        /// </summary>
        /// <remarks>
        /// This property is meant for debugging purposes. The default is not to print <see cref="PrintSolutionOption.False"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_print_sol.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_print_sol.htm">Full C API documentation (set).</seealso>
        public PrintSolutionOption PrintSolutionOption
        {
            get => NativeMethods.get_print_sol(_lp);
            set => NativeMethods.set_print_sol(_lp, value);
        }

        /// <summary>
        /// Defines the level of verbosity from lp_solve to the user.
        /// </summary>
        /// <returns>The <see cref="Verbosity"/> level.</returns>
        /// <remarks>
        /// <para>lp_solve reports information back to the user.
        /// How much information is reported depends on the verbosity level.
        /// The default verbosity level is <see cref="Verbosity.Normal"/>.
        /// lp_solve determines how verbose a given message is.
        /// For example specifying a wrong row/column index values is considered as a <see cref="Verbosity.Severe"/> error.
        /// Verbosity determines how much of the lp_solve messages are reported.
        /// All messages equal to and below the set level are reported.</para>
        /// <para>The default reporting device is the console screen.
        /// It is possible to set a used defined reporting callback via <see cref="PutLogHandler"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_verbose.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_verbose.htm">Full C API documentation (set).</seealso>
        public Verbosity Verbosity
        {
            get => NativeMethods.get_verbose(_lp);
            set => NativeMethods.set_verbose(_lp, value);
        }

        /// <summary>
        /// Flag that defines if all intermediate results and the branch-and-bound decisions must be printed while solving.
        /// </summary>
        /// <remarks>
        /// This method is meant for debugging purposes. The default is not to print (<c>false</c>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_debug.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_debug.htm">Full C API documentation.</seealso>
        public bool IsDebug
        {
            get => NativeMethods.is_debug(_lp);
            set => NativeMethods.set_debug(_lp, value);
        }

        /// <summary>
        /// Flag that defines if pivot selection must be printed while solving.
        /// </summary>
        /// <remarks>
        /// This method is meant for debugging purposes. The default is not to print (<c>false</c>).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_trace.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_trace.htm">Full C API documentation.</seealso>
        public bool IsTrace
        {
            get => NativeMethods.is_trace(_lp);
            set => NativeMethods.set_trace(_lp, value);
        }
        #endregion

        #region Debug/print

        /// <summary>
        /// Prints the values of the constraints of the lp model.
        /// </summary>
        /// <param name="columns">Number of columns to print solution.</param>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="Solve"/>.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_constraints.htm">Full C API documentation.</seealso>
        public void print_constraints(int columns)
            => NativeMethods.print_constraints(_lp, columns);

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
            => NativeMethods.print_debugdump(_lp, filename);

        /// <summary>
        /// Prints the values of the duals of the lp model.
        /// </summary>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="Solve"/>.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_duals.htm">Full C API documentation.</seealso>
        public void print_duals()
            => NativeMethods.print_duals(_lp);

        /// <summary>
        /// Prints the lp model.
        /// </summary>
        /// <remarks>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_lp.htm">Full C API documentation.</seealso>
        public void print_lp()
            => NativeMethods.print_lp(_lp);

        /// <summary>
        /// Prints the objective value of the lp model.
        /// </summary>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="Solve"/>.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_objective.htm">Full C API documentation.</seealso>
        public void print_objective()
            => NativeMethods.print_objective(_lp);

        /// <summary>
        /// Prints the scales of the lp model.
        /// </summary>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="Solve"/>.</para>
        /// <para>It will only output something when the model is scaled.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_scales.htm">Full C API documentation.</seealso>
        public void print_scales()
            => NativeMethods.print_scales(_lp);

        /// <summary>
        /// Prints the solution (variables) of the lp model.
        /// </summary>
        /// <param name="columns">Number of columns to print solution.</param>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="Solve"/>.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_solution.htm">Full C API documentation.</seealso>
        public void print_solution(int columns)
            => NativeMethods.print_solution(_lp, columns);

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
            => NativeMethods.print_str(_lp, str);

        /// <summary>
        /// Prints the tableau.
        /// </summary>
        /// <remarks>
        /// <para>This method only works after a successful <see cref="Solve"/>.</para>
        /// <para>This method is meant for debugging purposes. By default, the output is stdout.
        /// However this can be changed via a call to <see cref="set_outputfile"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/print_tableau.htm">Full C API documentation.</seealso>
        public void print_tableau()
            => NativeMethods.print_tableau(_lp);

        #endregion

        #region Write model to file

        /// <summary>
        /// Write the model in the lp format to <paramref name="filename"/>.
        /// </summary>
        /// <param name="filename">Filename to write the model to.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. See <see cref="EntryMode"/>.</para>
        /// <para>The model in the file will be in <seealso href="http://lpsolve.sourceforge.net/5.5/lp-format.htm">lp-format</seealso></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/write_lp.htm">Full C API documentation.</seealso>
        public bool write_lp(string filename)
            => NativeMethods.write_lp(_lp, filename);

        /// <summary>
        /// Write the model in the Free MPS format to <paramref name="filename"/> or 
        /// if <paramref name="filename"/> is <c>null</c>, to default output.
        /// </summary>
        /// <param name="filename">Filename to write the model to or <c>null</c> to write to default output.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. See <see cref="EntryMode"/>.</para>
        /// <para>When <paramref name="filename"/> is <c>null</c>, then output is written to the output 
        /// set by <see cref="set_outputfile"/>. By default this is stdout.</para>
        /// <para>The model in the file will be in <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">mps-format</seealso></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/write_mps.htm">Full C API documentation.</seealso>
        public bool write_freemps(string filename)
            => NativeMethods.write_freemps(_lp, filename);

        /// <summary>
        /// Write the model in the Fixed MPS format to <paramref name="filename"/> or 
        /// if <paramref name="filename"/> is <c>null</c>, to default output.
        /// </summary>
        /// <param name="filename">Filename to write the model to or <c>null</c> to write to default output.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. See <see cref="EntryMode"/>.</para>
        /// <para>When <paramref name="filename"/> is <c>null</c>, then output is written to the output 
        /// set by <see cref="set_outputfile"/>. By default this is stdout.</para>
        /// <para>The model in the file will be in <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">mps-format</seealso></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/write_mps.htm">Full C API documentation.</seealso>
        public bool write_mps(string filename)
            => NativeMethods.write_mps(_lp, filename);

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
            => NativeMethods.is_nativeXLI(_lp);

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
            => NativeMethods.has_XLI(_lp);

        /// <summary>
        /// Sets External Language Interfaces package.
        /// </summary>
        /// <param name="filename">The name of the XLI package.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This call is normally only needed when <see cref="write_XLI"/> will be called. 
        /// <see cref="CreateFromXLIFile"/> automatically calls this method</para>
        /// <para>See <seealso href="http://lpsolve.sourceforge.net/5.5/XLI.htm">External Language Interfaces</seealso>
        /// for a complete description on XLIs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_XLI.htm">Full C API documentation.</seealso>
        public bool set_XLI(string filename)
            => NativeMethods.set_XLI(_lp, filename);

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
            => NativeMethods.write_XLI(_lp, filename, options, results);

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
                NativeMethods.lp_solve_version(ref major, ref minor, ref release, ref build);
                return new Version(major, minor, release, build);
            }
        }

        /// <summary>
        /// Checks if a column is already present in the lp model.
        /// </summary>
        /// <param name="column">An array with 1+<see cref="NumberOfRows"/> elements that are checked against the existing columns in the lp model.</param>
        /// <returns>The (first) column number if the column is already in the lp model and 0 if not.</returns>
        /// <remarks>
        /// <para>It does not look at bounds and types, only at matrix values.</para>
        /// <para>The first matched column is returned. If there is no column match, then 0 is returned.</para>
        /// <para>Note that element 0 is the objective function value. Element 1 is column 1, and so on.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/column_in_lp.htm">Full C API documentation.</seealso>
        public int column_in_lp(double[] column)
            => NativeMethods.column_in_lp(_lp, column);

        /// <summary>
        /// Creates the dual of the current model.
        /// </summary>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/dualize_lp.htm">Full C API documentation.</seealso>
        public bool dualize_lp()
            => NativeMethods.dualize_lp(_lp);

        /// <summary>
        /// Returns the number of non-zero elements in the matrix.
        /// </summary>
        /// <returns>The number of non-zeros in the matrix.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_nonzeros.htm">Full C API documentation.</seealso>
        public int get_nonzeros()
            => NativeMethods.get_nonzeros(_lp);


        /// <summary>
        /// Returns the number of columns (variables) in the lp model.
        /// </summary>
        /// <remarks>
        /// <para>Note that the number of columns can change when a presolve is done
        /// or when negative variables are split in a positive and a negative part.</para>
        /// <para>Therefore it is advisable to use this method to determine how many columns there are
        /// in the lp model instead of relying on an own count.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_Ncolumns.htm">Full C API documentation.</seealso>
        public int NumberOfColumns
            => NativeMethods.get_Ncolumns(_lp);

        /// <summary>
        /// Returns the number of original columns (variables) in the lp model.
        /// </summary>
        /// <remarks>
        /// <para>Note that the number of columns (<see cref="NumberOfColumns"/>) can change when a presolve is done
        /// or when negative variables are split in a positive and a negative part.</para>
        /// <para><see cref="NumberOfColumnsOriginally"/> does not change and thus returns the original number of columns in the lp model.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_Norig_columns.htm">Full C API documentation.</seealso>
        public int NumberOfColumnsOriginally
            => NativeMethods.get_Norig_columns(_lp);

        /// <summary>
        /// Returns the number of rows (constraints) in the lp model.
        /// </summary>
        /// <remarks>
        /// <para>Note that the number of rows can change when a presolve is done.</para>
        /// <para>Therefore it is advisable to use this method to determine how many rows there are
        /// in the lp model instead of relying on an own count.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_Nrows.htm">Full C API documentation.</seealso>
        public int NumberOfRows
            => NativeMethods.get_Nrows(_lp);

        /// <summary>
        /// Returns the number of original rows (constraints) in the lp model.
        /// </summary>
        /// <remarks>
        /// <para>Note that the number of rows (<see cref="NumberOfRows"/>) can change when a presolve is done.</para>
        /// <para><see cref="NumberOfRowsOriginally"/> does not change and thus returns the original number of rows in the lp model.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_Norig_rows.htm">Full C API documentation.</seealso>
        public int NumberOfRowsOriginally
            => NativeMethods.get_Norig_rows(_lp);


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
            => NativeMethods.get_status(_lp);

        /// <summary>
        /// Returns the description of a returncode of the <see cref="Solve"/> method.
        /// </summary>
        /// <param name="statuscode">Returncode of <see cref="Solve"/></param>
        /// <returns>The description of a returncode of the <see cref="Solve"/> method</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_statustext.htm">Full C API documentation.</seealso>
        public string get_statustext(int statuscode)
            => NativeMethods.get_statustext(_lp, statuscode);

        /// <summary>
        /// Gets the time elapsed since start of solve.
        /// </summary>
        /// <returns>The number of seconds after <see cref="Solve"/> has started.</returns>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/time_elapsed.htm">Full C API documentation.</seealso>
        public double time_elapsed()
            => NativeMethods.time_elapsed(_lp);

        /// <summary>
        /// Returns the index in the lp of the original row/column.
        /// </summary>
        /// <param name="orig_index">Original constraint or column number. 
        /// If <paramref name="orig_index"/> is between 1 and <see cref="NumberOfRowsOriginally"/> then the index is a constraint (row) number.
        /// If <paramref name="orig_index"/> is between 1+<see cref="NumberOfRowsOriginally"/> and <see cref="NumberOfRowsOriginally"/> + <see cref="NumberOfColumnsOriginally"/>
        /// then the index is a column number.</param>
        /// <returns>The index in the lp of the original row/column.</returns>
        /// <remarks>
        /// <para>Note that the number of constraints(<see cref="NumberOfRows"/>) and columns(<see cref="NumberOfColumns"/>) can change when
        /// a presolve is done or when negative variables are split in a positive and a negative part.
        /// <see cref="get_lp_index"/> returns the position of the constraint/variable in the lp model.
        /// If <paramref name="orig_index"/> is not a legal index  or the constraint/column is deleted,
        /// the return value is <c>0</c>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_lp_index.htm">Full C API documentation.</seealso>
        public int get_lp_index(int orig_index)
            => NativeMethods.get_lp_index(_lp, orig_index);

        /// <summary>
        /// Returns the original row/column where a constraint/variable was before presolve.
        /// </summary>
        /// <param name="lp_index">Constraint or column number.
        /// If <paramref name="lp_index"/> is between 1 and <see cref="NumberOfRows"/> then the index is a constraint (row) number.
        /// If <paramref name="lp_index"/> is between 1+<see cref="NumberOfRows"/> and <see cref="NumberOfRows"/> + <see cref="NumberOfColumns"/>
        /// then the index is a column number.</param>
        /// <returns>The original row/column where a constraint/variable was before presolve.</returns>
        /// <remarks>
        /// <para>Note that the number of constraints(<see cref="NumberOfRows"/>) and columns(<see cref="NumberOfColumns"/>) can change when
        /// a presolve is done or when negative variables are split in a positive and a negative part.
        /// <see cref="get_orig_index"/> returns the original position of the constraint/variable.
        /// If <paramref name="lp_index"/> is not a legal index, the return value is <c>0</c>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_orig_index.htm">Full C API documentation.</seealso>
        public int get_orig_index(int lp_index)
            => NativeMethods.get_orig_index(_lp, lp_index);

        #endregion
    }
}
