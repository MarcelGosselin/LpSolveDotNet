namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Parameters constants for short-cut setting of tolerances.
    /// </summary>
    public enum ToleranceEpsilonLevel
    {
        /// <summary>Very tight epsilon values (default) (EPS_TIGHT = 0 in C code).</summary>
        Tight = 0,

        /// <summary>Medium epsilon values. (EPS_MEDIUM = 1 in C code)</summary>
        Medium = 1,

        /// <summary>Loose epsilon values. (EPS_LOOSE = 2 in C code)</summary>
        Loose = 2,

        /// <summary>Very loose epsilon values. (EPS_BAGGY = 3 in C code)</summary>
        Baggy = 3,

        /// <summary>Default: <see cref="Tight"/> (EPS_DEFAULT in C code)</summary>
        Default = Tight,
    }
}