namespace LpSolveDotNet.Idiomatic
{
    /// <summary>
    /// Defines which branch to take in branch-and-bound algorithm.
    /// </summary>
    public enum BranchMode
    {
        /// <summary>Take ceiling branch first (BRANCH_CEILING = 0 in C code).</summary>
        Ceiling = 0,
        /// <summary>Take floor branch first (BRANCH_FLOOR = 1 in C code).</summary>
        Floor = 1,
        /// <summary>Algorithm decides which branch being taken first (BRANCH_AUTOMATIC = 2 in C code).</summary>
        Automatic = 2,
        /// <summary>
        /// (To be used only in <see cref="ModelColumn.BranchAndBoundMode"/>).
        /// Means to use value set in <see cref="LpSolve.FirstBranch"/>.
        /// (BRANCH_DEFAULT = 3 in C code)
        /// </summary>
        Default = 3,
    }
}