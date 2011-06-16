/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;
using System.Security;
using System.Threading;
using Common.Utils;
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
        private AppMutex _blockingMutex;
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
        /// <exception cref="NotSupportedException">Thrown if one of the directory paths is invalid.</exception>
        public UpdateProcess(string source, string newVersion, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(newVersion)) throw new ArgumentNullException("newVersion");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            Source = Path.GetFullPath(source);
            try { NewVersion = new Version(newVersion); }
            #region Error handling
            catch (ArgumentException)
            {}
            catch (FormatException)
            {}
            catch (OverflowException)
            {}
            #endregion
            Target = Path.GetFullPath(target);

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
            string targetMutex = AppMutex.GenerateName(Target);
            while (AppMutex.Probe(targetMutex))
                Thread.Sleep(1000);
            AppMutex.Create(targetMutex + "-update", out _blockingMutex);
            while (AppMutex.Probe(targetMutex))
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
                    File.Delete(file);
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

        #region Run Ngen
        private static readonly string[] _ngenAssemblies = new[] {"ZeroInstall.exe", "0install.exe", "0install-win.exe", "0launch.exe", "0store.exe", "0store-win.exe", "StoreService.exe"};

        /// <summary>
        /// Runs ngen in the background to pre-compile new/updated .NET assemblies.
        /// </summary>
        public void RunNgen()
        {
            foreach (string assembly in _ngenAssemblies)
            {
                var startInfo = new ProcessStartInfo(
                    Path.Combine(Environment.GetEnvironmentVariable("windir") ?? @"C:\Windows", @"Microsoft.NET\Framework\v2.0.50727\ngen.exe"),
                    "install " + Path.Combine(Target, assembly) + "/queue") {WindowStyle = ProcessWindowStyle.Hidden};
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
                // Check if the target path is a Inno Setup installation
                using (var innoSetupKeyRead = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Zero Install_is1", false))
                {
                    if (innoSetupKeyRead == null) return; // Don't continue if there is no Inno Setup installation
                    string appPath = (innoSetupKeyRead.GetValue("Inno Setup: App Path") ?? "").ToString();
                    if (Path.GetFullPath(appPath) != Target) return; // Don't continue if this is an archive extracted in parallel to the Inno Setup installation
                }

                // Update the Uninstall version entry
                using (var innoSetupKeyWrite = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Zero Install_is1", true))
                {
                    if (innoSetupKeyWrite != null) innoSetupKeyWrite.SetValue("DisplayVersion", NewVersion);
                }
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
            _blockingMutex.Close();
        }
        #endregion
    }
}
