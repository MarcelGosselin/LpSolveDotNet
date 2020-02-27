using System;
using LpSolveDotNet.Idiomatic.Internal;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Helper class to handle model at column / variable level.
    /// </summary>
    public class ModelColumns
        : ModelSubObjectBase
    {
        internal ModelColumns(IntPtr lp)
            : base(lp)
        {
        }

        /// <summary>
        /// Returns the <see cref="ModelColumn"/> at the position <paramref name="columnIndex"/>.
        /// </summary>
        /// <param name="columnIndex">The 1-based index of the column to return.</param>
        /// <returns>The <see cref="ModelColumn"/> at the position <paramref name="columnIndex"/>.</returns>
        /// <remarks>
        /// <para>
        ///   Note that the number of columns can change when a presolve is done or 
        ///   when negative variables are split in a positive and a negative part.
        ///   <paramref name="columnIndex"/> represents the position of the variable in the lp model.
        ///   If you have the index before presolve/solve, Use <see cref="FromOriginalIndex"/> instead
        ///   to get your <see cref="ModelColumn"/>.
        /// </para>
        /// </remarks>
        public ModelColumn this[int columnIndex] => new ModelColumn(Lp, columnIndex);

        /// <summary>
        /// Returns the <see cref="ModelColumn"/> at the position <paramref name="columnIndex"/>
        /// as it was before presolve.
        /// </summary>
        /// <param name="columnIndex">The 1-based index of the column to return.</param>
        /// <returns>The <see cref="ModelColumn"/> at the position <paramref name="columnIndex"/>.</returns>
        /// <remarks>
        /// <para>
        ///   Note that the number of columns can change when a presolve is done or 
        ///   when negative variables are split in a positive and a negative part.
        ///   <paramref name="columnIndex"/> represents the position of the variable in the lp model.
        ///   If you have the index before presolve/solve, Use <see cref="FromOriginalIndex"/> instead
        ///   to get your <see cref="ModelColumn"/>.
        /// </para>
        /// </remarks>
        public ModelColumn FromOriginalIndex(int columnIndex)
        {
            columnIndex = NativeMethods.get_lp_index(Lp, NativeMethods.get_Norig_rows(Lp) + columnIndex);
            if (columnIndex == 0)
            {
                return null;
            }
            return new ModelColumn(Lp, columnIndex);
        }

        /// <summary>
        /// Returns the name of the column at the position <paramref name="columnIndex"/>
        /// as it was before presolve.
        /// </summary>
        /// <param name="columnIndex">The 1-based index of the column to return.</param>
        /// <returns>The name of the column at the original position <paramref name="columnIndex"/>.</returns>
        /// <remarks>
        /// <para>Column names are optional.
        /// Default value is Cx with x being the column number.
        /// </para>
        /// <para>
        /// The difference between this method and <c>model.Columns[columnIndex].Name</c> is only visible when a presolve (<see cref="LpSolve.PreSolveLevels"/>) was done. 
        /// Presolve can result in deletion of columns in the model.
        /// In <c>model.Columns[columnIndex].Name</c>, columnIndex specifies the column number after presolve was done.
        /// In <see cref="ModelColumns.GetNameFromOriginalIndex"/>, columnIndex specifies the column number before presolve was done, ie the original column number. 
        /// If presolve is not active then both methods are equal.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_origcol_name.htm">Full C API documentation.</seealso>
        public string GetNameFromOriginalIndex(int columnIndex)
             => NativeMethods.get_origcol_name(Lp, columnIndex);

        /// <summary>
        /// Gets the index in the LP model for the column with the given name.
        /// </summary>
        /// <param name="name">The name of the column for which the index (column number) must be retrieved.</param>
        /// <returns>Returns the index (column number) of the given column name.
        /// A return value of -1 indicates that the name does not exist.
        /// Note that the index is the original index number.
        /// So if presolve is active, it has no effect.
        /// It is the original column number that is returned.</returns>
        /// <remarks>
        /// Note that this index starts from 1.
        /// Some API methods expect zero-based indexes and thus this value must then be corrected with -1.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_nameindex.htm">Full C API documentation.</seealso>
        public int GetOriginalIndexFromName(string name)
        {
            return NativeMethods.get_nameindex(Lp, name, false);
        }

        /// <summary>
        /// Adds a column at the end of the model and sets all values of the column at once.
        /// </summary>
        /// <param name="columnValues">
        /// When <paramref name="rowNumbers"/> is <c>null</c>, an array of length 1+<see cref="LpSolve.NumberOfRows"/> with the values for the column for the objective function (index 0) and all the constraints (index 1..<see cref="LpSolve.NumberOfRows"/>).
        /// When <paramref name="rowNumbers"/> is not <c>null</c>, same thing but with a smaller array containing only the *non-zero* values and row index given in <paramref name="rowNumbers"/>.
        /// </param>
        /// <param name="rowNumbers">Specifies the row numbers for the values in <paramref name="columnValues"/>.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that when <paramref name="rowNumbers"/> is <c>null</c>, element 0 of the array is the value of the objective function for that column.
        /// Column 1 is element 1, column 2 is element 2, ...</para>
        /// <para>This method has the possibility to specify only the non-zero elements.
        /// In that case <paramref name="rowNumbers"/> specifies the row numbers 
        /// of the non-zero elements. Both <paramref name="columnValues"/> and <paramref name="rowNumbers"/> are then zero-based arrays. 
        /// This will speed up building the model considerably if there are a lot of zero values.
        /// In most cases the matrix is sparse and has many zero values.
        /// </para>
        /// <para><paramref name="columnValues"/> and <paramref name="rowNumbers"/> can both be <c>null</c>. In that case an empty column is added.</para>
        /// <para>Note that if you have to add many columns, performance can be improved by a call to <see cref="LpSolve.ResizeMatrix"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/add_column.htm">Full C API documentation.</seealso>
        public bool Add(double[] columnValues, int[] rowNumbers = null)
            => NativeMethods.add_columnex(Lp, columnValues?.Length ?? 0, columnValues, rowNumbers)
                .HandleResultAndReturnIt(ReturnValueHandler);
        // TODO: see TODO in ModelRows

        /// <summary>
        /// Deletes a column from the model.
        /// </summary>
        /// <param name="column">The column to delete. Must be between <c>1</c> and the number of columns in the model.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise. An error occurs if <paramref name="column"/> 
        /// is not between <c>1</c> and the number of columns in the model</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="LpSolve.EntryMode"/>.</para>
        /// <para>The column is effectively deleted from the model, so all columns after this column shift one left.</para>
        /// <para>Note that column 0 (the right hand side (RHS)) cannot be deleted. There is always a RHS.</para>
        /// <para>Note that you can also delete multiple columns by a call to <see cref="LpSolve.ResizeMatrix"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/del_column.htm">Full C API documentation.</seealso>
        public bool Delete(int column)
            => NativeMethods.del_column(Lp, column)
                .HandleResultAndReturnIt(ReturnValueHandler);
        // TODO: see TODO in ModelRows

    }
}
