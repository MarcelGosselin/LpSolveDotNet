using System;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Defines the events upon which the method that was set with <see cref="LpSolve.PutMessageHandler"/> will be called.
    /// </summary>
    [Flags]
    public enum MessageMasks
    {
        /// <summary>Do not handle any message. (MSG_NONE = 0 in C code)</summary>
        None = 0,

        /// <summary>Presolve done. (PreSolve = 1 in C code)</summary>
        PreSolve = 1,

        ///// <summary>?? Only used on user abort.</summary>
        //MSG_ITERATION = 2,

        ///// <summary>?? Only used on user abort.</summary>
        //MSG_INVERT = 4,

        /// <summary>Feasible solution found. (LPFeasible = 8 in C code)</summary>
        LPFeasible = 8,

        /// <summary>Real optimal solution found. Only fired when there are integer variables at the start of B&amp;B. (LPOptimal = 16 in C code)</summary>
        LPOptimal = 16,

        //MSG_LPEQUAL = 32, //not used in lpsolve code
        //MSG_LPBETTER = 64, //not used in lpsolve code

        /// <summary>First MILP solution found. Only fired when there are integer variables during B&amp;B. (MILPFeasible = 128 in C code)</summary>
        MILPFeasible = 128,

        /// <summary>Equal MILP solution found. Only fired when there are integer variables during B&amp;B. (MILPEqual = 256 in C code)</summary>
        MILPEqual = 256,

        /// <summary>Better MILPsolution found. Only fired when there are integer variables during B&amp;B. (MILPBetter = 512 in C code)</summary>
        MILPBetter = 512,

        ///// <summary>?? Only used on user abort.</summary>
        //MSG_MILPSTRATEGY = 1024,

        //MSG_MILPOPTIMAL = 2048, //not used in lpsolve code
        //MSG_PERFORMANCE = 4096, //not used in lpsolve code

        ///// <summary>?? Only used on user abort.</summary>
        //MSG_INITPSEUDOCOSR = 8192,
    }
}
