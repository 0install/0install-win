﻿/*
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
using ELFSharp;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// An ELF (Executable and Linkable Format) binary for a POSIX-style operation system.
    /// </summary>
    public sealed class PosixBinary : PosixExecutable
    {
        /// <inheritdoc/>
        internal override bool Analyze(FileInfo file)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException("file");
            #endregion

            if (!base.Analyze(file)) return false;

            IELF elfData;
            if (!ELFReader.TryLoad(file.FullName, out elfData)) return false;
            if (elfData.Class == Class.NotELF || elfData.Type != FileType.Executable) return false;
            Name = file.Name;
            Architecture = new Architecture(OS.Linux, GetCpu(elfData));
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
                    return Cpu.PPC;
                case Machine.PPC64:
                    return Cpu.PPC64;
                default:
                    return Cpu.Unknown;
            }
        }

        /// <summary>
        /// The specific POSIX-style operating system the binary is compiled for.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a non-POSIX <see cref="OS"/> value is specified.</exception>
        [Description("The specific POSIX-style operating system the binary is compiled for.")]
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
