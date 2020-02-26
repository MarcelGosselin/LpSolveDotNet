using System;
using LpSolveDotNet.Idiomatic.Internal;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Helper class to handle model at ObjectiveFunction level.
    /// </summary>
    public class ModelObjectiveFunction
        : ModelSubObjectBase
    {
        internal ModelObjectiveFunction(IntPtr lp)
            : base(lp)
        {
        }

        /// <summary>
        /// Represents an initial "at least better than" guess for objective function.
        /// </summary>
        /// <remarks>
        /// <para>This is only used in the branch-and-bound algorithm when integer variables exist in the model. All solutions with a worse objective value than this value are immediately rejected. This can result in faster solving times, but it can be difficult to predict what value to take for this bound. Also there is the chance that the found solution is not the most optimal one.</para>
        /// <para>The default is infinite.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_obj_bound.htm">Full C API documentation *get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj_bound.htm">Full C API documentation (set).</seealso>
        public double Bound
        {
            get => NativeMethods.get_obj_bound(Lp);
            set => NativeMethods.set_obj_bound(Lp, value);
        }

        /// <summary>
        /// Sets the objective function (row 0) of the matrix.
        /// </summary>
        /// <param name="rowValues">
        /// When <paramref name="columnNumbers"/> is <c>null</c>, an array of length 1+<see cref="LpSolve.NumberOfColumns"/> with all the values for the row (starting at index 1).
        /// When <paramref name="columnNumbers"/> is not <c>null</c>, same thing but with a smaller array containing only the *non-zero* values (starting index at 0).
        /// </param>
        /// <param name="columnNumbers">Zero-based array specifying the column numbers for the values in <paramref name="rowValues"/>.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This method sets the values of the objective function in the model at once.</para>
        /// <para>Note that when <paramref name="columnNumbers"/> is <c>null</c>, element 0 of the array is ignored.
        /// Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>This method has the possibility to specify only the non-zero elements.
        /// In that case <paramref name="columnNumbers"/> specifies the column numbers 
        /// of the non-zero elements. Both <paramref name="rowValues"/> and <paramref name="columnNumbers"/> are then zero-based arrays. 
        /// This will speed up building the model considerably if there are a lot of zero values.
        /// In most cases the matrix is sparse and has many zero values.
        /// </para>
        /// <para>It is more performant to call this method rather than multiple times <see cref="ModelObjectiveFunction.SetValue"/>.</para>
        /// <para>Note that it is advised to set the objective function before adding rows via <see cref="ModelRows.Add"/>.
        /// This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj_fn.htm">Full C API documentation.</seealso>
        public bool SetValues(double[] rowValues, int[] columnNumbers = null)
            => NativeMethods.set_obj_fnex(Lp, rowValues?.Length ?? 0, rowValues, columnNumbers)
                .HandleResultAndReturnIt(ReturnValueHandler);

        /// <summary>
        /// Sets the objective function (row 0) of the matrix for a single variable (column)
        /// </summary>
        /// <param name="column">The column number for which the value must be set.</param>
        /// <param name="value">The value that must be set.</param>
        /// <returns><c>true</c> if operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that element 0 of the array is not considered (i.e. ignored). Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>It is better to use <see cref="SetValues"/>.</para>
        /// <para>Note that it is advised to set the objective function before adding rows via <see cref="ModelRows.Add"/>.
        /// This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_obj.htm">Full C API documentation.</seealso>
        public bool SetValue(int column, double value)
            => NativeMethods.set_obj(Lp, column, value)
                .HandleResultAndReturnIt(ReturnValueHandler);

        /// <summary>
        /// Defines whether the lp model tries to maximize the objective function (<c>true</c>) or minimize it (<c>false</c>).
        /// </summary>
        /// <remarks>
        /// The default of lp_solve is to minimize, except for <see cref="LpSolve.CreateFromLPFile"/> where the default is to maximize.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_maxim.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_minim.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_maxim.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_sense.htm">Full C API documentation.</seealso>
        public bool IsMaximizing
        {
            get => NativeMethods.is_maxim(Lp);
            set => NativeMethods.set_sense(Lp, value);
        }
    }
}
