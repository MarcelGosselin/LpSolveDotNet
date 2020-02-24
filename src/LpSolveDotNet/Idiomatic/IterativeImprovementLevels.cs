using System;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Specifies the iterative improvement level
    /// </summary>
    [Flags]
    public enum IterativeImprovementLevels
    {
        /// <summary>improve none (IMPROVE_NONE = 0 in C code)</summary>
        None = 0,
        /// <summary>Running accuracy measurement of solved equations based on Bx=r (primal simplex), remedy is refactorization. (IMPROVE_SOLUTION = 1 in C code)</summary>
        Solution = 1,
        /// <summary>Improve initial dual feasibility by bound flips (highly recommended, and default) (IMPROVE_DUALFEAS = 2 in C code)</summary>
        DualFeasibility = 2,
        /// <summary>Low-cost accuracy monitoring in the dual, remedy is refactorization (IMPROVE_THETAGAP = 4 in C code)</summary>
        ThetaGap = 4,
        /// <summary>By default there is a check for primal/dual feasibility at optimum only for the relaxed problem,
        /// this also activates the test at the node level (IMPROVE_BBSIMPLEX = 8 in C code)</summary>
        BBSimplex = 8,
        /// <summary>The default is <see cref="DualFeasibility"/> | <see cref="ThetaGap"/>. (IMPROVE_DEFAULT in C code)</summary>
        Default = (DualFeasibility | ThetaGap),
        /// <summary>Equal to <see cref="Solution"/> | <see cref="ThetaGap"/> (IMPROVE_INVERSE in C code)</summary>
        Inverse = (Solution | ThetaGap)
    }
}