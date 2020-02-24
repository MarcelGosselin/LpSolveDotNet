using System;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Defines scaling parameters to add to scaling algorithms. You can combine more than one.
    /// </summary>
    [Flags]
    public enum ScaleParameters
    {
        /// <summary>No parameters (SCALE_NONE = 0 in C code)</summary>
        None = 0,
        /// <summary> (SCALE_QUADRATIC = 8 in C code)</summary>
        Quadratic = 8,
        /// <summary>Scale to convergence using logarithmic mean of all values (SCALE_LOGARITHMIC = 16 in C code)</summary>
        Logarithmic = 16,
        /// <summary>User can specify scalars (SCALE_USERWEIGHT = 31 in C code)</summary>
        UserWeight = 31,
        /// <summary>also do Power scaling (SCALE_POWER2 = 32 in C code)</summary>
        Power2 = 32,
        /// <summary>Make sure that no scaled number is above 1 (SCALE_EQUILIBRATE = 64 in C code)</summary>
        Equilibrate = 64,
        /// <summary>also scaling integer variables (SCALE_INTEGERS = 128 in C code)</summary>
        Integers = 128,
        /// <summary>dynamic update (SCALE_DYNUPDATE = 256 in C code)</summary>
        DynanicUpdate = 256,
        /// <summary>scale only rows (SCALE_ROWSONLY = 512 in C code)</summary>
        RowsOnly = 512,
        /// <summary>scale only columns (SCALE_COLSONLY = 1024 in C code)</summary>
        ColumnsOnly = 1024,
    }
}