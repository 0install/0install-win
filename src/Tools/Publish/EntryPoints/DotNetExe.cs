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
using System.ComponentModel;
using ZeroInstall.Publish.EntryPoints.Design;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    public enum DotNetRuntimeType
    {
        Any,
        MicrosoftOnlyClientProfile,
        MicrosoftOnlyFullProfile,
        MonoOnly
    }

    /// <summary>
    /// A .NET/Mono executable.
    /// </summary>
    public sealed class DotNetExe : WindowsExe
    {
        protected override bool Parse(PEHeader peHeader)
        {
            #region Sanity checks
            if (peHeader == null) throw new ArgumentNullException("peHeader");
            #endregion

            Architecture = new Architecture(OS.All, GetCpu(peHeader.FileHeader.Machine));
            if (peHeader.Subsystem >= Subsystem.WindowsCui) NeedsTerminal = true;
            return peHeader.Is32BitHeader
                ? (peHeader.OptionalHeader32.CLRRuntimeHeader.VirtualAddress != 0)
                : (peHeader.OptionalHeader64.CLRRuntimeHeader.VirtualAddress != 0);
        }

        /// <summary>
        /// The minimum version of the .NET Runtime required by the application.
        /// </summary>
        [Category("Details (.NET)"), DisplayName("Minimum .NET version"), Description("The minimum version of the .NET Runtime required by the application.")]
        [DefaultValue("")]
        [TypeConverter(typeof(DotNetVersionConverter))]
        public ImplementationVersion MinimumRuntimeVersion { get; set; }

        /// <summary>
        /// The types of .NET runtimes supported by the application.
        /// </summary>
        [Category("Details (.NET)"), DisplayName(".NET type"), Description("The types of .NET runtimes supported by the application.")]
        [DefaultValue(typeof(DotNetRuntimeType), "Any")]
        public DotNetRuntimeType RuntimeType { get; set; }

        /// <summary>
        /// Does this application have external dependencies that need to be injected by Zero Install? Only enable if you are sure!
        /// </summary>
        [Category("Details (.NET)"), DisplayName("External dependencies"), Description("Does this application have external dependencies that need to be injected by Zero Install? Only enable if you are sure!")]
        [DefaultValue(false)]
        public bool ExternalDependencies { get; set; }

        /// <inheritdoc/>
        public override Command Command
        {
            get
            {
                return new Command
                {
                    Name = Command.NameRun,
                    Path = RelativePath,
                    Runner = new Runner {InterfaceID = GetInterfaceID(), Versions = (VersionRange)MinimumRuntimeVersion}
                };
            }
        }

        private string GetInterfaceID()
        {
            switch (RuntimeType)
            {
                case DotNetRuntimeType.Any:
                default:
                    return ExternalDependencies
                        ? GetMonoPathInterfaceID()
                        : "http://0install.de/feeds/cli/cli.xml";

                case DotNetRuntimeType.MicrosoftOnlyClientProfile:
                    Architecture = new Architecture(OS.Windows, Architecture.Cpu);
                    return ExternalDependencies
                        ? GetMonoPathInterfaceID()
                        : "http://0install.de/feeds/cli/netfx-client.xml";

                case DotNetRuntimeType.MicrosoftOnlyFullProfile:
                    Architecture = new Architecture(OS.Windows, Architecture.Cpu);
                    return ExternalDependencies
                        ? GetMonoPathInterfaceID()
                        : "http://0install.de/feeds/cli/netf.xml";

                case DotNetRuntimeType.MonoOnly:
                    return "http://0install.de/feeds/cli/mono.xml";
            }
        }

        private string GetMonoPathInterfaceID()
        {
            return NeedsTerminal
                ? "http://0install.de/feeds/cli/cli-monopath-terminal.xml"
                : "http://0install.de/feeds/cli/cli-monopath.xml";
        }

        #region Equality
        private bool Equals(DotNetExe other)
        {
            return base.Equals(other) &&
                   MinimumRuntimeVersion == other.MinimumRuntimeVersion &&
                   RuntimeType == other.RuntimeType &&
                   ExternalDependencies == other.ExternalDependencies;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is DotNetExe && Equals((DotNetExe)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (MinimumRuntimeVersion != null ? MinimumRuntimeVersion.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)RuntimeType;
                hashCode = (hashCode * 397) ^ ExternalDependencies.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}
