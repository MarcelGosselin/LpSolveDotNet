namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Define a heuristic "crash procedure" to execute before the first simplex iteration
    /// to quickly choose a basis matrix that has fewer artificial variables.
    /// </summary>
    public enum BasisCrashMode
    {
        /// <summary>No basis crash (CRASH_NONE = 0 in C code)</summary>
        None = 0,
        /// <summary>Most feasible basis (CRASH_MOSTFEASIBLE = 2 in C code)</summary>
        MostFeasible = 2,
        /// <summary>Construct a basis that is in some measure the "least degenerate" (CRASH_LEASTDEGENERATE = 3 in C code)</summary>
        LeastDegenerate = 3,
    }
}