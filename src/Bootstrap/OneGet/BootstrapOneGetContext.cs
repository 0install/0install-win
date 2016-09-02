using System;
using System.IO;
using NanoByte.Common;
using NanoByte.Common.Storage;
using PackageManagement.Sdk;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// Pretends to handle OneGet <see cref="Request"/>s but instead bootstraps Zero Install (which contains the real package provider).
    /// </summary>
    public class BootstrapOneGetContext : IOneGetContext
    {
        private readonly Request _request;
        private readonly OneGetHandler _handler;

        public BootstrapOneGetContext(Request request)
        {
            _request = request;
            _handler = new OneGetHandler(request);
        }

        public void Dispose()
        {
            _handler.Dispose();
        }

        public void AddPackageSource(string uri)
        {
            Deploy();
        }

        public void RemovePackageSource(string uri)
        {
            Deploy();
        }

        public void ResolvePackageSources()
        {}

        public void FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion)
        {
            Deploy();
        }

        public void FindPackageBy(string identifier)
        {
            Deploy();
        }

        private static bool _deployCompleted;

        private bool MachineWide
        {
            get
            {
                switch (_request.GetOptionValue("Scope"))
                {
                    case "CurrentUser":
                        return false;
                    case "AllUsers":
                        return true;
                    default:
                        // Chose default based on instal location of bootstrap provider
                        return !Locations.InstallBase.StartsWith(Locations.HomeDir);
                }
            }
        }

        /// <summary>
        /// Deploys Zero Install to the computer.
        /// </summary>
        private void Deploy()
        {
            const string openNewSessionMessage = "Please open a new PowerShell session to start using the '0install' package provider.";
            if (_deployCompleted)
            {
                _request.Warning(openNewSessionMessage);
                return;
            }

            var process = new BootstrapProcess(_handler, gui: false);
            var result = process.Execute(MachineWide
                ? new[] {"maintenance", "deploy", "--batch", "--machine"}
                : new[] {"maintenance", "deploy", "--batch"});

            if (result == ExitCode.OK)
            {
                _deployCompleted = true;
                _request.Warning(openNewSessionMessage);

                // Try to move Bootstrap provider after deployment to prevent future PowerShell sessions from loading it again
                try
                {
                    File.Move(Program.ExePath, FileUtils.GetTempFile("0install"));
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
            else Log.Warn("Installed failed with exit code " + result + ".");
        }

        public void GetInstalledPackages(string name)
        {}

        public void DownloadPackage(string fastPackageReference, string location)
        {
            throw new NotImplementedException();
        }

        public void InstallPackage(string fastPackageReference)
        {
            throw new NotImplementedException();
        }

        public void UninstallPackage(string fastPackageReference)
        {
            throw new NotImplementedException();
        }

        public void GetPackageDetails(string fastPackageReference)
        {
            throw new NotImplementedException();
        }
    }
}
