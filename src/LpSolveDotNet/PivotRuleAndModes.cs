using System;
using System.ComponentModel;

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
        : IEquatable<PivotRuleAndModes>
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

        #region Equals

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is PivotRuleAndModes other && Equals(other);

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            int hashCode = -863896258;
            hashCode = hashCode * -1521134295 + Rule.GetHashCode();
            hashCode = hashCode * -1521134295 + Modes.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool operator ==(PivotRuleAndModes left, PivotRuleAndModes right) => left.Equals(right);

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool operator !=(PivotRuleAndModes left, PivotRuleAndModes right) => !(left == right);

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Equals(PivotRuleAndModes other) => Rule == other.Rule && Modes == other.Modes;

        #endregion
    }
}
