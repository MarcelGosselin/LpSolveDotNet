namespace LpSolveDotNet.Idiomatic.Internal
{
    /// <summary>
    /// Helper for handling boolean return values from lp_solve methods in a uniform way.
    /// </summary>
    /// <remarks>
    /// There are two methods that may do different things because <see cref="HandleAndReturnVoid"/>
    /// as no way to notify calling code that something happened besides exceptions.
    /// </remarks>
    public interface IReturnValueHandler
    {
        /// <summary>
        /// Looks at the status of the associated LP model, handles the error.
        /// It either returns <c>false</c> which is what the C code was doing
        /// or it throws depending on implementation.
        /// </summary>
        /// <returns><c>false</c></returns>
        bool HandleAndReturnBoolean();

        /// <summary>
        /// Looks at the status of the associated LP model and handles the error.
        /// </summary>
        void HandleAndReturnVoid();
    }

}