using System;

namespace ZeroInstall.Solver
{
    /// <summary>
    /// ...
    /// </summary>
    public interface ISolver
    {
        Selections Solve(Uri feed);
    }
}
