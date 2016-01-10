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

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands.Utils
{
    partial class MaintenanceManager
    {
        private static readonly string _netFxDir = WindowsUtils.GetNetFxDirectory(WindowsUtils.HasNetFxVersion(WindowsUtils.NetFx40) ? WindowsUtils.NetFx40 : WindowsUtils.NetFx20);

        private const string ServiceName = "0store-service";

        /// <summary>
        /// Stops the Zero Install Store Service if it is running.
        /// </summary>
        /// <returns><see langword="true"/> if the service was running; <see langword="false"/> otherwise.</returns>
        private bool ServiceStop()
        {
            // Determine whether the service is installed and running
            var service = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == ServiceName);
            if (service == null) return false;
            if (service.Status == ServiceControllerStatus.Stopped) return false;

            // Determine whether the service is installed in the target directory we are updating
            string imagePath = RegistryUtils.GetString(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\0store-service", "ImagePath").Trim('"');
            if (!imagePath.StartsWith(TargetDir)) return false;

            Handler.RunTask(new SimpleTask(Resources.StopService, () =>
            {
                try
                {
                    service.Stop();
                    Thread.Sleep(2000);
                }
                    #region Sanity checks
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
            }));
            return true;
        }

        /// <summary>
        /// Starts the Zero Install Store Service.
        /// </summary>
        /// <remarks>Must call this after <see cref="TargetMutexRelease"/>.</remarks>
        private void ServiceStart()
        {
            Handler.RunTask(new SimpleTask(Resources.StartService, () =>
            {
                var controller = new ServiceController(ServiceName);
                try
                {
                    controller.Start();
                }
                    #region Sanity checks
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
        }

        private static readonly string _installUtilExe = Path.Combine(_netFxDir, "InstallUtil.exe");
        private string ServiceExe { get { return Path.Combine(TargetDir, "0store-service.exe"); } }

        /// <summary>
        /// Installs the Zero Install Store Service.
        /// </summary>
        private void ServiceInstall()
        {
            Handler.RunTask(new SimpleTask(Resources.InstallService, () =>
                new ProcessStartInfo(_installUtilExe, ServiceExe.EscapeArgument())
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetTempPath()
                }.Run()));
        }

        /// <summary>
        /// Uninstalls the Zero Install Store Service.
        /// </summary>
        private void ServiceUninstall()
        {
            Handler.RunTask(new SimpleTask(Resources.UninstallService, () =>
                new ProcessStartInfo(_installUtilExe, new[] {"/u", ServiceExe}.JoinEscapeArguments())
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetTempPath()
                }.Run()));
        }

        private void DeleteServiceLogFiles()
        {
            File.Delete(Path.Combine(TargetDir, "0store-service.InstallLog"));
            File.Delete(Path.Combine(TargetDir, "InstallUtil.InstallLog"));
        }

        private static readonly string _ngenExe = Path.Combine(_netFxDir, "ngen.exe");

        private static readonly string[] _ngenAssemblies =
        {
            "0install.exe", "0install-win.exe", "0launch.exe", "0alias.exe", "0store.exe", "0store-service.exe", "ZeroInstall.exe",
            "ZeroInstall.OneGet.dll", "ZeroInstall.Store.XmlSerializers.dll", "ZeroInstall.DesktopIntegration.XmlSerializers.dll"
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
