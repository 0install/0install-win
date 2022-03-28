// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using PackageManagement.Sdk;

namespace ZeroInstall.OneGet;

/// <summary>
/// A OneGet package provider for Zero Install.
/// </summary>
public class PackageProvider : PackageProviderBase
{
    public override string PackageProviderName => "0install";

    /// <inheritdoc/>
    protected override IOneGetContext BuildContext(Request request) => new OneGetContext(request);
}
