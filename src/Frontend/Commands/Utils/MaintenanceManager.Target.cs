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

using System.IO;
using System.Threading;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Commands.Utils
{
    partial class MaintenanceManager
    {
        /// <summary>
        /// Loads the <see cref="Manifest"/> file in a directory.
        /// </summary>
        /// <param name="dirPath">The directory to check for a manifest file.</param>
        /// <returns>The loaded <see cref="Manifest"/>; <c>null</c> if no <see cref="Manifest"/> file was found.</returns>
        [CanBeNull]
        private static Manifest LoadManifest([NotNull] string dirPath)
        {
            string manifestPath = Path.Combine(dirPath, Manifest.ManifestFile);
            if (!File.Exists(manifestPath)) return null;

            return Manifest.Load(
                Path.Combine(dirPath, Manifest.ManifestFile),
                ManifestFormat.Sha256New); // The digests are not checked so the format does not matter
        }

        /// <summary>
        /// Provides a fake <see cref="Manifest"/> listing the files usually present in older deployments.
        /// </summary>
        [NotNull]
        private Manifest LegacyManifest
        {
            get
            {
                return Manifest.Load(
                    typeof(MaintenanceManager).GetEmbeddedString("legacy.manifest"),
                    ManifestFormat.Sha256New); // The digests are not checked so the format does not matter
            }
        }

        /// <summary>A mutex that prevents Zero Install instances from being launched while an update is in progress.</summary>
        [CanBeNull]
        private AppMutex _targetMutex;

        /// <summary>
        /// Waits for any Zero Install instances running in <see cref="TargetDir"/> to terminate and then prevents new ones from starting.
        /// </summary>
        /// <remarks>The <see cref="TargetDir"/> is encoded into an <see cref="AppMutex"/> name using <see cref="object.GetHashCode"/>.</remarks>
        private void TargetMutexAquire()
        {
            if (TargetDir == Locations.InstallBase)
            {
                Log.Info("Cannot use Mutex because source and target directory are the same: " + TargetDir);
                return;
            }

            int hashCode = TargetDir.GetHashCode();
            if (hashCode == Locations.InstallBase.GetHashCode())
            { // Very unlikely but possible, since .GetHashCode() is not a cryptographic hash
                Log.Warn("Hash collision between " + TargetDir + " and " + Locations.InstallBase + "! Not using Mutex.");
                return;
            }
            string targetMutex = "mutex-" + hashCode;

            Handler.RunTask(new SimpleTask(Resources.MutexWait, () =>
            {
                // Wait for existing instances to terminate
                while (AppMutex.Probe(targetMutex))
                    Thread.Sleep(1000);

                // Prevent new instances from starting
                AppMutex.Create(targetMutex + "-update", out _targetMutex);

                // Detect any new instances that started in the short time between detecting existing ones and blocking new ones
                while (AppMutex.Probe(targetMutex))
                    Thread.Sleep(1000);
            }));
        }

        /// <summary>
        /// Counterpart to <see cref="TargetMutexAquire"/>.
        /// </summary>
        private void TargetMutexRelease()
        {
            if (_targetMutex != null) _targetMutex.Close();
        }

        private void DeleteObsoleteInstallerFiles()
        {
            File.Delete(Path.Combine(TargetDir, "unins000.exe"));
            File.Delete(Path.Combine(TargetDir, "unins000.dat"));
            File.Delete(Path.Combine(TargetDir, FlagUtils.XbitFile));
        }
    }
}
