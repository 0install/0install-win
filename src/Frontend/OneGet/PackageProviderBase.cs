/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Info;
using PackageManagement.Sdk;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services.Injector;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// A common base for OneGet package providers for Zero Install. Implements OneGet's duck-typing interface.
    /// </summary>
    public abstract class PackageProviderBase
    {
        [PublicAPI]
        public abstract string PackageProviderName { get; }

        [PublicAPI]
        public string ProviderVersion { get { return AppInfo.Load(Assembly.GetExecutingAssembly()).Version.ToString(); } }

        [PublicAPI]
        public void OnUnhandledException(string methodName, Exception exception)
        {
            Log.Error("Unexpected Exception thrown in " + PackageProviderName + "::" + methodName);
            Log.Error(exception);
        }

        [PublicAPI]
        public void InitializeProvider(Request request)
        {
            request.Debug("Calling '{0}::InitializeProvider'", PackageProviderName);
        }

        [PublicAPI]
        public virtual void GetFeatures(Request request)
        {
            request.Debug("Calling '{0}::GetFeatures'", PackageProviderName);
            request.Yield(new Dictionary<string, string[]>
            {
                {Constants.Features.SupportedExtensions, new[] {"xml"}},
                {Constants.Features.SupportedSchemes, new[] {"http", "https", "file"}}
            });
        }

        /// <summary>
        /// Creates a <see cref="OneGetCommand"/> instance and executes a delegate on it, handling common exception types.
        /// </summary>
        private void Do(Request request, Action<OneGetCommand> action, [CallerMemberName] string caller = null)
        {
            request.Debug("Calling '{0}::{1}'", PackageProviderName, caller);
            try
            {
                using (var command = BuildCommand(request))
                    action(command);
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (FormatException ex)
            {
                request.Error(ErrorCategory.InvalidArgument, "", ex.Message);
            }
            catch (WebException ex)
            {
                request.Error(ErrorCategory.ConnectionError, "", ex.Message);
            }
            catch (NotSupportedException ex)
            {
                request.Error(ErrorCategory.NotImplemented, "", ex.Message);
            }
            catch (IOException ex)
            {
                request.Error(ErrorCategory.OpenError, "", ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                request.Error(ErrorCategory.PermissionDenied, "", ex.Message);
            }
            catch (InvalidDataException ex)
            {
                request.Error(ErrorCategory.InvalidData, "", ex.Message);
            }
            catch (SignatureException ex)
            {
                request.Error(ErrorCategory.MetadataError, "", ex.Message);
            }
            catch (DigestMismatchException ex)
            {
                request.Error(ErrorCategory.MetadataError, "", ex.Message);
            }
            catch (SolverException ex)
            {
                request.Error(ErrorCategory.MetadataError, "", ex.Message);
            }
            catch (ExecutorException ex)
            {
                request.Error(ErrorCategory.MetadataError, "", ex.Message);
            }
            catch (ConflictException ex)
            {
                request.Error(ErrorCategory.MetadataError, "", ex.Message);
            }
            #endregion
        }

        /// <summary>
        /// Creates a <see cref="OneGetCommand"/> for a given <see cref="Request"/>.
        /// </summary>
        protected abstract OneGetCommand BuildCommand(Request request);

        [PublicAPI]
        public void GetDynamicOptions(string category, Request request)
        {
            Do(request, x => x.GetDynamicOptions(category));
        }

        [PublicAPI]
        public void ResolvePackageSources(Request request)
        {
            Do(request, x => x.ResolvePackageSources());
        }

        [PublicAPI]
        public void AddPackageSource(string name, string location, bool trusted, Request request)
        {
            if (string.IsNullOrEmpty(location))
            {
                request.Error(ErrorCategory.InvalidArgument, "location", "Location missing");
                return;
            }

            Do(request, x => x.AddPackageSource(new FeedUri(location)));
        }

        [PublicAPI]
        public void RemovePackageSource(string name, Request request)
        {
            if (string.IsNullOrEmpty(name))
            {
                request.Error(ErrorCategory.InvalidArgument, "name", "Name missing");
                return;
            }

            Do(request, x => x.RemovePackageSource(new FeedUri(name)));
        }

        [PublicAPI]
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Request request)
        {
            if (string.IsNullOrEmpty(name))
            {
                request.Error(ErrorCategory.InvalidArgument, "name", "Name missing");
                return;
            }

            Do(request, x => x.FindPackage(name,
                string.IsNullOrEmpty(requiredVersion) ? null : new ImplementationVersion(requiredVersion),
                string.IsNullOrEmpty(minimumVersion) ? null : new ImplementationVersion(minimumVersion),
                string.IsNullOrEmpty(maximumVersion) ? null : new ImplementationVersion(maximumVersion)));
        }

        [PublicAPI]
        public void FindPackageByFile(string file, int id, Request request)
        {
            if (string.IsNullOrEmpty(file))
            {
                request.Error(ErrorCategory.InvalidArgument, "file", "File missing");
                return;
            }

            Do(request, x => x.FindPackageBy(file));
        }

        [PublicAPI]
        public void FindPackageByUri(Uri uri, int id, Request request)
        {
            if (uri == null)
            {
                request.Error(ErrorCategory.InvalidArgument, "uri", "Uri missing");
                return;
            }

            Do(request, x => x.FindPackageBy(uri.OriginalString));
        }

        [PublicAPI]
        public void DownloadPackage(string fastPackageReference, string location, Request request)
        {
            Do(request, x => x.DownloadPackage(fastPackageReference, location));
        }

        [PublicAPI]
        public void InstallPackage(string fastPackageReference, Request request)
        {
            Do(request, x => x.InstallPackage(fastPackageReference));
        }

        [PublicAPI]
        public void UninstallPackage(string fastPackageReference, Request request)
        {
            Do(request, x => x.UninstallPackage(fastPackageReference));
        }

        [PublicAPI]
        public void GetInstalledPackages(string name, string requiredVersion, string minimumVersion, string maximumVersion, Request request)
        {
            Do(request, x => x.GetInstalledPackages(name));
        }

        [PublicAPI]
        public void GetPackageDetails(string fastPackageReference, Request request)
        {
            Do(request, x => x.GetPackageDetails(fastPackageReference));
        }
    }
}
