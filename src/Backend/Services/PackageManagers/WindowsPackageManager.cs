/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services.PackageManagers
{
    /// <summary>
    /// Detects common Windows software packages (such as Java and .NET) that are installed natively.
    /// </summary>
    public class WindowsPackageManager : PackageManagerBase
    {
        public WindowsPackageManager()
        {
            if (!WindowsUtils.IsWindows) throw new NotSupportedException("WindowsPackageManager can only be used on the Windows platform.");
        }

        /// <inheritdoc/>
        protected override string DistributionName { get { return "Windows"; } }

        /// <inheritdoc/>
        protected override IEnumerable<ExternalImplementation> GetImplementations(string packageName)
        {
            #region Sanity checks
            if (packageName == null) throw new ArgumentNullException("packageName");
            #endregion

            switch (packageName)
            {
                case "openjdk-6-jre":
                    return FindJre(6);
                case "openjdk-7-jre":
                    return FindJre(7);
                case "openjdk-8-jre":
                    return FindJre(8);

                case "openjdk-6-jdk":
                    return FindJdk(6);
                case "openjdk-7-jdk":
                    return FindJdk(7);
                case "openjdk-8-jdk":
                    return FindJdk(8);

                case "netfx":
                    return new[]
                    {
                        FindNetFx(new ImplementationVersion("2.0"), WindowsUtils.NetFx20, WindowsUtils.NetFx20),
                        FindNetFx(new ImplementationVersion("3.0"), WindowsUtils.NetFx20, WindowsUtils.NetFx30),
                        FindNetFx(new ImplementationVersion("3.5"), WindowsUtils.NetFx20, WindowsUtils.NetFx35),
                        FindNetFx(new ImplementationVersion("4.0"), WindowsUtils.NetFx40, @"v4\Full"),
                        FindNetFx(new ImplementationVersion("4.5"), WindowsUtils.NetFx40, @"v4\Full", 378389),
                        FindNetFx(new ImplementationVersion("4.5.1"), WindowsUtils.NetFx40, @"v4\Full", 378675),
                        FindNetFx(new ImplementationVersion("4.5.2"), WindowsUtils.NetFx40, @"v4\Full", 379893)
                    }.Flatten();
                case "netfx-client":
                    return FindNetFx(new ImplementationVersion("4.0"), WindowsUtils.NetFx40, @"v4\Client");

                default:
                    return Enumerable.Empty<ExternalImplementation>();
            }
        }

        private IEnumerable<ExternalImplementation> FindJre(int version)
        {
            return FindJava(version,
                typeShort: "jre",
                typeLong: "Java Runtime Environment",
                mainExe: "java",
                secondaryCommand: Command.NameRunGui,
                secondaryExe: "javaw");
        }

        private IEnumerable<ExternalImplementation> FindJdk(int version)
        {
            return FindJava(version,
                typeShort: "jdk",
                typeLong: "Java Development Kit",
                mainExe: "javac",
                secondaryCommand: "java",
                secondaryExe: "java");
        }

        private IEnumerable<ExternalImplementation> FindJava(int version, string typeShort, string typeLong, string mainExe, string secondaryCommand, string secondaryExe)
        {
            foreach (var javaHome in GetRegistredPaths(@"JavaSoft\" + typeLong + @"\1." + version, "JavaHome"))
            {
                string mainPath = Path.Combine(javaHome.Value, @"bin\" + mainExe + ".exe");
                string secondaryPath = Path.Combine(javaHome.Value, @"bin\" + secondaryExe + ".exe");

                if (File.Exists(mainPath) && File.Exists(secondaryPath))
                {
                    yield return new ExternalImplementation(DistributionName,
                        package: "openjdk-" + version + "-" + typeShort,
                        version: new ImplementationVersion(FileVersionInfo.GetVersionInfo(mainPath).ProductVersion.GetLeftPartAtLastOccurrence(".")), // Trim patch level
                        architecture: javaHome.Key)
                    {
                        Commands =
                        {
                            new Command {Name = Command.NameRun, Path = mainPath},
                            new Command {Name = secondaryCommand, Path = secondaryPath}
                        },
                        IsInstalled = true,
                        QuickTestFile = mainPath
                    };
                }
            }
        }

        private static IEnumerable<KeyValuePair<Architecture, string>> GetRegistredPaths(string registrySuffix, string valueName)
        {
            // Check for system native architecture (may be 32-bit or 64-bit)
            string path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\" + registrySuffix, valueName, null) as string;
            if (!string.IsNullOrEmpty(path))
                yield return new KeyValuePair<Architecture, string>(Architecture.CurrentSystem, path);

            // Check for 32-bit on a 64-bit system
            if (WindowsUtils.Is64BitProcess)
            {
                path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\" + registrySuffix, valueName, null) as string;
                if (!string.IsNullOrEmpty(path))
                    yield return new KeyValuePair<Architecture, string>(new Architecture(OS.Windows, Cpu.I486), path);
            }
        }

        // Uses detection logic described here: http://msdn.microsoft.com/library/hh925568
        private IEnumerable<ExternalImplementation> FindNetFx(ImplementationVersion version, string clrVersion, string registryVersion, int releaseNumber = 0)
        {
            int install = GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\" + registryVersion, "Install", 0);
            int release = GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\" + registryVersion, "Release", 0);
            if (install == 1 && release >= releaseNumber)
            {
                yield return new ExternalImplementation(DistributionName, "netfx", version, Architecture.CurrentSystem)
                {
                    // .NET executables do not need a runner on Windows
                    Commands = {new Command {Name = Command.NameRun, Path = ""}},
                    IsInstalled = true,
                    QuickTestFile = Path.Combine(WindowsUtils.GetNetFxDirectory(clrVersion), "mscorlib.dll")
                };
            }
        }

        private static int GetDword(string key, string value, int defaultValue)
        {
            return Registry.GetValue(key, value, defaultValue) as int? ?? defaultValue;
        }
    }
}
