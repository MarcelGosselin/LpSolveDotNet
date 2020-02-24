namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Branch-and-bound rule.
    /// </summary>
    public enum BranchAndBoundRule
    {
        /// <summary>Select lowest indexed non-integer column (NODE_FIRSTSELECT = 0 in C code).</summary>
        FirstSelect = 0,
        /// <summary>Selection based on distance from the current bounds (NODE_GAPSELECT = 1 in C code).</summary>
        GapSelect = 1,
        /// <summary>Selection based on the largest current bound (NODE_RANGESELECT = 2 in C code).</summary>
        RangeSelect = 2,
        /// <summary>Selection based on largest fractional value (NODE_FRACTIONSELECT = 3 in C code).</summary>
        FractionSelect = 3,
        /// <summary>Simple, unweighted pseudo-cost of a variable (NODE_PSEUDOCOSTSELECT = 4 in C code).</summary>
        PseudoCostSelect = 4,
        /// <summary>This is an extended pseudo-costing strategy based on minimizing the number of integer infeasibilities (NODE_PSEUDONONINTSELECT = 5 in C code).</summary>
        PseudoNonIntegerSelect = 5,
        /// <summary>
        /// This is an extended pseudo-costing strategy based on maximizing the normal pseudo-cost divided by the number of infeasibilities.
        /// Effectively, it is similar to (the reciprocal of) a cost/benefit ratio.
        /// (NODE_PSEUDORATIOSELECT = 6 in C code)</summary>
        PseudoRatioSelect = 6,
        /// <summary> (NODE_USERSELECT = 7 in C code)</summary>
        UserSelect = 7,
    }
}