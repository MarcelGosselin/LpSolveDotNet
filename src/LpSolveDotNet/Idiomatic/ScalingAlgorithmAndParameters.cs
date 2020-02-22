using System;
using System.ComponentModel;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Represents both scaling algorithm and parameters together.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This can influence numerical stability considerably.
    /// It is advisable to always use some sort of scaling.</para>
    /// <para><see cref="LpSolve.SetScaling"/> must be called before solve is called.</para>
    /// <para><see cref="ScaleAlgorithm.Extreme"/>, <see cref="ScaleAlgorithm.Range"/>, <see cref="ScaleAlgorithm.Mean"/>,
    /// <see cref="ScaleAlgorithm.Geometric"/>, <see cref="ScaleAlgorithm.CurtisReid"/> are the possible scaling algorithms.
    /// <see cref="ScaleParameters.Quadratic"/>, <see cref="ScaleParameters.Logarithmic"/>, <see cref="ScaleParameters.UserWeight"/>,
    /// <see cref="ScaleParameters.Power2"/>, <see cref="ScaleParameters.Equilibrate"/>, <see cref="ScaleParameters.Integers"/>
    /// are possible additional scaling parameters.</para>
    /// <para><see cref="ScaleParameters.Power2"/> results in creating a scalar of power 2. May improve stability.</para>
    /// <para><see cref="ScaleParameters.Integers"/> results also in scaling Integer columns. Default they are not scaled.</para>
    /// <para><see cref="ScaleParameters.DynanicUpdate"/> is new from version 5.1.1.0</para>
    /// <para>It has always been so that scaling is done only once on the original model.
    /// If a solve is done again (most probably after changing some data in the model),
    /// the scaling factors aren't computed again.
    /// The scalars of the original model are used.
    /// This is not always good, especially if the data has changed considerably.
    /// One way to solve this was/is call unscale before a next solve.
    /// In that case, scale factors are recomputed.</para>
    /// <para>From version 5.1.1.0 on, there is another way to make sure that scaling factors are recomputed and this is by settings <see cref="ScaleParameters.DynanicUpdate"/>.
    /// In that case, the scaling factors are recomputed also when a restart is done.Note that they are then always recalculated with each solve,
    /// even when no change was made to the model, or a change that doesn't influence the scaling factors like changing the RHS (Right Hand Side) 
    /// values or the bounds/ranges. This can influence performance.
    /// It is up to you to decide if scaling factors must be recomputed or not for a new solve, but by default it still isn't so.
    /// It is possible to set/unset this flag at each next solve and it is even allowed to choose a new scaling algorithm between each solve.
    /// Note that the scaling done by the <see cref="ScaleParameters.DynanicUpdate"/> is incremental and the resulting scalars are typically 
    /// different from scalars recomputed from scratch.</para>
    /// <para>The default algorithm is <see cref="ScaleAlgorithm.Geometric"/> 
    /// with parameters <see cref="ScaleParameters.Equilibrate"/> | <see cref="ScaleParameters.Integers"/>.</para>
    /// </remarks>
    public struct ScalingAlgorithmAndParameters
        : IEquatable<ScalingAlgorithmAndParameters>
    {
        internal ScalingAlgorithmAndParameters(ScaleAlgorithm algorithm, ScaleParameters parameters)
        {
            Algorithm = algorithm;
            Parameters = parameters;
        }

        /// <summary>
        /// The scaling algorithm.
        /// </summary>
        public ScaleAlgorithm Algorithm { get; }

        /// <summary>
        /// The scaling parameters (modifiers to the <see cref="Algorithm"/>).
        /// </summary>
        public ScaleParameters Parameters { get; }

        #region Equals

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is ScalingAlgorithmAndParameters other && Equals(other);

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            int hashCode = -863896258;
            hashCode = hashCode * -1521134295 + Algorithm.GetHashCode();
            hashCode = hashCode * -1521134295 + Parameters.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool operator ==(ScalingAlgorithmAndParameters left, ScalingAlgorithmAndParameters right) => left.Equals(right);

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool operator !=(ScalingAlgorithmAndParameters left, ScalingAlgorithmAndParameters right) => !(left == right);

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Equals(ScalingAlgorithmAndParameters other) => Algorithm == other.Algorithm && Parameters == other.Parameters;

        #endregion
    }
}
