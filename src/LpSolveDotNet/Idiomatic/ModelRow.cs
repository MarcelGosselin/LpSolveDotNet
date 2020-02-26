using System;
using LpSolveDotNet.Idiomatic.Internal;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Helper class to handle model at row / variable level.
    /// </summary>
    public class ModelRow
        : ModelSubObjectBase
    {
        private readonly int _rowNumber;

        internal ModelRow(IntPtr lp, int rowNumber)
            : base(lp)
        {
            _rowNumber = rowNumber;
        }

        /// <summary>
        /// Represents the name of a row (constraint) in the model.
        /// </summary>
        /// <remarks>
        /// <para>Row names are optional.
        /// Default value is Rx with x being the row number.
        /// </para>
        /// <para>Row 0 is the objective function.</para>
        /// <para>
        /// The difference between this property and <see cref="ModelRows.GetNameFromOriginalIndex"/> is only visible when a presolve (<see cref="LpSolve.PreSolveLevels"/>) was done. 
        /// Presolve can result in deletion of rows in the model.
        /// Here, <c>model.Rows[rowIndex].Name</c>, rowIndex specifies the row number after presolve was done.
        /// In <see cref="ModelRows.GetNameFromOriginalIndex"/>, rowIndex specifies the row number before presolve was done, ie the original row number. 
        /// If presolve is not active then both methods are equal.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_row_name.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_row_name.htm">Full C API documentation (set).</seealso>
        public string Name
        {
            get => NativeMethods.get_row_name(Lp, _rowNumber);
            set => NativeMethods.set_row_name(Lp, _rowNumber, value)
                .HandleResultAndReturnVoid(ReturnValueHandler);
        }

        /// <summary>
        /// Specifies the type of constraint for that row (Equal, SmallerOrEqual, GreaterOrEqual).
        /// </summary>
        /// <remarks>
        /// <para>The default constraint type is <see cref="ConstraintOperator.LessOrEqual"/>.</para>
        /// <para>A free constraint (<see cref="ConstraintOperator.Free"/>) will act as if the constraint is not there.
        /// The lower bound is -Infinity and the upper bound is +Infinity.
        /// This can be used to temporary disable a constraint without having to delete it from the model.
        /// Note that the already set RHS and range on this constraint is overruled with Infinity by this.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_constr_type.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_constr_type.htm">Full C API documentation (set).</seealso>
        public ConstraintOperator ConstraintOperator
        {
            get => NativeMethods.get_constr_type(Lp, _rowNumber);
            set => NativeMethods.set_constr_type(Lp, _rowNumber, value)
                .HandleResultAndReturnVoid(ReturnValueHandler);
        }

        /// <summary>
        /// Specifies the value of the right-hand side (RHS) for the constraint's (in)equation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default value is <c>0</c>.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_rh.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh.htm">Full C API documentation (set).</seealso>
        public double RightHandSide
        {
            get => NativeMethods.get_rh(Lp, _rowNumber);
            set => NativeMethods.set_rh(Lp, _rowNumber, value)
                .HandleResultAndReturnVoid(ReturnValueHandler);
        }

        /// <summary>
        /// Specifies the range on the current constraint (row).
        /// </summary>
        /// <remarks>
        /// <para>Setting a range on a row is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a range doesn't increase the model size that means that the model stays smaller and will be solved faster.</para>
        /// <para>If the row has a less than constraint then the range means setting a minimum on the constraint that is equal to the RHS value minus the range.
        /// If the row has a greater than constraint then the range means setting a maximum on the constraint that is equal to the RHS value plus the range.</para>
        /// <para>Note that the range value is the difference value and not the absolute value.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_rh_range.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_rh_range.htm">Full C API documentation (set).</seealso>
        public double Range
        {
            get => NativeMethods.get_rh_range(Lp, _rowNumber);
            set => NativeMethods.set_rh_range(Lp, _rowNumber, value)
                .HandleResultAndReturnVoid(ReturnValueHandler);
        }

        /// <summary>
        /// Sets all values of the constraint at once.
        /// </summary>
        /// <param name="rowValues">
        /// When <paramref name="columnNumbers"/> is <c>null</c>, a 1-based array of length 1+<see cref="LpSolve.NumberOfColumns"/> with the values for every column.
        /// When <paramref name="columnNumbers"/> is not <c>null</c>, a 0-based array with only the *non-zero* values and for the column index given in <paramref name="columnNumbers"/>.
        /// </param>
        /// <param name="columnNumbers">Specifies the column numbers for the values in <paramref name="rowValues"/>.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that when <paramref name="columnNumbers"/> is <c>null</c>: element 0 of the array is ignored. Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>This method has the possibility to specify only the non-zero elements.
        /// In that case <paramref name="columnNumbers"/> specifies the row numbers 
        /// of the non-zero elements. Both <paramref name="rowValues"/> and <paramref name="columnNumbers"/> are then zero-based arrays. 
        /// This will speed up building the model considerably if there are a lot of zero values. In most cases the matrix is sparse and has many zero values.
        /// </para>
        /// <para>It is more performant to call this method than call <see cref="LpSolve.set_mat"/>.</para>
        /// <para>Note that unspecified values are set to zero.</para>
        /// <para>Note that these methods will perform much better when <see cref="LpSolve.EntryMode"/> is set to <c><see cref="EntryMode.Row"/></c> before adding constraints.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_row.htm">Full C API documentation.</seealso>
        public bool SetValues(double[] rowValues, int[] columnNumbers = null)
            => NativeMethods.set_rowex(Lp, _rowNumber, rowValues?.Length ?? 0, rowValues, columnNumbers)
                .HandleResultAndReturnIt(ReturnValueHandler);

        /// <summary>
        /// Gets all values from the model for this row.
        /// </summary>
        /// <param name="rowValues">Array in which the values are returned.
        /// If it is <c>null</c>, it will be allocated otherwise it must have a Length of at least 
        /// 1+<see cref="LpSolve.NumberOfColumns"/>.
        /// </param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="LpSolve.EntryMode"/>.</para>
        /// <para>Element 0 of the row array is not filled. Element 1 will contain the value for column 1, Element 2 the value for column 2, ...</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_row.htm">Full C API documentation.</seealso>
        public bool GetValues(ref double[] rowValues)
        {
            if (rowValues == null)
            {
                rowValues = new double[1 + NativeMethods.get_Ncolumns(Lp)];
            }
            return NativeMethods.get_row(Lp, _rowNumber, rowValues)
                .HandleResultAndReturnIt(ReturnValueHandler);
        }

        /// <summary>
        /// Gets all non-zero values from the model for this row.
        /// </summary>
        /// <param name="rowValues">Array in which the values are returned.
        /// If it is <c>null</c>, it will be allocated otherwise it must have a Length of at least 
        /// 1+<see cref="LpSolve.NumberOfColumns"/>.</param>
        /// <param name="columnNumbers">Array in which the row numbers are returned.
        /// If it is <c>null</c>, it will be allocated otherwise it must have a Length of at least 
        /// 1+<see cref="LpSolve.NumberOfColumns"/>.</param>
        /// <returns>The number of non-zero elements returned in <paramref name="rowValues"/> and <paramref name="columnNumbers"/>.
        /// If an error occurs, then -1 is returned.
        /// </returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="LpSolve.EntryMode"/>.</para>
        /// <para>Returned values in <paramref name="rowValues"/> and <paramref name="columnNumbers"/> start from element 0.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_row.htm">Full C API documentation.</seealso>
        public int GetNonZeroValues(ref double[] rowValues, ref int[] columnNumbers)
        {
            if (rowValues == null)
            {
                rowValues = new double[1 + NativeMethods.get_Ncolumns(Lp)];
            }
            if (columnNumbers == null)
            {
                columnNumbers = new int[1 + NativeMethods.get_Ncolumns(Lp)];
            }
            return NativeMethods.get_rowex(Lp, _rowNumber, rowValues, columnNumbers);
        }
    }

}
