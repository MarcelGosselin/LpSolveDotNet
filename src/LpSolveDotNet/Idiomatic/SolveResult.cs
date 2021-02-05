namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Defines the possible return values of method <see cref="LpSolve.Solve"/>.
    /// </summary>
    public enum SolveResult
    {
        /// <summary>Undefined internal error (UNKNOWNERROR = -5 in C code).</summary>
        UnknownError = -5,

        /// <summary>Invalid input data provided (DATAIGNORED = -4 in C code).</summary>
        DataIgnored = -4,

        /// <summary>No basis factorization package (NOBFP = -3 in C code).</summary>
        NoBFP = -3,

        /// <summary>Out of memory (NOMEMORY = -2 in C code).</summary>
        NoMemory = -2,

        /// <summary>Solver has not run, usually because of an empty model. (NOTRUN = -1 in C code).</summary>
        NotRun = -1,

        /// <summary>An optimal solution was obtained (OPTIMAL = 0 in C code).</summary>
        Optimal = 0,

        /// <summary>
        /// The model is sub-optimal. Only happens if there are integer variables and there is already an integer solution found. The solution is not guaranteed the most optimal one.
        /// <list>
        /// <item><description>A timeout occured (set via set_timeout or with the -timeout option in lp_solve)</description></item>
        /// <item><description><see cref="LpSolve.set_break_at_first"/> was called so that the first found integer solution is found (-f option in lp_solve)</description></item>
        /// <item><description><see cref="LpSolve.set_break_at_value"/> was called so that when integer solution is found that is better than the specified value that it stops (-o option in lp_solve)</description></item>
        /// <item><description><see cref="ModelTolerance.RelativeMipGap"/> or <see cref="ModelTolerance.AbsoluteMipGap"/> was called (-g/-ga/-gr options in lp_solve) to specify a MIP gap</description></item>
        /// <item><description>An abort callback is installed (<see cref="LpSolve.PutLogHandler"/>) and this callback returned <c>true</c></description></item>
        /// <item><description>At some point not enough memory could not be allocated</description></item>
        /// </list>
        /// (SUBOPTIMAL = 1 in C code).
        /// </summary>
        SubOptimal = 1,

        /// <summary>The model is infeasible (INFEASIBLE = 2 in C code).</summary>
        Infeasible = 2,

        /// <summary>The model is unbounded (UNBOUNDED = 3 in C code).</summary>
        Unbounded = 3,

        /// <summary>The model is degenerative (DEGENERATE = 4 in C code).</summary>
        Degenerate = 4,

        /// <summary>Numerical failure encountered (NUMFAILURE = 5 in C code).</summary>
        NumericalFailure = 5,

        /// <summary>
        /// The <see cref="LpSolve.PutAbortHandler">abort callback</see> returned <c>true</c>.
        /// (USERABORT = 6 in C code)
        /// </summary>
        UserAborted = 6,

        /// <summary>A timeout occurred. A timeout was set via <see cref="LpSolve.set_timeout"/> (TIMEOUT = 7 in C code).</summary>
        TimedOut = 7,

        /// <summary>
        /// The <see cref="LpSolve.Solve"/> operation is currently running on model.
        /// (RUNNING = 8 in C code).
        /// </summary>
        Running = 8,

        /// <summary>
        /// The model could be solved by presolve. This can only happen if presolve is active via <see cref="LpSolve.PreSolveLevels"/>
        /// (PRESOLVED = 9 in C code).
        /// </summary>
        PreSolved = 9,

        // defined as internal in lp_lib.h as of 5.5.2.5
        //PROCFAIL = 10,
        //PROCBREAK = 11,
        //FEASFOUND = 12,
        //NOFEASFOUND = 13,
        //FATHOMED = 14,
        //SWITCH_TO_PRIMAL = 20,
        //SWITCH_TO_DUAL   = 21,
        //SINGULAR_BASIS   = 22,
        //LOSTFEAS         = 23,
        //MATRIXERROR      = 24,

        /// <summary>Accuracy error encountered (ACCURACYERROR = 25 in C code).</summary>
        AccuracyError = 25,
    }
}