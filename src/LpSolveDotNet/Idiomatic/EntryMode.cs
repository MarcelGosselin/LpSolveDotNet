namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Specifies which entry methods perform best on the <see cref="LpSolve"/> model.
    /// </summary>
    public enum EntryMode
    {
        /// <summary>
        /// The methods <see cref="ModelColumns.Add"/> perform best.
        /// </summary>
        Column = 0,

        /// <summary>
        /// The methods <see cref="ModelRows.Add"/> performs best.
        /// </summary>
        Row = 1,
    }
}
