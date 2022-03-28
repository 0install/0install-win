// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using PackageManagement.Sdk;

namespace ZeroInstall.OneGet;

/// <summary>
/// Provides an execution context for handling a single OneGet <see cref="Request"/>.
/// </summary>
public interface IOneGetContext : IDisposable
{
    void AddPackageSource(string uri);
    void RemovePackageSource(string uri);
    void ResolvePackageSources();
    void FindPackage(string? name, string? requiredVersion, string? minimumVersion, string? maximumVersion);
    void FindPackageBy(string identifier);
    void GetInstalledPackages(string? name);
    void DownloadPackage(string fastPackageReference, string location);
    void InstallPackage(string fastPackageReference);
    void UninstallPackage(string fastPackageReference);
    void GetPackageDetails(string fastPackageReference);
}
