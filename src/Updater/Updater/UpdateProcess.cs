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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Threading;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Updater.Properties;

namespace ZeroInstall.Updater
{
    /// <summary>
    /// Controls the installation of an update for Zero Install (copying new files, removing old ones, etc.)
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Restart Manager is Dispose")]
    public class UpdateProcess
    {
        /// <summary>
        /// The full path to the directory containing the new/updated version.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// The full path to the directory containing the old version to be updated.
        /// </summary>
        public string Target { get; private set; }

        private bool IsPortable { get { return File.Exists(Path.Combine(Target, Locations.PortableFlagName)); } }

        /// <summary>
        /// The version number of the new/updated version.
        /// </summary>
        public Version NewVersion { get; private set; }

        /// <summary>
        /// Creates a new update process.
        /// </summary>
        /// <param name="source">The directory containing the new/updated version.</param>
        /// <param name="newVersion">The version number of the new/updated version.</param>
        /// <param name="target">The directory containing the old version to be updated.</param>
        /// <exception cref="ArgumentException">One of the directory paths or the version number is invalid.</exception>
        /// <exception cref="IOException">There was a problem accessing one of the directories.</exception>
        public UpdateProcess([NotNull] string source, [NotNull] string newVersion, [NotNull] string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(newVersion)) throw new ArgumentNullException("newVersion");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            try
            {
                Source = Path.GetFullPath(source);
                NewVersion = new Version(newVersion);
                Target = Path.GetFullPath(target);
            }
                #region Error handling
            catch (FormatException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new ArgumentException(ex.Message, ex);
            }
            catch (OverflowException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new ArgumentException(ex.Message, ex);
            }
            #endregion

            if (!Directory.Exists(Source)) throw new DirectoryNotFoundException(Resources.SourceMissing);
        }

        /// <summary>
        /// Runs the update process.
        /// </summary>
        /// <param name="reportStatus">Callback used to report the current status to the user.</param>
        /// <exception cref="UnauthorizedAccessException">Access to a resource was denied. Retrying as admin may help.</exception>
        /// <exception cref="IOException">An IO operation failed.</exception>
        /// <exception cref="InvalidOperationException">A generic error occured.</exception>
        public void Run(Action<string> reportStatus)
        {
            #region Sanity checks
            if (reportStatus == null) throw new ArgumentNullException("reportStatus");
            #endregion

            try
            {
                reportStatus(Resources.StopService);
                bool serviceWasRunning = ServiceStop();

                reportStatus(Resources.MutexWait);
                RestartManagerShutdown();
                MutexAquire();

                reportStatus(Resources.CopyFiles);
                FilesCopy();

                reportStatus(Resources.DeleteFiles);
                FilesDelete();

                reportStatus(Resources.RunNgen);
                Ngen();

                reportStatus(Resources.UpdateRegistry);
                Registry();

                reportStatus(Resources.StartService);
                MutexRelease();
                RestartManagerRestart();
                RestartManagerFinish();
                if (serviceWasRunning) ServiceStart();

                reportStatus(Resources.Done);
            }
            catch
            {
                MutexRelease();
                RestartManagerFinish();
                throw;
            }
        }

        #region Service
        /// <summary>
        /// Stops the Zero Install Store Service if it is running.
        /// </summary>
        /// <returns><see langword="true"/> if the service was running; <see langword="false"/> otherwise.</returns>
        /// <exception cref="UnauthorizedAccessException">Administrator rights are missing.</exception>
        private bool ServiceStop()
        {
            // Do not touch the service in portable mode
            if (IsPortable) return false;

            // Determine whether the service is installed and running
            var service = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == "0store-service");
            if (service == null) return false;
            if (service.Status == ServiceControllerStatus.Stopped) return false;

            // Determine whether the service is installed in the target directory we are updating
            string imagePath = RegistryUtils.GetString(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\0store-service", "ImagePath").Trim('"');
            if (!imagePath.StartsWith(Target)) return false;

            if (!WindowsUtils.IsAdministrator) throw new UnauthorizedAccessException();

            // Stop the service
            service.Stop();
            Thread.Sleep(2000);
            return true;
        }

        /// <summary>
        /// Starts the Zero Install Store Service.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Administrator rights are missing.</exception>
        /// <remarks>Must call this after <see cref="MutexRelease"/>.</remarks>
        private void ServiceStart()
        {
            if (!WindowsUtils.IsAdministrator) throw new UnauthorizedAccessException();

            // Ensure the service executable exists
            string servicePath = Path.Combine(Target, "0store-service.exe");
            if (!File.Exists(servicePath)) return;

            // Start the service
            var controller = new ServiceController("0store-service");
            controller.Start();
        }
        #endregion

        #region Mutex
        /// <summary>A mutex that prevents Zero Install instances from being launched while an update is in progress.</summary>
        private AppMutex _blockingMutexOld, _blockingMutexNew;

        /// <summary>
        /// Waits for any Zero Install instances running in <see cref="Target"/> to terminate and then prevents new ones from starting.
        /// </summary>
        private void MutexAquire()
        {
            // Installation paths are encoded into mutex names to allow instance detection
            // Support old versions that used SHA256 or MD5 for mutex names
            string targetMutexOld1 = "mutex-" + Target.Hash(SHA256.Create());
            string targetMutexOld2 = "mutex-" + Target.Hash(MD5.Create());
            string targetMutex = "mutex-" + Target.GetHashCode();

            // Wait for existing instances to terminate
            while (AppMutex.Probe(targetMutexOld1))
                Thread.Sleep(1000);
            while (AppMutex.Probe(targetMutexOld2))
                Thread.Sleep(1000);
            while (AppMutex.Probe(targetMutex))
                Thread.Sleep(1000);

            // Prevent new instances from starting
            AppMutex.Create(targetMutexOld1 + "-update", out _blockingMutexOld);
            AppMutex.Create(targetMutexOld2 + "-update", out _blockingMutexOld);
            AppMutex.Create(targetMutex + "-update", out _blockingMutexNew);

            // Detect any new instances that started in the short time between detecting existing ones and blocking new ones
            while (AppMutex.Probe(targetMutexOld1))
                Thread.Sleep(1000);
            while (AppMutex.Probe(targetMutexOld2))
                Thread.Sleep(1000);
            while (AppMutex.Probe(targetMutex))
                Thread.Sleep(1000);
        }

        /// <summary>
        /// Counterpart to <see cref="MutexAquire"/>.
        /// </summary>
        private void MutexRelease()
        {
            if (_blockingMutexOld != null) _blockingMutexOld.Close();
            if (_blockingMutexNew != null) _blockingMutexNew.Close();
        }
        #endregion

        #region Restart Manager
        [CanBeNull]
        private WindowsRestartManager _restartManager;

        /// <summary>
        /// Uses the <see cref="WindowsRestartManager"/> to shut down applications holding references to files we want to update.
        /// </summary>
        private void RestartManagerShutdown()
        {
            if (!WindowsUtils.IsWindowsVista) return;

            _restartManager = new WindowsRestartManager();
            _restartManager.RegisterResources(GetFilesToWrite());
            _restartManager.RegisterResources(GetFilesToDelete());
            _restartManager.ShutdownApps(new SilentTaskHandler());
        }

        /// <summary>
        /// Uses the <see cref="WindowsRestartManager"/> to restart applications closed by <see cref="RestartManagerShutdown"/>
        /// </summary>
        private void RestartManagerRestart()
        {
            if (_restartManager == null) return;

            _restartManager.RestartApps(new SilentTaskHandler());
        }

        /// <summary>
        /// Closes any open <see cref="WindowsRestartManager"/> sessions.
        /// </summary>
        private void RestartManagerFinish()
        {
            if (_restartManager == null) return;

            _restartManager.Dispose();
            _restartManager = null;
        }
        #endregion

        #region Files
        /// <summary>
        /// Copies the content of <see cref="Source"/> to <see cref="Target"/>.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Administrator rights are missing.</exception>
        private void FilesCopy()
        {
            try
            {
                new CopyDirectory(Source, Target, preserveDirectoryTimestamps: false, overwrite: true).Run();
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new UnauthorizedAccessException(ex.Message, ex);
            }
            #endregion
        }

        private string[] GetFilesToWrite()
        {
            var paths = new List<string>();

            var source = new DirectoryInfo(Source);
            source.Walk(fileAction: file => paths.Add(Path.Combine(Target, file.RelativeTo(source))));

            return paths.ToArray();
        }

        /// <summary>
        /// Deletes obsolete files from <see cref="Target"/>.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Administrator rights are missing.</exception>
        private void FilesDelete()
        {
            foreach (string file in GetFilesToDelete())
                File.Delete(file);
        }

        private string[] GetFilesToDelete()
        {
            var paths = new List<string>();

            if (NewVersion >= new Version("2.3.6"))
            {
                var appFiles = new[]
                {
                    "0store-win.exe", "0store-win.exe.config", Path.Combine("de", "0store-win.resources.dll"),
                    "StoreService.exe", Path.Combine("de", "StoreService.resources.dll")
                };
                paths.AddRange(appFiles.Select(x => Path.Combine(Target, x)));
            }
            if (NewVersion >= new Version("2.3.9"))
            {
                var appFiles = new[]
                {
                    "C5.dll",
                    "ZeroInstall.Backend.dll", Path.Combine("de", "ZeroInstall.Backend.resources.dll"),
                    "ZeroInstall.Fetchers.dll", Path.Combine("de", "ZeroInstall.Fetchers.resources.dll"),
                    "ZeroInstall.Solvers.dll", Path.Combine("de", "ZeroInstall.Solvers.resources.dll"),
                    "ZeroInstall.Injector.dll", Path.Combine("de", "ZeroInstall.Injecto.resourcesr.dll"),
                    "ZeroInstall.Model.dll", "ZeroInstall.Model.XmlSerializers.dll", Path.Combine("de", "ZeroInstall.Model.resources.dll")
                };
                paths.AddRange(appFiles.Select(x => Path.Combine(Target, x)));
            }
            if (NewVersion >= new Version("2.5.2"))
            {
                var appFiles = new[]
                {
                    "Common.dll", Path.Combine("de", "Common.resources.dll"),
                    "Common.WinForms.dll", Path.Combine("de", "Common.WinForms.resources.dll")
                };
                paths.AddRange(appFiles.Select(x => Path.Combine(Target, x)));
            }

            return paths.Where(File.Exists).ToArray();
        }
        #endregion

        #region Ngen
        private static readonly string[] _ngenAssemblies = {"ZeroInstall.exe", "0install.exe", "0install-win.exe", "0launch.exe", "0alias.exe", "0store.exe", "StoreService.exe", "ZeroInstall.DesktopIntegration.XmlSerializers.dll", "ZeroInstall.Store.XmlSerializers.dll", "ZeroInstall.OneGet.dll"};

        /// <summary>
        /// Runs ngen in the background to pre-compile new/updated .NET assemblies.
        /// </summary>
        private void Ngen()
        {
            if (IsPortable || !WindowsUtils.IsAdministrator) return;

            // Use .NET 4.0 if possible, otherwise 2.0
            string netFxDir = WindowsUtils.GetNetFxDirectory(
                WindowsUtils.HasNetFxVersion(WindowsUtils.NetFx40) ? WindowsUtils.NetFx40 : WindowsUtils.NetFx20);

            string ngenPath = Path.Combine(netFxDir, "ngen.exe");
            if (!File.Exists(ngenPath)) return;

            foreach (string assembly in _ngenAssemblies)
            {
                string arguments = new[] {"install", Path.Combine(Target, assembly)}.JoinEscapeArguments();
                var startInfo = new ProcessStartInfo(ngenPath, arguments) {WindowStyle = ProcessWindowStyle.Hidden};
                using (var process = startInfo.Start())
                    if (process != null) process.WaitForExit();
            }
        }
        #endregion

        #region Registry
        /// <summary>
        /// Update the registry entries.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Administrator rights are missing.</exception>
        private void Registry()
        {
            if (IsPortable) return;

            RegistryUtils.SetSoftwareString("Zero Install", "InstallLocation", Target);
            RegistryUtils.SetSoftwareString(@"Microsoft\PackageManagement", "ZeroInstall", Path.Combine(Target, "ZeroInstall.OneGet.dll"));

            RegistryInnoSetup();
        }

        /// <summary>
        /// Update the Inno Setup registry entries.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Administrator rights are missing.</exception>
        private void RegistryInnoSetup()
        {
            const string innoSetupRegKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Zero Install_is1";

            // Check if the target path is the same as the Inno Setup installation directory
            if (RegistryUtils.GetString(innoSetupRegKey, "Inno Setup: App Path") != Target) return;

            // Update the Uninstall version entry
            RegistryUtils.SetString(innoSetupRegKey, "DisplayVersion", NewVersion.ToString());
        }
        #endregion

        /// <summary>
        /// Restarts the "0install central" GUI.
        /// </summary>
        public void RestartCentral()
        {
            try
            {
                ProcessUtils.Start(Path.Combine(Target, WindowsUtils.IsWindows ? "0install-win.exe" : "0install-gtk.exe"), "central");
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Log.Error(ex);
            }
            #endregion
        }
    }
}
