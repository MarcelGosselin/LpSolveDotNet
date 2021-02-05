using System;
using LpSolveDotNet.Idiomatic.Internal;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Helper class to handle model at row / variable level.
    /// </summary>
    public class ModelRows
        : ModelSubObjectBase
    {
        internal ModelRows(IntPtr lp)
            : base(lp)
        {
        }

        /// <summary>
        /// Returns the <see cref="ModelRow"/> at the position <paramref name="rowIndex"/>.
        /// </summary>
        /// <param name="rowIndex">The 1-based index of the row to return.</param>
        /// <returns>The <see cref="ModelRow"/> at the position <paramref name="rowIndex"/>.</returns>
        /// <remarks>
        /// <para>
        ///   Note that the number of rows can change when a presolve is done.
        ///   If you have the index before presolve/solve, Use <see cref="FromOriginalIndex"/> instead
        ///   to get your <see cref="ModelRow"/>.
        /// </para>
        /// </remarks>
        public ModelRow this[int rowIndex] => new ModelRow(Lp, rowIndex);

        /// <summary>
        /// Returns the <see cref="ModelRow"/> at the position <paramref name="rowIndex"/>
        /// as it was before presolve.
        /// </summary>
        /// <param name="rowIndex">The 1-based index of the row to return.</param>
        /// <returns>The <see cref="ModelRow"/> at the position <paramref name="rowIndex"/>.</returns>
        /// <remarks>
        /// <para>
        ///   Note that the number of rows can change when a presolve is done.
        ///   <paramref name="rowIndex"/> represents the position of the constraint in the lp model.
        ///   If you have the index before presolve/solve, Use <see cref="FromOriginalIndex"/> instead
        ///   to get your <see cref="ModelRow"/>.
        /// </para>
        /// </remarks>
        public ModelRow FromOriginalIndex(int rowIndex)
        {
            rowIndex = NativeMethods.get_lp_index(Lp, NativeMethods.get_Norig_rows(Lp) + rowIndex);
            if (rowIndex == 0)
            {
                return null;
            }
            return new ModelRow(Lp, rowIndex);
        }

        /// <summary>
        /// Returns the name of the row at the position <paramref name="rowIndex"/>
        /// as it was before presolve.
        /// </summary>
        /// <param name="rowIndex">The 1-based index of the row to return.</param>
        /// <returns>The name of the row at the original position <paramref name="rowIndex"/>.</returns>
        /// <remarks>
        /// <para>Row names are optional.
        /// Default value is Rx with x being the row number.
        /// </para>
        /// <para>Row 0 is the objective function.</para>
        /// <para>
        /// The difference between this method and <c>model.Rows[rowIndex].Name</c> is only visible when a presolve (<see cref="LpSolve.PreSolveLevels"/>) was done. 
        /// Presolve can result in deletion of rows in the model.
        /// In <c>model.Rows[rowIndex].Name</c>, rowIndex specifies the row number after presolve was done.
        /// In <see cref="ModelRows.GetNameFromOriginalIndex"/>, rowIndex specifies the row number before presolve was done, ie the original row number. 
        /// If presolve is not active then both methods are equal.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_origrow_name.htm">Full C API documentation.</seealso>
        public string GetNameFromOriginalIndex(int rowIndex)
             => NativeMethods.get_origrow_name(Lp, rowIndex);

        /// <summary>
        /// Gets the index in the LP model for the row with the given name.
        /// </summary>
        /// <param name="name">The name of the row for which the index (row number) must be retrieved.</param>
        /// <returns>Returns the index (row number) of the given row name.
        /// A return value of -1 indicates that the name does not exist.
        /// Note that the index is the original index number.
        /// So if presolve is active, it has no effect.
        /// It is the original row number that is returned.</returns>
        /// <remarks>
        /// Note that this index starts from 1.
        /// Some API methods expect zero-based indexes and thus this value must then be corrected with -1.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_nameindex.htm">Full C API documentation.</seealso>
        public int GetOriginalIndexFromName(string name)
        {
            return NativeMethods.get_nameindex(Lp, name, true);
        }

        /// <summary>
        /// Adds a constraint at the end of the model and sets all values of the row at once.
        /// </summary>
        /// <param name="rowValues">
        /// When <paramref name="columnNumbers"/> is <c>null</c>, an array of length 1+<see cref="LpSolve.NumberOfColumns"/> with all the values for the row (starting at index 1).
        /// When <paramref name="columnNumbers"/> is not <c>null</c>, same thing but with a smaller array containing only the *non-zero* values (starting index at 0).
        /// </param>
        /// <param name="columnNumbers">Zero-based array specifying the column numbers for the values in <paramref name="rowValues"/>.</param>
        /// <param name="constraintType">The type of the constraint.</param>
        /// <param name="rhs">The value of the right-hand side (RHS) for the constraint (in)equation</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that when <paramref name="columnNumbers"/> is <c>null</c>, element 0 of the array is ignored.
        /// Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>This method has the possibility to specify only the non-zero elements.
        /// In that case <paramref name="columnNumbers"/> specifies the column numbers 
        /// of the non-zero elements. Both <paramref name="rowValues"/> and <paramref name="columnNumbers"/> are then zero-based arrays. 
        /// This will speed up building the model considerably if there are a lot of zero values.
        /// In most cases the matrix is sparse and has many zero values.
        /// </para>
        /// <para>Note that it is advised to set the objective function (via <see cref="ModelObjectiveFunction.SetValues"/>, <see cref="ModelObjectiveFunction.SetValue"/>)
        /// before adding rows. This especially for larger models. This will be much more performant than adding the objective function afterwards.</para>
        /// <para>Note that these methods will perform much better when <see cref="LpSolve.EntryMode"/> is set to <c><see cref="EntryMode.Row"/></c> before adding constraints.</para>
        /// <para>Note that if you have to add many rows, performance can be improved by a call to <see cref="LpSolve.ResizeMatrix"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_constraint.htm">Full C API documentation.</seealso>
        public bool Add(double[] rowValues, ConstraintOperator constraintType, double rhs, int[] columnNumbers = null)
            => NativeMethods.add_constraintex(Lp, rowValues?.Length ?? 0, rowValues, columnNumbers, constraintType, rhs)
                .HandleResultAndReturnIt(ReturnValueHandler);
        // TODO: would this be better as returning null or the row object?

        /// <summary>
        /// Removes a constraint (row) from the model.
        /// </summary>
        /// <param name="row">The row to delete. Must be between <c>1</c> and the number of rows in the model.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise. An error occurs if <paramref name="row"/> 
        /// is not between <c>1</c> and the number of rows in the model</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="LpSolve.EntryMode"/>.</para>
        /// <para>The row is effectively deleted from the model, so all rows after this row shift one up.</para>
        /// <para>Note that row 0 (the objective function) cannot be deleted. There is always an objective function.</para>
        /// <para>Note that you can also delete multiple constraints by a call to <see cref="LpSolve.ResizeMatrix"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/del_constraint.htm">Full C API documentation.</seealso>
        public bool Delete(int row)
            => NativeMethods.del_constraint(Lp, row)
                .HandleResultAndReturnIt(ReturnValueHandler);
        // TODO: would this be better as Rows[i].Delete(); ???
        // TODO: would this be better as throwing ArgOORange or IndexOOR 


        /// <summary>
        /// Gets/sets if constraint names are used.
        /// </summary>
        /// <returns>A boolean value indicating if constraint names are used.</returns>
        /// <remarks>
        /// <para>When a model is read from file or created via the API, constraints can be named.
        /// These names are used to report information or to save the model in a given format.
        /// However, sometimes it is required to ignore these names and to use the internal names of lp_solve.
        /// This is for example the case when the names do not comply to the syntax rules of the format
        /// that will be used to write the model to.</para>
        /// <para>Names are used by default.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_use_names.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_use_names.htm">Full C API documentation.</seealso>
        public bool UseNames
        {
            get => NativeMethods.is_use_names(Lp, true);
            set => NativeMethods.set_use_names(Lp, true, value);
        }
    }
}
