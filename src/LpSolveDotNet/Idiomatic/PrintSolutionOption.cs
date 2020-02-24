namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Defines if all intermediate valid solutions must be printed while solving.
    /// </summary>
    public enum PrintSolutionOption
    {
        /// <summary>No printing [Default] (FALSE = 0 in C code)</summary>
        False = 0,
        /// <summary>Print all values (TRUE = 1 in C code)</summary>
        True = 1,
        /// <summary>Print only non-zero values (AUTOMATIC = 2 in C code)</summary>
        Automatic = 2,
    }
}