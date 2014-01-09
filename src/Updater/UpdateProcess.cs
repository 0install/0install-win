/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Security;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using Common;
using Common.Storage;
using Common.Utils;
using Microsoft.Win32;
using ZeroInstall.Updater.Properties;

namespace ZeroInstall.Updater
{
    /// <summary>
    /// Controls the installation of an update for Zero Install (copying new files, removing old ones, etc.)
    /// </summary>
    public class UpdateProcess
    {
        #region Variables
        /// <summary>A mutex that prevents Zero Install instances from being launched while an update is in progress.</summary>
        private AppMutex _blockingMutexOld, _blockingMutexNew;
        #endregion

        #region Properties
        /// <summary>
        /// The full path to the directory containing the new/updated version.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// The full path to the directory containing the old version to be updated.
        /// </summary>
        public string Target { get; private set; }

        /// <summary>
        /// The version number of the new/updated version.
        /// </summary>
        public Version NewVersion { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new update process.
        /// </summary>
        /// <param name="source">The directory containing the new/updated version.</param>
        /// <param name="newVersion">The version number of the new/updated version.</param>
        /// <param name="target">The directory containing the old version to be updated.</param>
        /// <exception cref="IOException">Thrown if there was a problem accessing one of the directories.</exception>
        /// <exception cref="NotSupportedException">Thrown if one of the directory paths or the version number is invalid.</exception>
        public UpdateProcess(string source, string newVersion, string target)
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
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new NotSupportedException(ex.Message, ex);
            }
            catch (FormatException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new NotSupportedException(ex.Message, ex);
            }
            catch (OverflowException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new NotSupportedException(ex.Message, ex);
            }
            #endregion

            if (!Directory.Exists(Source)) throw new DirectoryNotFoundException(Resources.SourceMissing);
        }
        #endregion

        //--------------------//

        #region Mutex wait
        /// <summary>
        /// Waits for any Zero Install instances running in <see cref="Target"/> to terminate and then prevents new ones from starting.
        /// </summary>
        public void MutexAquire()
        {
            // Installation paths are encoded into mutex names to allow instance detection
            // Support old versions that used SHA256 for mutex names (unnecessarily complex since not security-relevant)
            string targetMutexOld = "mutex-" + Target.Hash(SHA256.Create());
            string targetMutexNew = "mutex-" + Target.Hash(MD5.Create());

            // Wait for existing instances to terminate
            while (AppMutex.Probe(targetMutexOld))
                Thread.Sleep(1000);
            while (AppMutex.Probe(targetMutexNew))
                Thread.Sleep(1000);

            // Prevent new instances from starting
            AppMutex.Create(targetMutexOld + "-update", out _blockingMutexOld);
            AppMutex.Create(targetMutexNew + "-update", out _blockingMutexNew);

            // Detect any new instances that started in the short time between detecting existing ones and blocking new ones
            while (AppMutex.Probe(targetMutexOld))
                Thread.Sleep(1000);
            while (AppMutex.Probe(targetMutexNew))
                Thread.Sleep(1000);
        }

        /// <summary>
        /// Counterpart to <see cref="MutexAquire"/>.
        /// </summary>
        public void MutexRelease()
        {
            _blockingMutexOld.Close();
            _blockingMutexNew.Close();
        }
        #endregion

        #region Stop service
        /// <summary>
        /// Stops the Zero Install Store Service if it is running.
        /// </summary>
        /// <returns><see langword="true"/> if the service was running; <see langword="false"/> otherwise.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if administrator rights are missing.</exception>
        public bool StopService()
        {
            // Do not touch the service in portable mode
            if (File.Exists(Path.Combine(Target, "_portable"))) return false;

            // Determine whether the service is installed and running
            var controller = ServiceController.GetServices().FirstOrDefault(service => service.ServiceName == "0store-service");
            if (controller == null) return false;
            if (controller.Status == ServiceControllerStatus.Stopped) return false;

            if (!WindowsUtils.IsAdministrator) throw new UnauthorizedAccessException();

            // Stop the service
            controller.Stop();
            Thread.Sleep(2000);
            return true;
        }
        #endregion

        #region Copy files
        /// <summary>
        /// Copies the content of <see cref="Source"/> to <see cref="Target"/>.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown if administrator rights are missing.</exception>
        public void CopyFiles()
        {
            FileUtils.CopyDirectory(Source, Target, preserveDirectoryModificationTime: false, overwrite: true);
        }
        #endregion

        #region Delete files
        /// <summary>
        /// Deletes obsolete files from <see cref="Target"/>.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown if administrator rights are missing.</exception>
        public void DeleteFiles()
        {
            var filesToDelete = new List<string>();

            string userProgams = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string commonPrograms = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "Common Programs", "").ToString();

            if (NewVersion >= new Version("2.3.6"))
            {
                var appFiles = new[]
                {
                    "0store-win.exe", "0store-win.exe.config", Path.Combine("de", "0store-win.resources.dll"),
                    "StoreService.exe", Path.Combine("de", "StoreService.resources.dll")
                };
                filesToDelete.AddRange(appFiles.Select(x => Path.Combine(Target, x)));

                var shortcuts = new[] {"Cache management.lnk", "Cache Verwaltung.lnk"}.Select(x => Path.Combine("Zero Install", x)).ToArray();
                filesToDelete.AddRange(shortcuts.Select(x => Path.Combine(userProgams, x)));
                filesToDelete.AddRange(shortcuts.Select(x => Path.Combine(commonPrograms, x)));
            }

            foreach (string file in filesToDelete.Where(File.Exists))
                File.Delete(file);
        }
        #endregion

        #region Inno Setup
        /// <summary>The registry key Inno Setup uses to store uninstall information for Zero Install.</summary>
        private const string InnoSetupRegKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Zero Install_is1";

        /// <summary>
        /// Indicates whether the <see cref="Target"/> was installed with Inno Setup.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Registry exceptions are passed through.")]
        public bool IsInnoSetup
        {
            get
            {
                try
                {
                    // Check if the target path is the same as the Inno Setup installation directory
                    var installationDirectory = Registry.GetValue(InnoSetupRegKey, "Inno Setup: App Path", "") as string;
                    return (installationDirectory == Target);
                }
                catch (SecurityException)
                {
                    return false;
                }
            }
        }
        #endregion

        #region Run Ngen
        private static readonly string[] _ngenAssemblies = {"ZeroInstall.exe", "0install.exe", "0install-win.exe", "0launch.exe", "0alias.exe", "0store.exe", "StoreService.exe", "ZeroInstall.Model.XmlSerializers.dll", "ZeroInstall.DesktopIntegration.XmlSerializers.dll", "ZeroInstall.Store.XmlSerializers.dll"};

        /// <summary>
        /// Runs ngen in the background to pre-compile new/updated .NET assemblies.
        /// </summary>
        public void RunNgen()
        {
            // Use .NET 4.0 if possible, otherwise 2.0
            string netFxDir = WindowsUtils.GetNetFxDirectory(
                WindowsUtils.HasNetFxVersion(WindowsUtils.NetFx40) ? WindowsUtils.NetFx40 : WindowsUtils.NetFx20);

            string ngenPath = Path.Combine(netFxDir, "ngen.exe");
            if (!File.Exists(ngenPath)) return;

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (string assembly in _ngenAssemblies)
            {
                string arguments = new[] {"install", Path.Combine(Target, assembly), "/queue"}.JoinEscapeArguments();
                var startInfo = new ProcessStartInfo(ngenPath, arguments) {WindowStyle = ProcessWindowStyle.Hidden};
                using (var process = Process.Start(startInfo))
                    process.WaitForExit();
            }
        }
        #endregion

        #region Update registry
        /// <summary>
        /// Update the registry entries.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown if administrator rights are missing.</exception>
        public void UpdateRegistry()
        {
            try
            {
                // Update the Uninstall version entry
                Registry.SetValue(InnoSetupRegKey, "DisplayVersion", NewVersion, RegistryValueKind.String);

                // Store installation location in registry to allow other applications or bootstrappers to locate Zero Install
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Zero Install", "InstallLocation", Target, RegistryValueKind.String);
                if (WindowsUtils.Is64BitProcess) Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Zero Install", "InstallLocation", Target, RegistryValueKind.String);
            }
                #region Error handling
            catch (SecurityException ex)
            {
                // Wrap exception since only certain exception types are allowed in tasks
                throw new UnauthorizedAccessException(ex.Message, ex);
            }
            #endregion
        }
        #endregion

        #region Fix permissions
        /// <summary>
        /// Fixes NTFS ACLs (permissions) for shared directories.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown if administrator rights are missing.</exception>
        public void FixPermissions()
        {
            // Do not touch ACLs in portable mode
            if (File.Exists(Path.Combine(Target, "_portable"))) return;

            var directory = new DirectoryInfo(Path.Combine(Locations.SystemCacheDir, "0install.net"));
            if (directory.Exists)
            {
                // Only reset ACLs if directory is not already under admin control
                var owner = directory.GetAccessControl().GetOwner(typeof(SecurityIdentifier));
                if (!owner.Equals(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null)))
                {
                    if (!WindowsUtils.IsAdministrator) throw new UnauthorizedAccessException();

                    directory.ResetAcl();
                    directory.SetAccessControl(Locations.SecureSharedAcl);
                }
            }
        }
        #endregion

        #region Start service
        /// <summary>
        /// Starts the Zero Install Store Service.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown if administrator rights are missing.</exception>
        /// <remarks>Must call this after <see cref="MutexRelease"/>.</remarks>
        public void StartService()
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
    }
}
