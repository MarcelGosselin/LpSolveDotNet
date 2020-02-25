namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Specifies which entry methods perform best on the <see cref="LpSolve"/> model.
    /// </summary>
    public enum EntryMode
    {
        /// <summary>
        /// The methods <see cref="LpSolve.add_column"/>, <see cref="LpSolve.add_columnex"/> perform best.
        /// </summary>
        Column = 0,

        /// <summary>
        /// The methods <see cref="LpSolve.add_constraint(double[], ConstraintOperator, double)"/>, <see cref="LpSolve.add_constraintex(int, double[], int[], ConstraintOperator, double)"/> perform best.
        /// </summary>
        Row = 1,
    }
}
