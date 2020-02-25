using System;
using LpSolveDotNet.Idiomatic.Internal;

namespace LpSolveDotNet.Idiomatic
{

    /// <summary>
    /// Helper class to handle model about Basis.
    /// </summary>
    public class ModelBasis
        : ModelSubObjectBase
    {
        internal ModelBasis(IntPtr lp)
            : base(lp)
        {
        }

        /// <summary>
        /// Sets the starting base to an all slack basis (the default simplex starting basis).
        /// </summary>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/default_basis.htm">Full C API documentation.</seealso>
        public void SetDefault()
            => NativeMethods.default_basis(Lp);

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
        /// <para>When a restart is done, lp_solve continues at the last basis, except if <see cref="Set"/>,
        /// <see cref="SetDefault"/>, <see cref="Guess"/> or <see cref="ReadFromFile"/> is called.</para>
        /// <para>The basis in the file must be in <see href="http://lpsolve.sourceforge.net/5.5/bas-format.htm">MPS bas file format</see>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/read_basis.htm">Full C API documentation.</seealso>
        public bool ReadFromFile(string filename, string info)
            => NativeMethods.read_basis(Lp, filename, info)
                .HandleResultAndReturnIt(ReturnValueHandler);

        /// <summary>
        /// Writes current basis to a file.
        /// </summary>
        /// <param name="filename">Name of file to write the basis to.</param>
        /// <returns><c>true</c> if basis could be written from <paramref name="filename"/> and <c>false</c> if not.
        /// A <c>false</c> return value indicates an error.
        /// Specifically file could not be opened or written to.</returns>
        /// <remarks>
        /// <para>This method writes current basis to a file which can later be reused by <see cref="ReadFromFile"/> to reset the basis.</para>
        /// <para>Setting an initial basis can speed up the solver considerably.
        /// It is the starting point from where the algorithm continues to find an optimal solution.</para>
        /// <para>When a restart is done, lp_solve continues at the last basis, except if <see cref="Set"/>,
        /// <see cref="SetDefault"/>, <see cref="Guess"/> or <see cref="ReadFromFile"/> is called.</para>
        /// <para>The basis in the file is written in <see href="http://lpsolve.sourceforge.net/5.5/bas-format.htm">MPS bas file format</see>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/write_basis.htm">Full C API documentation.</seealso>
        public bool WriteToFile(string filename)
            => NativeMethods.write_basis(Lp, filename)
                .HandleResultAndReturnIt(ReturnValueHandler);

        /// <summary>
        /// Sets an initial basis of the model.
        /// </summary>
        /// <param name="bascolumn">An array with 1+<see cref="LpSolve.NumberOfRows"/> or
        /// 1+<see cref="LpSolve.NumberOfRows"/>+<see cref="LpSolve.NumberOfColumns"/> elements that specifies the basis.</param>
        /// <param name="nonbasic">If <c>false</c>, then <paramref name="bascolumn"/> must have 
        /// 1+<see cref="LpSolve.NumberOfRows"/> elements and only contains the basic variables. 
        /// If <c>true</c>, then <paramref name="bascolumn"/> must have 1+<see cref="LpSolve.NumberOfRows"/>+<see cref="LpSolve.NumberOfColumns"/> 
        /// elements and will also contain the non-basic variables.</param>
        /// <returns><c>true</c> if provided basis was set. <c>false</c> if not.
        /// If <c>false</c> then provided data was invalid.</returns>
        /// <remarks>
        /// <para>The array receives the basic variables and if <paramref name="nonbasic"/> is <c>true</c>,
        /// then also the non-basic variables.
        /// If an element is less then zero then it means on lower bound, else on upper bound.</para>
        /// <para>Element 0 of the array is unused.</para>
        /// <para>The default initial basis is <c><paramref name="bascolumn"/>[x] = -x</c>.</para>
        /// <para>Each element represents a basis variable.
        /// If the absolute value is between 1 and <see cref="LpSolve.NumberOfRows"/>, it represents a slack variable 
        /// and if it is between <see cref="LpSolve.NumberOfRows"/>+1 and <see cref="LpSolve.NumberOfRows"/>+<see cref="LpSolve.NumberOfColumns"/>
        /// then it represents a regular variable.
        /// If the value is negative, then the variable is on its lower bound.
        /// If positive it is on its upper bound.</para>
        /// <para>Setting an initial basis can speed up the solver considerably.
        /// It is the starting point from where the algorithm continues to find an optimal solution.</para>
        /// <para>When a restart is done, lp_solve continues at the last basis, except if except if <see cref="Set"/>,
        /// <see cref="SetDefault"/>, <see cref="Guess"/> or <see cref="ReadFromFile"/> is called.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_basis.htm">Full C API documentation.</seealso>
        public bool Set(int[] bascolumn, bool nonbasic)
        {
            return NativeMethods.set_basis(Lp, bascolumn, nonbasic)
                .HandleResultAndReturnIt(ReturnValueHandler);
        }

        /// <summary>
        /// Returns the basis of the model.
        /// </summary>
        /// <param name="bascolumn">An array with 1+<see cref="LpSolve.NumberOfRows"/> or
        /// 1+<see cref="LpSolve.NumberOfRows"/>+<see cref="LpSolve.NumberOfColumns"/> elements that will contain the basis after the call.</param>
        /// <param name="nonbasic">If <c>false</c>, then <paramref name="bascolumn"/> must have 
        /// 1+<see cref="LpSolve.NumberOfRows"/> elements and only contains the basic variables. 
        /// If <c>true</c>, then <paramref name="bascolumn"/> must have 1+<see cref="LpSolve.NumberOfRows"/>+<see cref="LpSolve.NumberOfColumns"/> 
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
        /// If the absolute value is between 1 and <see cref="LpSolve.NumberOfRows"/>, it represents a slack variable 
        /// and if it is between <see cref="LpSolve.NumberOfRows"/>+1 and <see cref="LpSolve.NumberOfRows"/>+<see cref="LpSolve.NumberOfColumns"/>
        /// then it represents a regular variable.
        /// If the value is negative, then the variable is on its lower bound.
        /// If positive it is on its upper bound.</para>
        /// <para>Setting an initial basis can speed up the solver considerably.
        /// It is the starting point from where the algorithm continues to find an optimal solution.</para>
        /// <para>When a restart is done, lp_solve continues at the last basis, except if except if <see cref="Set"/>,
        /// <see cref="SetDefault"/>, <see cref="Guess"/> or <see cref="ReadFromFile"/> is called.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_basis.htm">Full C API documentation.</seealso>
        public bool Get(int[] bascolumn, bool nonbasic)
            => NativeMethods.get_basis(Lp, bascolumn, nonbasic)
                .HandleResultAndReturnIt(ReturnValueHandler);

        /// <summary>
        /// Create a starting base from the provided guess vector.
        /// </summary>
        /// <param name="guessvector">A vector that must contain a feasible solution vector.
        /// It must contain at least 1+<see cref="LpSolve.NumberOfColumns"/> elements.
        /// Element 0 is not used.</param>
        /// <param name="basisvector">When successful, this vector contains a feasible basis corresponding to guessvector.
        /// The array must already be dimensioned for at least 1+<see cref="LpSolve.NumberOfRows"/>+<see cref="LpSolve.NumberOfColumns"/> elements.
        /// When the method returns successfully, <paramref name="basisvector"/> is filled with the basis.
        /// This array can be provided to <see cref="Set"/>.</param>
        /// <returns><c>true</c> if a valid base could be determined, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method is meant to find a basis based on provided variable values.
        /// This basis can be provided to lp_solve via <see cref="Set"/>.
        /// This can result in getting faster to an optimal solution.
        /// However the simplex algorithm doesn't guarantee you that.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/guess_basis.htm">Full C API documentation.</seealso>
        public bool Guess(double[] guessvector, int[] basisvector)
            => NativeMethods.guess_basis(Lp, guessvector, basisvector);

        /// <summary>
        /// Represents which basis crash mode must be used.
        /// </summary>
        /// <remarks>
        /// <para>Default is <see cref="BasisCrashMode.None"/></para>
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
        public BasisCrashMode CrashMode
        {
            get => NativeMethods.get_basiscrash(Lp);
            set => NativeMethods.set_basiscrash(Lp, value);
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
        public bool HasBasisFactorizationPackage
            => NativeMethods.has_BFP(Lp);

        /// <summary>
        /// Returns if the native (build-in) basis factorization package (BFP) is used, or an external package.
        /// </summary>
        /// <returns><c>true</c> if the native (build-in) BFP is used, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method checks if an external basis factorization package (BFP) is set or not.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/BFP.htm">Basis Factorization Packages</see> for a complete description on BFPs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_nativeBFP.htm">Full C API documentation.</seealso>
        public bool IsNativeBasisFactorizationPackage
            => NativeMethods.is_nativeBFP(Lp);

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
        public bool SetBasisFactorizationPackage(string filename)
            => NativeMethods.set_BFP(Lp, filename)
                .HandleResultAndReturnIt(ReturnValueHandler);
    }
}
