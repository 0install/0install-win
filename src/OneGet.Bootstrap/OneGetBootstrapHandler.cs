// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using PackageManagement.Sdk;

namespace ZeroInstall.OneGet;

/// <summary>
/// Manages communication between <see cref="ITask"/>s and a OneGet <see cref="Request"/> during the bootstrap process.
/// </summary>
public class OneGetBootstrapHandler(Request request) : OneGetHandler(request), IBootstrapHandler
{
    /// <inheritdoc/>
    public bool IsGui => false;

    /// <inheritdoc/>
    public bool Background { get => false; set {} }

    /// <inheritdoc/>
    public string? GetCustomStorePath(bool machineWide, string? currentPath) => currentPath;
}
