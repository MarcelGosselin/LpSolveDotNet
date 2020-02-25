using System;
using LpSolveDotNet.Idiomatic.Internal;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Helper class to handle model about Tolerance.
    /// </summary>
    public class ModelTolerance
        : ModelSubObjectBase
    {
        internal ModelTolerance(IntPtr lp)
            : base(lp)
        {
        }

        /// <summary>
        /// Represents the value that is used as a tolerance for the Right Hand Side (RHS) to determine whether a value should be considered as <c>0</c>.
        /// </summary>
        /// <remarks>
        /// <para>Floating-point calculations always result in loss of precision and rounding errors.
        /// Therefore a very small value (example <c>1e-99</c>) could be the result of such errors
        /// and should be considered as <c>0</c> for the algorithm.
        /// <see cref="RightHandSideEpsilon"/> specifies the tolerance to determine if a RHS value should be considered as <c>0</c>.
        /// If abs(value) is less than this <see cref="RightHandSideEpsilon"/> value in the RHS, it is considered as <c>0</c>.</para>
        /// <para>The default <see cref="RightHandSideEpsilon"/> value is <c>1e-10</c></para>
        /// <para>This is called <c>epsb</c> in lp_solve documentation.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epsb.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epsb.htm">Full C API documentation (set).</seealso>
        public double RightHandSideEpsilon
        {
            get => NativeMethods.get_epsb(Lp);
            set => NativeMethods.set_epsb(Lp, value);
        }

        /// <summary>
        /// Represents the value that is used as a tolerance for the reduced costs to determine whether a value should be considered as <c>0</c>.
        /// </summary>
        /// <remarks>
        /// <para>Floating-point calculations always result in loss of precision and rounding errors.
        /// Therefore a very small value (example <c>1e-99</c>) could be the result of such errors 
        /// and should be considered as <c>0</c> for the algorithm. 
        /// <see cref="ReducedCostEpsilon"/> specifies the tolerance to determine if a reducedcost value should be considered as <c>0</c>.
        /// If abs(value) is less than this <see cref="ReducedCostEpsilon"/> value, it is considered as <c>0</c>.</para>
        /// <para>The default <see cref="ReducedCostEpsilon"/> value is <c>1e-9</c></para>
        /// <para>This is called <c>epsd</c> in lp_solve documentation.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epsd.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epsd.htm">Full C API documentation (set).</seealso>
        public double ReducedCostEpsilon
        {
            get => NativeMethods.get_epsd(Lp);
            set => NativeMethods.set_epsd(Lp, value);
        }

        /// <summary>
        /// Represents the value that is used as a tolerance for rounding values to zero and is not covered by other properties of this class.
        /// </summary>
        /// <remarks>
        /// <para>Floating-point calculations always result in loss of precision and rounding errors.
        /// Therefore a very small value (example 1e-99) could be the result of such errors and should be considered as <c>0</c> for the algorithm.
        /// <see cref="DefaultEpsilon"/> specifies the tolerance to determine if a value should be considered as <c>0</c>.
        /// If abs(value) is less than this <see cref="DefaultEpsilon"/> value, it is considered as <c>0</c>.</para>
        /// <para><see cref="DefaultEpsilon"/> is used on all other places where <see cref="IntegerEpsilon"/>, <see cref="RightHandSideEpsilon"/>,
        /// <see cref="ReducedCostEpsilon"/>, <see cref="PerturbationScalarEpsilon"/>, <see cref="PerturbationScalarEpsilon"/> are not used.</para>
        /// <para>The default value for <see cref="DefaultEpsilon"/> is <c>1e-12</c></para>
        /// <para>This is called <c>epsel</c> in lp_solve documentation.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epsel.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epsel.htm">Full C API documentation (set).</seealso>
        public double DefaultEpsilon
        {
            get => NativeMethods.get_epsel(Lp);
            set => NativeMethods.set_epsel(Lp, value);
        }

        /// <summary>
        /// Represents the tolerance that is used to determine whether a floating-point number is in fact an integer.
        /// </summary>
        /// <remarks>
        /// <para>This is only used when there is at least one integer variable and the branch and bound algorithm is used to make variables integer.</para>
        /// <para>Integer variables are internally in the algorithm also stored as floating point.
        /// Therefore a tolerance is needed to determine if a value is to be considered as integer or not.
        /// If the absolute value of the variable minus the closed integer value is less than <see cref="IntegerEpsilon"/>, it is considered as integer.
        /// For example if a variable has the value <c>0.9999999</c> and <see cref="IntegerEpsilon"/> is <c>0.000001</c>
        /// then it is considered integer because <c>abs(0.9999999 - 1) = 0.0000001</c> and this is less than <c>0.000001</c></para>
        /// <para>The default value for <see cref="IntegerEpsilon"/> is <c>1e-7</c></para>
        /// <para>So by changing <see cref="IntegerEpsilon"/> you determine how close a value must approximate the nearest integer.
        /// Changing this tolerance value to for example <c>0.001</c> will generally result in faster solving times, but your solution is less integer.</para>
        /// <para>So it is a compromise.</para>
        /// </remarks>
        /// <para>This is called <c>epsint</c> in lp_solve documentation.</para>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epsint.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epsint.htm">Full C API documentation (set).</seealso>
        public double IntegerEpsilon
        {
            get => NativeMethods.get_epsint(Lp);
            set => NativeMethods.set_epsint(Lp, value);
        }

        /// <summary>
        /// Represents the value that is used as perturbation scalar for degenerative problems.
        /// </summary>
        /// <remarks>The default <see cref="PerturbationScalarEpsilon"/> value is <c>1e-5</c></remarks>
        /// <para>This is called <c>epsperturb</c> in lp_solve documentation.</para>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epsperturb.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epsperturb.htm">Full C API documentation (set).</seealso>
        public double PerturbationScalarEpsilon
        {
            get => NativeMethods.get_epsperturb(Lp);
            set => NativeMethods.set_epsperturb(Lp, value);
        }

        /// <summary>
        /// Represents the value that is used as a tolerance for the pivot element to determine whether a value should be considered as <c>0</c>.
        /// </summary>
        /// <remarks>
        /// <para>Floating-point calculations always result in loss of precision and rounding errors.
        /// Therefore a very small value (example 1e-99) could be the result of such errors and should be considered as <c>0</c> 
        /// for the algorithm. <see cref="PerturbationScalarEpsilon"/> specifies the tolerance to determine if a pivot element should be considered as <c>0</c>.
        /// If abs(value) is less than this <see cref="PerturbationScalarEpsilon"/> value it is considered as <c>0</c> and at first instance rejected as pivot element.
        /// Only when no larger other pivot element can be found and the value is different from <c>0</c> it will be used as pivot element.</para>
        /// <para>The default <see cref="PerturbationScalarEpsilon"/> value is <c>2e-7</c></para>
        /// <para>This is called <c>epspivot</c> in lp_solve documentation.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_epspivot.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epspivot.htm">Full C API documentation (set).</seealso>
        public double PivotEpsilon
        {
            get => NativeMethods.get_epspivot(Lp);
            set => NativeMethods.set_epspivot(Lp, value);
        }

        /// <summary>
        /// Represents the relative MIP gap value that specifies a tolerance for the branch and bound algorithm.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This tolerance is the difference between the best-found solution yet and the current solution.
        /// If the difference is smaller than this tolerance then the solution (and all the sub-solutions) is rejected.
        /// This can result in faster solving times, but results in a solution which is not the perfect solution.
        /// So be careful with this tolerance.</para>
        /// <para>The default is <c>1e-11.</c></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_mip_gap.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_mip_gap.htm">Full C API documentation (set).</seealso>
        public double RelativeMipGap
        {
            get => NativeMethods.get_mip_gap(Lp, false);
            set => NativeMethods.set_mip_gap(Lp, false, value);
        }

        /// <summary>
        /// Represents the absolute MIP gap value that specifies a tolerance for the branch and bound algorithm.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This tolerance is the difference between the best-found solution yet and the current solution.
        /// If the difference is smaller than this tolerance then the solution (and all the sub-solutions) is rejected.
        /// This can result in faster solving times, but results in a solution which is not the perfect solution.
        /// So be careful with this tolerance.</para>
        /// <para>The default is <c>1e-11.</c></para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/get_mip_gap.htm">Full C API documentation (get).</seealso>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_mip_gap.htm">Full C API documentation (set).</seealso>
        public double AbsoluteMipGap
        {
            get => NativeMethods.get_mip_gap(Lp, true);
            set => NativeMethods.set_mip_gap(Lp, true, value);
        }

        /// <summary>
        /// This is a simplified way of specifying multiple Epsilon thresholds that are "logically" consistent.
        /// </summary>
        /// <param name="level">The level to set.</param>
        /// <returns><c>true</c> if level is accepted and <c>false</c> if an invalid epsilon level was provided.</returns>
        /// <remarks>
        /// <para>It sets the following values: <see cref="DefaultEpsilon"/>, <see cref="RightHandSideEpsilon"/>, <see cref="ReducedCostEpsilon"/>,
        /// <see cref="PivotEpsilon"/>, <see cref="IntegerEpsilon"/>, <see cref="RelativeMipGap"/>, <see cref="AbsoluteMipGap"/>.</para>
        /// <para>The default is <see cref="ToleranceEpsilonLevel.Tight"/>.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_epslevel.htm">Full C API documentation.</seealso>
        public bool SetEpsilonLevel(ToleranceEpsilonLevel level)
        {
            return NativeMethods.set_epslevel(Lp, level);
        }
    }
}
