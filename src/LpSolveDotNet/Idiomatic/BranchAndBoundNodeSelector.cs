namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// User function that specifies which non-integer variable to select next to make integer in the B&amp;B solve
    /// </summary>
    /// <param name="lp">Model which called the selector while solving.</param>
    /// <returns>
    /// <para>When it returns a positive number, it is the node (column number) to make integer.</para>
    /// <para>When it returns <c>0</c> then it indicates that all variables are integers.</para>
    /// <para>When a negative value is returned, lp_solve will determine the next variable to make integer as if the routine is not set.</para>
    /// </returns>
    public delegate int BranchAndBoundNodeSelector(LpSolve lp);
}