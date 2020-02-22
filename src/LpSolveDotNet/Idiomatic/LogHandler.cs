namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// User function that is called when certain log events occur. Set up by <see cref="LpSolve.PutLogHandler"/>.
    /// </summary>
    /// <param name="lp">Model being solved.</param>
    /// <param name="message">The log message.</param>
    public delegate void LogHandler(LpSolve lp, string message);
}