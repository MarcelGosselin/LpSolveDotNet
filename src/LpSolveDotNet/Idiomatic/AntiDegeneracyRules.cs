using System;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Strategy codes to avoid or recover from degenerate pivots,
    /// infeasibility or numeric errors via randomized bound relaxation
    /// </summary>
    [Flags]
    public enum AntiDegeneracyRules
    {
        /// <summary>No anti-degeneracy handling (ANTIDEGEN_NONE = 0 in C code)</summary>
        None = 0,
        /// <summary>Check if there are equality slacks in the basis and try to drive them out in order to reduce chance of degeneracy in Phase 1 (ANTIDEGEN_FIXEDVARS = 1 in C code)</summary>
        FixedVariables = 1,
        /// <summary> (ANTIDEGEN_COLUMNCHECK = 2 in C code)</summary>
        ColumnCheck = 2,
        /// <summary> (ANTIDEGEN_STALLING = 4 in C code)</summary>
        Stalling = 4,
        /// <summary> (ANTIDEGEN_NUMFAILURE = 8 in C code)</summary>
        NumericalFailure = 8,
        /// <summary> (ANTIDEGEN_LOSTFEAS = 16 in C code)</summary>
        LostDualFeasibility = 16,
        /// <summary> (ANTIDEGEN_INFEASIBLE = 32 in C code)</summary>
        Infeasible = 32,
        /// <summary> (ANTIDEGEN_DYNAMIC = 64 in C code)</summary>
        Dynamic = 64,
        /// <summary> (ANTIDEGEN_DURINGBB = 128 in C code)</summary>
        DuringBranchAndBound = 128,
        /// <summary>Perturbation of the working RHS at refactorization (ANTIDEGEN_RHSPERTURB = 256 in C code)</summary>
        RHSPerturbation = 256,
        /// <summary>Limit bound flips that can sometimes contribute to degeneracy in some models (ANTIDEGEN_BOUNDFLIP = 512 in C code)</summary>
        BoundFlips = 512
    }
}