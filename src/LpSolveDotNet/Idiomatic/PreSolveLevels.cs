using System;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Presolve levels. Can be the OR-combination of any of the values.
    /// </summary>
    [Flags]
    public enum PreSolveLevels
    {
        /// <summary>No presolve at all (PRESOLVE_NONE = 0 in C code)</summary>
        None = 0,
        /// <summary>Presolve rows (PRESOLVE_ROWS = 1 in C code)</summary>
        Rows = 1,
        /// <summary>Presolve columns (PRESOLVE_COLS = 2 in C code)</summary>
        Columns = 2,
        /// <summary>Eliminate linearly dependent rows (PRESOLVE_LINDEP = 4 in C code)</summary>
        LinearlyDependentRows = 4,
        //PRESOLVE_AGGREGATE = 8, //not implemented
        //PRESOLVE_SPARSER = 16, //not implemented
        /// <summary>Convert constraints to SOSes (only SOS1 handled) (PRESOLVE_SOS = 32 in C code)</summary>
        SOS = 32,
        /// <summary>If the phase 1 solution process finds that a constraint is redundant then this constraint is deleted (PRESOLVE_REDUCEMIP = 64 in C code)</summary>
        ReduceMIP = 64,
        /// <summary>Simplification of knapsack-type constraints through addition of an extra variable, which also helps bound the OF (PRESOLVE_KNAPSACK = 128 in C code)</summary>
        Knapsack = 128,
        /// <summary>Direct substitution of one variable in 2-element equality constraints; this requires changes to the constraint matrix.
        /// Elimeq2 simply eliminates a variable by substitution when you have 2-element equality constraints.
        /// This can sometimes cause fill-in of the constraint matrix, and also be a source of rounding errors which can lead to problems in the simplex.
        ///  (PRESOLVE_ELIMEQ2 = 256 in C code)
        /// </summary>
        ElimEQ2 = 256,
        /// <summary>Identify implied free variables (releasing their explicit bounds) (PRESOLVE_IMPLIEDFREE = 512 in C code)</summary>
        ImpliedFreeVariables = 512,
        /// <summary>Reduce (tighten) coefficients in integer models based on GCD argument.
        /// Reduce GCD is for mixed integer programs where it is possible to adjust the constraint coefficies due to integrality.
        /// This can cause the dual objective ("lower bound") to increase and may make it easier to prove optimality.
        ///  (PRESOLVE_REDUCEGCD = 1024 in C code)
        /// </summary>
        ReduceGCD = 1024,
        /// <summary>Attempt to fix binary variables at one of their bounds (PRESOLVE_PROBEFIX = 2048 in C code)</summary>
        ProbeFix = 2048,
        /// <summary>Attempt to reduce coefficients in binary models (PRESOLVE_PROBEREDUCE = 4096 in C code)</summary>
        ProbeReduce = 4096,
        /// <summary>Idenfify and delete qualifying constraints that are dominated by others, also fixes variables at a bound (PRESOLVE_ROWDOMINATE = 8192 in C code)</summary>
        RowDominate = 8192,
        /// <summary>Deletes variables (mainly binary), that are dominated by others (only one can be non-zero) (PRESOLVE_COLDOMINATE = 16384 in C code)</summary>
        ColumnDominate = 16384,
        /// <summary>Merges neighboring >= or &lt;= constraints when the vectors are otherwise relatively identical into a single ranged constraint (PRESOLVE_MERGEROWS = 32768 in C code)</summary>
        MergeRows = 32768,
        /// <summary>Converts qualifying equalities to inequalities by converting a column singleton variable to slack.
        /// The method also detects implicit duplicate slacks from inequality constraints, fixes and removes the redundant variable.
        /// This latter removal also tends to reduce the risk of degeneracy.
        /// The combined function of this option can have a dramatic simplifying effect on some models.
        /// Implied slacks is when, for example, there is a column singleton (with zero OF) in an equality constraint.
        /// In this case, the column can be deleted and the constraint converted to a LE constraint.
        ///  (PRESOLVE_IMPLIEDSLK = 65536 in C code)</summary>
        ImpliedSlack = 65536,
        /// <summary>Variable fixing and removal based on considering signs of the associated dual constraint.
        /// Dual fixing is when the (primal) variable can be fixed due to the implied value of the dual being infinite.
        ///  (PRESOLVE_COLFIXDUAL = 131072 in C code)</summary>
        ColumnFixDual = 131072,
        /// <summary>Does bound tightening based on full-row constraint information. This can assist in tightening the OF bound, eliminate variables and constraints.
        /// At the end of presolve, it is checked if any variables can be deemed free, thereby reducing any chance that degeneracy is introduced via this presolve option.
        ///  (PRESOLVE_BOUNDS = 262144 in C code)</summary>
        BoundsTightening = 262144,
        /// <summary>Calculate duals (PRESOLVE_DUALS = 524288 in C code)</summary>
        CalculateDuals = 524288,
        /// <summary>Calculate sensitivity if there are integer variables (PRESOLVE_SENSDUALS = 1048576 in C code)</summary>
        CalculateSensitivityDuals = 1048576
    }
}