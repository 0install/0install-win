// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

namespace ZeroInstall;

/// <summary>
/// Informs the user about the progress of tasks and ask questions during the bootstrap process using console output.
/// </summary>
/// <remarks>This class is thread-safe.</remarks>
public class CliBootstrapHandler : CliTaskHandler, IBootstrapHandler
{
    /// <inheritdoc/>
    public bool IsGui => false;
}
