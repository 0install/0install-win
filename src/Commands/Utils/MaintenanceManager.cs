// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations.Deployment;

namespace ZeroInstall.Commands.Utils
{
    /// <summary>
    /// Represents a specific Zero Install instance that is to be deployed, updated or removed.
    /// </summary>
    /// <remarks>
    /// To prevent race-conditions there may only be one maintenance class instance active at any given time.
    /// This class acquires a mutex upon calling its constructor and releases it upon calling <see cref="IDisposable.Dispose"/>.
    /// </remarks>
    public partial class MaintenanceManager : ManagerBase
    {
        #region Constants
        /// <summary>
        /// The name of the cross-process mutex used to signal that a maintenance operation is currently in progress.
        /// </summary>
        protected override string MutexName => "ZeroInstall.Commands.MaintenanceManager";

        /// <summary>
        /// The window message ID (for use with <see cref="WindowsUtils.BroadcastMessage"/>) that signals that a maintenance operation has been performed.
        /// </summary>
        public static readonly int PerformedWindowMessageID = WindowsUtils.RegisterWindowMessage("ZeroInstall.Commands.MaintenanceManager");
        #endregion

        /// <summary>
        /// The full path to the directory containing the Zero Install instance.
        /// </summary>
        [NotNull]
        public string TargetDir { get; }

        /// <summary>
        /// Controls whether the Zero Install instance at <see cref="TargetDir"/> should be a portable instance.
        /// </summary>
        public bool Portable { get; }

        /// <summary>
        /// Creates a new maintenance manager.
        /// </summary>
        /// <param name="targetDir">The full path to the directory containing the Zero Install instance.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <param name="portable">Controls whether the Zero Install instance at <paramref name="targetDir"/> should be a portable instance.</param>
        public MaintenanceManager([NotNull] string targetDir, [NotNull] ITaskHandler handler, bool machineWide, bool portable)
            : base(handler, machineWide)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(targetDir)) throw new ArgumentNullException(nameof(targetDir));
            if (machineWide && portable) throw new ArgumentException("Cannot combine portable and machineWide flags.", nameof(machineWide));
            #endregion

            TargetDir = targetDir;
            Portable = portable;

            try
            {
                AquireMutex();
            }
            catch (TimeoutException)
            {
                throw new UnauthorizedAccessException("You can only perform one maintenance operation at a time.");
            }
        }

        /// <summary>
        /// Runs the deployment process.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Access to a resource was denied.</exception>
        /// <exception cref="IOException">An IO operation failed.</exception>
        public void Deploy()
        {
            if (TargetDir == Locations.InstallBase)
                throw new IOException(string.Format(Resources.AlreadyDeployedTo, TargetDir));

            var newManifest = LoadManifest(Locations.InstallBase);
            if (newManifest == null) throw new IOException(Resources.MaintenanceMissingManifest);
            var oldManifest = LoadManifest(TargetDir) ?? LegacyManifest;

            if (WindowsUtils.IsWindows && MachineWide)
                ServiceStop();

            try
            {
                TargetMutexAquire();

                using (var clearDir = new ClearDirectory(TargetDir, oldManifest, Handler))
                using (var deployDir = new DeployDirectory(Locations.InstallBase, newManifest, TargetDir, Handler))
                {
                    deployDir.Stage();
                    clearDir.Stage();
                    if (Portable) FileUtils.Touch(Path.Combine(TargetDir, Locations.PortableFlagName));
                    deployDir.Commit();
                    clearDir.Commit();
                }

                if (!Portable)
                {
                    DesktopIntegrationApply();

                    if (WindowsUtils.IsWindows)
                    {
                        RegistryApply(newManifest.TotalSize);
                        WindowsUtils.BroadcastMessage(PerformedWindowMessageID);
                        RemoveOneGetBootstrap();
                    }
                }

                TargetMutexRelease();

                if (WindowsUtils.IsWindows && MachineWide)
                {
                    NgenApply();
                    ServiceInstall();
                    ServiceStart();
                }
            }
            catch
            {
                TargetMutexRelease();
                throw;
            }
        }

        /// <summary>
        /// Runs the removal process.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Access to a resource was denied.</exception>
        /// <exception cref="IOException">An IO operation failed.</exception>
        public void Remove()
        {
            var targetManifest = LoadManifest(TargetDir);
            if (targetManifest == null) throw new IOException(Resources.MaintenanceMissingManifest);

            if (WindowsUtils.IsWindows && MachineWide)
            {
                ServiceStop();
                ServiceUninstall();
                NgenRemove();
            }

            try
            {
                TargetMutexAquire();

                using (var clearDir = new ClearDirectory(TargetDir, targetManifest, Handler) {NoRestart = true})
                {
                    clearDir.Stage();
                    DeleteServiceLogFiles();
                    if (Portable) File.Delete(Path.Combine(TargetDir, Locations.PortableFlagName));
                    clearDir.Commit();
                }

                if (!Portable)
                {
                    if (WindowsUtils.IsWindows) RegistryRemove();
                    DesktopIntegrationRemove();
                }
            }
            finally
            {
                TargetMutexRelease();
            }
        }
    }
}
