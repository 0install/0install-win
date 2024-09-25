// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

namespace ZeroInstall;

/// <summary>
/// Used to run and track <see cref="ITask"/>s and ask the user questions during the bootstrap process.
/// </summary>
/// <remarks>Implementations of this interface are thread-safe.</remarks>
public interface IBootstrapHandler : ITaskHandler
{
    /// <summary>
    /// Indicates whether this handler is a GUI.
    /// </summary>
    bool IsGui { get; }

    /// <summary>
    /// Hides the GUI. Has no effect when <see cref="IsGui"/> is <c>false</c>.
    /// </summary>
    bool Background { get; set; }

    /// <summary>
    /// Asks the user to provide a custom path for storing implementations.
    /// </summary>
    /// <param name="machineWide">Ask for a path for machine-wide deployment instead of just for the current user.</param>
    /// <param name="currentPath">The currently set custom path for storing implementations; <c>null</c> if using default location.</param>
    /// <returns>The path to a directory; <c>null</c> or empty to use the default location.</returns>
    /// <exception cref="OperationCanceledException">The user cancelled the operation.</exception>
    string? GetCustomStorePath(bool machineWide, string? currentPath);
}
