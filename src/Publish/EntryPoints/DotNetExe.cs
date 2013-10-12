/*
 * Copyright 2010-2013 Bastian Eicher
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

using System.ComponentModel;
using ZeroInstall.Model;
using ZeroInstall.Publish.EntryPoints.Design;

namespace ZeroInstall.Publish.EntryPoints
{
    public enum DotNetRuntimeType
    {
        Any,
        ClientProfile,
        MicrosoftOnly,
        MonoOnly
    }

    /// <summary>
    /// A .NET/Mono executable.
    /// </summary>
    public sealed class DotNetExe : WindowsExe
    {
        protected override bool Parse(PEHeader peHeader)
        {
            Architecture = new Architecture(OS.All, GetCpu(peHeader.FileHeader.Machine));
            if (peHeader.Subsystem >= Subsystem.WindowsCui) NeedsTerminal = true;
            return peHeader.Is32BitHeader
                ? (peHeader.OptionalHeader32.CLRRuntimeHeader.VirtualAddress != 0)
                : (peHeader.OptionalHeader64.CLRRuntimeHeader.VirtualAddress != 0);
        }

        /// <summary>
        /// The range of versions of the .NET Framework supported by the application.
        /// </summary>
        [Description("Supported .NET Framework versions")]
        [DefaultValue("")]
        [TypeConverter(typeof(DotNetRuntimeVersionConverter))]
        public VersionRange RuntimeVersion { get; set; }

        /// <summary>
        /// The types of .NET Runtime supported by the application.
        /// </summary>
        [Description("Supported types of .NET Runtime")]
        [DefaultValue(typeof(DotNetExe), "Any")]
        public DotNetRuntimeType RuntimeType { get; set; }

        /// <summary>
        /// Does this application have external dependencies that need to be injected by Zero Install?
        /// </summary>
        [Description("External dependencies to be injected by Zero Install?")]
        [DefaultValue(false)]
        public bool HasDependencies { get; set; }

        /// <inheritdoc/>
        public override Command Command
        {
            get
            {
                string interfaceID = HasDependencies
                    ? (NeedsTerminal
                        ? "http://0install.de/feeds/cli/cli-monopath-terminal.xml"
                        : "http://0install.de/feeds/cli/cli-monopath.xml")
                    : "http://0install.de/feeds/cli/cli.xml";

                return new Command
                {
                    Name = Command.NameRun,
                    Path = RelativePath,
                    Runner = new Runner {Interface = interfaceID, Versions = RuntimeVersion}
                };
            }
        }

        #region Equality
        private bool Equals(DotNetExe other)
        {
            return base.Equals(other) &&
                   RuntimeVersion == other.RuntimeVersion &&
                   RuntimeType == other.RuntimeType &&
                   HasDependencies == other.HasDependencies;
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
                hashCode = (hashCode * 397) ^ (RuntimeVersion != null ? RuntimeVersion.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)RuntimeType;
                hashCode = (hashCode * 397) ^ HasDependencies.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}
