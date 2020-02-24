using System;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Branch-and-bound modes to apply to <see cref="BranchAndBoundRule"/>.
    /// </summary>
    [Flags]
    public enum BranchAndBoundModes
    {
        /// <summary>No modes applied to <see cref="BranchAndBoundRule"/>.</summary>
        None = 0,
        /// <summary>Select by criterion minimum (worst), rather than criterion maximum (best) (NODE_WEIGHTREVERSEMODE = 8 in C code).</summary>
        WeightReverseMode = 8,
        /// <summary>In case when <see cref="LpSolve.FirstBranch"/> is <see cref="BranchMode.Automatic"/>,
        /// selects the opposite direction (lower/upper branch) that <see cref="BranchMode.Automatic"/> had chosen (NODE_BRANCHREVERSEMODE = 16 in C code).</summary>
        BranchReverseMode = 16,
        /// <summary> (NODE_GREEDYMODE = 32 in C code)</summary>
        GreedyMode = 32,
        /// <summary>Toggles between weighting based on pseudocost or objective function value (NODE_PSEUDOCOSTMODE = 64 in C code).</summary>
        PseudoCostMode = 64,
        /// <summary>Select the node that has already been selected before the most number of times (NODE_DEPTHFIRSTMODE = 128 in C code).</summary>
        DepthFirstMode = 128,
        /// <summary>Adds a randomization factor to the score for any node candicate (NODE_RANDOMIZEMODE = 256 in C code).</summary>
        RandomizeMode = 256,
        /// <summary>Enables GUB mode. Still in development and should not be used at this time (NODE_GUBMODE = 512 in C code).</summary>
        GUBMode = 512,
        /// <summary>When <see cref="DepthFirstMode"/> is selected, switch off this mode when a first solution is found (NODE_DYNAMICMODE = 1024 in C code).</summary>
        DynamicMode = 1024,
        /// <summary>Enables regular restarts of pseudocost value calculations (NODE_RESTARTMODE = 2048 in C code).</summary>
        RestartMode = 2048,
        /// <summary>Select the node that has been selected before the fewest number of times or not at all (NODE_BREADTHFIRSTMODE = 4096 in C code).</summary>
        BreadthFirstMode = 4096,
        /// <summary>Create an "optimal" B&amp;B variable ordering. Can speed up B&amp;B algorithm (NODE_AUTOORDER = 8192 in C code).</summary>
        AutoOrder = 8192,
        /// <summary>Do bound tightening during B&amp;B based of reduced cost information (NODE_RCOSTFIXING = 16384 in C code).</summary>
        RCostFixing = 16384,
        /// <summary>Initialize pseudo-costs by strong branching (NODE_STRONGINIT = 32768 in C code).</summary>
        StrongInit = 32768
    }
}