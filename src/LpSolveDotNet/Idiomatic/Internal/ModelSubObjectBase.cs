using System;

namespace LpSolveDotNet.Idiomatic.Internal
{
    /// <summary>
    /// Base class for Model Helper classes.
    /// </summary>
    public abstract class ModelSubObjectBase
    {
        // This class does not OWN _lp so we MUST NOT free it.
        internal IntPtr Lp { get; }
        internal IReturnValueHandler ReturnValueHandler { get; set; }

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="lp">The model which is wrapped by this helper.</param>
        protected ModelSubObjectBase(IntPtr lp)
        {
            Lp = lp;
        }
    }
}
