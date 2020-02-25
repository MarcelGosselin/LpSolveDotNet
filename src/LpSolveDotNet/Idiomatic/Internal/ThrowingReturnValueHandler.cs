using System;

namespace LpSolveDotNet.Idiomatic.Internal
{
    internal class ThrowingReturnValueHandler
        : IReturnValueHandler
    {
        IntPtr _lp;

        internal ThrowingReturnValueHandler(IntPtr lp)
        {
            _lp = lp;
        }

        void IReturnValueHandler.HandleAndReturnVoid()
        {
            _ = Handle();
        }

        bool IReturnValueHandler.HandleAndReturnBoolean()
        {
            return Handle();
        }

        private bool Handle()
        {
            int statusCode = NativeMethods.get_status(_lp);
            string message = NativeMethods.get_statustext(_lp, statusCode);
            throw new Exception(message);
        }
    }
}