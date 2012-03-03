/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
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
        public void MutexWait()
        {
            // Installation paths are encoded into mutex names to allow instance detection
            // Support old versions that used SHA256 for mutex names (unnecessarily complex since not security-relevant)
            string targetMutexOld = "mutex-" + StringUtils.Hash(Target, SHA256.Create());
            string targetMutexNew = "mutex-" + StringUtils.Hash(Target, MD5.Create());

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
        #endregion

        #region Delete files
        /// <summary>
        /// Deletes obsolete files from <see cref="Target"/>.
        /// </summary>
        public void DeleteFiles()
        {
            if (NewVersion >= new Version("0.54.4"))
            {
                foreach (string file in new[] {"ZeroInstall.MyApps.dll", Path.Combine("de", "ZeroInstall.MyApps.resources.dll")})
                    if (File.Exists(file)) File.Delete(file);
            }
        }
        #endregion

        #region Copy files
        /// <summary>
        /// Copies the content of <see cref="Source"/> to <see cref="Target"/>.
        /// </summary>
        public void CopyFiles()
        {
            FileUtils.CopyDirectory(Source, Target, false, true);
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
                    #region Error handling
                catch (SecurityException ex)
                {
                    // Wrap exception since only certain exception types are allowed in tasks
                    throw new UnauthorizedAccessException(ex.Message, ex);
                }
                #endregion
            }
        }
        #endregion

        #region Run Ngen
        private static readonly string[] _ngenAssemblies = new[] {"ZeroInstall.exe", "0install.exe", "0install-win.exe", "0launch.exe", "0alias.exe", "0store.exe", "0store-win.exe", "StoreService.exe", "ZeroInstall.Model.XmlSerializers.dll", "ZeroInstall.Injector.XmlSerializers.dll", "ZeroInstall.DesktopIntegration.XmlSerializers.dll"};

        /// <summary>
        /// Runs ngen in the background to pre-compile new/updated .NET assemblies.
        /// </summary>
        public void RunNgen()
        {
            string fileName = FileUtils.PathCombine(
                (Environment.GetEnvironmentVariable("windir") ?? @"C:\Windows"),
                "Microsoft.NET", (WindowsUtils.Is64BitOperatingSystem ? "Framework64" : "Framework"),
                "v2.0.50727", "ngen.exe");

            foreach (string assembly in _ngenAssemblies)
            {
                string arguments = StringUtils.ConcatenateEscapeArgument(new[] {"install", Path.Combine(Target, assembly), "/queue"});
                var startInfo = new ProcessStartInfo(fileName, arguments) {WindowStyle = ProcessWindowStyle.Hidden};
                Process.Start(startInfo).WaitForExit();
            }
        }
        #endregion

        #region Update registry
        /// <summary>
        /// Update the registry entries.
        /// </summary>
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

        #region Done
        /// <summary>
        /// Finishes the update process. Counterpart to <see cref="MutexWait"/>.
        /// </summary>
        public void Done()
        {
            _blockingMutexOld.Close();
            _blockingMutexNew.Close();
        }
        #endregion
    }
}
