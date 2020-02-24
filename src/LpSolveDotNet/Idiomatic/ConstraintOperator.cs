namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Defines the operator used in the equation/inequation representing the constraint.
    /// </summary>
    public enum ConstraintOperator
    {
        /// <summary>Free. A free constraint will act as if the constraint is not there. The lower bound is -Infinity and the upper bound is +Infinity.
        /// This can be used to temporary disable a constraint without having to delete it from the model.
        /// Note that the already set RHS and range on this constraint is overruled with Infinity by this.
        /// (FR = 0 in C code)</summary>
        Free = 0,
        /// <summary>Less than or equal (&lt;=) (LE = 1 in C code)</summary>
        LessOrEqual = 1,
        /// <summary>Greater than or equal (&gt;=) (GE = 2 in C code)</summary>
        GreaterOrEqual = 2,
        /// <summary>Equal (=) (EQ = 3 in C code)</summary>
        Equal = 3,
        /// <summary>Objective Function. This is not an operator to use but a marker for the function to optimize. (OF = 4 in C code)</summary>
        ObjectiveFunction = 4,
    }
}