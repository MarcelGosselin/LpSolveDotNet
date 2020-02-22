namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// User function that is called regularly while solving the model to verify if solving should abort.
    /// </summary>
    /// <param name="lp">Model being solved.</param>
    /// <returns>If <c>true</c> then lp_solve aborts the solver and returns with an appropriate code.</returns>
    public delegate bool AbortHandler(LpSolve lp);
}