namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// User function that specifies which B&amp;B branching to use given a column to branch on.
    /// </summary>
    /// <param name="lp">Model which called the selector while solving.</param>
    /// <param name="column">The column on which to branch.</param>
    /// <returns>
    /// The <see cref="BranchSelectorResult"/> branching 
    /// </returns>
    public delegate BranchSelectorResult BranchAndBoundBranchSelector(LpSolve lp, int column);
}