namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Pivot Rule
    /// </summary>
    public enum PivotRule
    {
        /// <summary>Select first (PRICER_FIRSTINDEX = 0 in C code)</summary>
        FirstIndex = 0,
        /// <summary>Select according to Dantzig (PRICER_DANTZIG = 1 in C code)</summary>
        Dantzig = 1,
        /// <summary>Devex pricing from Paula Harris (PRICER_DEVEX = 2 in C code)</summary>
        Devex = 2,
        /// <summary>Steepest Edge (PRICER_STEEPESTEDGE = 3 in C code)</summary>
        SteepestEdge = 3,
    }
}