// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Implementations.Manifests;

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
        private static Manifest LegacyManifest
        {
            get
            {
                using (var stream = typeof(MaintenanceManager).GetEmbeddedStream("legacy.manifest"))
                    return Manifest.Load(stream, ManifestFormat.Sha256New); // The digests are not checked so the format does not matter
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
                _targetMutex = AppMutex.Create(targetMutex + "-update");

                // Detect any new instances that started in the short time between detecting existing ones and blocking new ones
                while (AppMutex.Probe(targetMutex))
                    Thread.Sleep(1000);
            }));
        }

        /// <summary>
        /// Counterpart to <see cref="TargetMutexAquire"/>.
        /// </summary>
        private void TargetMutexRelease() => _targetMutex?.Close();

        /// <summary>
        /// Try to remove OneGet Bootstrap module to prevent future PowerShell sessions from loading it again.
        /// </summary>
        private void RemoveOneGetBootstrap()
        {
            RemoveOneGetBootstrap(FileUtils.PathCombine(
                Environment.GetFolderPath(MachineWide ? Environment.SpecialFolder.ProgramFiles : Environment.SpecialFolder.MyDocuments),
                "PackageManagement", "ProviderAssemblies", "0install"));
            RemoveOneGetBootstrap(FileUtils.PathCombine(
                Environment.GetFolderPath(MachineWide ? Environment.SpecialFolder.ProgramFiles : Environment.SpecialFolder.LocalApplicationData),
                "WindowsPowerShell", "Modules", "0install"));
        }

        private static void RemoveOneGetBootstrap(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return;

            try
            {
                foreach (string subDirPath in Directory.GetDirectories(dirPath))
                {
                    foreach (string filePath in Directory.GetFiles(subDirPath))
                    {
                        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                        Log.Debug($"Trying to move '{filePath}' to '{tempPath}' to prevent future PowerShell sessions from loading it again");
                        File.Move(filePath, tempPath);
                    }
                    Directory.Delete(subDirPath);
                }
                Directory.Delete(dirPath);
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
    }
}
