using System;
using System.ComponentModel;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Represents both branch-and-bound rule and branch-and-bound modes together
    /// which define which non-integer variable is to be selected.
    /// </summary>
    /// <remarks>
    /// <para>The rule is an exclusive option and the modes are modifiers to the rule.
    /// This rule/mode can influence solving times considerably.
    /// Depending on the model one rule/mode can be best and for another model another rule/mode.</para>
    /// <para>The default rule is <see cref="BranchAndBoundRule.PseudoNonIntegerSelect"/>
    /// and the default modes are <see cref="BranchAndBoundModes.GreedyMode"/> | <see cref="BranchAndBoundModes.DynamicMode"/> | <see cref="BranchAndBoundModes.RCostFixing"/>.</para>
    /// </remarks>
    public struct BranchAndBoundRuleAndModes
        : IEquatable<BranchAndBoundRuleAndModes>
    {
        internal BranchAndBoundRuleAndModes(BranchAndBoundRule rule, BranchAndBoundModes modes)
        {
            Rule = rule;
            Modes = modes;
        }

        /// <summary>
        /// The branch-and-bound rule (rule for which non-integer variable is to be selected).
        /// </summary>
        public BranchAndBoundRule Rule { get; }

        /// <summary>
        /// The branch-and-bound modes (modifiers to the <see cref="Rule"/>).
        /// </summary>
        public BranchAndBoundModes Modes { get; }

        #region Equals

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is BranchAndBoundRuleAndModes other && Equals(other);

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
        public static bool operator ==(BranchAndBoundRuleAndModes left, BranchAndBoundRuleAndModes right) => left.Equals(right);

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool operator !=(BranchAndBoundRuleAndModes left, BranchAndBoundRuleAndModes right) => !(left == right);

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Equals(BranchAndBoundRuleAndModes other) => Rule == other.Rule && Modes == other.Modes;

        #endregion
    }
}
