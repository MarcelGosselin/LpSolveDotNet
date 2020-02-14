namespace LpSolveDotNet
{
    /// <summary>
    /// Defines which branch to use first for a given column.
    /// </summary>
    public enum BranchSelectorResult
    {
        /// <summary>Take ceiling branch first</summary>
        Ceiling = 0,
        /// <summary>Take floor branch first</summary>
        Floor = 1,
    }
}