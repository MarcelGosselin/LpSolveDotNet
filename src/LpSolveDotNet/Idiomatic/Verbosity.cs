namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Defines the amount of information that is reported back to the user.
    /// </summary>
    public enum Verbosity
    {
        /// <summary>Only some specific debug messages in the debug print methods are reported. (NEUTRAL = 0 in C code)</summary>
        Neutral = 0,
        /// <summary>Only critical messages are reported. Hard errors like instability, out of memory, ... (CRITICAL = 1 in C code)</summary>
        Critical = 1,
        /// <summary>Only severe messages are reported. Errors. (SEVERE = 2 in C code)</summary>
        Severe = 2,
        /// <summary>Only important messages are reported. Warnings and Errors. (IMPORTANT = 3 in C code)</summary>
        Important = 3,
        /// <summary>Normal messages are reported. This is the default. (NORMAL = 4 in C code)</summary>
        Normal = 4,
        /// <summary>Detailed messages are reported. Like model size, continuing B&amp;B improvements, ... (DETAILED = 5 in C code)</summary>
        Detailed = 5,
        /// <summary>All messages are reported. Useful for debugging purposes and small models. (FULL = 6 in C code)</summary>
        Full = 6,
    }
}