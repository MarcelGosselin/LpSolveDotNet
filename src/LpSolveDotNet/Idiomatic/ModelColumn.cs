using System;
using LpSolveDotNet.Idiomatic.Internal;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Helper class to handle model at column / variable level.
    /// </summary>
    public class ModelColumn
        : ModelSubObjectBase
    {
        private readonly int _columnNumber;

        internal ModelColumn(IntPtr lp, int columnNumber)
            : base(lp)
        {
            _columnNumber = columnNumber;
        }

        /// <summary>
        /// Represents the name of a column in the model.
        /// </summary>
        /// <remarks>
        /// <para>Column names are optional.
        /// Default value is Cx with x being the column number.
        /// </para>
        /// <para>
        /// The difference between this property and <see cref="ModelColumns.GetNameFromOriginalIndex"/> is only visible when a presolve (<see cref="LpSolve.PreSolveLevels"/>) was done. 
        /// Presolve can result in deletion of columns in the model.
        /// Here, <c>model.Columns[columnIndex].Name</c>, columnIndex specifies the column number after presolve was done.
        /// In <see cref="ModelColumns.GetNameFromOriginalIndex"/>, columnIndex specifies the column number before presolve was done, ie the original column number. 
        /// If presolve is not active then both methods are equal.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_col_name.htm">Full C API documentation.</seealso>
        public string Name
        {
            get => NativeMethods.get_col_name(Lp, _columnNumber);
            set => NativeMethods.set_col_name(Lp, _columnNumber, value)
                .HandleResultAndReturnVoid(ReturnValueHandler);
        }

        /// <summary>
        /// Specifies whether the variable is negative or not.
        /// </summary>
        /// <remarks>
        /// Negative means a lower and upper bound that are both negative.
        /// By default a variable is not negative because it has a lower bound of 0 (and an upper bound of +infinity).
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_negative.htm">Full C API documentation.</seealso>
        public bool IsNegative
        {
            get => NativeMethods.is_negative(Lp, _columnNumber);
        }

        /// <summary>
        /// Specifies whether the variable is of type Integer or floating point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default a variable is not integer.
        /// </para>
        /// <para>
        /// From the moment there is at least one integer variable in the model,
        /// the Branch and Bound algorithm is used to make these variables integer.
        /// Note that solving times can be considerably larger when there are integer variables.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/integer.htm">integer variables</see> for a description about integer variables.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_int.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_int.htm">Full C API documentation (set).</seealso>
        public bool IsInteger
        {
            get => NativeMethods.is_int(Lp, _columnNumber);
            set => NativeMethods.set_int(Lp, _columnNumber, value)
                .HandleResultAndReturnVoid(ReturnValueHandler);
        }

        /// <summary>
        /// Specifies whether the variable is of type Binary or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default a variable is not binary.
        /// </para>
        /// <para>
        /// Note that when setting this property to <c>false</c>, you not only remove make the variable non-binary, you also make it floating point.
        /// </para>
        /// <para>
        /// A binary variable is an integer variable with lower bound 0 and upper bound 1.
        /// From the moment there is at least one integer variable in the model,
        /// the Branch and Bound algorithm is used to make these variables integer.
        /// Note that solving times can be considerably larger when there are integer variables.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/integer.htm">integer variables</see> for a description about integer variables.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_binary.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_binary.htm">Full C API documentation (set).</seealso>
        public bool IsBinary
        {
            get => NativeMethods.is_binary(Lp, _columnNumber);
            set => NativeMethods.set_binary(Lp, _columnNumber, value)
                .HandleResultAndReturnVoid(ReturnValueHandler);
        }

        /// <summary>
        /// Specifies whether the variable is of type semi-continuous or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default a variable is not semi-continuous.
        /// </para>
        /// <para>
        /// Note that a semi-continuous variable must also have a lower bound to have effect.
        /// This because the default lower bound on variables is zero, also when defined as semi-continuous, and without
        /// a lower bound it has no point to define a variable as such.
        /// The lower bound may be set before or after setting the semi-continuous status.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/semi-cont.htm">semi-continuous variables</see> for a description about semi-continuous variables.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_semicont.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_semicont.htm">Full C API documentation (set).</seealso>
        public bool IsSemiContinuous
        {
            get => NativeMethods.is_semicont(Lp, _columnNumber);
            set => NativeMethods.set_semicont(Lp, _columnNumber, value)
                .HandleResultAndReturnVoid(ReturnValueHandler);
        }

        /// <summary>
        /// Specifies whether the variable is free or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default a variable is not free because it has a lower bound of 0 (and an upper bound of +infinity).
        /// </para>
        /// <para>
        /// Free means a lower bound of -infinity and an upper bound of +infinity.
        /// See <see href="http://lpsolve.sourceforge.net/5.5/free.htm">free variables</see> for a description about free variables.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_unbounded.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_unbounded.htm">Full C API documentation (set).</seealso>
        public bool IsUnbounded
        {
            get => NativeMethods.is_unbounded(Lp, _columnNumber);
            set => NativeMethods.set_unbounded(Lp, _columnNumber)
                .HandleResultAndReturnVoid(ReturnValueHandler);
        }

        /// <summary>
        /// Specifies the lower bound of a variable.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// </para>
        /// <para>
        /// The default lower bound of a variable is <c>0</c>.
        /// So variables will never take negative values if no negative lower bound is set.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_lowbo.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_lowbo.htm">Full C API documentation (set).</seealso>
        public double LowerBound
        {
            get => NativeMethods.get_lowbo(Lp, _columnNumber);
            set => NativeMethods.set_lowbo(Lp, _columnNumber, value)
                .HandleResultAndReturnVoid(ReturnValueHandler);
        }

        /// <summary>
        /// Specifies the upper bound of a variable.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// </para>
        /// <para>
        /// The default upper bound of a variable is the value of <see cref="LpSolve.InfiniteValue"/>.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_upbo.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_upbo.htm">Full C API documentation (set).</seealso>
        public double UpperBound
        {
            get => NativeMethods.get_upbo(Lp, _columnNumber);
            set => NativeMethods.set_upbo(Lp, _columnNumber, value)
                .HandleResultAndReturnVoid(ReturnValueHandler);
        }


        /// <summary>
        /// Sets the lower and upper bound of a variable.
        /// </summary>
        /// <param name="lower">The lower bound on the variable.</param>
        /// <param name="upper">The upper bound on the variable.</param>
        /// <returns><c>true</c> if variable is operation was successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>
        /// Setting a bound on a variable is the way to go instead of adding an extra constraint (row) to the model.
        /// Setting a bound doesn't increase the model size that means that the model stays smaller and will be solved faster.
        /// </para>
        /// <para>
        /// Note that the default lower bound of each variable is 0.
        /// So variables will never take negative values if no negative lower bound is set.
        /// </para>
        /// <para>
        /// The default upper bound of a variable is the value of <see cref="LpSolve.InfiniteValue"/>.
        /// </para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_bounds.htm">Full C API documentation.</seealso>
        public bool SetBounds(double lower, double upper)
            => NativeMethods.set_bounds(Lp, _columnNumber, lower, upper)
                .HandleResultAndReturnIt(ReturnValueHandler);

        /// <summary>
        /// Sets all values of the column at once.
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
        /// This will speed up building the model considerably if there are a lot of zero values. In most cases the matrix is sparse and has many zero values.
        /// </para>
        /// <para>It is more performant to call this method than call cell-by-bell the <see cref="LpSolve.Cells"/> property.</para>
        /// <para>Note that unspecified values are set to zero.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_column.htm">Full C API documentation.</seealso>
        public bool SetValues(double[] columnValues, int[] rowNumbers = null)
            => NativeMethods.set_columnex(Lp, _columnNumber, columnValues?.Length ?? 0, columnValues, rowNumbers)
                .HandleResultAndReturnIt(ReturnValueHandler);

        /// <summary>
        /// Gets all values from the model for this column.
        /// </summary>
        /// <param name="columnValues">Array in which the values are returned.
        /// If it is <c>null</c>, it will be allocated otherwise it must have a Length of at least 
        /// 1+<see cref="LpSolve.NumberOfRows"/>.
        /// </param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="LpSolve.EntryMode"/>.</para>
        /// <para>Note that element 0 of the array is row 0 (objective function). element 1 is row 1, ...</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_column.htm">Full C API documentation.</seealso>
        public bool GetValues(ref double[] columnValues)
        {
            if (columnValues == null)
            {
                columnValues = new double[1 + NativeMethods.get_Nrows(Lp)];
            }
            return NativeMethods.get_column(Lp, _columnNumber, columnValues)
                .HandleResultAndReturnIt(ReturnValueHandler);
        }

        /// <summary>
        /// Gets all non-zero values from the model for this column.
        /// </summary>
        /// <param name="columnValues">Array in which the values are returned.
        /// If it is <c>null</c>, it will be allocated otherwise it must have a Length of at least 
        /// 1+<see cref="LpSolve.NumberOfRows"/>.</param>
        /// <param name="rowNumbers">Array in which the row numbers are returned.
        /// If it is <c>null</c>, it will be allocated otherwise it must have a Length of at least 
        /// 1+<see cref="LpSolve.NumberOfRows"/>.</param>
        /// <returns>The number of non-zero elements returned in <paramref name="columnValues"/> and <paramref name="rowNumbers"/>.</returns>
        /// <remarks>
        /// <para>Note that row entry mode must be off, else this method fails. <see cref="LpSolve.EntryMode"/>.</para>
        /// <para>Returned values in <paramref name="columnValues"/> and <paramref name="rowNumbers"/> start from element 0.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_column.htm">Full C API documentation.</seealso>
        public int GetNonZeroValues(ref double[] columnValues, ref int[] rowNumbers)
        {
            if (columnValues == null)
            {
                columnValues = new double[1 + NativeMethods.get_Nrows(Lp)];
            }
            if (rowNumbers == null)
            {
                rowNumbers = new int[1 + NativeMethods.get_Nrows(Lp)];
            }
            return NativeMethods.get_columnex(Lp, _columnNumber, columnValues, rowNumbers);
        }

        /// <summary>
        /// Returns if the variable is a SOS (Special Ordered Set) or not.
        /// </summary>
        /// <remarks>
        /// <para>The property returns if a variable is a SOS variable or not.
        /// By default a variable is not a SOS. A variable becomes a SOS variable via <see cref="LpSolve.add_SOS"/>.</para>
        /// <para>See <see href="http://lpsolve.sourceforge.net/5.5/SOS.htm">Special Ordered Sets</see> for a description about SOS variables.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_SOS_var.htm">Full C API documentation.</seealso>
        public bool IsSOS
            => NativeMethods.is_SOS_var(Lp, _columnNumber);

        /// <summary>
        /// The priority the variable has in the branch-and-bound algorithm.
        /// </summary>
        /// <remarks>
        /// The proeprty returns the priority the variable has in the branch-and-bound algorithm.
        /// This priority is determined by the weights set by <see cref="LpSolve.SetBranchAndBoundVariableWeights"/>.
        /// The default priorities are the column positions of the variables in the model.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_var_priority.htm">Full C API documentation.</seealso>
        public int BranchAndBoundPriority
            => NativeMethods.get_var_priority(Lp, _columnNumber);

        /// <summary>
        /// Specifies which branch to take first in branch-and-bound algorithm.
        /// </summary>
        /// <remarks>
        /// This property defines which branch to take first in branch-and-bound algorithm.
        /// This can influence solving times considerably.
        /// Depending on the model one rule can be best and for another model another rule.
        /// When no value was set explicitly, or when the value set is <see cref="BranchMode.Default" />,
        /// the value is that of <see cref="LpSolve.FirstBranch"/>.
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_var_branch.htm">Full C API documentation.</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_var_branch.htm">Full C API documentation.</seealso>
        public BranchMode BranchAndBoundMode
        {
            get => NativeMethods.get_var_branch(Lp, _columnNumber);
            set => NativeMethods.set_var_branch(Lp, _columnNumber, value);
        }
    }
}
