/*
 * Copyright 2010-2016 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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
