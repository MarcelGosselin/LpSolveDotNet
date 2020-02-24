namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// User function that is called when certain events occur. Set up by <see cref="LpSolve.PutMessageHandler"/>.
    /// </summary>
    /// <param name="lp">Model being solved.</param>
    /// <param name="message">The event that triggered a call to this handler method.</param>
    public delegate void MessageHandler(LpSolve lp, MessageMasks message);
}