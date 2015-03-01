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

using System;
using System.ComponentModel;
using System.IO;
using ELFSharp.ELF;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// An ELF (Executable and Linkable Format) binary for a POSIX-style operation system.
    /// </summary>
    public sealed class PosixBinary : PosixExecutable
    {
        /// <inheritdoc/>
        internal override bool Analyze(DirectoryInfo baseDirectory, FileInfo file)
        {
            if (!base.Analyze(baseDirectory, file)) return false;
            if (!HasMagicBytes(file)) return false;

            Name = file.Name;
            Architecture = new Architecture(OS.Linux, Cpu.All);

            IELF elfData = null;
            try
            {
                if (ELFReader.TryLoad(file.FullName, out elfData))
                {
                    if (elfData.Class == Class.NotELF || elfData.Type != FileType.Executable) return false;

                    Architecture = new Architecture(OS.Linux, GetCpu(elfData));
                }
            }
            catch (NullReferenceException)
            {}
            finally
            {
                if (elfData != null) elfData.Dispose();
            }

            return true;
        }

        private static bool HasMagicBytes(FileInfo file)
        {
            using (var stream = file.OpenRead())
            {
                var magic = new byte[4];
                stream.Read(magic, 0, 4);
                if (magic[0] != 0x7f || magic[1] != 0x45 || magic[2] != 0x4c || magic[3] != 0x46) return false;
            }
            return true;
        }

        private static Cpu GetCpu(IELF elfData)
        {
            switch (elfData.Machine)
            {
                case Machine.Intel386:
                    return Cpu.I386;
                case Machine.Intel486:
                    return Cpu.I486;
                case Machine.AMD64:
                    return Cpu.X64;
                case Machine.PPC:
                    return Cpu.Ppc;
                case Machine.PPC64:
                    return Cpu.Ppc64;
                default:
                    return Cpu.Unknown;
            }
        }

        /// <summary>
        /// The specific POSIX-style operating system the binary is compiled for.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">A non-POSIX <see cref="OS"/> value is specified.</exception>
        [Category("Details (POSIX)"), DisplayName(@"Operating system"), Description("The specific POSIX-style operating system the binary is compiled for.")]
        [DefaultValue(typeof(OS), "Linux")]
        public OS OS
        {
            get { return Architecture.OS; }
            set
            {
                if (value < OS.Linux || value >= OS.Cygwin) throw new ArgumentOutOfRangeException("value", "Must be a specific POSIX OS!");
                Architecture = new Architecture(value, Architecture.Cpu);
            }
        }
    }
}
