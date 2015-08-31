/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Globalization;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Values;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Provides utility methods for application entry points.
    /// </summary>
    public static class ProgramUtils
    {
        /// <summary>
        /// Indicates whether the application is installed in a user-specific location.
        /// </summary>
        public static bool PerUserInstall { get { return Locations.InstallBase.StartsWith(Locations.HomeDir); } }

        /// <summary>
        /// Indicates whether the application's current location is prone to change, e.g. because this is a portable installation or executed from a cache directory.
        /// </summary>
        public static bool TransientInstall { get { return Locations.IsPortable || StoreUtils.PathInAStore(Locations.InstallBase); } }

        /// <summary>
        /// Common initialization code to be called by every Frontend executable right after startup.
        /// </summary>
        public static void Init()
        {
            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + Locations.InstallBase.GetHashCode();
            if (AppMutex.Probe(mutexName + "-update")) Environment.Exit(999);
            AppMutex.Create(mutexName);

            if (WindowsUtils.IsWindows)
            {
                if (UILanguage != null) Languages.SetUI(UILanguage);

                if (!TransientInstall)
                {
                    try
                    {
                        RegistryUtils.SetSoftwareString("Zero Install", "InstallLocation", Locations.InstallBase);
                        RegistryUtils.SetSoftwareString(@"Microsoft\PackageManagement", "ZeroInstall", Path.Combine(Locations.InstallBase, "ZeroInstall.OneGet.dll"));
                    }
                    catch (IOException)
                    {}
                    catch (UnauthorizedAccessException)
                    {}
                }
            }

            NetUtils.ApplyProxy();
            if (!WindowsUtils.IsWindows7) NetUtils.TrustCertificates(SyncIntegrationManager.DefaultServerPublicKey);
        }

        /// <summary>
        /// The current UI language; <see langword="null"/> to use system default.
        /// </summary>
        /// <remarks>This value is only used on Windows and is stored in the Registry. For non-Windows platforms use the <code>LC_*</code> environment variables instead.</remarks>
        [CanBeNull]
        public static CultureInfo UILanguage
        {
            get
            {
                string language = RegistryUtils.GetSoftwareString("Zero Install", "Language");
                if (!string.IsNullOrEmpty(language))
                {
                    try
                    {
                        return Languages.FromString(language);
                    }
                    catch (ArgumentException ex)
                    {
                        Log.Warn(ex);
                    }
                }
                return null;
            }
            set { RegistryUtils.SetSoftwareString("Zero Install", "Language", (value == null) ? "" : value.ToString()); }
        }

        /// <summary>
        /// The EXE name for the Command GUI best suited for the current system; <see langword="null"/> if no GUI subsystem is running.
        /// </summary>
        [CanBeNull]
        public static readonly string GuiAssemblyName =
            WindowsUtils.IsWindows
                ? (WindowsUtils.IsInteractive ? "0install-win" : null)
                : (UnixUtils.HasGui ? "0install-gtk" : null);
    }
}
