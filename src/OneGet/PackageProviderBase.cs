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
using System.Collections.Generic;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Info;
using PackageManagement.Sdk;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// Common base for OneGet package providers. Implements OneGet's duck-typing interface.
    /// </summary>
    public abstract class PackageProviderBase
    {
        [PublicAPI]
        public abstract string PackageProviderName { get; }

        [PublicAPI]
        public string ProviderVersion => AppInfo.CurrentLibrary.Version.ToString();

        [PublicAPI]
        public void OnUnhandledException(string methodName, Exception exception)
        {
            Log.Error("Unexpected exception thrown in " + PackageProviderName + "::" + methodName);
            Log.Error(exception);
        }

        [PublicAPI]
        public void InitializeProvider(Request request) { request.Debug("Calling '{0}::InitializeProvider'", PackageProviderName); }

        [PublicAPI]
        public void GetFeatures(Request request)
        {
            request.Debug("Calling '{0}::GetFeatures'", PackageProviderName);
            request.Yield(new Dictionary<string, string[]>
            {
                [Constants.Features.SupportedExtensions] = new[] {"xml"},
                [Constants.Features.SupportedSchemes] = new[] {"http", "https", "file"}
            });
        }

        [PublicAPI]
        public void GetDynamicOptions(string category, Request request)
        {
            request.Debug("Calling '{0}::GetDynamicOptions'", PackageProviderName);
            switch ((category ?? string.Empty).ToLowerInvariant())
            {
                case "package":
                    request.YieldDynamicOption("Refresh", Constants.OptionType.Switch, isRequired: false);
                    request.YieldDynamicOption("AllVersions", Constants.OptionType.Switch, isRequired: false);
                    request.YieldDynamicOption("GlobalSearch", Constants.OptionType.Switch, isRequired: false);
                    break;

                case "install":
                    request.YieldDynamicOption("Refresh", Constants.OptionType.Switch, isRequired: false);
                    request.YieldDynamicOption("DeferDownload", Constants.OptionType.Switch, isRequired: false);
                    request.YieldDynamicOption("Scope", Constants.OptionType.String, isRequired: false, permittedValues: new[] {"CurrentUser", "AllUsers"});
                    break;

                case "source":
                    break;
            }
        }

        /// <summary>
        /// Creates a <see cref="IOneGetContext"/> instance and executes a delegate on it, handling common exception types.
        /// </summary>
        private void Do(Request request, Action<IOneGetContext> action)
        {
            try
            {
                using (var context = BuildContext(request))
                    action(context);
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
            catch (Exception ex)
            {
                request.Error(ErrorCategory.MetadataError, "", ex.Message);
            }
            #endregion
        }

        /// <summary>
        /// Creates a <see cref="IOneGetContext"/> for a given <see cref="Request"/>.
        /// </summary>
        [NotNull]
        protected abstract IOneGetContext BuildContext(Request request);

        [PublicAPI]
        public void ResolvePackageSources(Request request)
        {
            request.Debug("Calling '{0}::ResolvePackageSources'", PackageProviderName);
            Do(request, x => x.ResolvePackageSources());
        }

        [PublicAPI]
        public void AddPackageSource(string name, string location, bool trusted, Request request)
        {
            request.Debug("Calling '{0}::AddPackageSource'", PackageProviderName);
            if (string.IsNullOrEmpty(location))
            {
                request.Error(ErrorCategory.InvalidArgument, "location", "Location parameter missing");
                return;
            }
            Do(request, x => x.AddPackageSource(location));
        }

        [PublicAPI]
        public void RemovePackageSource(string name, Request request)
        {
            request.Debug("Calling '{0}::RemovePackageSource'", PackageProviderName);
            if (string.IsNullOrEmpty(name))
            {
                request.Error(ErrorCategory.InvalidArgument, "name", "Name parameter missing");
                return;
            }

            Do(request, x => x.RemovePackageSource(name));
        }

        [PublicAPI]
        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Request request)
        {
            request.Debug("Calling '{0}::FindPackage'", PackageProviderName);
            Do(request, x => x.FindPackage(name, requiredVersion, minimumVersion, maximumVersion));
        }

        [PublicAPI]
        public void FindPackageByFile(string file, int id, Request request)
        {
            request.Debug("Calling '{0}::FindPackageByFile'", PackageProviderName);
            if (string.IsNullOrEmpty(file))
            {
                request.Error(ErrorCategory.InvalidArgument, "file", "File parameter missing");
                return;
            }
            Do(request, x => x.FindPackageBy(file));
        }

        [PublicAPI]
        public void FindPackageByUri(Uri uri, int id, Request request)
        {
            request.Debug("Calling '{0}::FindPackageByUri'", PackageProviderName);
            if (uri == null)
            {
                request.Error(ErrorCategory.InvalidArgument, "uri", "Uri parameter missing");
                return;
            }
            Do(request, x => x.FindPackageBy(uri.OriginalString));
        }

        [PublicAPI]
        public void DownloadPackage(string fastPackageReference, string location, Request request)
        {
            request.Debug("Calling '{0}::DownloadPackage'", PackageProviderName);
            if (string.IsNullOrEmpty(location))
            {
                request.Error(ErrorCategory.InvalidArgument, "location", "Location parameter missing");
                return;
            }
            Do(request, x => x.DownloadPackage(fastPackageReference, location));
        }

        [PublicAPI]
        public void InstallPackage(string fastPackageReference, Request request)
        {
            request.Debug("Calling '{0}::InstallPackage'", PackageProviderName);
            Do(request, x => x.InstallPackage(fastPackageReference));
        }

        [PublicAPI]
        public void UninstallPackage(string fastPackageReference, Request request)
        {
            request.Debug("Calling '{0}::UninstallPackage'", PackageProviderName);
            Do(request, x => x.UninstallPackage(fastPackageReference));
        }

        [PublicAPI]
        public void GetInstalledPackages(string name, string requiredVersion, string minimumVersion, string maximumVersion, Request request)
        {
            request.Debug("Calling '{0}::GetInstalledPackages'", PackageProviderName);
            Do(request, x => x.GetInstalledPackages(name));
        }

        [PublicAPI]
        public void GetPackageDetails(string fastPackageReference, Request request)
        {
            request.Debug("Calling '{0}::GetPackageDetails'", PackageProviderName);
            Do(request, x => x.GetPackageDetails(fastPackageReference));
        }
    }
}
