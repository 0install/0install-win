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
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using LinFu.DynamicProxy;
using NanoByte.Common;
using NanoByte.Common.Storage;
using PackageManagement.Sdk;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// Forwards requests to an <see cref="IOneGetContext"/> loaded from a deployed Zero Install instance. Deploys an instance if there is none.
    /// </summary>
    public class OneGetContextInterceptor : IInterceptor
    {
        private readonly Request _request;

        public OneGetContextInterceptor([NotNull] Request request)
        {
            #region Sanity checks
            if (request == null) throw new ArgumentNullException(nameof(request));
            #endregion

            _request = request;
        }

        [CanBeNull]
        private object _context;

        // Prevent multiple Zero Install deployments being started in parallel
        private static readonly object _initLock = new object();

        public object Intercept(InvocationInfo info)
        {
            lock (_initLock)
            {
                if (_context != null) return _context;
                _context = InitExternalContext();
            }

            _request.Debug("Forwarding to deployed Zero Install instance: {0}", info.TargetMethod.Name);
            return _context.DuckType(info);
        }

        /// <summary>
        /// Initializes an <see cref="IOneGetContext"/> loaded from a deployed Zero Install instance. Deploys an instance if there is none.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to deploy Zero Install.</exception>
        [NotNull]
        private object InitExternalContext()
        {
            string providerPath = GetDeployedProviderPath();
            if (providerPath == null) Deploy();
            providerPath = GetDeployedProviderPath();
            if (providerPath == null) throw new InvalidOperationException("Unable to find deployed Zero Install instance.");

            _request.Debug("Building OneGet context from {0}", providerPath);
            var assembly = Assembly.LoadFrom(providerPath);
            var requestType = assembly.GetType("PackageManagement.Sdk.Request", throwOnError: true);
            object requestProxy = new ProxyFactory().CreateProxy(requestType, new RequestInterceptor(_request));
            var contextType = assembly.GetType("ZeroInstall.OneGet.OneGetContext", throwOnError: true);
            return Activator.CreateInstance(contextType, requestProxy);
        }

        /// <summary>
        /// Trys to get the path to a Zero Install OneGet Provider DLL deployed on this machine.
        /// </summary>
        /// <returns>A full path or <c>null</c> if no deployed provider was found.</returns>
        [CanBeNull]
        private static string GetDeployedProviderPath()
        {
            string installLocation = RegistryUtils.GetSoftwareString("Zero Install", "InstallLocation");
            if (string.IsNullOrEmpty(installLocation)) return null;

            string providerDll = Path.Combine(installLocation, "ZeroInstall.OneGet.dll");
            if (!File.Exists(providerDll)) return null;

            return providerDll;
        }

        /// <summary>
        /// Deploys Zero Install to this machine.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to deploy Zero Install.</exception>
        private void Deploy()
        {
            using (var handler = new OneGetHandler(_request))
            {
                Log.Info("Deploying Zero Install");

                var process = new BootstrapProcess(handler, gui: false);
                var result = process.Execute(Locations.InstallBase.StartsWith(Locations.HomeDir)
                    ? new[] {"--no-existing", "maintenance", "deploy", "--batch"}
                    : new[] {"--no-existing", "maintenance", "deploy", "--batch", "--machine"});

                if (result == ExitCode.OK)
                {
                    // Try to move Bootstrap provider after deployment to prevent future PowerShell sessions from loading it again
                    try
                    {
                        Log.Debug("Trying to remove boostrap provider assembly after deployment");
                        File.Move(Program.ExePath, Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                    }
                        #region Error handling
                    catch (IOException ex)
                    {
                        Log.Debug(ex);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log.Debug(ex);
                    }
                    #endregion
                }
                else throw new InvalidOperationException("Zero Install deployment failed with exit code " + result + ".");
            }
        }

        /// <summary>
        /// Duck-types from <see cref="Request"/> to an external type with the same method signatures.
        /// </summary>
        private class RequestInterceptor : IInterceptor
        {
            private readonly Request _request;

            public RequestInterceptor([NotNull] Request request)
            {
                _request = request;
            }

            public object Intercept(InvocationInfo info) => _request.DuckType(info);
        }
    }
}