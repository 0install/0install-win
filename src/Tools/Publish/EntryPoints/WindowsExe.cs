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
using System.Diagnostics;
using System.IO;
using NanoByte.Common.Utils;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// A native PE (Portable Executable) for Windows.
    /// </summary>
    public class WindowsExe : NativeExecutable
    {
        /// <inheritdoc/>
        internal override bool Analyze(DirectoryInfo baseDirectory, FileInfo file)
        {
            if (!base.Analyze(baseDirectory, file)) return false;
            if (!StringUtils.EqualsIgnoreCase(file.Extension, ".exe")) return false;

            try
            {
                Parse(FileVersionInfo.GetVersionInfo(file.FullName));
                return Parse(new PEHeader(file.FullName));
            }
                #region Error handling
            catch (IOException)
            {
                return false;
            }
            #endregion
        }

        private void Parse(FileVersionInfo versionInfo)
        {
            Name = versionInfo.ProductName;
            Summary = string.IsNullOrEmpty(versionInfo.Comments) ? versionInfo.FileDescription : versionInfo.Comments;
            if (!string.IsNullOrEmpty(versionInfo.ProductVersion))
            {
                try
                {
                    Version = new ImplementationVersion(versionInfo.ProductVersion.Trim());
                }
                catch (ArgumentException)
                {}
            }
        }

        protected virtual bool Parse(PEHeader peHeader)
        {
            #region Sanity checks
            if (peHeader == null) throw new ArgumentNullException("peHeader");
            #endregion

            Architecture = new Architecture(OS.Windows, GetCpu(peHeader.FileHeader.Machine));
            if (peHeader.Subsystem >= Subsystem.WindowsCui) NeedsTerminal = true;
            return peHeader.Is32BitHeader
                ? (peHeader.OptionalHeader32.CLRRuntimeHeader.VirtualAddress == 0)
                : (peHeader.OptionalHeader64.CLRRuntimeHeader.VirtualAddress == 0);
        }

        [CLSCompliant(false)]
        protected static Cpu GetCpu(MachineType machine)
        {
            switch (machine)
            {
                case MachineType.I386:
                    return Cpu.All;
                case MachineType.X64:
                    return Cpu.X64;
                default:
                    return Cpu.Unknown;
            }
        }

        /// <summary>
        /// Extracts the primary icon of the EXE. 
        /// </summary>
        public System.Drawing.Icon ExtractIcon()
        {
            return System.Drawing.Icon.ExtractAssociatedIcon(Path.Combine(BaseDirectory.FullName, RelativePath));
        }
    }
}
