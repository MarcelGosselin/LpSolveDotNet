using System;
using LpSolveDotNet.Idiomatic.Internal;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Helper class to handle model at cell level.
    /// </summary>
    public class ModelCells
        : ModelSubObjectBase
    {
        internal ModelCells(IntPtr lp)
            : base(lp)
        {
        }

        /// <summary>
        /// Sets a single element in the matrix.
        /// </summary>
        /// <param name="rowIndex">Row number of the matrix. Must be between 0 and number of rows in the model. Row 0 is objective function.</param>
        /// <param name="columnIndex">Column number of the matrix. Must be between 1 and number of columns in the model.</param>
        /// <remarks>
        /// <para>Getter: This method is not efficient if many values are to be retrieved.
        /// Consider to use <see cref="ModelRow.GetValues"/>, <see cref="ModelRow.GetNonZeroValues"/>, <see cref="ModelColumn.GetValues"/>, <see cref="ModelColumn.GetNonZeroValues"/>.
        /// </para>
        /// <para>Getter: If <paramref name="rowIndex"/> and/or <paramref name="columnIndex"/> are outside the allowed range, the function returns <c>0</c>.</para>
        /// <para>Setter: If there was already a value for this element, it is replaced and if there was no value, it is added.</para>
        /// <para>Setter: This method is not efficient if many values are to be set.
        /// Consider to use <see cref="ModelRows.Add"/>,
        /// <see cref="ModelRow.SetValues"/>, <see cref="ModelObjectiveFunction.SetValues"/>, <see cref="ModelObjectiveFunction.SetValue"/>, <see cref="ModelColumns.Add"/>,
        /// <see cref="ModelColumn.SetValues"/>.</para>
        /// <para>Setter: Note that row entry mode must be off, else this method also fails.
        /// See <see cref="EntryMode"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_mat.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_mat.htm">Full C API documentation (set).</seealso>
        public double this[int rowIndex, int columnIndex]
        {
            get => NativeMethods.get_mat(Lp, rowIndex, columnIndex);
            set => NativeMethods.set_mat(Lp, rowIndex, columnIndex, value);
        }
    }
}
