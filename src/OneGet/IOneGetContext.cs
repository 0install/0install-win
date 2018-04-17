// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using JetBrains.Annotations;
using PackageManagement.Sdk;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// Provides an execution context for handling a single OneGet <see cref="Request"/>.
    /// </summary>
    public interface IOneGetContext : IDisposable
    {
        void AddPackageSource([NotNull] string uri);
        void RemovePackageSource([NotNull] string uri);
        void ResolvePackageSources();
        void FindPackage([CanBeNull] string name, [CanBeNull] string requiredVersion, [CanBeNull] string minimumVersion, [CanBeNull] string maximumVersion);
        void FindPackageBy([NotNull] string identifier);
        void GetInstalledPackages([CanBeNull] string name);
        void DownloadPackage([NotNull] string fastPackageReference, [NotNull] string location);
        void InstallPackage([NotNull] string fastPackageReference);
        void UninstallPackage([NotNull] string fastPackageReference);
        void GetPackageDetails([NotNull] string fastPackageReference);
    }
}
