using System;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Pivot Mode. Can be a combination of many values.
    /// </summary>
    [Flags]
    public enum PivotModes
    {
        /// <summary>In case of Steepest Edge, fall back to DEVEX in primal (PRICE_PRIMALFALLBACK = 4 in C code)</summary>
        PrimalFallback = 4,
        /// <summary>Preliminary implementation of the multiple pricing scheme.
        /// This means that attractive candidate entering columns from one iteration may be used in the subsequent iteration,
        /// avoiding full updating of reduced costs.In the current implementation, lp_solve only reuses the 2nd best entering
        /// column alternative (PRICE_MULTIPLE = 8 in C code)</summary>
        Multiple = 8,
        /// <summary>Enable partial pricing (PRICE_PARTIAL = 16 in C code)</summary>
        Partial = 16,
        /// <summary>Temporarily use alternative strategy if cycling is detected (PRICE_ADAPTIVE = 32 in C code)</summary>
        Adaptive = 32,
        // /// <summary>NOT_IMPLEMENTED</summary>
        // PRICE_HYBRID = 64,
        /// <summary>Adds a small randomization effect to the selected pricer (PRICE_RANDOMIZE = 128 in C code)</summary>
        Randomize = 128,
        /// <summary>Indicates automatic detection of segmented/staged/blocked models.It refers to partial pricing rather than full pricing.
        /// With full pricing, all non-basic columns are scanned, but with partial pricing only a subset is scanned for every iteration.
        /// This can speed up several models
        ///  (PRICE_AUTOPARTIAL = 256 in C code)</summary>
        AutoPartial = 256,
        /// <summary>Automatically select multiple pricing (primal simplex) (PRICE_AUTOMULTIPLE = 512 in C code)</summary>
        AutoMultiple = 512,
        /// <summary>Scan entering/leaving columns left rather than right (PRICE_LOOPLEFT = 1024 in C code)</summary>
        LoopLeft = 1024,
        /// <summary>Scan entering/leaving columns alternatingly left/right (PRICE_LOOPALTERNATE = 2048 in C code)</summary>
        LoopAlternate = 2048,
        /// <summary>Use Harris' primal pivot logic rather than the default (PRICE_HARRISTWOPASS = 4096 in C code)</summary>
        HarrisTwoPass = 4096,
        // /// <summary>Non-user option to force full pricing (PRICE_FORCEFULL = 8192 in C code)</summary>
        // non user option PRICE_FORCEFULL = 8192,
        /// <summary>Use true norms for Devex and Steepest Edge initializations (PRICE_TRUENORMINIT = 16384 in C code)</summary>
        TrueNormInit = 16384,
        /// <summary>Disallow automatic bound-flip during pivot (PRICE_NOBOUNDFLIP = 65536 in C code)</summary>
        NoBoundFlip = 65536,
    }
}