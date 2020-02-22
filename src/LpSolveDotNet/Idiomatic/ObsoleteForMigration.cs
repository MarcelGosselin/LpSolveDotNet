using System;

#pragma warning disable 1591

namespace LpSolveDotNet.Idiomatic
{
    public sealed partial class LpSolve
    {
        [Obsolete("Replaced by " + nameof(PutBranchAndBoundNodeSelector), true)]
        public void put_bb_nodefunc(params object[] _)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(PutBranchAndBoundBranchSelector), true)]
        public void put_bb_branchfunc(params object[] _)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(PutMessageHandler), true)]
        public void put_msgfunc(msgfunctemp handler, params object[] _)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(PutLogHandler), true)]
        public void put_logfunc(logfunctemp handler, params object[] _)
            => throw new NotImplementedException();

        [Obsolete("Replaced by " + nameof(PutAbortHandler), true)]
        public void put_abortfunc(ctrlcfunctemp handler, params object[] _)
            => throw new NotImplementedException();
    }
    public delegate bool ctrlcfunctemp(IntPtr lp, IntPtr userhandle);

    public delegate void msgfunctemp(IntPtr lp, IntPtr userhandle, lpsolve_msgmask message);

    public delegate void logfunctemp(IntPtr lp, IntPtr userhandle, string buf);
}

#pragma warning restore 1591
