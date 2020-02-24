using System;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Represents both pivot rule and pivot modes together.
    /// </summary>
    /// <remarks>
    /// <para>The rule is an exclusive option and the mode is a modifier to the rule.
    /// This rule/mode can influence solving times considerably.
    /// Depending on the model one rule/mode can be best and for another model another rule/mode.</para>
    /// <para>The default rule is <see cref="PivotRule.Devex"/>
    /// and the default mode is <see cref="PivotModes.Adaptive"/>.</para>
    /// </remarks>
    public struct PivotRuleAndModes
        : IEquatable<PivotRuleAndModes>
    {
        internal PivotRuleAndModes(PivotRule rule, PivotModes modes)
        {
            Rule = rule;
            Modes = modes;
        }

        /// <summary>
        /// The pivot rule (rule for selecting row and column entering/leaving).
        /// </summary>
        public PivotRule Rule { get; }

        /// <summary>
        /// The pivot modes (modifiers to the <see cref="Rule"/>).
        /// </summary>
        public PivotModes Modes { get; }

        #region Equals

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is PivotRuleAndModes other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -863896258;
            hashCode = hashCode * -1521134295 + Rule.GetHashCode();
            hashCode = hashCode * -1521134295 + Modes.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(PivotRuleAndModes left, PivotRuleAndModes right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(PivotRuleAndModes left, PivotRuleAndModes right) => !(left == right);

        /// <inheritdoc/>
        public bool Equals(PivotRuleAndModes other) => Rule == other.Rule && Modes == other.Modes;

        #endregion
    }
}
