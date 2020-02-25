namespace LpSolveDotNet.Idiomatic.Internal
{
    internal static class BoolReturningMethodsExtensions
    {
        /// <summary>
        /// If <paramref name="result"/> is <c>false</c>, this method will call <see cref="IReturnValueHandler.HandleAndReturnBoolean"/> on <paramref name="handler"/>.
        /// </summary>
        /// <param name="result">The value returned from a lp_solve method.</param>
        /// <param name="handler">The error handler defined for the current LP model.</param>
        /// <returns>Either <paramref name="result"/> or an exception will be thrown, depending on <paramref name="handler"/>.</returns>
        public static bool HandleResultAndReturnIt(this bool result, IReturnValueHandler handler)
        {
            if (result)
            {
                return true;
            }
            return handler.HandleAndReturnBoolean();
        }

        /// <summary>
        /// If <paramref name="result"/> is <c>false</c>, this method will call <see cref="IReturnValueHandler.HandleAndReturnVoid"/> on <paramref name="handler"/>.
        /// </summary>
        /// <param name="result">The value returned from a lp_solve method.</param>
        /// <param name="handler">The error handler defined for the current LP model.</param>
        public static void HandleResultAndReturnVoid(this bool result, IReturnValueHandler handler)
        {
            if (result)
            {
                return;
            }
            handler.HandleAndReturnVoid();
        }
    }
}