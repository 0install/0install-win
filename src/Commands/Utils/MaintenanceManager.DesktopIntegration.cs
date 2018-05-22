// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

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
        /// <param name="size">The size of the installed files in bytes.</param>
        /// <exception cref="UnauthorizedAccessException">Administrator rights are missing.</exception>
        private void RegistryApply(long size)
        {
            var hive = MachineWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var uninsKey = hive.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Zero Install_is1"))
            {
                if (uninsKey == null) return;

                uninsKey.SetValue("InstallLocation", TargetDir + @"\");
                uninsKey.SetValue("Publisher", "0install.de");
                uninsKey.SetValue("URLInfoAbout", "http://0install.de/");
                uninsKey.SetValue("DisplayName", MachineWide ? AppInfo.CurrentLibrary.ProductName : AppInfo.CurrentLibrary.ProductName + " (current user)");
                uninsKey.SetValue("DisplayVersion", AppInfo.CurrentLibrary.Version.ToString());
                uninsKey.SetValue("MajorVersion", AppInfo.CurrentLibrary.Version.Major, RegistryValueKind.DWord);
                uninsKey.SetValue("MinorVersion", AppInfo.CurrentLibrary.Version.Minor, RegistryValueKind.DWord);
                uninsKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                uninsKey.SetValue("EstimatedSize", size / 1024, RegistryValueKind.DWord);

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
                uninsKey?.DeleteSubKey("Zero Install_is1", throwOnMissingSubKey: false);
        }
    }
}
