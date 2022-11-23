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
}
