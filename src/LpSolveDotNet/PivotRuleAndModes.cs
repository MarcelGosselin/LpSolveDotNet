namespace LpSolveDotNet
{
    /// <summary>
    /// Represents both pivot rule and pivot modes together.
    /// <remarks>
    /// <para>The rule is an exclusive option and the mode is a modifier to the rule.
    /// This rule/mode can influence solving times considerably.
    /// Depending on the model one rule/mode can be best and for another model another rule/mode.</para>
    /// <para>The default rule is <see cref="lpsolve_pivot_rule.PRICER_DEVEX"/>
    /// and the default mode is <see cref="lpsolve_pivot_modes.PRICE_ADAPTIVE"/>.</para>
    /// </remarks>
    /// </summary>
    public struct PivotRuleAndModes
    {
        internal PivotRuleAndModes(lpsolve_pivot_rule rule, lpsolve_pivot_modes modes)
        {
            Rule = rule;
            Modes = modes;
        }

        /// <summary>
        /// The pivot rule (rule for selecting row and column entering/leaving).
        /// </summary>
        public lpsolve_pivot_rule Rule { get; }

        /// <summary>
        /// The pivot modes (modifiers to the <see cref="Rule"/>).
        /// </summary>
        public lpsolve_pivot_modes Modes { get; }
    }
}
