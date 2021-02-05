using System;
using LpSolveDotNet.Idiomatic.Internal;

namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Helper class to handle model operations regarding External Language Interface (XLI).
    /// </summary>
    public class ExternalLanguageInterface
        : ModelSubObjectBase
    {
        internal ExternalLanguageInterface(IntPtr lp)
            : base(lp)
        {
        }

        /// <summary>
        /// Returns if a built-in External Language Interfaces (XLI) is available or not.
        /// </summary>
        /// <returns><c>true</c> if there is a built-in XLI is available, <c>false</c> if not.</returns>
        /// <remarks>
        /// <para>At this moment, this method always returns <c>false</c> since no built-in XLI is available.</para>
        /// <para>See <seealso href="http://lpsolve.sourceforge.net/5.5/XLI.htm">External Language Interfaces</seealso>
        /// for a complete description on XLIs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/is_nativeXLI.htm">Full C API documentation.</seealso>
        public bool IsNativeXLIAvailable
            => NativeMethods.is_nativeXLI(Lp);


        /// <summary>
        /// Returns if there is an external language interface (XLI) set.
        /// </summary>
        /// <returns><c>true</c> if there is an XLI is set, else <c>false</c>.</returns>
        /// <remarks>
        /// <para>See <seealso href="http://lpsolve.sourceforge.net/5.5/XLI.htm">External Language Interfaces</seealso>
        /// for a complete description on XLIs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/has_XLI.htm">Full C API documentation.</seealso>
        public bool IsSet
            => NativeMethods.has_XLI(Lp);

        /// <summary>
        /// Sets External Language Interfaces package.
        /// </summary>
        /// <param name="filename">The name of the XLI package.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>This call is normally only needed when <see cref="Write"/> will be called. 
        /// <see cref="LpSolve.CreateFromXLIFile"/> automatically calls this method</para>
        /// <para>See <seealso href="http://lpsolve.sourceforge.net/5.5/XLI.htm">External Language Interfaces</seealso>
        /// for a complete description on XLIs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/set_XLI.htm">Full C API documentation.</seealso>
        public bool Set(string filename)
            => NativeMethods.set_XLI(Lp, filename);

        /// <summary>
        /// Writes a model to a file via the External Language Interface.
        /// </summary>
        /// <param name="filename">Filename to write the model to.</param>
        /// <param name="options">Extra options that can be used by the writer.</param>
        /// <param name="results"><c>false</c> to generate a model file, <c>true</c> to generate a solution file.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// <para>Note that <see cref="Set"/> must be called before this method to set an XLI.</para>
        /// <para>See <seealso href="http://lpsolve.sourceforge.net/5.5/XLI.htm">External Language Interfaces</seealso>
        /// for a complete description on XLIs.</para>
        /// </remarks>
        /// <seealso href="http://lpsolve.sourceforge.net/5.5/write_XLI.htm">Full C API documentation.</seealso>
        public bool Write(string filename, string options, bool results)
            => NativeMethods.write_XLI(Lp, filename, options, results);
    }
}
