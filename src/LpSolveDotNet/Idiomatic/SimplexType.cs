namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Defines the desired combination of primal and dual simplex algorithms.
    /// </summary>
    public enum SimplexType
    {
        /// <summary>Phase1 Primal, Phase2 Primal (SIMPLEX_PRIMAL_PRIMAL = 5 in C code)</summary>
        PrimalPrimal = 5,
        /// <summary>Phase1 Dual, Phase2 Primal (SIMPLEX_DUAL_PRIMAL = 6 in C code)</summary>
        DualPrimal = 6,
        /// <summary>Phase1 Primal, Phase2 Dual (SIMPLEX_PRIMAL_DUAL = 9 in C code)</summary>
        PrimalDual = 9,
        /// <summary>Phase1 Dual, Phase2 Dual (SIMPLEX_DUAL_DUAL = 10 in C code)</summary>
        DualDual = 10,
    }
}