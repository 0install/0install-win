// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using LinFu.DynamicProxy;
using PackageManagement.Sdk;

namespace ZeroInstall.OneGet;

/// <summary>
/// A OneGet package provider that bootstraps/deploys Zero Install.
/// </summary>
public class BootstrapPackageProvider : PackageProviderBase
{
    public override string PackageProviderName => "0install";

    /// <inheritdoc/>
    protected override IOneGetContext BuildContext(Request request) =>
        new ProxyFactory().CreateProxy<IOneGetContext>(new OneGetContextInterceptor(request));
}
