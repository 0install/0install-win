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
using System.IO;
using Microsoft.Win32;
using NanoByte.Common;
using NanoByte.Common.Info;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration.Windows;

namespace ZeroInstall.Commands.Utils
{
    partial class MaintenanceManager
    {
        private void DesktopIntegrationApply()
        {
            if (WindowsUtils.IsWindows)
            {
                Handler.RunTask(new SimpleTask(Resources.DesktopIntegrationApply, () =>
                {
                    Shortcut.Create(
                        path: Shortcut.GetStartMenuPath("", "Zero Install", MachineWide),
                        targetPath: Path.Combine(TargetDir, "ZeroInstall.exe"));

                    PathEnv.AddDir(TargetDir, MachineWide);
                }));
            }
        }

        private void DesktopIntegrationRemove()
        {
            if (WindowsUtils.IsWindows)
            {
                Handler.RunTask(new SimpleTask(Resources.DesktopIntegrationRemove, () =>
                {
                    DeleteIfExists(Shortcut.GetStartMenuPath("", "Zero Install", MachineWide));
                    DeleteIfExists(Shortcut.GetStartMenuPath("Zero install", "Zero Install", MachineWide));
                    DeleteIfExists(Shortcut.GetDesktopPath("Zero Install", MachineWide));

                    PathEnv.RemoveDir(TargetDir, MachineWide);
                }));
            }
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        /// <summary>
        /// Update the registry entries.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Administrator rights are missing.</exception>
        private void RegistryApply()
        {
            var hive = MachineWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var uninsKey = hive.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Zero Install_is1"))
            {
                if (uninsKey == null) return;

                uninsKey.SetValue("InstallLocation", TargetDir + @"\");
                uninsKey.SetValue("Publisher", "0install.de");
                uninsKey.SetValue("URLInfoAbout", "http://0install.de/");
                uninsKey.SetValue("DisplayName", MachineWide ? AppInfo.Current.ProductName : AppInfo.Current.ProductName + " (current user)");
                uninsKey.SetValue("DisplayVersion", AppInfo.Current.Version.ToString());
                uninsKey.SetValue("MajorVersion", AppInfo.Current.Version.Major, RegistryValueKind.DWord);
                uninsKey.SetValue("MinorVersion", AppInfo.Current.Version.Minor, RegistryValueKind.DWord);
                uninsKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));

                uninsKey.SetValue("DisplayIcon", Path.Combine(TargetDir, "ZeroInstall.exe"));
                uninsKey.SetValue("UninstallString", new[] {Path.Combine(TargetDir, "0install-win.exe"), MaintenanceMan.Name, MaintenanceMan.Remove.Name}.JoinEscapeArguments());
                uninsKey.SetValue("QuietUninstallString", new[] {Path.Combine(TargetDir, "0install-win.exe"), MaintenanceMan.Name, MaintenanceMan.Remove.Name, "--batch", "--background"}.JoinEscapeArguments());
                uninsKey.SetValue("NoModify", 1, RegistryValueKind.DWord);
                uninsKey.SetValue("NoRepiar", 1, RegistryValueKind.DWord);
            }

            RegistryUtils.SetSoftwareString("Zero Install", "InstallLocation", TargetDir, MachineWide);
            RegistryUtils.SetSoftwareString(@"Microsoft\PackageManagement", "ZeroInstall", Path.Combine(TargetDir, "ZeroInstall.OneGet.dll"), MachineWide);
        }

        private void RegistryRemove()
        {
            RegistryUtils.DeleteSoftwareValue("Zero Install", "InstallLocation", MachineWide);
            RegistryUtils.DeleteSoftwareValue(@"Microsoft\PackageManagement", "ZeroInstall", MachineWide);

            var hive = MachineWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var uninsKey = hive.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
                if (uninsKey != null) uninsKey.DeleteSubKey("Zero Install_is1", throwOnMissingSubKey: false);
        }
    }
}
