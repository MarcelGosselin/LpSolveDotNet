using System;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Options used when reading MPS files. You can combine them.
    /// </summary>
    [Flags]
    public enum MPSOptions
    {
        /// <summary>Fixed MPS Format [Default] (MPS_FIXED = 0 in C code)</summary>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">MPS file format</seealso>
        FixedMPSFormat = 0,
        /// <summary>Free MPS Format (MPS_FREE = 8 in C code)</summary>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">MPS file format</seealso>
        FreeMPSFormat = 8,
        /// <summary>Interprete integer variables without bounds as binary variables. That is the original IBM standard.
        /// By default lp_solve interpretes variables without bounds as having no upperbound as for real variables. (MPS_IBM = 16 in C code)</summary>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">MPS file format (section G)</seealso>
        IBM = 16,
        /// <summary>Interprete the objective constant with an opposite sign. Some solvers interprete the objective constant
        /// as a value in the RHS and negate it when brought at the LHS.
        /// This option allows to let lp_solve do this also. (MPS_NEGOBJCONST = 32 in C code)</summary>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/mps-format.htm">MPS file format</seealso>
        NegateObjectiveConstant = 32,
    }
}