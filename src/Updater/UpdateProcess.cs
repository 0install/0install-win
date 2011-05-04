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
        #region Properties
        /// <summary>
        /// The full path to the directory containing the new/updated version.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// The full path to the directory containing the old version to be updated.
        /// </summary>
        public string Target { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new update process.
        /// </summary>
        /// <param name="source">The directory containing the new/updated version.</param>
        /// <param name="target">The directory containing the old version to be updated.</param>
        /// <exception cref="IOException">Thrown if there was a problem accessing one of the directories.</exception>
        /// <exception cref="NotSupportedException">Thrown if one of the directory paths is invalid.</exception>
        public UpdateProcess(string source, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            Source = Path.GetFullPath(source);
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
            AppMutex.Create(targetMutex + "-update");
            while (AppMutex.Probe(targetMutex))
                Thread.Sleep(1000);
        }
        #endregion

        #region Delete files
        private static readonly string[] _obsoleteFiles = new string[] {};

        /// <summary>
        /// Deletes obsolete files from <see cref="Target"/>.
        /// </summary>
        public void DeleteFiles()
        {
            foreach(string filePattern in _obsoleteFiles)
            {
                foreach (string file in Directory.GetFiles(Target, filePattern))
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
            // ToDo: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Zero Install_is1\DisplayVersion
        }
        #endregion
    }
}
