// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands.Utils
{
    partial class MaintenanceManager
    {
        private static readonly string _runtimeDir = WindowsUtils.GetNetFxDirectory(
            (Environment.Version.Major == 4) ? WindowsUtils.NetFx40 : WindowsUtils.NetFx20);

        private const string ServiceName = "0store-service";

        /// <summary>
        /// Stops the Zero Install Store Service if it is running.
        /// </summary>
        private void ServiceStop()
        {
            // Determine whether the service is installed and running
            var service = GetServiceController();
            if (service?.Status != ServiceControllerStatus.Running) return;

            // Determine whether the service is installed in the target directory we are updating
            string imagePath = RegistryUtils.GetString(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\" + ServiceName, "ImagePath").Trim('"');
            if (!imagePath.StartsWith(TargetDir)) return;

            Handler.RunTask(new SimpleTask(Resources.StopService, () =>
            {
                try
                {
                    service.Stop();
                }
                #region Error handling
                catch (InvalidOperationException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new IOException("Failed to stop service.", ex);
                }
                catch (Win32Exception ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new IOException("Failed to stop service.", ex);
                }
                #endregion

                Thread.Sleep(2000);
            }));
        }

        /// <summary>
        /// Starts the Zero Install Store Service.
        /// </summary>
        /// <remarks>Must be called after <see cref="TargetMutexRelease"/>.</remarks>
        private void ServiceStart() => Handler.RunTask(new SimpleTask(Resources.StartService, () =>
        {
            try
            {
                GetServiceController()?.Start();
            }
            #region Error handling
            catch (InvalidOperationException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException("Failed to start service.", ex);
            }
            catch (Win32Exception ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException("Failed to start service.", ex);
            }
            #endregion
        }));

        [CanBeNull]
        private static ServiceController GetServiceController()
            => ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == ServiceName);

        private static readonly string _installUtilExe = Path.Combine(_runtimeDir, "InstallUtil.exe");
        private string ServiceExe => Path.Combine(TargetDir, ServiceName + ".exe");

        /// <summary>
        /// Installs the Zero Install Store Service.
        /// </summary>
        private void ServiceInstall() => Handler.RunTask(new SimpleTask(Resources.InstallService, () =>
            new ProcessStartInfo(_installUtilExe, ServiceExe.EscapeArgument())
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetTempPath()
            }.Run()));

        /// <summary>
        /// Uninstalls the Zero Install Store Service.
        /// </summary>
        private void ServiceUninstall() => Handler.RunTask(new SimpleTask(Resources.UninstallService, () =>
            new ProcessStartInfo(_installUtilExe, new[] {"/u", ServiceExe}.JoinEscapeArguments())
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetTempPath()
            }.Run()));

        private void DeleteServiceLogFiles()
        {
            File.Delete(Path.Combine(TargetDir, "0store-service.InstallLog"));
            File.Delete(Path.Combine(TargetDir, "InstallUtil.InstallLog"));
        }

        private static readonly string _ngenExe = Path.Combine(_runtimeDir, "ngen.exe");

        private static readonly string[] _ngenAssemblies =
        {
            "0install.exe",
            "0install-win.exe",
            "0launch.exe",
            "0alias.exe",
            "0store.exe",
            "0store-service.exe",
            "ZeroInstall.exe",
            "ZeroInstall.OneGet.dll",
            "ZeroInstall.Store.XmlSerializers.dll",
            "ZeroInstall.DesktopIntegration.XmlSerializers.dll"
        };

        /// <summary>
        /// Runs ngen in the background to pre-compile new/updated .NET assemblies.
        /// </summary>
        private void NgenApply()
        {
            if (!WindowsUtils.IsWindows) return;
            if (!File.Exists(_ngenExe)) return;

            Handler.RunTask(ForEachTask.Create(Resources.RunNgen, _ngenAssemblies, assembly =>
            {
                string arguments = new[] {"install", Path.Combine(TargetDir, assembly), "/queue"}.JoinEscapeArguments();
                new ProcessStartInfo(_ngenExe, arguments) {WindowStyle = ProcessWindowStyle.Hidden}.Run();
            }));
        }

        /// <summary>
        /// Runs ngen to remove pre-compiled .NET assemblies.
        /// </summary>
        private void NgenRemove()
        {
            if (!WindowsUtils.IsWindows) return;
            if (!File.Exists(_ngenExe)) return;

            foreach (string assembly in _ngenAssemblies)
            {
                string arguments = new[] {"uninstall", Path.Combine(TargetDir, assembly)}.JoinEscapeArguments();
                new ProcessStartInfo(_ngenExe, arguments) {WindowStyle = ProcessWindowStyle.Hidden}.Run();
            }
        }
    }
}
