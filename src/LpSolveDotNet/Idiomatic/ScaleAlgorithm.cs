namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Defines scaling algorithm to use..
    /// </summary>
    public enum ScaleAlgorithm
    {
        /// <summary>No scaling (SCALE_NONE = 0 in C code)</summary>
        None = 0,
        /// <summary>Scale to convergence using largest absolute value (SCALE_EXTREME = 1 in C code)</summary>
        Extreme = 1,
        /// <summary>Scale based on the simple numerical range (SCALE_RANGE = 2 in C code)</summary>
        Range = 2,
        /// <summary>Numerical range-based scaling (SCALE_MEAN = 3 in C code)</summary>
        Mean = 3,
        /// <summary>Geometric scaling (SCALE_GEOMETRIC = 4 in C code)</summary>
        Geometric = 4,
        /// <summary>Curtis-reid scaling (SCALE_CURTISREID = 7 in C code)</summary>
        CurtisReid = 7,
    }
}